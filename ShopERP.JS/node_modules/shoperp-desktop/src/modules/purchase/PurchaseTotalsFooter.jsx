export default function PurchaseTotalsFooter({ totals }) {
  return (
    <div className="purchase-footer-grid">
      <div className="purchase-bank-box">
        <p>A/c No: 0000000000</p>
        <p>IFSC: ABCD0000000</p>
        <p>Bank: YOUR BANK</p>
      </div>
      <div className="purchase-total-box">
        <div>
          <span>Subtotal</span>
          <strong>{totals.subtotal.toFixed(2)}</strong>
        </div>
        <div>
          <span>Total Discounts</span>
          <strong>{totals.totalDiscount.toFixed(2)}</strong>
        </div>
        <div>
          <span>Taxable Value</span>
          <strong>{totals.taxable.toFixed(2)}</strong>
        </div>
        <div>
          <span>CGST</span>
          <strong>{totals.totalCgst.toFixed(2)}</strong>
        </div>
        <div>
          <span>SGST</span>
          <strong>{totals.totalSgst.toFixed(2)}</strong>
        </div>
        <div>
          <span>Grand Total</span>
          <strong>{totals.grandTotal.toFixed(2)}</strong>
        </div>
        <div>
          <span>Round Off</span>
          <strong>{totals.roundOff.toFixed(2)}</strong>
        </div>
      </div>
    </div>
  );
}
