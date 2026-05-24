import { Suspense, lazy } from 'react';

const BackupModule = lazy(() => import('../modules/BackupModule'));
const BillingModule = lazy(() => import('../modules/BillingModule'));
const DoctorsModule = lazy(() => import('../modules/DoctorsModule'));
const LedgerModule = lazy(() => import('../modules/LedgerModule'));
const PurchaseModule = lazy(() => import('../modules/PurchaseModule'));
const ReturnsModule = lazy(() => import('../modules/ReturnsModule'));
const ReportsModule = lazy(() => import('../modules/ReportsModule'));
const StockModule = lazy(() => import('../modules/StockModule'));
const SuppliersModule = lazy(() => import('../modules/SuppliersModule'));

const moduleComponentMap = {
  'Billing (F1)': BillingModule,
  'Purchase (F2)': PurchaseModule,
  Doctors: DoctorsModule,
  Suppliers: SuppliersModule,
  'Suppliers (F6)': SuppliersModule,
  Returns: ReturnsModule,
  'Returns (F7)': ReturnsModule,
  'Stock (F3)': StockModule,
  'Ledger (F4)': LedgerModule,
  'Reports (F5)': ReportsModule,
  Backup: BackupModule,
  'Backup (F8)': BackupModule
};

export default function ModulePage({ moduleName }) {
  const CurrentModule = moduleComponentMap[moduleName];
  if (CurrentModule) {
    return (
      <Suspense fallback={<div className="module-loading">Loading module...</div>}>
        <CurrentModule />
      </Suspense>
    );
  }

  return (
    <section className="module-page">
      <h2>{moduleName}</h2>
      <p>Open one of the quick modules from sidebar.</p>
    </section>
  );
}
