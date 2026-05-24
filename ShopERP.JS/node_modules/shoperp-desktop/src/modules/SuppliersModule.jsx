import { useEffect, useState } from 'react';
import { createSupplier, deleteSupplier, listSuppliers, updateSupplier } from '../services/api';

const emptyForm = {
  name: '',
  phone: '',
  email: '',
  address: '',
  gstin: '',
  drugLicense: ''
};

export default function SuppliersModule() {
  const [rows, setRows] = useState([]);
  const [loading, setLoading] = useState(true);
  const [status, setStatus] = useState('');
  const [form, setForm] = useState(emptyForm);
  const [editingId, setEditingId] = useState('');

  const loadSuppliers = async () => {
    setLoading(true);
    try {
      const data = await listSuppliers();
      setRows(data);
    } catch (error) {
      setRows([]);
      setStatus(error?.response?.data?.message || 'Failed to load suppliers');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadSuppliers();
  }, []);

  const submit = async (event) => {
    event.preventDefault();

    if (!form.name.trim()) {
      setStatus('Supplier name is required');
      return;
    }

    try {
      const payload = {
        name: form.name.trim(),
        phone: form.phone.trim() || undefined,
        email: form.email.trim() || undefined,
        address: form.address.trim() || undefined,
        gstin: form.gstin.trim() || undefined,
        drugLicense: form.drugLicense.trim() || undefined
      };

      if (editingId) {
        await updateSupplier(editingId, payload);
        setStatus(`Supplier updated: ${form.name.trim()}`);
      } else {
        await createSupplier(payload);
        setStatus(`Supplier created: ${form.name.trim()}`);
      }

      setForm(emptyForm);
      setEditingId('');
      await loadSuppliers();
    } catch (error) {
      setStatus(
        error?.response?.data?.message || (editingId ? 'Failed to update supplier' : 'Failed to create supplier')
      );
    }
  };

  const startEdit = (supplier) => {
    setEditingId(supplier.id);
    setForm({
      name: supplier.name || '',
      phone: supplier.phone || '',
      email: supplier.email || '',
      address: supplier.address || '',
      gstin: supplier.gstin || '',
      drugLicense: supplier.drugLicense || ''
    });
    setStatus(`Editing: ${supplier.name}`);
  };

  const cancelEdit = () => {
    setEditingId('');
    setForm(emptyForm);
    setStatus('Edit canceled');
  };

  const removeSupplier = async (supplier) => {
    const ok = window.confirm(`Delete supplier \"${supplier.name}\"?`);
    if (!ok) return;

    try {
      await deleteSupplier(supplier.id);
      if (editingId === supplier.id) {
        setEditingId('');
        setForm(emptyForm);
      }
      setStatus(`Supplier deleted: ${supplier.name}`);
      await loadSuppliers();
    } catch (error) {
      setStatus(error?.response?.data?.message || 'Failed to delete supplier');
    }
  };

  return (
    <section className="module-page suppliers-page">
      <h3>Suppliers</h3>
      <p className="small-row">Create and maintain supplier profiles for purchase workflows.</p>

      <form className="row-3" onSubmit={submit}>
        <input
          value={form.name}
          onChange={(e) => setForm((prev) => ({ ...prev, name: e.target.value }))}
          placeholder="Supplier Name *"
        />
        <input
          value={form.phone}
          onChange={(e) => setForm((prev) => ({ ...prev, phone: e.target.value }))}
          placeholder="Phone"
        />
        <input
          value={form.email}
          onChange={(e) => setForm((prev) => ({ ...prev, email: e.target.value }))}
          placeholder="Email"
        />
        <input
          value={form.address}
          onChange={(e) => setForm((prev) => ({ ...prev, address: e.target.value }))}
          placeholder="Address"
        />
        <input
          value={form.gstin}
          onChange={(e) => setForm((prev) => ({ ...prev, gstin: e.target.value }))}
          placeholder="GSTIN"
        />
        <input
          value={form.drugLicense}
          onChange={(e) => setForm((prev) => ({ ...prev, drugLicense: e.target.value }))}
          placeholder="Drug License"
        />
        <button type="submit">{editingId ? 'Update Supplier' : 'Create Supplier'}</button>
        {editingId && (
          <button type="button" className="secondary-btn" onClick={cancelEdit}>
            Cancel
          </button>
        )}
      </form>

      <h4>Supplier List</h4>
      {loading ? (
        <p>Loading suppliers...</p>
      ) : (
        <table className="table">
          <thead>
            <tr>
              <th>Name</th>
              <th>Phone</th>
              <th>Email</th>
              <th>GSTIN</th>
              <th>Drug License</th>
              <th>Address</th>
              <th>Action</th>
            </tr>
          </thead>
          <tbody>
            {rows.map((supplier) => (
              <tr key={supplier.id}>
                <td>{supplier.name}</td>
                <td>{supplier.phone || '-'}</td>
                <td>{supplier.email || '-'}</td>
                <td>{supplier.gstin || '-'}</td>
                <td>{supplier.drugLicense || '-'}</td>
                <td>{supplier.address || '-'}</td>
                <td>
                  <div className="action-group">
                    <button type="button" className="action-btn" onClick={() => startEdit(supplier)}>
                      Edit
                    </button>
                    <button type="button" className="action-btn" onClick={() => removeSupplier(supplier)}>
                      Delete
                    </button>
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}

      {status && <p className="small-row">{status}</p>}
    </section>
  );
}
