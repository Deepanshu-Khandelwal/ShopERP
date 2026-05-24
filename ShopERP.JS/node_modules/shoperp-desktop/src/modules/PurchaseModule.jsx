import { useEffect, useMemo, useState } from 'react';
import { createProductEntry, createPurchase, listProducts, listSuppliers } from '../services/api';
import { uid } from './shared';
import PurchaseItemsTable from './purchase/PurchaseItemsTable';
import PurchaseQuickAddForm from './purchase/PurchaseQuickAddForm';
import PurchaseTotalsFooter from './purchase/PurchaseTotalsFooter';

export default function PurchaseModule() {
  const [products, setProducts] = useState([]);
  const [suppliers, setSuppliers] = useState([]);
  const [supplierId, setSupplierId] = useState('');
  const [billNo, setBillNo] = useState(`PB-${Date.now()}`);
  const [supplierName, setSupplierName] = useState('');
  const [supplierAddress, setSupplierAddress] = useState('');
  const [supplierPhone, setSupplierPhone] = useState('');
  const [selectedProductId, setSelectedProductId] = useState('');
  const [purchaseDate, setPurchaseDate] = useState(new Date().toISOString().slice(0, 10));
  const [items, setItems] = useState([]);
  const [removingItemIds, setRemovingItemIds] = useState([]);
  const [status, setStatus] = useState('');
  const [newProduct, setNewProduct] = useState({
    name: '',
    sku: '',
    batchNo: '',
    unit: 'strip',
    purchaseRate: 0,
    saleRate: 0,
    gstPercent: 0
  });

  const loadBaseData = async () => {
    try {
      const [productRows, supplierRows] = await Promise.all([listProducts(), listSuppliers()]);
      setProducts(productRows);
      setSuppliers(supplierRows);

      if (!selectedProductId && productRows.length > 0) {
        setSelectedProductId(productRows[0].id);
      }
    } catch {
      setProducts([]);
      setSuppliers([]);
    }
  };

  useEffect(() => {
    loadBaseData();
  }, []);

  const productMap = useMemo(() => {
    return new Map(products.map((row) => [row.id, row]));
  }, [products]);

  const onSupplierChange = (value) => {
    setSupplierId(value);
    const selected = suppliers.find((row) => row.id === value);
    if (!selected) {
      setSupplierName('');
      setSupplierPhone('');
      setSupplierAddress('');
      return;
    }

    setSupplierName(selected.name || '');
    setSupplierPhone(selected.phone || '');
    setSupplierAddress(selected.address || '');
  };

  const buildItemFromProduct = (product, overrides = {}) => ({
    id: uid(),
    productId: product.id,
    quantity: Number(overrides.quantity ?? 1),
    unitRate: Number(overrides.unitRate ?? product.purchaseRate ?? 0),
    discount: Number(overrides.discount ?? 0),
    gstPercent: Number(overrides.gstPercent ?? product.gstPercent ?? 0),
    batchNo: String(overrides.batchNo ?? `B-${Date.now()}`),
    mrp: Number(overrides.mrp ?? product.saleRate ?? 0),
    expiryDate: overrides.expiryDate || '',
    pack: overrides.pack || '1x1',
    scheme: overrides.scheme || '-',
    schedule: overrides.schedule || 'H1'
  });

  const addItem = (productIdArg) => {
    const productId = productIdArg || selectedProductId || products[0]?.id;
    if (!productId) {
      setStatus('No products available. Add a product first.');
      return;
    }

    const product = productMap.get(productId);
    if (!product) {
      setStatus('Selected product was not found');
      return;
    }

    setItems((prev) => [...prev, buildItemFromProduct(product)]);
    setStatus('Item row added');
  };

  const addDuplicateLastItem = () => {
    const last = items[items.length - 1];
    if (!last) {
      setStatus('No item found to duplicate');
      return;
    }

    const product = productMap.get(last.productId);
    if (!product) {
      setStatus('Last item product not found');
      return;
    }

    setItems((prev) => [
      ...prev,
      buildItemFromProduct(product, {
        unitRate: last.unitRate,
        gstPercent: last.gstPercent,
        mrp: last.mrp,
        batchNo: last.batchNo,
        expiryDate: last.expiryDate,
        pack: last.pack,
        scheme: last.scheme,
        schedule: last.schedule
      })
    ]);
    setStatus('Last row duplicated');
  };

  const addRowFromKeyboard = (currentRow) => {
    if (!currentRow?.productId) {
      setStatus('Select product before adding next row');
      return;
    }

    const product = productMap.get(currentRow.productId);
    if (!product) {
      setStatus('Product not found for next row');
      return;
    }

    setItems((prev) => [
      ...prev,
      buildItemFromProduct(product, {
        unitRate: currentRow.unitRate,
        gstPercent: currentRow.gstPercent,
        mrp: currentRow.mrp,
        batchNo: currentRow.batchNo,
        expiryDate: currentRow.expiryDate,
        pack: currentRow.pack,
        scheme: currentRow.scheme,
        schedule: currentRow.schedule
      })
    ]);
    setStatus('Next row added');
  };

  const removeItem = (itemId) => {
    setRemovingItemIds((prev) => (prev.includes(itemId) ? prev : [...prev, itemId]));
    window.setTimeout(() => {
      setItems((prev) => prev.filter((row) => row.id !== itemId));
      setRemovingItemIds((prev) => prev.filter((id) => id !== itemId));
    }, 180);
  };

  const validateItems = () => {
    if (!items.length) {
      return 'Add at least one item before saving purchase';
    }

    for (const row of items) {
      if (!row.productId) return 'Each row must have a product';
      if (!String(row.batchNo || '').trim()) return 'Batch number is required for each row';
      if (Number(row.quantity || 0) <= 0) return 'Quantity must be greater than 0 for each row';
      if (Number(row.unitRate || 0) <= 0) return 'Purchase rate must be greater than 0 for each row';
    }

    return '';
  };

  const savePurchase = async () => {
    const validationError = validateItems();
    if (validationError) {
      setStatus(validationError);
      return;
    }

    try {
      await createPurchase({
        billNo,
        supplierId: supplierId || null,
        billDate: purchaseDate,
        items: items.map((row) => ({
          ...row,
          batchNo: String(row.batchNo || '').trim()
        }))
      });

      setStatus('Purchase saved and stock updated');
      setItems([]);
      setBillNo(`PB-${Date.now()}`);
      await loadBaseData();
    } catch (error) {
      setStatus(error?.response?.data?.message || 'Purchase failed');
    }
  };

  const totals = useMemo(() => {
    const subtotal = items.reduce((sum, row) => sum + Number(row.quantity || 0) * Number(row.unitRate || 0), 0);
    const totalDiscount = items.reduce((sum, row) => sum + Number(row.discount || 0), 0);
    const taxable = Math.max(0, subtotal - totalDiscount);
    const totalGst = items.reduce(
      (sum, row) =>
        sum +
        ((Math.max(0, Number(row.quantity || 0) * Number(row.unitRate || 0) - Number(row.discount || 0))) *
          Number(row.gstPercent || 0)) /
          100,
      0
    );
    const totalCgst = totalGst / 2;
    const totalSgst = totalGst / 2;
    const grandTotal = taxable + totalGst;
    const rounded = Math.round(grandTotal);
    const roundOff = rounded - grandTotal;
    return {
      subtotal,
      taxable,
      totalGst,
      totalCgst,
      totalSgst,
      totalDiscount,
      grandTotal,
      rounded,
      roundOff
    };
  }, [items]);

  const totalQty = useMemo(() => items.reduce((sum, row) => sum + Number(row.quantity || 0), 0), [items]);

  const addProduct = async () => {
    if (!newProduct.name.trim()) {
      setStatus('Product name is required');
      return;
    }

    try {
      const created = await createProductEntry({
        name: newProduct.name.trim(),
        sku: newProduct.sku.trim() || undefined,
        unit: newProduct.unit,
        purchaseRate: Number(newProduct.purchaseRate || 0),
        saleRate: Number(newProduct.saleRate || 0),
        gstPercent: Number(newProduct.gstPercent || 0)
      });

      const batchNo = newProduct.batchNo.trim() || `B-${Date.now()}`;
      setItems((prev) => [
        ...prev,
        {
          id: uid(),
          productId: created.id,
          quantity: 1,
          unitRate: Number(newProduct.purchaseRate || 0),
          discount: 0,
          gstPercent: Number(newProduct.gstPercent || 0),
          batchNo,
          mrp: Number(newProduct.saleRate || 0),
          expiryDate: '',
          pack: '1x1',
          scheme: '-',
          schedule: 'H1'
        }
      ]);

      const rows = await listProducts();
      setProducts(rows);
      setSelectedProductId(created.id);
      setStatus(`Product added: ${newProduct.name.trim()}`);
      setNewProduct({
        name: '',
        sku: '',
        batchNo: '',
        unit: 'strip',
        purchaseRate: 0,
        saleRate: 0,
        gstPercent: 0
      });
    } catch (error) {
      setStatus(error?.response?.data?.message || 'Failed to add product');
    }
  };

  return (
    <section className="module-page purchase-page">
      <h3>Purchase (GST Invoice Style)</h3>
      <div className="purchase-strip">
        <p>
          <span>Item Rows</span>
          <strong>{items.length}</strong>
        </p>
        <p>
          <span>Total Qty</span>
          <strong>{totalQty}</strong>
        </p>
        <p>
          <span>Net Amount</span>
          <strong>{totals.grandTotal.toFixed(2)}</strong>
        </p>
        <p>
          <span>Supplier</span>
          <strong>{supplierName || 'Not selected'}</strong>
        </p>
      </div>

      <div className="invoice-title-bar">PURCHASE INVOICE</div>

        <div className="purchase-head-grid">
          <div className="purchase-meta-card">
            <div className="purchase-meta-row">
              <span>INV NO</span>
              <input value={billNo} onChange={(e) => setBillNo(e.target.value)} />
            </div>
            <div className="purchase-meta-row">
              <span>INV Date</span>
              <input type="date" value={purchaseDate} onChange={(e) => setPurchaseDate(e.target.value)} />
            </div>
            <div className="purchase-meta-row">
              <span>Payment Mode</span>
              <strong>CASH</strong>
            </div>
            <div className="purchase-meta-row">
              <span>Supplier ID</span>
              <select value={supplierId} onChange={(e) => onSupplierChange(e.target.value)}>
                <option value="">Select Supplier</option>
                {suppliers.map((supplier) => (
                  <option key={supplier.id} value={supplier.id}>
                    {supplier.name}
                  </option>
                ))}
              </select>
            </div>
          </div>
        </div>

        <div className="purchase-supplier-box">
          <h4>Supplier / Receiver Details</h4>
          <div className="row-3">
            <input
              value={supplierName}
              onChange={(e) => setSupplierName(e.target.value)}
              placeholder="Supplier Name"
            />
            <input
              value={supplierPhone}
              onChange={(e) => setSupplierPhone(e.target.value)}
              placeholder="Phone"
            />
            <input
              value={supplierAddress}
              onChange={(e) => setSupplierAddress(e.target.value)}
              placeholder="Address"
            />
          </div>
        </div>

        <PurchaseItemsTable
          items={items}
          setItems={setItems}
          products={products}
          removingItemIds={removingItemIds}
          onRemoveItem={removeItem}
          onAddNextRow={addRowFromKeyboard}
        />
        <PurchaseTotalsFooter totals={totals} />
      

      <button type="button" className="purchase-save-btn" onClick={savePurchase}>
        Save Purchase
      </button>
      {status && <p className="small-row">{status}</p>}
    </section>
  );
}

