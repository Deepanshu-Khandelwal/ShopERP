export default function BillingPopup({
  popup,
  editForm,
  setEditForm,
  doctorOptions = [],
  doctorEditSelection,
  setDoctorEditSelection,
  onSaveEdit,
  onClose
}) {
  if (!popup.open || !popup.bill) return null;

  return (
    <div className="modal-backdrop">
      <div className="modal-card">
        <h4>{popup.mode === 'view' ? 'View Bill' : 'Edit Bill'}</h4>
        <p>Invoice: {popup.bill.invoiceNo}</p>

        {popup.mode === 'view' ? (
          <>
            <p>Customer: {popup.bill.customerName || ''}</p>
            <p>Bill Amount: {Number(popup.bill.grandTotal || 0).toFixed(2)}</p>
            <p>Date: {new Date(popup.bill.billDate).toLocaleDateString('en-GB')}</p>
            <table className="table">
              <thead>
                <tr>
                  <th>Item</th>
                  <th>Qty</th>
                  <th>Rate</th>
                  <th>Total</th>
                </tr>
              </thead>
              <tbody>
                {(popup.bill.items || []).map((item) => (
                  <tr key={item.id}>
                    <td>{item?.product?.name || 'Item'}</td>
                    <td>{Number(item.quantity || 0)}</td>
                    <td>{Number(item.unitRate || 0).toFixed(2)}</td>
                    <td>{Number(item.lineTotal || 0).toFixed(2)}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </>
        ) : (
          <div className="row-3">
            <input
              value={editForm.customerName}
              onChange={(e) => setEditForm((f) => ({ ...f, customerName: e.target.value }))}
              placeholder="Customer Name"
            />
            <div className="doctor-select-stack">
              <select
                value={doctorEditSelection}
                onChange={(e) => {
                  const selected = e.target.value;
                  setDoctorEditSelection(selected);

                  if (selected === '__custom__') return;
                  setEditForm((f) => ({ ...f, doctorName: selected }));
                }}
              >
                <option value="">Select doctor</option>
                {doctorOptions.map((doctor) => (
                  <option key={doctor.name} value={doctor.name}>
                    {doctor.name}
                  </option>
                ))}
                <option value="__custom__">Other / Manual entry</option>
              </select>
              <input
                value={editForm.doctorName}
                onChange={(e) => {
                  const value = e.target.value;
                  setEditForm((f) => ({ ...f, doctorName: value }));
                  if (!value.trim()) {
                    setDoctorEditSelection('');
                  } else if (!doctorOptions.some((doctor) => doctor.name === value)) {
                    setDoctorEditSelection('__custom__');
                  }
                }}
                placeholder="Doctor Name"
              />
            </div>
            <input
              type="date"
              value={editForm.billDate}
              onChange={(e) => setEditForm((f) => ({ ...f, billDate: e.target.value }))}
            />
            <button type="button" onClick={onSaveEdit}>
              Save Changes
            </button>
          </div>
        )}

        <button type="button" className="secondary-btn" onClick={onClose}>
          Close
        </button>
      </div>
    </div>
  );
}
