const { app, BrowserWindow } = require('electron');
const { spawn, fork } = require('child_process');
const path = require('path');
const net = require('net');

let nextServerProcess;
let backendProcess;
let mainWindow;

// ─────────────────────────────────────────────
// Utility: Find a free TCP port in a given range
// ─────────────────────────────────────────────
function findFreePort(from = 3000, to = 9999) {
  return new Promise((resolve, reject) => {
    const tryPort = (port) => {
      if (port > to) return reject(new Error('No free port found'));
      const server = net.createServer();
      server.listen(port, '127.0.0.1', () => {
        server.close(() => resolve(port));
      });
      server.on('error', () => tryPort(port + 1));
    };
    tryPort(from);
  });
}

// ─────────────────────────────────────────────
// Utility: Wait until a TCP port is accepting connections
// ─────────────────────────────────────────────
function waitForPort(port, host = '127.0.0.1', retries = 40, delay = 500) {
  return new Promise((resolve, reject) => {
    const attempt = (remaining) => {
      if (remaining === 0) return reject(new Error(`Port ${port} never opened`));
      const sock = new net.Socket();
      sock.setTimeout(delay);
      sock.connect(port, host, () => {
        sock.destroy();
        resolve();
      });
      sock.on('error', () => {
        sock.destroy();
        setTimeout(() => attempt(remaining - 1), delay);
      });
      sock.on('timeout', () => {
        sock.destroy();
        setTimeout(() => attempt(remaining - 1), delay);
      });
    };
    attempt(retries);
  });
}

// ─────────────────────────────────────────────
// Boot the .NET Backend API
// ─────────────────────────────────────────────
async function startBackend(apiPort) {
  const isDev = !app.isPackaged;

  let backendExe;

  if (isDev) {
    // In dev mode, assume dotnet is on PATH and run the published DLL
    const backendDistPath = path.join(__dirname, 'backend-dist');
    backendExe = path.join(backendDistPath, 'POS.API');
  } else {
    // In packaged production, the backend-dist folder is bundled as an extraResource
    backendExe = path.join(
      process.resourcesPath,
      'app.asar.unpacked',
      'backend-dist',
      'POS.API'
    );
  }

  console.log(`[Electron] Starting .NET backend on port ${apiPort}...`);
  console.log(`[Electron] Backend executable: ${backendExe}`);

  backendProcess = spawn(backendExe, [], {
    env: {
      ...process.env,
      ASPNETCORE_URLS: `http://127.0.0.1:${apiPort}`,
      ASPNETCORE_ENVIRONMENT: 'Production',
      // Forward the SQL Server connection string from the host environment if set
      ConnectionStrings__DefaultConnection: process.env.ConnectionStrings__DefaultConnection || '',
    },
    stdio: isDev ? 'inherit' : 'ignore',
  });

  backendProcess.on('error', (err) => {
    console.error('[Electron] Failed to start backend:', err);
  });

  backendProcess.on('exit', (code) => {
    console.log(`[Electron] Backend exited with code ${code}`);
  });

  // Wait until the backend HTTP port is actually open
  await waitForPort(apiPort);
  console.log(`[Electron] Backend is ready on http://127.0.0.1:${apiPort}`);
}

// ─────────────────────────────────────────────
// Boot the Next.js Standalone Server (Production only)
// ─────────────────────────────────────────────
async function startNextServer(frontendPort, apiPort) {
  const serverPath = path.join(
    process.resourcesPath,
    'app.asar.unpacked',
    '.next',
    'standalone',
    'server.js'
  );

  console.log(`[Electron] Starting Next.js standalone server on port ${frontendPort}...`);

  nextServerProcess = fork(serverPath, [], {
    env: {
      ...process.env,
      PORT: String(frontendPort),
      NODE_ENV: 'production',
      HOSTNAME: '127.0.0.1',
      // Inject the dynamic API URL so the frontend calls the local backend
      NEXT_PUBLIC_API_URL: `http://127.0.0.1:${apiPort}`,
    },
  });

  nextServerProcess.on('error', (err) => {
    console.error('[Electron] Failed to start Next.js server:', err);
  });

  await waitForPort(frontendPort);
  console.log(`[Electron] Next.js server is ready on http://127.0.0.1:${frontendPort}`);
}

// ─────────────────────────────────────────────
// Create the Electron Window
// ─────────────────────────────────────────────
async function createWindow() {
  const isDev = !app.isPackaged;

  mainWindow = new BrowserWindow({
    width: 1280,
    height: 800,
    minWidth: 1024,
    minHeight: 700,
    title: 'POS System',
    webPreferences: {
      nodeIntegration: false,
      contextIsolation: true,
      sandbox: true,
    },
  });

  // Hide the menu bar for a cleaner desktop feel
  mainWindow.setMenuBarVisibility(false);

  if (isDev) {
    // Dev Mode: Next.js and .NET are started externally via npm run dev / dotnet run
    console.log('[Electron] Dev mode: connecting to http://localhost:3000...');
    await waitForPort(3000).catch(() => {});
    mainWindow.loadURL('http://localhost:3000');
  } else {
    // Production Mode: Boot backend → boot frontend → open window
    const apiPort = await findFreePort(5100, 5999);
    const frontendPort = await findFreePort(3100, 3999);

    await startBackend(apiPort);
    await startNextServer(frontendPort, apiPort);

    mainWindow.loadURL(`http://127.0.0.1:${frontendPort}`);
  }
}

// ─────────────────────────────────────────────
// Electron App Lifecycle
// ─────────────────────────────────────────────
app.whenReady().then(createWindow);

app.on('window-all-closed', () => {
  if (process.platform !== 'darwin') app.quit();
});

app.on('activate', () => {
  if (BrowserWindow.getAllWindows().length === 0) createWindow();
});

app.on('will-quit', () => {
  console.log('[Electron] Shutting down background processes...');
  if (nextServerProcess) nextServerProcess.kill();
  if (backendProcess) backendProcess.kill();
});
