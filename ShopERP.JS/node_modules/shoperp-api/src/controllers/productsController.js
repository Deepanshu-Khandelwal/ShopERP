import { prisma } from '../lib/prisma.js';

function requireShop(req, res) {
  const shopId = req.user?.shopId;
  if (!shopId) {
    res.status(400).json({ message: 'shopId missing in token' });
    return null;
  }
  return shopId;
}

export async function listProducts(req, res) {
  const shopId = requireShop(req, res);
  if (!shopId) return;

  const rows = await prisma.product.findMany({
    where: { shopId },
    orderBy: { updatedAt: 'desc' }
  });
  res.json(rows);
}

export async function createProduct(req, res) {
  const shopId = requireShop(req, res);
  if (!shopId) return;

  const body = req.body;
  const row = await prisma.product.create({
    data: {
      shopId,
      name: body.name,
      sku: body.sku,
      hsn: body.hsn,
      unit: body.unit,
      purchaseRate: Number(body.purchaseRate || 0),
      saleRate: Number(body.saleRate || 0),
      gstPercent: Number(body.gstPercent || 0),
      stockQty: 0,
      expiryDate: body.expiryDate ? new Date(body.expiryDate) : null
    }
  });

  res.status(201).json(row);
}

export async function updateProductStock(req, res) {
  const shopId = requireShop(req, res);
  if (!shopId) return;

  const productId = String(req.params.id || '');
  const nextStockQty = Number(req.body.stockQty);

  if (!productId) {
    return res.status(400).json({ message: 'product id is required' });
  }

  if (!Number.isFinite(nextStockQty) || nextStockQty < 0) {
    return res.status(400).json({ message: 'stockQty must be a number greater than or equal to 0' });
  }

  const existing = await prisma.product.findFirst({
    where: { id: productId, shopId },
    select: { id: true }
  });

  if (!existing) {
    return res.status(404).json({ message: 'Product not found' });
  }

  const updated = await prisma.product.update({
    where: { id: productId },
    data: { stockQty: nextStockQty }
  });

  return res.json(updated);
}

export async function stockSummary(req, res) {
  const shopId = requireShop(req, res);
  if (!shopId) return;

  const [totalProducts, lowStock, expiringSoon] = await Promise.all([
    prisma.product.count({ where: { shopId } }),
    prisma.product.count({ where: { shopId, stockQty: { lte: 10 } } }),
    prisma.product.count({
      where: {
        shopId,
        expiryDate: {
          lte: new Date(Date.now() + 1000 * 60 * 60 * 24 * 30),
          gte: new Date()
        }
      }
    })
  ]);

  res.json({ totalProducts, lowStock, expiringSoon });
}

export async function lowStockProducts(req, res) {
  const shopId = requireShop(req, res);
  if (!shopId) return;

  const rows = await prisma.product.findMany({
    where: { shopId, stockQty: { lte: 10 } },
    orderBy: { stockQty: 'asc' }
  });
  res.json(rows);
}

export async function expiringProducts(req, res) {
  const shopId = requireShop(req, res);
  if (!shopId) return;

  const rows = await prisma.product.findMany({
    where: {
      shopId,
      expiryDate: {
        lte: new Date(Date.now() + 1000 * 60 * 60 * 24 * 30),
        gte: new Date()
      }
    },
    orderBy: { expiryDate: 'asc' }
  });
  res.json(rows);
}

export async function searchProducts(req, res) {
  const shopId = requireShop(req, res);
  if (!shopId) return;

  const query = String(req.query.q || '').trim();
  if (!query) {
    return res.json([]);
  }

  const products = await prisma.product.findMany({
    where: {
      shopId,
      isActive: true,
      OR: [{ name: { contains: query } }, { sku: { contains: query } }]
    },
    take: 20,
    orderBy: { name: 'asc' },
    include: {
      batches: {
        where: {
          quantity: { gt: 0 },
          OR: [{ expiryDate: null }, { expiryDate: { gte: new Date() } }]
        },
        orderBy: [{ expiryDate: 'asc' }, { createdAt: 'asc' }]
      }
    }
  });

  const rows = products.map((product) => ({
    ...product,
    availableQty: product.batches.reduce((sum, b) => sum + Number(b.quantity || 0), 0)
  }));

  res.json(rows);
}
