const { app, BrowserWindow } = require('electron');
const path = require('path');
const { spawn } = require('child_process');

let apiProcess = null;
let ownsApiProcess = false;

function startApiProcess() {
  const apiDir = path.resolve(__dirname, '..', '..', 'api');
  apiProcess = spawn(process.execPath, ['src/server.js'], {
    cwd: apiDir,
    stdio: 'inherit',
    env: process.env
  });
  ownsApiProcess = true;

  apiProcess.on('exit', (code) => {
    if (code !== 0) {
      console.error(`API process exited with code ${code}`);
    }
  });
}

async function waitForApi(maxAttempts = 50) {
  for (let i = 0; i < maxAttempts; i += 1) {
    try {
      const response = await fetch('http://localhost:5050/health');
      if (response.ok) return true;
    } catch {
      // Retry until timeout.
    }

    await new Promise((resolve) => setTimeout(resolve, 300));
  }

  return false;
}

function createWindow() {
  const win = new BrowserWindow({
    width: 1366,
    height: 860,
    minWidth: 1024,
    minHeight: 720,
    backgroundColor: '#f5f4ef',
    webPreferences: {
      preload: path.join(__dirname, 'preload.cjs'),
      contextIsolation: true,
      nodeIntegration: false
    }
  });

  win.loadURL('http://localhost:5173');
}

app.whenReady().then(async () => {
  const apiAlreadyRunning = await waitForApi(2);
  if (apiAlreadyRunning) {
    console.log('API already running on port 5050. Reusing existing process.');
  } else {
    startApiProcess();
  }

  const apiReady = await waitForApi();
  if (!apiReady) {
    console.error('API startup timed out.');
  }

  createWindow();
  app.on('activate', () => {
    if (BrowserWindow.getAllWindows().length === 0) createWindow();
  });
});

app.on('window-all-closed', () => {
  if (ownsApiProcess && apiProcess && !apiProcess.killed) {
    apiProcess.kill();
  }
  if (process.platform !== 'darwin') app.quit();
});
