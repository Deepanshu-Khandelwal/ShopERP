export default function BillingPrintInvoice({ invoice }) {
  if (!invoice) return null;

  return (
    <section className="print-invoice-root">
      <header className="invoice-title-bar">MEDICAL STORE BILL BOOK FORMAT / TEMPLATE</header>
      <div className="invoice-shell">
        <div className="invoice-company-block">
          <div className="logo-box">LOGO</div>
          <div className="company-info-box">
            <h2>{invoice.companyName}</h2>
            <p>{invoice.companyAddressLine1}</p>
            <p>{invoice.companyAddressLine2}</p>
            <p>
              Phone: {invoice.phone} | Email: {invoice.email}
            </p>
          </div>
        </div>

        <div className="invoice-meta-grid">
          <div className="invoice-field">
            <span>Invoice number</span>
            <strong>{invoice.invoiceNo}</strong>
          </div>
          <div className="invoice-field">
            <span>Date of issue</span>
            <strong>{invoice.dateOfIssue}</strong>
          </div>
          <div className="invoice-field">
            <span>Delivery Date</span>
            <strong>{invoice.deliveryDate}</strong>
          </div>
        </div>

        <div className="invoice-billto">
          <h4>Bill To</h4>
          <p>{invoice.customerName || ' Customer'}</p>
          <p>{invoice.customerAddress}</p>
        </div>

        <table className="invoice-items-table">
          <thead>
            <tr>
              <th>Sl No.</th>
              <th>Item description</th>
              <th>Qty</th>
              <th>Price/Unit</th>
              <th>Amount</th>
            </tr>
          </thead>
          <tbody>
            {invoice.items.map((item) => (
              <tr key={`${invoice.invoiceNo}-${item.slNo}`}>
                <td>{item.slNo}</td>
                <td>{item.itemDescription}</td>
                <td>{item.qty}</td>
                <td>{item.pricePerUnit.toFixed(2)}</td>
                <td>{item.amount.toFixed(2)}</td>
              </tr>
            ))}
          </tbody>
        </table>

        <div className="invoice-bottom-grid">
          <div className="invoice-terms-box">
            <h4>Terms and Conditions</h4>
            <p>Goods once sold will not be taken back without valid reason.</p>
            <p>Payment Type: {String(invoice.paymentType).toUpperCase()}</p>
          </div>

          <div className="invoice-total-box">
            <div>
              <span>Subtotal</span>
              <strong>{invoice.subtotal.toFixed(2)}</strong>
            </div>
            <div>
              <span>Discount</span>
              <strong>{invoice.discount.toFixed(2)}</strong>
            </div>
            <div>
              <span>GST ({invoice.gstPercent.toFixed(2)}%)</span>
              <strong>{(invoice.subtotal * (invoice.gstPercent / 100)).toFixed(2)}</strong>
            </div>
            <div>
              <span>Tax</span>
              <strong>{invoice.tax.toFixed(2)}</strong>
            </div>
            <div className="invoice-grand-total">
              <span>Invoice Total</span>
              <strong>{invoice.invoiceTotal.toFixed(2)}</strong>
            </div>
          </div>
        </div>
      </div>
    </section>
  );
}
