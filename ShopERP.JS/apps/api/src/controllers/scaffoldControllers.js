function notImplemented(moduleName) {
  return (_req, res) => {
    res.json({
      module: moduleName,
      status: 'scaffolded',
      message: 'Endpoint migrated to JavaScript surface and ready for feature-parity implementation.'
    });
  };
}

export const returnsAction = notImplemented('returns');
export const paymentsAction = notImplemented('payments');
export const reportsAction = notImplemented('reports');
export const ledgersAction = notImplemented('ledgers');
export const purchaseOrdersAction = notImplemented('purchase-orders');
export const profileAction = notImplemented('profile');
export const backupAction = notImplemented('backup');
export const syncAction = notImplemented('sync');

export async function notificationsList(req, res) {
  const rows = await req.prisma.notificationEntry.findMany({
    where: { shopId: req.user.shopId },
    orderBy: { createdAt: 'desc' },
    take: 100
  });
  res.json(rows);
}
