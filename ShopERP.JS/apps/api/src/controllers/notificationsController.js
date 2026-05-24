import { prisma } from '../lib/prisma.js';

export async function listNotifications(req, res) {
  const rows = await prisma.notificationEntry.findMany({
    where: { shopId: req.user.shopId },
    orderBy: { createdAt: 'desc' },
    take: 100
  });
  res.json(rows);
}

export async function checkNotifications(req, res) {
  const shopId = req.user.shopId;
  const lowStockThreshold = Number(req.body.lowStockThreshold || 10);
  const expiryDays = Number(req.body.expiryDays || 30);

  const [lowStockRows, expiryRows] = await Promise.all([
    prisma.product.findMany({
      where: { shopId, stockQty: { lte: lowStockThreshold }, isActive: true },
      select: { id: true, name: true, stockQty: true }
    }),
    prisma.productBatch.findMany({
      where: {
        shopId,
        quantity: { gt: 0 },
        expiryDate: {
          lte: new Date(Date.now() + 1000 * 60 * 60 * 24 * expiryDays),
          gte: new Date()
        }
      },
      include: { product: { select: { name: true } } }
    })
  ]);

  const todayToken = new Date().toISOString().slice(0, 10);
  const payload = [];

  for (const row of lowStockRows) {
    payload.push({
      shopId,
      title: 'Low Stock Alert',
      message: `[${todayToken}] ${row.name} is low: ${row.stockQty}`
    });
  }

  for (const row of expiryRows) {
    payload.push({
      shopId,
      title: 'Expiry Alert',
      message: `[${todayToken}] ${row.product.name} batch ${row.batchNo} expires on ${row.expiryDate?.toISOString().slice(0, 10)}`
    });
  }

  let created = 0;
  for (const entry of payload) {
    const existing = await prisma.notificationEntry.findFirst({
      where: {
        shopId,
        title: entry.title,
        message: entry.message
      }
    });

    if (!existing) {
      await prisma.notificationEntry.create({ data: entry });
      created += 1;
    }
  }

  res.json({ created, lowStockCount: lowStockRows.length, expiryCount: expiryRows.length });
}
