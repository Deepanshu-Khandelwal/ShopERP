import { prisma } from '../lib/prisma.js';

export async function customerLedger(req, res) {
  const shopId = req.user.shopId;
  const customerName = req.query.customerName;

  const rows = await prisma.customerLedgerEntry.findMany({
    where: {
      shopId,
      ...(customerName ? { customerName: { equals: String(customerName) } } : {})
    },
    orderBy: { entryDate: 'asc' }
  });

  const totals = rows.reduce(
    (acc, row) => {
      acc.debit += row.debit;
      acc.credit += row.credit;
      return acc;
    },
    { debit: 0, credit: 0 }
  );

  const [bills, payments] = await Promise.all([
    prisma.salesBill.findMany({
      where: {
        shopId,
        ...(customerName ? { customerName: { equals: String(customerName) } } : {})
      },
      select: {
        id: true,
        invoiceNo: true,
        billDate: true,
        grandTotal: true
      },
      orderBy: { billDate: 'desc' }
    }),
    prisma.paymentEntry.findMany({
      where: {
        shopId,
        ...(customerName ? { customerName: { equals: String(customerName) } } : {}),
        salesBillId: { not: null }
      },
      select: {
        id: true,
        paymentDate: true,
        amount: true,
        method: true,
        salesBillId: true
      },
      orderBy: { paymentDate: 'desc' }
    })
  ]);

  const totalBill = bills.reduce((sum, b) => sum + Number(b.grandTotal || 0), 0);
  const paidAmount = payments.reduce((sum, p) => sum + Number(p.amount || 0), 0);
  const remainingDue = Math.max(0, totalBill - paidAmount);

  res.json({
    rows,
    balance: totals.debit - totals.credit,
    totals,
    summary: {
      totalBill,
      paidAmount,
      remainingDue
    },
    transactions: rows.map((row) => ({
      date: row.entryDate,
      billNo: row.note?.includes('invoice') ? row.note : '',
      amount: row.debit || row.credit,
      type: row.debit > 0 ? 'DEBIT' : 'CREDIT',
      note: row.note
    }))
  });
}

export async function supplierLedger(req, res) {
  const shopId = req.user.shopId;
  const supplierName = req.query.supplierName;

  const rows = await prisma.supplierLedgerEntry.findMany({
    where: {
      shopId,
      ...(supplierName ? { supplierName: { equals: String(supplierName) } } : {})
    },
    orderBy: { entryDate: 'asc' }
  });

  const totals = rows.reduce(
    (acc, row) => {
      acc.debit += row.debit;
      acc.credit += row.credit;
      return acc;
    },
    { debit: 0, credit: 0 }
  );

  res.json({ rows, balance: totals.debit - totals.credit, totals });
}
