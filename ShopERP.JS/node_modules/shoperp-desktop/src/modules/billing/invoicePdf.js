import jsPDF from 'jspdf';
import autoTable from 'jspdf-autotable';

export function downloadInvoicePdf(invoice) {
  if (!invoice) return;

  const doc = new jsPDF({ unit: 'pt', format: 'a4' });
  const left = 40;

  doc.setFontSize(16);
  doc.text(invoice.companyName || 'MEDICAL STORE', left, 45);
  doc.setFontSize(10);
  doc.text(`Invoice: ${invoice.invoiceNo || '-'}`, left, 62);
  doc.text(`Date: ${invoice.dateOfIssue || '-'}`, left + 220, 62);
  doc.text(`Customer: ${invoice.customerName || ' Customer'}`, left, 78);

  autoTable(doc, {
    startY: 95,
    head: [['Sl No', 'Item', 'Qty', 'Price', 'Amount']],
    body: (invoice.items || []).map((item) => [
      String(item.slNo || ''),
      String(item.itemDescription || ''),
      String(item.qty || 0),
      Number(item.pricePerUnit || 0).toFixed(2),
      Number(item.amount || 0).toFixed(2)
    ]),
    styles: { fontSize: 9 }
  });

  const y = doc.lastAutoTable?.finalY ? doc.lastAutoTable.finalY + 20 : 430;
  doc.text(`Subtotal: ${Number(invoice.subtotal || 0).toFixed(2)}`, left, y);
  doc.text(`Discount: ${Number(invoice.discount || 0).toFixed(2)}`, left, y + 14);
  doc.text(`GST: ${Number((invoice.subtotal || 0) * ((invoice.gstPercent || 0) / 100)).toFixed(2)}`, left, y + 28);
  doc.text(`Total: ${Number(invoice.invoiceTotal || 0).toFixed(2)}`, left, y + 42);
  doc.text(`Payment: ${String(invoice.paymentType || '').toUpperCase()}`, left + 220, y + 42);

  doc.save(`${invoice.invoiceNo || 'invoice'}.pdf`);
}
