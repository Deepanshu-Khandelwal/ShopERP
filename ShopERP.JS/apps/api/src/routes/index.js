import { Router } from 'express';
import { prisma } from '../lib/prisma.js';
import { auth } from '../middlewares/auth.js';
import { createShopAction, createUserAction, loginAction } from '../controllers/authController.js';
import {
  createProduct,
  expiringProducts,
  listProducts,
  lowStockProducts,
  searchProducts,
  stockSummary,
  updateProductStock
} from '../controllers/productsController.js';
import {
  createSupplier,
  deleteSupplier,
  listSuppliers,
  updateSupplier
} from '../controllers/suppliersController.js';
import { createSale, getSaleById, listSales, updateSale } from '../controllers/salesController.js';
import { createPurchase, listPurchases } from '../controllers/purchasesController.js';
import { createPurchaseReturn, createSalesReturn } from '../controllers/returnsController.js';
import { createPayment, listPayments } from '../controllers/paymentsController.js';
import { customerLedger, supplierLedger } from '../controllers/ledgersController.js';
import {
  dashboardReport,
  expiredStockReport,
  gstReport,
  listDoctors,
  profitReport
} from '../controllers/reportsController.js';
import {
  convertPurchaseOrder,
  createPurchaseOrder,
  listPurchaseOrders,
  sendPurchaseOrder
} from '../controllers/purchaseOrdersController.js';
import { getProfile, updateProfile } from '../controllers/profileController.js';
import { backupHistory, runBackup } from '../controllers/backupController.js';
import { syncMySql } from '../controllers/syncController.js';
import { checkNotifications, listNotifications } from '../controllers/notificationsController.js';

export const apiRouter = Router();

apiRouter.use((req, _res, next) => {
  req.prisma = prisma;
  next();
});

apiRouter.post('/auth/login', loginAction);
apiRouter.post('/auth/shop/create', createShopAction);
apiRouter.post('/auth/user/create', auth(), createUserAction);

apiRouter.get('/products', auth(), listProducts);
apiRouter.post('/products/direct-entry', auth(), createProduct);
apiRouter.patch('/products/:id/stock', auth(), updateProductStock);
apiRouter.get('/products/stock', auth(), stockSummary);
apiRouter.get('/products/expiry', auth(), expiringProducts);
apiRouter.get('/products/low-stock', auth(), lowStockProducts);
apiRouter.get('/products/search', auth(), searchProducts);

apiRouter.get('/suppliers', auth(), listSuppliers);
apiRouter.post('/suppliers', auth(), createSupplier);
apiRouter.put('/suppliers/:id', auth(), updateSupplier);
apiRouter.delete('/suppliers/:id', auth(), deleteSupplier);

apiRouter.get('/sales', auth(), listSales);
apiRouter.get('/sales/:id', auth(), getSaleById);
apiRouter.post('/sales', auth(), createSale);
apiRouter.put('/sales/:id', auth(), updateSale);
apiRouter.get('/sales/customer-history', auth(), listSales);
apiRouter.get('/sales/doctor-wise', auth(), listSales);
apiRouter.post('/sales/repeat-from-bill', auth(), createSale);

apiRouter.get('/purchases', auth(), listPurchases);
apiRouter.post('/purchases', auth(), createPurchase);

apiRouter.post('/returns/purchase', auth(), createPurchaseReturn);
apiRouter.post('/returns/sales', auth(), createSalesReturn);

apiRouter.get('/payments', auth(), listPayments);
apiRouter.post('/payments', auth(), createPayment);

apiRouter.get('/reports/dashboard', auth(), dashboardReport);
apiRouter.get('/reports/doctors', auth(), listDoctors);
apiRouter.get('/reports/gst', auth(), gstReport);
apiRouter.get('/reports/profit', auth(), profitReport);
apiRouter.get('/reports/expired-stock', auth(), expiredStockReport);

apiRouter.get('/ledgers/customer', auth(), customerLedger);
apiRouter.get('/ledgers/supplier', auth(), supplierLedger);

apiRouter.get('/purchase-orders', auth(), listPurchaseOrders);
apiRouter.post('/purchase-orders', auth(), createPurchaseOrder);
apiRouter.post('/purchase-orders/send', auth(), sendPurchaseOrder);
apiRouter.post('/purchase-orders/convert', auth(), convertPurchaseOrder);

apiRouter.get('/profile', auth(), getProfile);
apiRouter.put('/profile', auth(), updateProfile);

apiRouter.get('/notifications', auth(), listNotifications);
apiRouter.post('/notifications/check', auth(), checkNotifications);

apiRouter.post('/backup/run', auth(), runBackup);
apiRouter.get('/backup/history', auth(), backupHistory);

apiRouter.post('/sync/mysql', auth(), syncMySql);
