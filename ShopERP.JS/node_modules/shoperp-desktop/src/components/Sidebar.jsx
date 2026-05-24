const modules = [
  'Dashboard',
  'Billing (F1)',
  'Purchase (F2)',
  'Doctors',
  'Suppliers (F6)',
  'Returns (F7)',
  'Stock (F3)',
  'Ledger (F4)',
  'Reports (F5)',
  'Backup (F8)'
];

export default function Sidebar({ active, onSelect, open }) {
  const renderLabel = (value) => {
    const shortcutMatch = value.match(/\((F\d)\)/);
    if (!shortcutMatch) return <span>{value}</span>;

    const shortcut = shortcutMatch[1];
    const clean = value.replace(` (${shortcut})`, '');

    return (
      <>
        <span>{clean}</span>
        <kbd className="nav-kbd">{shortcut}</kbd>
      </>
    );
  };

  return (
    <aside className={`sidebar ${open ? 'open' : ''}`}>
      <div className="sidebar-head">
        <div>
          <h1 className="brand">ShopERP JS</h1>
          <p className="tag">Smart Retail Workspace</p>
        </div>
      </div>
      <nav>
        {modules.map((m) => (
          <button
            key={m}
            className={`nav-btn ${active === m ? 'active' : ''}`}
            onClick={() => onSelect(m)}
          >
            {renderLabel(m)}
          </button>
        ))}
      </nav>
    </aside>
  );
}
