import { prisma } from '../lib/prisma.js';

export async function createSalesReturn(req, res) {
  const shopId = req.user.shopId;
  const { salesBillId, amount, reason, items = [] } = req.body;

  try {
    const result = await prisma.$transaction(async (tx) => {
    const row = await tx.salesReturn.create({
      data: {
        shopId,
        salesBillId,
        amount: Number(amount || 0),
        reason,
        createdById: req.user.sub
      }
    });

    for (const item of items) {
      if (item.batchId) {
        await tx.productBatch.update({
          where: { id: item.batchId },
          data: { quantity: { increment: Number(item.quantity || 0) } }
        });
      }

      await tx.product.update({
        where: { id: item.productId },
        data: { stockQty: { increment: Number(item.quantity || 0) } }
      });
    }

    await tx.salesBill.update({
      where: { id: salesBillId },
      data: { dueTotal: { decrement: Number(amount || 0) } }
    });

    return row;
    });

    return res.status(201).json(result);
  } catch (error) {
    return res.status(400).json({ message: error.message || 'Failed to save sales return' });
  }
}

export async function createPurchaseReturn(req, res) {
  const shopId = req.user.shopId;
  const { purchaseBillId, amount, reason, items = [] } = req.body;

  try {
    const result = await prisma.$transaction(async (tx) => {
    const row = await tx.purchaseReturn.create({
      data: {
        shopId,
        purchaseBillId,
        amount: Number(amount || 0),
        reason,
        createdById: req.user.sub
      }
    });

    for (const item of items) {
      if (item.batchId) {
        const batch = await tx.productBatch.findUnique({ where: { id: item.batchId } });
        if (!batch || Number(batch.quantity || 0) < Number(item.quantity || 0)) {
          throw new Error('Insufficient stock for purchase return batch');
        }

        await tx.productBatch.update({
          where: { id: item.batchId },
          data: { quantity: { decrement: Number(item.quantity || 0) } }
        });
      }

      await tx.product.update({
        where: { id: item.productId },
        data: { stockQty: { decrement: Number(item.quantity || 0) } }
      });
    }

    return row;
    });

    return res.status(201).json(result);
  } catch (error) {
    return res.status(400).json({ message: error.message || 'Failed to save purchase return' });
  }
}
