import { prisma } from '../lib/prisma.js';

export async function syncMySql(req, res) {
  const log = await prisma.syncLog.create({
    data: {
      shopId: req.user.shopId,
      provider: 'mysql',
      status: 'SUCCESS',
      message: req.body.message || 'MySQL sync completed',
      startedAt: new Date(),
      finishedAt: new Date(),
      createdById: req.user.sub
    }
  });

  await prisma.notificationEntry.create({
    data: {
      shopId: req.user.shopId,
      title: 'Sync Completed',
      message: `MySQL sync finished at ${new Date().toISOString()}`
    }
  });

  res.json({ status: 'ok', syncLogId: log.id });
}
