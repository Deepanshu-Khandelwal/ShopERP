import { prisma } from '../lib/prisma.js';

function calcLine(item) {
  const qty = Number(item.quantity || 0);
  const rate = Number(item.unitRate || 0);
  const discount = Number(item.discount || 0);
  const gst = Number(item.gstPercent || 0);

  const base = qty * rate - discount;
  const tax = (base * gst) / 100;
  const total = base + tax;

  return { qty, rate, discount, gst, total };
}

function normalizeOptionalName(value) {
  if (value === undefined) return undefined;
  const normalized = String(value || '').trim();
  return normalized || null;
}

function normalizeDoctorKey(name) {
  return String(name || '').trim().toLowerCase();
}

async function upsertDoctor(client, shopId, doctorName, { incrementVisit = false, lastVisitAt = null } = {}) {
  const normalized = normalizeOptionalName(doctorName);
  if (!normalized) return;

  const normalizedName = normalizeDoctorKey(normalized);

  await client.doctor.upsert({
    where: {
      shopId_normalizedName: {
        shopId,
        normalizedName
      }
    },
    create: {
      shopId,
      name: normalized,
      normalizedName,
      visitCount: incrementVisit ? 1 : 0,
      lastVisitAt: lastVisitAt || new Date()
    },
    update: {
      name: normalized,
      lastVisitAt: lastVisitAt || new Date(),
      ...(incrementVisit ? { visitCount: { increment: 1 } } : {})
    }
  });
}

export async function listSales(req, res) {
  const rows = await prisma.salesBill.findMany({
    where: { shopId: req.user.shopId },
    include: {
      items: {
        include: {
          product: { select: { name: true } },
          batch: { select: { batchNo: true } }
        }
      }
    },
    orderBy: { billDate: 'desc' }
  });
  res.json(rows);
}

export async function getSaleById(req, res) {
  const shopId = req.user.shopId;
  const id = String(req.params.id || '');

  const row = await prisma.salesBill.findFirst({
    where: { id, shopId },
    include: {
      items: {
        include: {
          product: { select: { name: true } },
          batch: { select: { batchNo: true } }
        }
      }
    }
  });

  if (!row) {
    return res.status(404).json({ message: 'Bill not found' });
  }

  return res.json(row);
}

export async function updateSale(req, res) {
  const shopId = req.user.shopId;
  const id = String(req.params.id || '');

  const existing = await prisma.salesBill.findFirst({ where: { id, shopId } });
  if (!existing) {
    return res.status(404).json({ message: 'Bill not found' });
  }

  const payload = {
    customerName: req.body.customerName ?? existing.customerName,
    doctorName:
      req.body.doctorName === undefined
        ? existing.doctorName
        : normalizeOptionalName(req.body.doctorName),
    billDate: req.body.billDate ? new Date(req.body.billDate) : existing.billDate
  };

  const updated = await prisma.salesBill.update({
    where: { id: existing.id },
    data: payload,
    include: {
      items: {
        include: {
          product: { select: { name: true } },
          batch: { select: { batchNo: true } }
        }
      }
    }
  });

  await upsertDoctor(prisma, shopId, payload.doctorName, {
    incrementVisit: false,
    lastVisitAt: updated.billDate
  });

  return res.json(updated);
}

export async function createSale(req, res) {
  const shopId = req.user.shopId;
  const {
    invoiceNo,
    customerName,
    doctorName,
    items = [],
    paymentType = 'cash',
    paidAmount = 0
  } = req.body;
  const normalizedDoctorName = normalizeOptionalName(doctorName);

  if (!invoiceNo) {
    return res.status(400).json({ message: 'invoiceNo is required' });
  }

  if (!Array.isArray(items) || items.length === 0) {
    return res.status(400).json({ message: 'At least one sale item is required' });
  }

  const paymentMode = String(paymentType).toLowerCase();
  if (!['cash', 'credit', 'online'].includes(paymentMode)) {
    return res.status(400).json({ message: 'paymentType must be cash, credit, or online' });
  }

  const normalizedItems = items.map((item) => ({
    ...item,
    quantity: Number(item.quantity || 0)
  }));

  if (normalizedItems.some((item) => item.quantity <= 0)) {
    return res.status(400).json({ message: 'All item quantities must be greater than 0' });
  }

  const computed = normalizedItems.map(calcLine);
  const subtotal = computed.reduce((sum, r) => sum + (r.qty * r.rate - r.discount), 0);
  const taxTotal = computed.reduce((sum, r) => sum + ((r.qty * r.rate - r.discount) * r.gst) / 100, 0);
  const grandTotal = subtotal + taxTotal;
  const paid = ['cash', 'online'].includes(paymentMode)
    ? grandTotal
    : Math.max(0, Number(paidAmount || 0));
  const due = Math.max(0, grandTotal - paid);

  try {
    const result = await prisma.$transaction(async (tx) => {
    const productIds = [...new Set(normalizedItems.map((i) => i.productId).filter(Boolean))];
    const products = await tx.product.findMany({
      where: { shopId, id: { in: productIds } },
      select: { id: true, purchaseRate: true }
    });
    const productMap = new Map(products.map((p) => [p.id, p]));

    const batchIds = [...new Set(normalizedItems.map((i) => i.batchId).filter(Boolean))];
    const batches = batchIds.length
      ? await tx.productBatch.findMany({
          where: { shopId, id: { in: batchIds } },
          select: {
            id: true,
            productId: true,
            quantity: true,
            expiryDate: true,
            purchasePrice: true,
            mrp: true
          }
        })
      : [];
    const batchMap = new Map(batches.map((b) => [b.id, b]));

    for (const item of normalizedItems) {
      const batch = item.batchId ? batchMap.get(item.batchId) : null;
      if (item.batchId && !batch) {
        throw new Error(`Invalid batch selected for product ${item.productId}`);
      }
      if (batch && batch.productId !== item.productId) {
        throw new Error('Selected batch does not match product');
      }
      if (batch && batch.expiryDate && batch.expiryDate < new Date()) {
        throw new Error('Expired batch cannot be sold');
      }
      if (batch && batch.quantity < item.quantity) {
        throw new Error('Insufficient stock in selected batch');
      }

      if (!batch) {
        const available = await tx.productBatch.aggregate({
          where: {
            shopId,
            productId: item.productId,
            quantity: { gt: 0 },
            OR: [{ expiryDate: null }, { expiryDate: { gte: new Date() } }]
          },
          _sum: { quantity: true }
        });
        if (Number(available._sum.quantity || 0) < item.quantity) {
          throw new Error('Insufficient stock');
        }
      }
    }

    const bill = await tx.salesBill.create({
      data: {
        shopId,
        invoiceNo,
        customerName,
        doctorName: normalizedDoctorName,
        subtotal,
        taxTotal,
        grandTotal,
        paidTotal: paid,
        dueTotal: due,
        items: {
          create: normalizedItems.map((item, i) => ({
            productId: item.productId,
            batchId: item.batchId || null,
            quantity: computed[i].qty,
            unitRate: computed[i].rate,
            costRate: item.batchId
              ? Number(batchMap.get(item.batchId)?.purchasePrice || 0)
              : Number(productMap.get(item.productId)?.purchaseRate || 0),
            discount: computed[i].discount,
            gstPercent: computed[i].gst,
            lineTotal: computed[i].total
          }))
        }
      },
      include: { items: true }
    });

    await upsertDoctor(tx, shopId, normalizedDoctorName, {
      incrementVisit: true,
      lastVisitAt: bill.billDate
    });

    for (const item of normalizedItems) {
      if (item.batchId) {
        await tx.productBatch.update({
          where: { id: item.batchId },
          data: { quantity: { decrement: Number(item.quantity || 0) } }
        });
      } else {
        const available = await tx.productBatch.findMany({
          where: {
            shopId,
            productId: item.productId,
            quantity: { gt: 0 },
            OR: [{ expiryDate: null }, { expiryDate: { gte: new Date() } }]
          },
          orderBy: [{ expiryDate: 'asc' }, { createdAt: 'asc' }]
        });

        let remaining = Number(item.quantity || 0);
        for (const batch of available) {
          if (remaining <= 0) break;
          const consume = Math.min(remaining, Number(batch.quantity || 0));
          if (consume > 0) {
            await tx.productBatch.update({
              where: { id: batch.id },
              data: { quantity: { decrement: consume } }
            });
            remaining -= consume;
          }
        }

        if (remaining > 0) {
          throw new Error('Insufficient stock during allocation');
        }
      }

      await tx.product.update({
        where: { id: item.productId },
        data: { stockQty: { decrement: Number(item.quantity || 0) } }
      });
    }

    if (customerName && due > 0) {
      await tx.customerLedgerEntry.create({
        data: {
          shopId,
          customerName,
          debit: due,
          note: `Sale invoice ${invoiceNo}`,
          salesBillId: bill.id
        }
      });
    }

    if (paid > 0) {
      await tx.paymentEntry.create({
        data: {
          shopId,
          amount: paid,
          method: paymentMode.toUpperCase(),
          customerName,
          salesBillId: bill.id,
          createdById: req.user.sub
        }
      });

      if (customerName && paymentMode === 'credit') {
        await tx.customerLedgerEntry.create({
          data: {
            shopId,
            customerName,
            credit: paid,
            note: `Advance received on invoice ${invoiceNo}`,
            salesBillId: bill.id
          }
        });
      }
    }

    return bill;
    });

    return res.status(201).json(result);
  } catch (error) {
    return res.status(400).json({ message: error.message || 'Failed to create sale' });
  }
}
