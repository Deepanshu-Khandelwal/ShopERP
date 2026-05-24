import { prisma } from '../lib/prisma.js';

function startEndDates(from, to) {
  const start = from ? new Date(String(from)) : new Date(Date.now() - 1000 * 60 * 60 * 24 * 30);
  const end = to ? new Date(String(to)) : new Date();
  return { start, end };
}

export async function dashboardReport(req, res) {
  const shopId = req.user.shopId;
  const todayStart = new Date();
  todayStart.setHours(0, 0, 0, 0);
  const todayEnd = new Date();
  todayEnd.setHours(23, 59, 59, 999);

  const [todaySales, todayItems, lowStockCount, expiryAlertsCount, topRaw] = await Promise.all([
    prisma.salesBill.aggregate({
      where: { shopId, billDate: { gte: todayStart, lte: todayEnd } },
      _sum: { grandTotal: true }
    }),
    prisma.salesItem.findMany({
      where: {
        bill: {
          shopId,
          billDate: { gte: todayStart, lte: todayEnd }
        }
      },
      select: {
        quantity: true,
        lineTotal: true,
        costRate: true
      }
    }),
    prisma.product.count({ where: { shopId, stockQty: { lte: 10 }, isActive: true } }),
    prisma.productBatch.count({
      where: {
        shopId,
        quantity: { gt: 0 },
        expiryDate: {
          lte: new Date(Date.now() + 1000 * 60 * 60 * 24 * 30),
          gte: new Date()
        }
      }
    }),
    prisma.salesItem.groupBy({
      by: ['productId'],
      where: {
        bill: {
          shopId,
          billDate: { gte: todayStart, lte: todayEnd }
        }
      },
      _sum: { quantity: true },
      orderBy: { _sum: { quantity: 'desc' } },
      take: 5
    })
  ]);

  const topIds = topRaw.map((row) => row.productId);
  const topProducts = topIds.length
    ? await prisma.product.findMany({
        where: { id: { in: topIds }, shopId },
        select: { id: true, name: true }
      })
    : [];
  const nameMap = new Map(topProducts.map((p) => [p.id, p.name]));

  const topSellingProducts = topRaw.map((row) => ({
    productId: row.productId,
    name: nameMap.get(row.productId) || row.productId,
    qty: Number(row._sum.quantity || 0)
  }));

  const salesTotal = Number(todaySales._sum.grandTotal || 0);
  const costTotal = todayItems.reduce((sum, item) => sum + Number(item.quantity || 0) * Number(item.costRate || 0), 0);
  const profitToday = todayItems.reduce((sum, item) => sum + Number(item.lineTotal || 0), 0) - costTotal;

  res.json({
    todaySales: salesTotal,
    profitToday,
    lowStockCount,
    expiryAlertsCount,
    topSellingProducts
  });
}

export async function listDoctors(req, res) {
  const shopId = req.user.shopId;

  const doctors = await prisma.doctor.findMany({
    where: { shopId },
    orderBy: [{ visitCount: 'desc' }, { name: 'asc' }],
    select: {
      name: true,
      visitCount: true,
      lastVisitAt: true
    }
  });

  res.json(doctors);
}

export async function gstReport(req, res) {
  const shopId = req.user.shopId;
  const { start, end } = startEndDates(req.query.from, req.query.to);

  const [salesRows, purchaseRows] = await Promise.all([
    prisma.salesBill.findMany({
      where: { shopId, billDate: { gte: start, lte: end } },
      select: { taxTotal: true }
    }),
    prisma.purchaseBill.findMany({
      where: { shopId, billDate: { gte: start, lte: end } },
      select: { taxTotal: true }
    })
  ]);

  const outputGst = salesRows.reduce((sum, r) => sum + r.taxTotal, 0);
  const inputGst = purchaseRows.reduce((sum, r) => sum + r.taxTotal, 0);
  res.json({ outputGst, inputGst, netPayable: outputGst - inputGst });
}

export async function profitReport(req, res) {
  const shopId = req.user.shopId;
  const { start, end } = startEndDates(req.query.from, req.query.to);

  const [sales, items] = await Promise.all([
    prisma.salesBill.aggregate({
      where: { shopId, billDate: { gte: start, lte: end } },
      _sum: { grandTotal: true }
    }),
    prisma.salesItem.findMany({
      where: {
        bill: {
          shopId,
          billDate: { gte: start, lte: end }
        }
      },
      select: {
        quantity: true,
        lineTotal: true,
        costRate: true
      }
    })
  ]);

  const revenue = sales._sum.grandTotal || 0;
  const cost = items.reduce((sum, item) => sum + Number(item.quantity || 0) * Number(item.costRate || 0), 0);
  const grossProfit = items.reduce((sum, item) => sum + Number(item.lineTotal || 0), 0) - cost;
  res.json({ revenue, cost, grossProfit });
}

export async function expiredStockReport(req, res) {
  const shopId = req.user.shopId;
  const rows = await prisma.product.findMany({
    where: { shopId, expiryDate: { lte: new Date() } },
    orderBy: { expiryDate: 'asc' }
  });
  res.json(rows);
}
