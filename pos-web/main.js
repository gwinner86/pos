const { app, BrowserWindow, dialog } = require('electron');
const { spawn, fork } = require('child_process');
const path = require('path');
const net = require('net');
const fs = require('fs');

let nextServerProcess;
let backendProcess;
let mainWindow;

// ─────────────────────────────────────────────
// Logging Utility
// ─────────────────────────────────────────────
const logFilePath = path.join(app.getPath('userData'), 'app.log');
const logStream = fs.createWriteStream(logFilePath, { flags: 'a' });

function log(message) {
  const timestamp = new Date().toISOString();
  const formattedMessage = `[${timestamp}] ${message}\n`;
  console.log(message);
  logStream.write(formattedMessage);
}

log(`--- Application Starting (Version: ${app.getVersion()}) ---`);
log(`User Data Path: ${app.getPath('userData')}`);
log(`App Path: ${app.getAppPath()}`);
log(`Resources Path: ${process.resourcesPath}`);

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
function waitForPort(port, host = '127.0.0.1', retries = 60, delay = 500) {
  return new Promise((resolve, reject) => {
    const attempt = (remaining) => {
      if (remaining === 0) return reject(new Error(`Port ${port} never opened on ${host}`));
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
  let backendDir;

  if (isDev) {
    backendDir = path.join(__dirname, 'backend-dist');
    backendExe = path.join(backendDir, 'POS.API');
  } else {
    backendDir = path.join(
      process.resourcesPath,
      'app.asar.unpacked',
      'backend-dist'
    );
    backendExe = path.join(backendDir, 'POS.API');
  }

  log(`[Backend] Starting on port ${apiPort}...`);
  log(`[Backend] Executable: ${backendExe}`);
  log(`[Backend] Working Directory: ${backendDir}`);

  if (!fs.existsSync(backendExe)) {
    throw new Error(`Backend executable not found at: ${backendExe}`);
  }

  // Read connection string from appropriate appsettings file
  let connectionString = '';
  const env = isDev ? 'Development' : 'Production';
  
  try {
    const primaryConfigPath = path.join(backendDir, `appsettings.${env}.json`);
    const fallbackConfigPath = path.join(backendDir, 'appsettings.json');
    const configPath = fs.existsSync(primaryConfigPath) ? primaryConfigPath : fallbackConfigPath;
    
    if (fs.existsSync(configPath)) {
      const config = JSON.parse(fs.readFileSync(configPath, 'utf8'));
      connectionString = config.ConnectionStrings?.DefaultConnection || '';
      
      // Resolve the data path
      let dataPath;
      if (isDev) {
        dataPath = path.join(__dirname, '..'); // Project root in dev
      } else {
        dataPath = process.resourcesPath; // Resources folder in production
      }

      // Replace placeholder with actual path
      if (connectionString.includes('{DATA_PATH}')) {
        connectionString = connectionString.replace(/{DATA_PATH}/g, dataPath);
      }

      // macOS Fallback: If on Mac and connection string uses LocalDB, switch to Docker SQL Server
      if (process.platform === 'darwin' && connectionString.includes('(localdb)')) {
        log(`[Backend] macOS detected with LocalDB connection string. Switching to Docker SQL Server fallback.`);
        connectionString = "Server=127.0.0.1,1434;Database=POS_DB;User Id=sa;Password=Password123!;TrustServerCertificate=True;MultipleActiveResultSets=true";
      }
      
      log(`[Backend] Using connection string: ${connectionString}`);
      log(`[Backend] Environment: ${env}`);
    }
  } catch (err) {
    log(`[Backend] Warning: Could not read appsettings.json: ${err.message}`);
  }

  // Ensure executable permissions on Unix systems
  if (process.platform !== 'win32') {
    try {
      fs.chmodSync(backendExe, '755');
    } catch (err) {
      log(`[Backend] Warning: Could not set executable permissions: ${err.message}`);
    }
  }

  backendProcess = spawn(backendExe, [], {
    cwd: backendDir,
    env: {
      ...process.env,
      ASPNETCORE_URLS: `http://127.0.0.1:${apiPort}`,
      ASPNETCORE_ENVIRONMENT: env,
      ConnectionStrings__DefaultConnection: connectionString,
    },
    stdio: ['ignore', 'pipe', 'pipe'],
  });

  backendProcess.stdout.on('data', (data) => log(`[Backend STDOUT] ${data}`));
  backendProcess.stderr.on('data', (data) => log(`[Backend STDERR] ${data}`));

  backendProcess.on('error', (err) => {
    log(`[Backend] Failed to start: ${err.message}`);
  });

  backendProcess.on('exit', (code) => {
    log(`[Backend] Exited with code ${code}`);
  });

  await waitForPort(apiPort);
  log(`[Backend] Ready on http://127.0.0.1:${apiPort}`);
}

// ─────────────────────────────────────────────
// Boot the Next.js Standalone Server (Production only)
// ─────────────────────────────────────────────
async function startNextServer(frontendPort, apiPort) {
  const standalonePath = path.join(
    process.resourcesPath,
    'app.asar.unpacked',
    '.next',
    'standalone'
  );
  const serverPath = path.join(standalonePath, 'server.js');
  const nodeModulesPath = path.join(standalonePath, 'node_modules');

  log(`[Next.js] Starting on port ${frontendPort}...`);
  log(`[Next.js] Standalone path: ${standalonePath}`);
  log(`[Next.js] Server path: ${serverPath}`);

  if (!fs.existsSync(serverPath)) {
    throw new Error(`Next.js server.js not found at: ${serverPath}`);
  }

  // Use spawn with ELECTRON_RUN_AS_NODE to avoid module resolution issues
  nextServerProcess = spawn(process.execPath, [serverPath], {
    cwd: standalonePath,
    stdio: ['ignore', 'pipe', 'pipe'],
    env: {
      ...process.env,
      ELECTRON_RUN_AS_NODE: '1',
      PORT: String(frontendPort),
      NODE_ENV: 'production',
      HOSTNAME: '127.0.0.1',
      NEXT_PUBLIC_API_URL: `http://127.0.0.1:${apiPort}`,
      NODE_PATH: nodeModulesPath, // Explicitly set NODE_PATH
    },
  });

  nextServerProcess.stdout.on('data', (data) => log(`[Next.js STDOUT] ${data}`));
  nextServerProcess.stderr.on('data', (data) => log(`[Next.js STDERR] ${data}`));

  nextServerProcess.on('error', (err) => {
    log(`[Next.js] Failed to start: ${err.message}`);
  });

  await waitForPort(frontendPort);
  log(`[Next.js] Ready on http://127.0.0.1:${frontendPort}`);
}

// ─────────────────────────────────────────────
// Create the Electron Window
// ─────────────────────────────────────────────
async function createWindow() {
  const isDev = !app.isPackaged;
  const apiPort = isDev ? 5047 : await findFreePort(5100, 5999);
  const frontendPort = isDev ? 3000 : await findFreePort(3100, 3999);

  // Set environment variable for preload to pick up
  process.env.DYNAMIC_API_URL = `http://127.0.0.1:${apiPort}`;

  mainWindow = new BrowserWindow({
    width: 1280,
    height: 800,
    minWidth: 1024,
    minHeight: 700,
    title: 'POS System',
    webPreferences: {
      nodeIntegration: false,
      contextIsolation: true,
      sandbox: false, 
      preload: path.join(__dirname, 'preload.js'),
    },
  });

  mainWindow.setMenuBarVisibility(false);

  if (isDev) {
    log(`[Electron] Dev mode: connecting to http://127.0.0.1:${frontendPort}...`);
    mainWindow.webContents.openDevTools();
    await waitForPort(frontendPort).catch(() => {
      log(`[Electron] Warning: Dev server port ${frontendPort} not responding.`);
    });
    mainWindow.loadURL(`http://127.0.0.1:${frontendPort}`);
  } else {
    try {
      log(`[Electron] Starting production services...`);
      await startBackend(apiPort);
      await startNextServer(frontendPort, apiPort);

      log(`[Electron] Loading URL: http://127.0.0.1:${frontendPort}`);
      mainWindow.loadURL(`http://127.0.0.1:${frontendPort}`);
      
      // Keep devtools open in production for now to see client-side errors
      mainWindow.webContents.openDevTools();

      mainWindow.webContents.on('did-fail-load', (event, errorCode, errorDescription) => {
        log(`[Electron] Failed to load URL: ${errorCode} ${errorDescription}`);
      });

    } catch (err) {
      log(`[Electron] Critical startup error: ${err.stack}`);
      dialog.showErrorBox('Startup Error', `Failed to start application services:\n${err.message}`);
    }
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
  log('[Electron] Shutting down background processes...');
  if (nextServerProcess) nextServerProcess.kill();
  if (backendProcess) backendProcess.kill();
  logStream.end();
});

