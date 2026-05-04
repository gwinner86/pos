const { contextBridge } = require('electron');

// Expose API_URL to the frontend securely
// We read it from the environment variable set in main.js
const apiUrl = process.env.DYNAMIC_API_URL;

contextBridge.exposeInMainWorld('ENV', {
  API_URL: apiUrl || null
});
