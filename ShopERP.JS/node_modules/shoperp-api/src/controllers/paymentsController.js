import { prisma } from '../lib/prisma.js';

export async function listPayments(req, res) {
  const rows = await prisma.paymentEntry.findMany({
    where: { shopId: req.user.shopId },
    include: { supplier: true, salesBill: true, purchaseBill: true },
    orderBy: { paymentDate: 'desc' }
  });
  res.json(rows);
}

export async function createPayment(req, res) {
  const shopId = req.user.shopId;
  const payload = {
    shopId,
    amount: Number(req.body.amount || 0),
    method: req.body.method || 'CASH',
    referenceNo: req.body.referenceNo,
    note: req.body.note,
    customerName: req.body.customerName,
    supplierId: req.body.supplierId,
    salesBillId: req.body.salesBillId,
    purchaseBillId: req.body.purchaseBillId,
    createdById: req.user.sub
  };

  const result = await prisma.$transaction(async (tx) => {
    const payment = await tx.paymentEntry.create({ data: payload });

    if (payload.salesBillId) {
      await tx.salesBill.update({
        where: { id: payload.salesBillId },
        data: {
          paidTotal: { increment: payload.amount },
          dueTotal: { decrement: payload.amount }
        }
      });

      if (payload.customerName) {
        await tx.customerLedgerEntry.create({
          data: {
            shopId,
            customerName: payload.customerName,
            credit: payload.amount,
            note: `Payment received via ${payload.method}`,
            salesBillId: payload.salesBillId
          }
        });
      }
    }

    if (!payload.salesBillId && payload.customerName) {
      await tx.customerLedgerEntry.create({
        data: {
          shopId,
          customerName: payload.customerName,
          credit: payload.amount,
          note: payload.note || `Payment received via ${payload.method}`
        }
      });
    }

    if (payload.purchaseBillId) {
      const supplier = payload.supplierId
        ? await tx.supplier.findUnique({ where: { id: payload.supplierId } })
        : null;
      await tx.supplierLedgerEntry.create({
        data: {
          shopId,
          supplierId: payload.supplierId,
          supplierName: supplier?.name || req.body.supplierName || 'Supplier',
          credit: payload.amount,
          note: `Payment made via ${payload.method}`,
          purchaseBillId: payload.purchaseBillId
        }
      });
    }

    return payment;
  });

  res.status(201).json(result);
}
