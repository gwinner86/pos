import axios from 'axios';

declare global {
    interface Window {
        API_URL?: string;
        ENV?: {
            API_URL: string | null;
        };
    }
}

const getBaseURL = () => {
    if (typeof window !== 'undefined') {
        // 1. Check for Electron preload injected URL
        if (window.ENV?.API_URL) return window.ENV.API_URL;
        // 2. Check for manually injected URL
        if (window.API_URL) return window.API_URL;
    }
    // 3. Fallback to env var or default
    return process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5047';
};

const api = axios.create({
    baseURL: getBaseURL(),
    headers: {
        'Content-Type': 'application/json',
    },
});

// Request Interceptor: Attach Token
api.interceptors.request.use(
    (config) => {
        const token = typeof window !== 'undefined' ? localStorage.getItem('token') : null;
        if (token) {
            config.headers.Authorization = `Bearer ${token}`;
        }
        return config;
    },
    (error) => Promise.reject(error)
);

// Response Interceptor: Handle Errors
api.interceptors.response.use(
    (response) => response,
    (error) => {
        if (error.response?.status === 401) {
            // Check if we are not already on login page to avoid loops
            if (typeof window !== 'undefined' && !window.location.pathname.startsWith('/login')) {
                // Clear token and redirect
                localStorage.removeItem('token');
                window.location.href = '/login';
            }
        }
        return Promise.reject(error);
    }
);

export default api;
