export default function PurchaseItemsTable({
  items,
  setItems,
  products,
  removingItemIds = [],
  onRemoveItem,
  onAddNextRow
}) {
  const productMap = new Map(products.map((row) => [row.id, row]));

  return (
    <table className="purchase-items-table">
      <thead>
        <tr>
          <th>SN</th>
          <th>Product Name</th>
          <th>Batch</th>
          <th>EXP</th>
          <th>Qty</th>
          <th>Pack</th>
          <th>Scheme</th>
          <th>MRP</th>
          <th>Rate</th>
          <th>Disc</th>
          <th>IGST</th>
          <th>CGST</th>
          <th>SGST</th>
          <th>Schedule</th>
          <th>Total</th>
          <th>Action</th>
        </tr>
      </thead>
      <tbody>
        {items.map((item, index) => {
          const discount = Number(item.discount || 0);
          const preTax = Number(item.quantity || 0) * Number(item.unitRate || 0);
          const base = Math.max(0, preTax - discount);
          const igst = (base * Number(item.gstPercent || 0)) / 100;
          const half = igst / 2;
          return (
            <tr
              key={item.id}
              className={removingItemIds.includes(item.id) ? 'table-row-animated row-removing' : 'table-row-animated'}
              onKeyDown={(e) => {
                if (e.key !== 'Enter' || e.shiftKey) return;
                const target = e.target;
                const tag = target?.tagName;
                if (tag !== 'INPUT' && tag !== 'SELECT') return;
                e.preventDefault();
                onAddNextRow?.(item);
              }}
            >
              <td>{index + 1}</td>
              <td>
                <select
                  value={item.productId}
                  onChange={(e) => {
                    const productId = e.target.value;
                    const selected = productMap.get(productId);

                    setItems((prev) =>
                      prev.map((row) =>
                        row.id === item.id
                          ? {
                              ...row,
                              productId,
                              unitRate: Number(selected?.purchaseRate || row.unitRate || 0),
                              gstPercent: Number(selected?.gstPercent || row.gstPercent || 0),
                              mrp: Number(selected?.saleRate || row.mrp || 0)
                            }
                          : row
                      )
                    )
                  }}
                >
                  <option value="">Select Product</option>
                  {products.map((p) => (
                    <option key={p.id} value={p.id}>
                      {p.name}
                    </option>
                  ))}
                </select>
              </td>
              <td>
                <input
                  value={item.batchNo}
                  onChange={(e) =>
                    setItems((prev) =>
                      prev.map((row) => (row.id === item.id ? { ...row, batchNo: e.target.value } : row))
                    )
                  }
                />
              </td>
              <td>
                <input
                  type="date"
                  value={item.expiryDate}
                  onChange={(e) =>
                    setItems((prev) =>
                      prev.map((row) => (row.id === item.id ? { ...row, expiryDate: e.target.value } : row))
                    )
                  }
                />
              </td>
              <td>
                <input
                  type="number"
                  min="0"
                  step="1"
                  value={item.quantity}
                  onChange={(e) =>
                    setItems((prev) =>
                      prev.map((row) =>
                        row.id === item.id ? { ...row, quantity: Number(e.target.value || 0) } : row
                      )
                    )
                  }
                />
              </td>
              <td>
                <input
                  value={item.pack}
                  onChange={(e) =>
                    setItems((prev) =>
                      prev.map((row) => (row.id === item.id ? { ...row, pack: e.target.value } : row))
                    )
                  }
                />
              </td>
              <td>
                <input
                  value={item.scheme}
                  onChange={(e) =>
                    setItems((prev) =>
                      prev.map((row) => (row.id === item.id ? { ...row, scheme: e.target.value } : row))
                    )
                  }
                />
              </td>
              <td>
                <input
                  type="number"
                  min="0"
                  step="0.01"
                  value={item.mrp}
                  onChange={(e) =>
                    setItems((prev) =>
                      prev.map((row) => (row.id === item.id ? { ...row, mrp: Number(e.target.value || 0) } : row))
                    )
                  }
                />
              </td>
              <td>
                <input
                  type="number"
                  min="0"
                  step="0.01"
                  value={item.unitRate}
                  onChange={(e) =>
                    setItems((prev) =>
                      prev.map((row) =>
                        row.id === item.id ? { ...row, unitRate: Number(e.target.value || 0) } : row
                      )
                    )
                  }
                />
              </td>
              <td>
                <input
                  type="number"
                  min="0"
                  step="0.01"
                  value={item.discount || 0}
                  onChange={(e) =>
                    setItems((prev) =>
                      prev.map((row) =>
                        row.id === item.id ? { ...row, discount: Number(e.target.value || 0) } : row
                      )
                    )
                  }
                />
              </td>
              <td>{igst.toFixed(2)}</td>
              <td>{half.toFixed(2)}</td>
              <td>{half.toFixed(2)}</td>
              <td>
                <input
                  value={item.schedule}
                  onChange={(e) =>
                    setItems((prev) =>
                      prev.map((row) => (row.id === item.id ? { ...row, schedule: e.target.value } : row))
                    )
                  }
                />
              </td>
              <td>{(base + igst).toFixed(2)}</td>
              <td>
                <button type="button" className="action-btn row-remove-btn" onClick={() => onRemoveItem?.(item.id)}>
                  Remove
                </button>
              </td>
            </tr>
          );
        })}
      </tbody>
    </table>
  );
}
