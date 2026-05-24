import { useEffect, useMemo, useRef, useState } from 'react';
import { createSale, getSaleById, listDoctors, listSales, searchProducts, updateSale } from '../services/api';
import { uid } from './shared';
import BillingPopup from './billing/BillingPopup';
import BillingPrintInvoice from './billing/BillingPrintInvoice';
import BillingSalesTable from './billing/BillingSalesTable';
import { createInvoiceObjectFromBill, filterSalesRows } from './billing/billingUtils';
import { downloadInvoicePdf } from './billing/invoicePdf';

export default function BillingModule() {
  const [tab, setTab] = useState('new');
  const [printInvoice, setPrintInvoice] = useState(null);
  const [salesRows, setSalesRows] = useState([]);
  const [salesLoading, setSalesLoading] = useState(false);
  const [customerFilter, setCustomerFilter] = useState('');
  const [fromDate, setFromDate] = useState('');
  const [toDate, setToDate] = useState('');
  const [allBillsPage, setAllBillsPage] = useState(1);
  const [popup, setPopup] = useState({ open: false, mode: 'view', bill: null });
  const [editForm, setEditForm] = useState({ customerName: '', doctorName: '', billDate: '' });
  const [query, setQuery] = useState('');
  const [suggestions, setSuggestions] = useState([]);
  const [activeIndex, setActiveIndex] = useState(0);
  const [cart, setCart] = useState([]);
  const [discount, setDiscount] = useState(0);
  const [paymentType, setPaymentType] = useState('cash');
  const [customerName, setCustomerName] = useState('');
  const [doctorName, setDoctorName] = useState('');
  const [doctorOptions, setDoctorOptions] = useState([]);
  const [doctorSelection, setDoctorSelection] = useState('');
  const [doctorEditSelection, setDoctorEditSelection] = useState('');
  const [billDate, setBillDate] = useState(new Date().toISOString().slice(0, 10));
  const [status, setStatus] = useState('');
  const [batchSelection, setBatchSelection] = useState(null);
  const [removingCartIds, setRemovingCartIds] = useState([]);
  const searchRef = useRef(null);

  useEffect(() => {
    searchRef.current?.focus();
  }, []);

  const loadSales = async () => {
    setSalesLoading(true);
    try {
      const rows = await listSales();
      setSalesRows(rows);
    } catch {
      setSalesRows([]);
    } finally {
      setSalesLoading(false);
    }
  };

  useEffect(() => {
    loadSales();
  }, []);

  const loadDoctors = async () => {
    try {
      const rows = await listDoctors();
      setDoctorOptions(rows);
    } catch {
      setDoctorOptions([]);
    }
  };

  useEffect(() => {
    loadDoctors();
  }, []);

  useEffect(() => {
    setAllBillsPage(1);
  }, [customerFilter, fromDate, toDate]);

  useEffect(() => {
    const timer = setTimeout(async () => {
      if (!query.trim()) {
        setSuggestions([]);
        return;
      }
      try {
        const rows = await searchProducts(query.trim());
        setSuggestions(rows);
        setActiveIndex(0);
      } catch {
        setSuggestions([]);
      }
    }, 150);
    return () => clearTimeout(timer);
  }, [query]);

  useEffect(() => {
    const handler = (event) => {
      if (event.key === 'F2') {
        event.preventDefault();
        setPaymentType('cash');
        setStatus('Payment mode: CASH');
      }
      if (event.key === 'F3') {
        event.preventDefault();
        setPaymentType('credit');
        setStatus('Payment mode: CREDIT');
      }
    };

    window.addEventListener('keydown', handler);
    return () => window.removeEventListener('keydown', handler);
  }, []);

  const totals = useMemo(() => {
    const subtotal = cart.reduce((sum, item) => sum + item.quantity * item.unitRate, 0);
    const gst = cart.reduce((sum, item) => sum + ((item.quantity * item.unitRate) * item.gstPercent) / 100, 0);
    const total = Math.max(0, subtotal + gst - Number(discount || 0));
    return { subtotal, gst, total };
  }, [cart, discount]);

  const addFromSuggestion = (product) => {
    if (!product) return;

    if (!product.batches?.length) {
      const fallbackAvailableQty = Number(product.availableQty || product.stockQty || 0);
      if (fallbackAvailableQty <= 0) {
        setStatus('No available stock for selected medicine');
        return;
      }

      setCart((prev) => [
        ...prev,
        {
          id: uid(),
          productId: product.id,
          productName: product.name,
          batchId: null,
          batchNo: '-',
          quantity: 1,
          unitRate: Number(product.saleRate || 0),
          gstPercent: Number(product.gstPercent || 0),
          availableQty: fallbackAvailableQty
        }
      ]);
      setQuery('');
      setSuggestions([]);
      setStatus('Item added. Type quantity directly.');
      searchRef.current?.focus();
      return;
    }

    if (product.batches.length > 1) {
      setBatchSelection({ product, selected: 0 });
      return;
    }

    const batch = product.batches[0];
    setCart((prev) => [
      ...prev,
      {
        id: uid(),
        productId: product.id,
        productName: product.name,
        batchId: batch.id,
        batchNo: batch.batchNo,
        quantity: 1,
        unitRate: Number(batch.mrp || product.saleRate || 0),
        gstPercent: Number(product.gstPercent || 0),
        availableQty: Number(batch.quantity || 0)
      }
    ]);
    setQuery('');
    setSuggestions([]);
    setStatus('Item added. Type quantity directly.');
    searchRef.current?.focus();
  };

  const removeCartItem = (itemId) => {
    setRemovingCartIds((prev) => (prev.includes(itemId) ? prev : [...prev, itemId]));
    window.setTimeout(() => {
      setCart((prev) => prev.filter((row) => row.id !== itemId));
      setRemovingCartIds((prev) => prev.filter((id) => id !== itemId));
    }, 180);
  };

  const handleSearchKeyDown = (event) => {
    if (event.key === 'ArrowDown' && suggestions.length > 0) {
      event.preventDefault();
      setActiveIndex((i) => (i + 1) % suggestions.length);
    }
    if (event.key === 'ArrowUp' && suggestions.length > 0) {
      event.preventDefault();
      setActiveIndex((i) => (i - 1 + suggestions.length) % suggestions.length);
    }
    if (event.key === 'Enter' && suggestions.length > 0) {
      event.preventDefault();
      addFromSuggestion(suggestions[activeIndex]);
    }
  };

  const saveBill = async () => {
    if (!cart.length) {
      setStatus('Add at least one item before billing');
      return;
    }

    for (const item of cart) {
      if (Number(item.quantity || 0) > Number(item.availableQty || 0)) {
        setStatus(`Blocked: stock less than qty for ${item.productName}`);
        return;
      }
    }

    const invoiceNo = `INV-${Date.now()}`;
    const discountValue = Math.max(0, Number(discount || 0));

    try {
      await createSale({
        invoiceNo,
        customerName,
        doctorName,
        billDate,
        paymentType,
        items: cart.map((item, index) => ({
          productId: item.productId,
          batchId: item.batchId,
          quantity: Number(item.quantity || 0),
          unitRate: item.unitRate,
          gstPercent: item.gstPercent,
          discount: index === 0 ? discountValue : 0
        }))
      });

      setStatus(`Bill ${invoiceNo} saved`);
      setCart([]);
      setDiscount(0);
      setQuery('');
      setSuggestions([]);
      setDoctorSelection('');
      setDoctorName('');
      setTab('recent');
      await loadDoctors();
      await loadSales();
      searchRef.current?.focus();
    } catch (error) {
      setStatus(error?.response?.data?.message || 'Billing failed');
    }
  };

  const handleBillAction = async (mode, billId) => {
    try {
      const bill = await getSaleById(billId);
      const invoiceObject = createInvoiceObjectFromBill(bill);

      if (mode === 'print') {
        setPrintInvoice(invoiceObject);
        setTimeout(() => window.print(), 80);
        return;
      }

      if (mode === 'pdf') {
        downloadInvoicePdf(invoiceObject);
        setStatus(`PDF generated for ${invoiceObject.invoiceNo}`);
        return;
      }

      if (mode === 'edit') {
        const billDoctorName = bill.doctorName || '';
        const matchedDoctor = doctorOptions.some((row) => row.name === billDoctorName);

        setEditForm({
          customerName: bill.customerName || '',
          doctorName: billDoctorName,
          billDate: bill.billDate ? new Date(bill.billDate).toISOString().slice(0, 10) : ''
        });
        setDoctorEditSelection(
          billDoctorName
            ? (matchedDoctor ? billDoctorName : '__custom__')
            : ''
        );
      }

      setPopup({ open: true, mode, bill });
    } catch {
      setStatus('Failed to load bill details');
    }
  };

  const saveBillEdit = async () => {
    if (!popup.bill?.id) return;

    try {
      await updateSale(popup.bill.id, {
        customerName: editForm.customerName,
        doctorName: editForm.doctorName,
        billDate: editForm.billDate
      });
      setPopup({ open: false, mode: 'view', bill: null });
      await loadDoctors();
      await loadSales();
      setStatus('Bill updated');
    } catch (error) {
      setStatus(error?.response?.data?.message || 'Failed to update bill');
    }
  };

  const filteredSales = useMemo(() => {
    return filterSalesRows(salesRows, customerFilter, fromDate, toDate);
  }, [salesRows, customerFilter, fromDate, toDate]);

  const recentSales = filteredSales.slice(0, 20);
  const perPage = 10;
  const totalPages = Math.max(1, Math.ceil(filteredSales.length / perPage));
  const safePage = Math.min(allBillsPage, totalPages);
  const pagedSales = filteredSales.slice((safePage - 1) * perPage, safePage * perPage);

  return (
    <section className="module-page billing">
      <h3>Billing (Keyboard: F2 Cash, F3 Credit)</h3>
      <div className="tab-switch">
        <button type="button" className={tab === 'new' ? 'active-tab' : ''} onClick={() => setTab('new')}>
          New Bill
        </button>
        <button type="button" className={tab === 'recent' ? 'active-tab' : ''} onClick={() => setTab('recent')}>
          Last 20 Bills
        </button>
        <button type="button" className={tab === 'all' ? 'active-tab' : ''} onClick={() => setTab('all')}>
          All Bills
        </button>
      </div>

      {(tab === 'recent' || tab === 'all') && (
        <div className="bill-filter-row billing-filter-row">
          <input
            value={customerFilter}
            onChange={(e) => setCustomerFilter(e.target.value)}
            placeholder="Search customer name"
          />
          <input type="date" value={fromDate} onChange={(e) => setFromDate(e.target.value)} />
          <input type="date" value={toDate} onChange={(e) => setToDate(e.target.value)} />
          <button
            type="button"
            className="secondary-btn"
            onClick={() => {
              setCustomerFilter('');
              setFromDate('');
              setToDate('');
            }}
          >
            Clear Filters
          </button>
        </div>
      )}

      {tab === 'new' && (
        <>

          <div className="invoice-shell invoice-live-shell">
            <div className="invoice-meta-grid">
              <div className="invoice-field">
                <span>Invoice number</span>
                <strong>Will generate on save</strong>
              </div>
              <div className="invoice-field">
                <span>Date</span>
                <strong>{billDate ? new Date(`${billDate}T00:00:00`).toLocaleDateString('en-GB') : '-'}</strong>
              </div>
            </div>

            <div className="row-3 billing-customer-grid">

              <input
                value={customerName}
                onChange={(e) => setCustomerName(e.target.value)}
                placeholder="Customer name"
              />
              <div className="doctor-select-stack">
                <select
                  value={doctorSelection}
                  onChange={(e) => {
                    const selected = e.target.value;
                    setDoctorSelection(selected);

                    if (selected === '__custom__') {
                      return;
                    }

                    setDoctorName(selected);
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
                  value={doctorName}
                  onChange={(e) => {
                    const value = e.target.value;
                    setDoctorName(value);
                    if (!value.trim()) {
                      setDoctorSelection('');
                    } else if (!doctorOptions.some((doctor) => doctor.name === value)) {
                      setDoctorSelection('__custom__');
                    }
                  }}
                  placeholder="Enter doctor name"
                />
                {doctorSelection && doctorSelection !== '__custom__' && (
                  <small className="field-hint">Selected from doctor history</small>
                )}
              </div>

            </div>

            <div className="row-3 billing-meta-grid">
              <input type="date" value={billDate} onChange={(e) => setBillDate(e.target.value)} />
              <select value={paymentType} onChange={(e) => setPaymentType(e.target.value)}>
                <option value="cash">Cash</option>
                <option value="credit">Credit</option>
                <option value="online">Online</option>
              </select>
              <strong className="billing-payment-chip">Payment: {paymentType.toUpperCase()}</strong>
            </div>

          </div>

          <div className="table-card billing-items-card">
            <div className="table-card-head">
              <h4>Current Bill Items</h4>
              <p>{cart.length} line items</p>
            </div>
            <div className="doctor-select-stack">
              <input
                ref={searchRef}
                value={query}
                onChange={(e) => setQuery(e.target.value)}
                onKeyDown={handleSearchKeyDown}
                placeholder="Search medicine..."
              />
              {suggestions.length > 0 && (
                <div className="suggestions">
                  {suggestions.map((s, index) => (
                    <button
                      key={s.id}
                      type="button"
                      className={index === activeIndex ? 'suggestion' : 'suggestion'}
                      onClick={() => addFromSuggestion(s)}
                    >
                      {s.name} | Stock: {s.availableQty}
                    </button>
                  ))}
                </div>
              )}

              {batchSelection && (
                <div className="batch-popup">
                  <h4>Select Batch</h4>
                  {batchSelection.product.batches.map((batch, index) => (
                    <button
                      key={batch.id}
                      type="button"
                      className={index === batchSelection.selected ? 'suggestion active' : 'suggestion'}
                      onClick={() => {
                        setCart((prev) => [
                          ...prev,
                          {
                            id: uid(),
                            productId: batchSelection.product.id,
                            productName: batchSelection.product.name,
                            batchId: batch.id,
                            batchNo: batch.batchNo,
                            quantity: 1,
                            unitRate: Number(batch.mrp || batchSelection.product.saleRate || 0),
                            gstPercent: Number(batchSelection.product.gstPercent || 0),
                            availableQty: Number(batch.quantity || 0)
                          }
                        ]);
                        setBatchSelection(null);
                        setQuery('');
                        setSuggestions([]);
                        searchRef.current?.focus();
                      }}
                    >
                      Batch {batch.batchNo} | Exp: {batch.expiryDate?.slice(0, 10) || 'NA'} | Qty: {batch.quantity}
                    </button>
                  ))}
                </div>)}
              <table className="invoice-items-table">
                <thead>
                  <tr>
                    <th>Sl No.</th>
                    <th>Item description</th>
                    <th>Batch</th>
                    <th>Exp</th>
                    <th>Qty</th>
                    <th>Price/Unit</th>
                    <th>Amount</th>
                    <th>Action</th>
                  </tr>
                </thead>
                <tbody>
                  {cart.map((item, index) => (
                    <tr
                      key={item.id}
                      className={removingCartIds.includes(item.id) ? 'table-row-animated row-removing' : 'table-row-animated'}
                    >
                      <td>{index + 1}</td>
                      <td>{item.productName}</td>
                      <td>{item.batchNo}</td>
                      <td>{item.expiryDate?.slice(0, 10) || '-'}</td>
                      <td>
                        <input
                          value={item.quantity}
                          onChange={(e) => {
                            const qty = Number(e.target.value || 0);
                            setCart((prev) => prev.map((row) => (row.id === item.id ? { ...row, quantity: qty } : row)));
                          }}
                        />
                      </td>
                      <td>{item.unitRate.toFixed(2)}</td>
                      <td>{(item.unitRate * item.quantity).toFixed(2)}</td>
                      <td>
                        <button type="button" className="action-btn row-remove-btn" onClick={() => removeCartItem(item.id)}>
                          Remove
                        </button>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>

            <div className="invoice-bottom-grid">
              <div className="invoice-terms-box">
                <h4>Terms and Conditions</h4>
                <p>Goods once sold will not be taken back without valid reason.</p>
                <p>Payment Type: {String(paymentType).toUpperCase()}</p>
              </div>

              <div className="invoice-total-box">
                <div>
                  <span>Subtotal</span>
                  <strong>{totals.subtotal.toFixed(2)}</strong>
                </div>
                <div>
                  <span>Discount</span>
                  <strong>{Number(discount || 0).toFixed(2)}</strong>
                </div>
                <div>
                  <span>GST</span>
                  <strong>{totals.gst.toFixed(2)}</strong>
                </div>
                <div>
                  <span>Tax</span>
                  <strong>0.00</strong>
                </div>
                <div className="invoice-grand-total">
                  <span>Invoice Total</span>
                  <strong>{Math.max(0, totals.total).toFixed(2)}</strong>
                </div>
              </div>
            </div>
          </div>

          <div className="row-2 billing-foot-row">
            <label>
              Discount
              <input value={discount} onChange={(e) => setDiscount(Number(e.target.value || 0))} />
            </label>

          </div>
          <button type="button" onClick={saveBill}>
            Save Bill
          </button>
        </>
      )}

      {tab === 'recent' && (
        <section className="billing-list-panel">
          <h4>Last 20 Bills</h4>
          {salesLoading ? <p>Loading bills...</p> : <BillingSalesTable rows={recentSales} onAction={handleBillAction} />}
        </section>
      )}

      {tab === 'all' && (
        <section className="billing-list-panel">
          <h4>All Bills</h4>
          {salesLoading ? <p>Loading bills...</p> : <BillingSalesTable rows={pagedSales} onAction={handleBillAction} />}
          <div className="pager-row">
            <button
              type="button"
              className="secondary-btn"
              disabled={safePage <= 1}
              onClick={() => setAllBillsPage((p) => Math.max(1, p - 1))}
            >
              Prev
            </button>
            <span>
              Page {safePage} / {totalPages}
            </span>
            <button
              type="button"
              className="secondary-btn"
              disabled={safePage >= totalPages}
              onClick={() => setAllBillsPage((p) => Math.min(totalPages, p + 1))}
            >
              Next
            </button>
          </div>
        </section>
      )}

      {status && <p className="small-row">{status}</p>}

      <BillingPopup
        popup={popup}
        editForm={editForm}
        setEditForm={setEditForm}
        doctorOptions={doctorOptions}
        doctorEditSelection={doctorEditSelection}
        setDoctorEditSelection={setDoctorEditSelection}
        onSaveEdit={saveBillEdit}
        onClose={() => setPopup({ open: false, mode: 'view', bill: null })}
      />

      <BillingPrintInvoice invoice={printInvoice} />
    </section>
  );
}

