import { useEffect, useState } from 'react';
import { addPayment, getCustomerLedger } from '../services/api';

export default function LedgerModule() {
  const [customerName, setCustomerName] = useState('');
  const [ledger, setLedger] = useState(null);
  const [payAmount, setPayAmount] = useState(0);
  const [status, setStatus] = useState('');

  const load = async () => {
    const data = await getCustomerLedger(customerName);
    setLedger(data);
  };

  useEffect(() => {
    load().catch(() => setLedger(null));
  }, []);

  const savePayment = async () => {
    try {
      await addPayment({
        amount: Number(payAmount || 0),
        method: 'CASH',
        customerName,
        salesBillId: null,
        note: 'Ledger payment'
      });
      await load();
      setStatus('Payment saved');
      setPayAmount(0);
    } catch (error) {
      setStatus(error?.response?.data?.message || 'Payment failed');
    }
  };

  return (
    <section className="module-page ledger-page">
      <h3>Ledger</h3>
      <div className="row-3">
        <input value={customerName} onChange={(e) => setCustomerName(e.target.value)} placeholder="Customer" />
        <button type="button" onClick={load}>
          Load
        </button>
      </div>

      {ledger?.summary && (
        <div className="row-3">
          <div>Total Bill: {ledger.summary.totalBill.toFixed(2)}</div>
          <div>Paid Amount: {ledger.summary.paidAmount.toFixed(2)}</div>
          <div>Remaining Due: {ledger.summary.remainingDue.toFixed(2)}</div>
        </div>
      )}

      <table className="table">
        <thead>
          <tr>
            <th>Date</th>
            <th>Bill</th>
            <th>Amount</th>
            <th>Type</th>
          </tr>
        </thead>
        <tbody>
          {ledger?.transactions?.map((tx, index) => (
            <tr key={`${tx.date}-${index}`}>
              <td>{String(tx.date).slice(0, 10)}</td>
              <td>{tx.billNo || '-'}</td>
              <td>{Number(tx.amount || 0).toFixed(2)}</td>
              <td>{tx.type}</td>
            </tr>
          ))}
        </tbody>
      </table>

      <div className="row-3">
        <input value={payAmount} onChange={(e) => setPayAmount(Number(e.target.value || 0))} placeholder="Amount" />
        <button type="button" onClick={savePayment}>
          Save Payment
        </button>
      </div>
      {status && <p className="small-row">{status}</p>}
    </section>
  );
}
