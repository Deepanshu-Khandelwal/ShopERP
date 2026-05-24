import { useEffect, useState } from 'react';
import { listBackupHistory, runBackup } from '../services/api';

export default function BackupModule() {
  const [destination, setDestination] = useState('');
  const [status, setStatus] = useState('');
  const [history, setHistory] = useState([]);
  const [loadingHistory, setLoadingHistory] = useState(false);

  const loadHistory = async () => {
    setLoadingHistory(true);
    try {
      const rows = await listBackupHistory();
      setHistory(rows);
    } catch {
      setHistory([]);
    } finally {
      setLoadingHistory(false);
    }
  };

  useEffect(() => {
    loadHistory();
  }, []);

  const latestBackup = history[0] || null;

  const openLatestBackupFolder = async () => {
    if (!latestBackup?.destination || !window.desktopInfo?.openPath) {
      setStatus('No backup folder available to open yet.');
      return;
    }

    const folderPath = latestBackup.destination.replace(/[\\/][^\\/]+$/, '');
    const opened = await window.desktopInfo.openPath(folderPath || latestBackup.destination);
    setStatus(opened ? `Opened: ${folderPath || latestBackup.destination}` : 'Could not open backup folder.');
  };

  const openLatestBackupFile = async () => {
    if (!latestBackup?.destination || !window.desktopInfo?.openPath) {
      setStatus('No backup file available to open yet.');
      return;
    }

    const opened = await window.desktopInfo.openPath(latestBackup.destination);
    setStatus(opened ? `Opened file: ${latestBackup.destination}` : 'Could not open backup file.');
  };

  const copyLatestBackupPath = async () => {
    if (!latestBackup?.destination) {
      setStatus('No backup path available to copy yet.');
      return;
    }

    try {
      await navigator.clipboard.writeText(latestBackup.destination);
      setStatus('Latest backup path copied to clipboard.');
    } catch {
      setStatus('Copy failed. Your system may block clipboard access.');
    }
  };

  const doBackup = async () => {
    try {
      const row = await runBackup(destination || undefined);
      setStatus(`Backup saved: ${row.destination}`);
      await loadHistory();
    } catch (error) {
      const details = error?.response?.data?.details || error?.response?.data?.message;
      setStatus(
        details ||
          'Backup failed. Install MySQL client tools or set MYSQLDUMP_PATH to the mysqldump executable.'
      );
    }
  };

  return (
    <section className="module-page backup-page">
      <h3>Backup</h3>
      <p className="small-row">Daily reminder: click backup to export a MySQL .sql file and keep a recent copy off-device.</p>
      <div className="row-3 backup-actions">
        <input
          value={destination}
          onChange={(e) => setDestination(e.target.value)}
          placeholder="Destination folder (optional)"
        />
        <button type="button" onClick={doBackup}>Backup Now</button>
        <button type="button" className="secondary-btn" onClick={openLatestBackupFolder}>Open Latest Folder</button>
        <button type="button" className="secondary-btn" onClick={openLatestBackupFile}>Open Latest File</button>
        <button type="button" className="secondary-btn" onClick={copyLatestBackupPath}>Copy Latest Path</button>
      </div>
      {status && <p className="small-row">{status}</p>}

      <div className="backup-history-panel">
        <div className="table-card-head">
          <h4>Recent Backups</h4>
          <p>{loadingHistory ? 'Loading...' : `${history.length} records`}</p>
        </div>
        {latestBackup && (
          <div className="backup-latest-note">
            <span>Latest file</span>
            <strong>{latestBackup.destination}</strong>
          </div>
        )}
        <div className="backup-history-list">
          {history.length ? (
            history.map((row) => (
              <div key={row.id} className={`backup-history-row backup-${String(row.status || '').toLowerCase()}`}>
                <div>
                  <strong>{row.status}</strong>
                  <p>{row.destination}</p>
                </div>
                <small>{row.createdAt ? new Date(row.createdAt).toLocaleString('en-GB') : '-'}</small>
              </div>
            ))
          ) : (
            <p className="small-row">No backup history yet.</p>
          )}
        </div>
      </div>
    </section>
  );
}
