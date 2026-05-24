export function createInvoiceObjectFromBill(bill) {
  const subtotal = Number(bill?.subtotal || 0);
  const gstTotal = Number(bill?.taxTotal || 0);
  const discountTotal = Number(bill?.discountTotal || 0);

  return {
    companyName: 'MEDICAL STORE',
    companyAddressLine1: 'Shop Address Line 1',
    companyAddressLine2: 'Shop Address Line 2',
    phone: '+91-0000000000',
    email: 'shop@example.com',
    invoiceNo: bill?.invoiceNo || '-',
    dateOfIssue: bill?.billDate ? new Date(bill.billDate).toLocaleDateString('en-GB') : '-',
    deliveryDate: bill?.billDate ? new Date(bill.billDate).toLocaleDateString('en-GB') : '-',
    customerName: bill?.customerName || ' Customer',
    customerAddress: '-',
    items: (bill?.items || []).map((item, index) => ({
      slNo: index + 1,
      itemDescription: `${item?.product?.name || 'Item'}${item?.batch?.batchNo ? ` (${item.batch.batchNo})` : ''}`,
      qty: Number(item?.quantity || 0),
      pricePerUnit: Number(item?.unitRate || 0),
      amount: Number(item?.lineTotal || 0)
    })),
    subtotal,
    discount: discountTotal,
    gstPercent: subtotal > 0 ? (gstTotal / subtotal) * 100 : 0,
    tax: 0,
    invoiceTotal: Number(bill?.grandTotal || 0),
    paymentType: Number(bill?.dueTotal || 0) > 0 ? 'credit' : 'cash'
  };
}

export function filterSalesRows(salesRows, customerFilter, fromDate, toDate) {
  return salesRows.filter((bill) => {
    const customer = String(bill.customerName || '').toLowerCase();
    const matchesCustomer =
      !customerFilter.trim() || customer.includes(customerFilter.trim().toLowerCase());

    const billDate = bill.billDate ? new Date(bill.billDate) : null;
    const from = fromDate ? new Date(`${fromDate}T00:00:00`) : null;
    const to = toDate ? new Date(`${toDate}T23:59:59`) : null;

    const matchesFrom = !from || (billDate && billDate >= from);
    const matchesTo = !to || (billDate && billDate <= to);

    return matchesCustomer && matchesFrom && matchesTo;
  });
}
