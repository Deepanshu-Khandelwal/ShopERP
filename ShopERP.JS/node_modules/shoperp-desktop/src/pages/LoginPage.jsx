import { useState } from 'react';
import { login, registerShop } from '../services/api';

function extractErrorMessage(err, fallback) {
  const data = err?.response?.data;
  if (!data) return fallback;
  if (typeof data.message === 'string' && data.message.trim()) return data.message;

  const fieldErrors = data?.fieldErrors;
  if (fieldErrors && typeof fieldErrors === 'object') {
    const messages = Object.values(fieldErrors)
      .flat()
      .filter(Boolean);
    if (messages.length > 0) return String(messages[0]);
  }

  const formErrors = data?.formErrors;
  if (Array.isArray(formErrors) && formErrors.length > 0) {
    return String(formErrors[0]);
  }

  return fallback;
}

export default function LoginPage({ onLoggedIn }) {
  const [mode, setMode] = useState('login');
  const [shopName, setShopName] = useState('Medical Store');
  const [displayName, setDisplayName] = useState('');
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [error, setError] = useState('');
  const [message, setMessage] = useState('');
  const [busy, setBusy] = useState(false);

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (mode === 'register' && password !== confirmPassword) {
      setError('Password and confirm password must match');
      setMessage('');
      return;
    }

    setBusy(true);
    setError('');
    setMessage('');
    try {
      if (mode === 'login') {
        const result = await login(username, password);
        onLoggedIn(result);
      } else {
        await registerShop({
          shopName,
          username,
          password,
          displayName: displayName || undefined
        });
        setMode('login');
        setConfirmPassword('');
        setMessage('Registration successful. Please login with your new account.');
      }
    } catch (err) {
      setError(extractErrorMessage(err, mode === 'login' ? 'Login failed' : 'Registration failed'));
    } finally {
      setBusy(false);
    }
  };

  return (
    <main className="login-wrap">
      <section className="login-card">
        <aside className="login-brand">
          <h2>ShopERP Desktop</h2>
          <p>
            One smart workspace for billing, purchasing, stock tracking, ledgers, and daily
            reporting.
          </p>
          <span className="pill">Secure Local Access</span>
        </aside>

        <section className="login-form-shell">
          <h3>{mode === 'login' ? 'Welcome Back' : 'Create Your Shop'}</h3>
          <p>{mode === 'login' ? 'Sign in to continue' : 'Register a new shop account'}</p>

          <form onSubmit={handleSubmit} className="login-form">
            {mode === 'register' && (
              <>
                <input
                  value={shopName}
                  onChange={(e) => setShopName(e.target.value)}
                  placeholder="Shop Name"
                />
                <input
                  value={displayName}
                  onChange={(e) => setDisplayName(e.target.value)}
                  placeholder="Display Name (optional)"
                />
              </>
            )}
            <input
              value={username}
              onChange={(e) => setUsername(e.target.value)}
              placeholder="Username"
            />
            <input
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              type="password"
              placeholder="Password"
            />
            {mode === 'register' && (
              <input
                value={confirmPassword}
                onChange={(e) => setConfirmPassword(e.target.value)}
                type="password"
                placeholder="Confirm Password"
              />
            )}
            <button disabled={busy}>
              {busy
                ? mode === 'login'
                  ? 'Signing in...'
                  : 'Registering...'
                : mode === 'login'
                  ? 'Login'
                  : 'Register'}
            </button>
          </form>

          <div className="login-actions">
            {mode === 'login' ? (
              <button
                type="button"
                className="secondary-btn"
                onClick={() => {
                  setMode('register');
                  setConfirmPassword('');
                  setError('');
                  setMessage('');
                }}
              >
                Registration
              </button>
            ) : (
              <button
                type="button"
                className="secondary-btn"
                onClick={() => {
                  setMode('login');
                  setConfirmPassword('');
                  setError('');
                  setMessage('');
                }}
              >
                Back to Login
              </button>
            )}
          </div>

          {error && <small className="error">{error}</small>}
          {message && <small className="success">{message}</small>}
        </section>
      </section>
    </main>
  );
}
