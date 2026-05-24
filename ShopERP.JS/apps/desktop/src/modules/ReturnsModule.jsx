import { useEffect, useMemo, useState } from 'react';
import { createPurchaseReturn, createSalesReturn, listPurchases, listSales } from '../services/api';

export default function ReturnsModule() {
  const [tab, setTab] = useState('sales');
  const [salesRows, setSalesRows] = useState([]);
  const [purchaseRows, setPurchaseRows] = useState([]);
  const [selectedSalesBillId, setSelectedSalesBillId] = useState('');
  const [selectedPurchaseBillId, setSelectedPurchaseBillId] = useState('');
  const [salesReason, setSalesReason] = useState('');
  const [purchaseReason, setPurchaseReason] = useState('');
  const [salesQtyByItem, setSalesQtyByItem] = useState({});
  const [purchaseQtyByItem, setPurchaseQtyByItem] = useState({});
  const [status, setStatus] = useState('');
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const load = async () => {
      setLoading(true);
      try {
        const [sales, purchases] = await Promise.all([listSales(), listPurchases()]);
        setSalesRows(sales);
        setPurchaseRows(purchases);
        if (sales.length && !selectedSalesBillId) setSelectedSalesBillId(sales[0].id);
        if (purchases.length && !selectedPurchaseBillId) setSelectedPurchaseBillId(purchases[0].id);
      } catch (error) {
        setStatus(error?.response?.data?.message || 'Failed to load return data');
      } finally {
        setLoading(false);
      }
    };

    load();
  }, []);

  const selectedSalesBill = useMemo(
    () => salesRows.find((row) => row.id === selectedSalesBillId),
    [salesRows, selectedSalesBillId]
  );

  const selectedPurchaseBill = useMemo(
    () => purchaseRows.find((row) => row.id === selectedPurchaseBillId),
    [purchaseRows, selectedPurchaseBillId]
  );

  const buildReturnItems = (bill, qtyByItem) => {
    if (!bill?.items?.length) return [];

    return bill.items
      .map((item) => ({
        productId: item.productId,
        batchId: item.batchId,
        quantity: Number(qtyByItem[item.id] || 0)
      }))
      .filter((item) => item.quantity > 0);
  };

  const submitSalesReturn = async () => {
    if (!selectedSalesBill) {
      setStatus('Select a sales bill first');
      return;
    }

    const items = buildReturnItems(selectedSalesBill, salesQtyByItem);
    if (!items.length) {
      setStatus('Enter at least one return quantity');
      return;
    }

    const amount = items.reduce((sum, item) => {
      const source = selectedSalesBill.items.find((row) => row.productId === item.productId && row.batchId === item.batchId);
      const unit = Number(source?.unitRate || 0);
      return sum + item.quantity * unit;
    }, 0);

    try {
      await createSalesReturn({
        salesBillId: selectedSalesBill.id,
        amount,
        reason: salesReason || 'Sales return',
        items
      });
      setSalesQtyByItem({});
      setSalesReason('');
      setStatus(`Sales return posted. Credit note amount: ${amount.toFixed(2)}`);
    } catch (error) {
      setStatus(error?.response?.data?.message || 'Failed to save sales return');
    }
  };

  const submitPurchaseReturn = async () => {
    if (!selectedPurchaseBill) {
      setStatus('Select a purchase bill first');
      return;
    }

    const items = buildReturnItems(selectedPurchaseBill, purchaseQtyByItem);
    if (!items.length) {
      setStatus('Enter at least one return quantity');
      return;
    }

    const amount = items.reduce((sum, item) => {
      const source = selectedPurchaseBill.items.find((row) => row.productId === item.productId && row.batchId === item.batchId);
      const unit = Number(source?.unitRate || 0);
      return sum + item.quantity * unit;
    }, 0);

    try {
      await createPurchaseReturn({
        purchaseBillId: selectedPurchaseBill.id,
        amount,
        reason: purchaseReason || 'Purchase return',
        items
      });
      setPurchaseQtyByItem({});
      setPurchaseReason('');
      setStatus(`Purchase return posted. Credit note amount: ${amount.toFixed(2)}`);
    } catch (error) {
      setStatus(error?.response?.data?.message || 'Failed to save purchase return');
    }
  };

  const renderItems = (bill, qtyByItem, setQtyByItem) => {
    if (!bill?.items?.length) return <p>No items found for selected bill.</p>;

    return (
      <table className="table">
        <thead>
          <tr>
            <th>Item</th>
            <th>Batch</th>
            <th>Sold/Purchased Qty</th>
            <th>Return Qty</th>
          </tr>
        </thead>
        <tbody>
          {bill.items.map((item) => (
            <tr key={item.id}>
              <td>{item?.product?.name || item.productId}</td>
              <td>{item?.batch?.batchNo || '-'}</td>
              <td>{Number(item.quantity || 0)}</td>
              <td>
                <input
                  value={qtyByItem[item.id] || ''}
                  onChange={(e) =>
                    setQtyByItem((prev) => ({
                      ...prev,
                      [item.id]: Number(e.target.value || 0)
                    }))
                  }
                />
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    );
  };

  return (
    <section className="module-page returns-page">
      <h3>Returns & Credit Notes</h3>
      <p className="small-row">Manage sales returns and purchase returns with automatic stock and ledger impact.</p>

      <div className="tab-switch">
        <button type="button" className={tab === 'sales' ? 'active-tab' : ''} onClick={() => setTab('sales')}>
          Sales Return
        </button>
        <button type="button" className={tab === 'purchase' ? 'active-tab' : ''} onClick={() => setTab('purchase')}>
          Purchase Return
        </button>
      </div>

      {loading && <p>Loading return data...</p>}

      {!loading && tab === 'sales' && (
        <section>
          <div className="row-2">
            <select value={selectedSalesBillId} onChange={(e) => setSelectedSalesBillId(e.target.value)}>
              {salesRows.map((row) => (
                <option key={row.id} value={row.id}>
                  {row.invoiceNo} | {row.customerName || ''} | {Number(row.grandTotal || 0).toFixed(2)}
                </option>
              ))}
            </select>
            <input
              value={salesReason}
              onChange={(e) => setSalesReason(e.target.value)}
              placeholder="Reason for sales return"
            />
          </div>
          {renderItems(selectedSalesBill, salesQtyByItem, setSalesQtyByItem)}
          <button type="button" onClick={submitSalesReturn}>
            Save Sales Return (Generate Credit Note)
          </button>
        </section>
      )}

      {!loading && tab === 'purchase' && (
        <section>
          <div className="row-2">
            <select value={selectedPurchaseBillId} onChange={(e) => setSelectedPurchaseBillId(e.target.value)}>
              {purchaseRows.map((row) => (
                <option key={row.id} value={row.id}>
                  {row.billNo} | {row.supplier?.name || 'Supplier'} | {Number(row.grandTotal || 0).toFixed(2)}
                </option>
              ))}
            </select>
            <input
              value={purchaseReason}
              onChange={(e) => setPurchaseReason(e.target.value)}
              placeholder="Reason for purchase return"
            />
          </div>
          {renderItems(selectedPurchaseBill, purchaseQtyByItem, setPurchaseQtyByItem)}
          <button type="button" onClick={submitPurchaseReturn}>
            Save Purchase Return (Generate Credit Note)
          </button>
        </section>
      )}

      {status && <p className="small-row">{status}</p>}
    </section>
  );
}
