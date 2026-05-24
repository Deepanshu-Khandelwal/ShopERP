import { prisma } from '../lib/prisma.js';

function calcLine(item) {
  const qty = Number(item.quantity || 0);
  const rate = Number(item.unitRate || 0);
  const discount = Number(item.discount || 0);
  const gst = Number(item.gstPercent || 0);
  const base = qty * rate - discount;
  const tax = (base * gst) / 100;
  return { qty, rate, discount, gst, total: base + tax };
}

export async function listPurchaseOrders(req, res) {
  const rows = await prisma.purchaseOrder.findMany({
    where: { shopId: req.user.shopId },
    include: { items: true, supplier: true },
    orderBy: { orderDate: 'desc' }
  });
  res.json(rows);
}

export async function createPurchaseOrder(req, res) {
  const shopId = req.user.shopId;
  const { orderNo, supplierId, notes, items = [] } = req.body;
  const computed = items.map(calcLine);

  const subtotal = computed.reduce((sum, r) => sum + (r.qty * r.rate - r.discount), 0);
  const taxTotal = computed.reduce((sum, r) => sum + ((r.qty * r.rate - r.discount) * r.gst) / 100, 0);
  const grandTotal = subtotal + taxTotal;

  const row = await prisma.purchaseOrder.create({
    data: {
      shopId,
      orderNo,
      supplierId,
      notes,
      subtotal,
      taxTotal,
      grandTotal,
      createdById: req.user.sub,
      items: {
        create: items.map((item, i) => ({
          productId: item.productId,
          quantity: computed[i].qty,
          unitRate: computed[i].rate,
          discount: computed[i].discount,
          gstPercent: computed[i].gst,
          lineTotal: computed[i].total
        }))
      }
    },
    include: { items: true }
  });

  res.status(201).json(row);
}

export async function sendPurchaseOrder(req, res) {
  const { purchaseOrderId } = req.body;
  const row = await prisma.purchaseOrder.update({
    where: { id: purchaseOrderId },
    data: { status: 'SENT' }
  });
  res.json(row);
}

export async function convertPurchaseOrder(req, res) {
  const { purchaseOrderId, billNo } = req.body;

  const result = await prisma.$transaction(async (tx) => {
    const order = await tx.purchaseOrder.findUnique({
      where: { id: purchaseOrderId },
      include: { items: true }
    });

    if (!order) {
      throw new Error('Purchase order not found');
    }

    const bill = await tx.purchaseBill.create({
      data: {
        shopId: order.shopId,
        billNo,
        supplierId: order.supplierId,
        subtotal: order.subtotal,
        taxTotal: order.taxTotal,
        grandTotal: order.grandTotal,
        items: {
          create: order.items.map((item) => ({
            productId: item.productId,
            quantity: item.quantity,
            unitRate: item.unitRate,
            discount: item.discount,
            gstPercent: item.gstPercent,
            lineTotal: item.lineTotal
          }))
        }
      },
      include: { items: true }
    });

    for (const item of order.items) {
      await tx.product.update({
        where: { id: item.productId },
        data: { stockQty: { increment: item.quantity } }
      });
    }

    await tx.purchaseOrder.update({
      where: { id: purchaseOrderId },
      data: { status: 'CONVERTED' }
    });

    return bill;
  });

  res.status(201).json(result);
}
