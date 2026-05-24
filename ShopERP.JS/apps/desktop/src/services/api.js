import axios from 'axios';
import { useSession } from '../store/session';

export const api = axios.create({
  baseURL: 'http://localhost:5050/api'
});

api.interceptors.request.use((config) => {
  const token = useSession.getState().token;
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

export async function login(username, password) {
  const { data } = await api.post('/auth/login', { username, password });
  return data;
}

export async function registerShop(payload) {
  const { data } = await api.post('/auth/shop/create', payload);
  return data;
}

export async function getDashboardSummary() {
  const { data } = await api.get('/reports/dashboard');
  return data;
}

export async function runStartupAlertCheck() {
  const { data } = await api.post('/notifications/check', {});
  return data;
}

export async function listAlerts() {
  const { data } = await api.get('/notifications');
  return data;
}

export async function searchProducts(query) {
  const { data } = await api.get('/products/search', { params: { q: query } });
  return data;
}

export async function createSale(payload) {
  const { data } = await api.post('/sales', payload);
  return data;
}

export async function listSales() {
  const { data } = await api.get('/sales');
  return data;
}

export async function getSaleById(id) {
  const { data } = await api.get(`/sales/${id}`);
  return data;
}

export async function updateSale(id, payload) {
  const { data } = await api.put(`/sales/${id}`, payload);
  return data;
}

export async function createPurchase(payload) {
  const { data } = await api.post('/purchases', payload);
  return data;
}

export async function listPurchases() {
  const { data } = await api.get('/purchases');
  return data;
}

export async function createSalesReturn(payload) {
  const { data } = await api.post('/returns/sales', payload);
  return data;
}

export async function createPurchaseReturn(payload) {
  const { data } = await api.post('/returns/purchase', payload);
  return data;
}

export async function createProductEntry(payload) {
  const { data } = await api.post('/products/direct-entry', payload);
  return data;
}

export async function listSuppliers() {
  const { data } = await api.get('/suppliers');
  return data;
}

export async function createSupplier(payload) {
  const { data } = await api.post('/suppliers', payload);
  return data;
}

export async function updateSupplier(id, payload) {
  const { data } = await api.put(`/suppliers/${id}`, payload);
  return data;
}

export async function deleteSupplier(id) {
  await api.delete(`/suppliers/${id}`);
}

export async function listProducts() {
  const { data } = await api.get('/products');
  return data;
}

export async function updateProductStock(id, stockQty) {
  const { data } = await api.patch(`/products/${id}/stock`, { stockQty });
  return data;
}

export async function getLowStock() {
  const { data } = await api.get('/products/low-stock');
  return data;
}

export async function getExpiring() {
  const { data } = await api.get('/products/expiry');
  return data;
}

export async function getCustomerLedger(customerName) {
  const { data } = await api.get('/ledgers/customer', { params: { customerName } });
  return data;
}

export async function addPayment(payload) {
  const { data } = await api.post('/payments', payload);
  return data;
}

export async function getProfitReport(from, to) {
  const { data } = await api.get('/reports/profit', { params: { from, to } });
  return data;
}

export async function listDoctors() {
  const { data } = await api.get('/reports/doctors');
  return data;
}

export async function runBackup(destination) {
  const { data } = await api.post('/backup/run', { destination });
  return data;
}

export async function listBackupHistory() {
  const { data } = await api.get('/backup/history');
  return data;
}
