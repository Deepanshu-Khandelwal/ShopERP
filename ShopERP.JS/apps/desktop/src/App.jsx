import { useEffect, useMemo, useState } from 'react';
import Sidebar from './components/Sidebar';
import DashboardPage from './pages/DashboardPage';
import LoginPage from './pages/LoginPage';
import ModulePage from './pages/ModulePage';
import { useSession } from './store/session';

export default function App() {
  const { token, user, login, logout } = useSession();
  const [active, setActive] = useState('Dashboard');
  const [menuOpen, setMenuOpen] = useState(false);
  const [theme, setTheme] = useState(() => localStorage.getItem('shoperp-theme') || 'ocean');

  useEffect(() => {
    const handler = (event) => {
      if (!token) return;
      const inBilling = active === 'Billing (F1)';

      if (event.key === 'F1') {
        event.preventDefault();
        setActive('Billing (F1)');
      } else if (event.key === 'F2' && !inBilling) {
        event.preventDefault();
        setActive('Purchase (F2)');
      } else if (event.key === 'F3' && !inBilling) {
        event.preventDefault();
        setActive('Stock (F3)');
      } else if (event.key === 'F4') {
        event.preventDefault();
        setActive('Ledger (F4)');
      } else if (event.key === 'F5') {
        event.preventDefault();
        setActive('Reports (F5)');
      } else if (event.key === 'F6') {
        event.preventDefault();
        setActive('Suppliers (F6)');
      } else if (event.key === 'F7') {
        event.preventDefault();
        setActive('Returns (F7)');
      } else if (event.key === 'F8') {
        event.preventDefault();
        setActive('Backup (F8)');
      }
    };

    window.addEventListener('keydown', handler);
    return () => window.removeEventListener('keydown', handler);
  }, [token, active]);

  const body = useMemo(() => {
    if (active === 'Dashboard') return <DashboardPage />;
    return <ModulePage moduleName={active} />;
  }, [active]);

  useEffect(() => {
    setMenuOpen(false);
  }, [active]);

  useEffect(() => {
    document.documentElement.setAttribute('data-theme', theme);
    localStorage.setItem('shoperp-theme', theme);
  }, [theme]);

  if (!token) {
    return <LoginPage onLoggedIn={login} />;
  }

  return (
    <main className="app-grid">
      <div
        className={`sidebar-backdrop ${menuOpen ? 'open' : ''}`}
        onClick={() => setMenuOpen(false)}
      />
      <Sidebar
        active={active}
        onSelect={setActive}
        open={menuOpen}
        onClose={() => setMenuOpen(false)}
      />
      <section className="content-shell">
        <header className="topbar">
          <div className="topbar-main">
            <button className="mobile-menu-btn" onClick={() => setMenuOpen(true)}>
              Menu
            </button>
            <div>
              <h2>{active}</h2>
              <small>
                Logged in as {user?.displayName || user?.username} ({user?.role})
              </small>
            </div>
          </div>
          <div className="topbar-actions">
            <label className="theme-picker" htmlFor="theme-select">
              Theme
            </label>
            <select
              id="theme-select"
              className="theme-select"
              value={theme}
              onChange={(e) => setTheme(e.target.value)}
            >
              <option value="ocean">Ocean</option>
              <option value="sunset">Sunset</option>
              <option value="midnight">Midnight</option>
            </select>
            <button className="logout" onClick={logout}>
              Logout
            </button>
          </div>
        </header>
        <section className="surface-panel">{body}</section>
      </section>
    </main>
  );
}
