import { useState } from 'react';
import { getProfitReport } from '../services/api';

export default function ReportsModule() {
  const [from, setFrom] = useState('');
  const [to, setTo] = useState('');
  const [result, setResult] = useState(null);

  const fetchReport = async () => {
    const data = await getProfitReport(from || undefined, to || undefined);
    setResult(data);
  };

  return (
    <section className="module-page reports-page">
      <h3>Reports</h3>
      <div className="row-4">
        <input type="date" value={from} onChange={(e) => setFrom(e.target.value)} />
        <input type="date" value={to} onChange={(e) => setTo(e.target.value)} />
        <button type="button" onClick={fetchReport}>
          Fetch Data
        </button>
        <button type="button" onClick={() => window.print()}>
          Print
        </button>
      </div>
      {result && (
        <div className="row-3">
          <div>Total Sales: {Number(result.revenue || 0).toFixed(2)}</div>
          <div>Total Purchase Cost: {Number(result.cost || 0).toFixed(2)}</div>
          <div>Profit: {Number(result.grossProfit || 0).toFixed(2)}</div>
        </div>
      )}
    </section>
  );
}
