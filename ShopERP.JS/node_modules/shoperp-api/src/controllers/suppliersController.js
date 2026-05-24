import { prisma } from '../lib/prisma.js';

function shopIdFrom(req, res) {
  const shopId = req.user?.shopId;
  if (!shopId) {
    res.status(400).json({ message: 'shopId missing in token' });
    return null;
  }
  return shopId;
}

export async function listSuppliers(req, res) {
  const shopId = shopIdFrom(req, res);
  if (!shopId) return;

  const rows = await prisma.supplier.findMany({
    where: { shopId },
    orderBy: { updatedAt: 'desc' }
  });
  res.json(rows);
}

export async function createSupplier(req, res) {
  const shopId = shopIdFrom(req, res);
  if (!shopId) return;

  const row = await prisma.supplier.create({
    data: {
      shopId,
      name: req.body.name,
      phone: req.body.phone,
      email: req.body.email,
      address: req.body.address,
      gstin: req.body.gstin,
      drugLicense: req.body.drugLicense
    }
  });
  res.status(201).json(row);
}

export async function updateSupplier(req, res) {
  const shopId = shopIdFrom(req, res);
  if (!shopId) return;

  const existing = await prisma.supplier.findFirst({
    where: { id: req.params.id, shopId }
  });

  if (!existing) {
    res.status(404).json({ message: 'Supplier not found' });
    return;
  }

  const row = await prisma.supplier.update({
    where: { id: req.params.id },
    data: {
      name: req.body.name,
      phone: req.body.phone,
      email: req.body.email,
      address: req.body.address,
      gstin: req.body.gstin,
      drugLicense: req.body.drugLicense
    }
  });

  res.json({ ...row, shopId });
}

export async function deleteSupplier(req, res) {
  const shopId = shopIdFrom(req, res);
  if (!shopId) return;

  const existing = await prisma.supplier.findFirst({
    where: { id: req.params.id, shopId }
  });

  if (!existing) {
    res.status(404).json({ message: 'Supplier not found' });
    return;
  }

  await prisma.supplier.delete({ where: { id: req.params.id } });
  res.status(204).send();
}
