import { useEffect, useState } from 'react';
import { getExpiring, getLowStock, listProducts, updateProductStock } from '../services/api';

export default function StockModule() {
  const [lowStock, setLowStock] = useState([]);
  const [expiring, setExpiring] = useState([]);
  const [products, setProducts] = useState([]);
  const [stockDrafts, setStockDrafts] = useState({});
  const [status, setStatus] = useState('');

  const loadStockData = async () => {
    try {
      const [low, exp, all] = await Promise.all([getLowStock(), getExpiring(), listProducts()]);
      setLowStock(low);
      setExpiring(exp);
      setProducts(all);
      setStockDrafts(Object.fromEntries(all.map((row) => [row.id, Number(row.stockQty || 0)])));
    } catch {
      setLowStock([]);
      setExpiring([]);
      setProducts([]);
      setStockDrafts({});
    }
  };

  useEffect(() => {
    loadStockData();
  }, []);

  const saveStock = async (productId) => {
    const nextQty = Number(stockDrafts[productId]);
    if (!Number.isFinite(nextQty) || nextQty < 0) {
      setStatus('Stock must be a number greater than or equal to 0');
      return;
    }

    try {
      await updateProductStock(productId, nextQty);
      setStatus('Stock updated');
      await loadStockData();
    } catch (error) {
      setStatus(error?.response?.data?.message || 'Failed to update stock');
    }
  };

  return (
    <section className="module-page stock-page">
      <h3>Stock Control</h3>
      <p className="small-row">Manual stock correction is enabled for adjustments.</p>

      <div className="table-card">
        <div className="table-card-head">
          <h4>Update Product Stock</h4>
          <p>{products.length} products</p>
        </div>
        <table className="table">
          <thead>
            <tr>
              <th>Product</th>
              <th>Current Qty</th>
              <th>New Qty</th>
              <th>Action</th>
            </tr>
          </thead>
          <tbody>
            {products.map((row) => (
              <tr key={row.id}>
                <td>{row.name}</td>
                <td>{Number(row.stockQty || 0)}</td>
                <td>
                  <input
                    type="number"
                    min="0"
                    step="1"
                    value={stockDrafts[row.id] ?? 0}
                    onChange={(e) =>
                      setStockDrafts((prev) => ({
                        ...prev,
                        [row.id]: e.target.value
                      }))
                    }
                  />
                </td>
                <td>
                  <button type="button" className="action-btn" onClick={() => saveStock(row.id)}>
                    Update Stock
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      <div className="row-2">
        <article>
          <h4>Low Stock</h4>
          {lowStock.map((row) => (
            <p key={row.id} className="small-row">
              {row.name}: {row.stockQty}
            </p>
          ))}
        </article>
        <article>
          <h4>Expiry within 30 days</h4>
          {expiring.map((row) => (
            <p key={row.id} className="small-row">
              {row.name}: {row.expiryDate?.slice(0, 10)}
            </p>
          ))}
        </article>
      </div>
      {status && <p className="small-row">{status}</p>}
    </section>
  );
}
