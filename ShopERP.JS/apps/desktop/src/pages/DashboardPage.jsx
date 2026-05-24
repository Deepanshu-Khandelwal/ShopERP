import { useEffect, useState } from 'react';
import { getDashboardSummary, listAlerts, runStartupAlertCheck } from '../services/api';

export default function DashboardPage() {
  const [state, setState] = useState({ loading: true, data: null, error: '', alerts: [] });

  useEffect(() => {
    let mounted = true;
    Promise.all([runStartupAlertCheck().catch(() => null), getDashboardSummary(), listAlerts()])
      .then(([, data, alerts]) => {
        if (mounted) setState({ loading: false, data, error: '', alerts: alerts.slice(0, 5) });
      })
      .catch((err) => {
        if (mounted) {
          setState({
            loading: false,
            data: null,
            alerts: [],
            error: err?.response?.data?.message || 'Failed to load dashboard'
          });
        }
      });

    return () => {
      mounted = false;
    };
  }, []);

  if (state.loading) {
    return (
      <section className="dashboard-stack">
        <article className="hero-card skeleton-block" />
        <section className="cards">
          <article className="card metric-card skeleton-card" />
          <article className="card metric-card skeleton-card" />
          <article className="card metric-card skeleton-card" />
          <article className="card metric-card skeleton-card" />
        </section>
        <article className="card alerts-card skeleton-card" />
      </section>
    );
  }
  if (state.error) return <p className="error">{state.error}</p>;

  return (
    <section className="dashboard-stack">
      <article className="hero-card stagger-item">
        <h3>Store Health Snapshot</h3>
        <p>Track sales velocity, stock risk, and active alerts from one place.</p>
      </article>

      <section className="cards">
        <article className="card metric-card metric-sales stagger-item">
          <h3>Today Sales</h3>
          <p>{state.data.todaySales.toFixed(2)}</p>
        </article>
        <article className="card metric-card metric-profit stagger-item">
          <h3>Profit Today</h3>
          <p>{state.data.profitToday.toFixed(2)}</p>
        </article>
        <article className="card metric-card metric-stock stagger-item">
          <h3>Low Stock Count</h3>
          <p>{state.data.lowStockCount}</p>
        </article>
        <article className="card metric-card metric-alert stagger-item">
          <h3>Expiry Alerts Count</h3>
          <p>{state.data.expiryAlertsCount}</p>
        </article>
      </section>

      {state.alerts.length > 0 && (
        <article className="card alerts-card stagger-item">
          <h3>Recent Alerts</h3>
          <div className="alert-list">
            {state.alerts.map((alert) => (
              <p key={alert.id} className="small-row">
                <strong>{alert.title}</strong>: {alert.message}
              </p>
            ))}
          </div>
        </article>
      )}

      {state.data?.topSellingProducts?.length > 0 && (
        <article className="card alerts-card stagger-item">
          <h3>Top Selling Products (Today)</h3>
          <div className="alert-list">
            {state.data.topSellingProducts.map((row) => (
              <p key={row.productId} className="small-row">
                <strong>{row.name}</strong>: {Number(row.qty || 0)} qty
              </p>
            ))}
          </div>
        </article>
      )}
    </section>
  );
}
