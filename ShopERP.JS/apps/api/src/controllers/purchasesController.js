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

export async function listPurchases(req, res) {
  const rows = await prisma.purchaseBill.findMany({
    where: { shopId: req.user.shopId },
    include: { items: true, supplier: true },
    orderBy: { billDate: 'desc' }
  });
  res.json(rows);
}

export async function createPurchase(req, res) {
  const shopId = req.user.shopId;
  const { billNo, supplierId, billDate, items = [] } = req.body;

  if (!billNo) {
    return res.status(400).json({ message: 'billNo is required' });
  }

  if (!Array.isArray(items) || items.length === 0) {
    return res.status(400).json({ message: 'At least one purchase item is required' });
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

  try {
    const result = await prisma.$transaction(async (tx) => {
    const bill = await tx.purchaseBill.create({
      data: {
        shopId,
        billNo,
        supplierId,
        billDate: billDate ? new Date(billDate) : undefined,
        subtotal,
        taxTotal,
        grandTotal,
        items: {
          create: normalizedItems.map((item, i) => ({
            productId: item.productId,
            quantity: computed[i].qty,
            unitRate: computed[i].rate,
            discount: computed[i].discount,
            gstPercent: computed[i].gst,
            lineTotal: computed[i].total,
            batchId: null
          }))
        }
      },
      include: { items: true }
    });

    for (const item of normalizedItems) {
      const batchNo = String(item.batchNo || '').trim();
      if (!batchNo) {
        throw new Error('batchNo is required for purchase item');
      }

      const expiryDate = item.expiryDate ? new Date(item.expiryDate) : null;
      const purchasePrice = Number(item.unitRate || item.purchasePrice || 0);
      const mrp = Number(item.mrp || item.unitRate || 0);

      const existingBatch = await tx.productBatch.findFirst({
        where: { shopId, productId: item.productId, batchNo }
      });

      const batch = existingBatch
        ? await tx.productBatch.update({
            where: { id: existingBatch.id },
            data: {
              quantity: { increment: Number(item.quantity || 0) },
              purchasePrice,
              mrp,
              expiryDate
            }
          })
        : await tx.productBatch.create({
            data: {
              shopId,
              productId: item.productId,
              batchNo,
              quantity: Number(item.quantity || 0),
              purchasePrice,
              mrp,
              expiryDate
            }
          });

      await tx.purchaseItem.updateMany({
        where: {
          billId: bill.id,
          productId: item.productId,
          quantity: Number(item.quantity || 0),
          unitRate: Number(item.unitRate || 0),
          batchId: null
        },
        data: { batchId: batch.id }
      });

      await tx.product.update({
        where: { id: item.productId },
        data: {
          stockQty: { increment: Number(item.quantity || 0) },
          purchaseRate: purchasePrice,
          saleRate: mrp,
          expiryDate
        }
      });
    }

    if (supplierId) {
      const supplier = await tx.supplier.findUnique({ where: { id: supplierId } });
      await tx.supplierLedgerEntry.create({
        data: {
          shopId,
          supplierId,
          supplierName: supplier?.name || 'Supplier',
          debit: grandTotal,
          note: `Purchase bill ${billNo}`,
          purchaseBillId: bill.id
        }
      });
    }

    return bill;
    });

    return res.status(201).json(result);
  } catch (error) {
    return res.status(400).json({ message: error.message || 'Failed to create purchase' });
  }
}
