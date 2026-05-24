import { ActionIcon } from '../shared';

export default function BillingSalesTable({ rows, onAction }) {
  return (
    <table className="table bill-list-table">
      <thead>
        <tr>
          <th>Customer Name</th>
          <th>Bill Amount</th>
          <th>Date Created</th>
          <th>Action</th>
        </tr>
      </thead>
      <tbody>
        {rows.map((bill) => (
          <tr key={bill.id}>
            <td>{bill.customerName || ''}</td>
            <td>{Number(bill.grandTotal || 0).toFixed(2)}</td>
            <td>{bill.billDate ? new Date(bill.billDate).toLocaleDateString('en-GB') : '-'}</td>
            <td>
              <div className="action-group">
                <button type="button" className="action-btn" onClick={() => onAction('view', bill.id)}>
                  <ActionIcon kind="view" />
                  View
                </button>
                <button type="button" className="action-btn" onClick={() => onAction('edit', bill.id)}>
                  <ActionIcon kind="edit" />
                  Edit
                </button>
                <button type="button" className="action-btn" onClick={() => onAction('print', bill.id)}>
                  <ActionIcon kind="print" />
                  Print
                </button>
                <button type="button" className="action-btn" onClick={() => onAction('pdf', bill.id)}>
                  <ActionIcon kind="print" />
                  PDF
                </button>
              </div>
            </td>
          </tr>
        ))}
      </tbody>
    </table>
  );
}
