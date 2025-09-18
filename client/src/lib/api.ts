import axios from 'axios'

export const API_URL = 'http://localhost:5011/api'
export const api = axios.create({
    headers: {
        'Content-Type': 'application/json',
    },
    withCredentials: true
});

// Ensure axios instance targets the API base URL by default
api.defaults.baseURL = API_URL;

let accessToken = ""
let refreshTokenPromise: Promise<string> | null = null

export const setAccessToken = (token: string) => accessToken = token;

//Intercept Requests and add token to them
api.interceptors.request.use(request => {
    if (accessToken) {
        request.headers['Authorization'] = `Bearer ${accessToken}`;
    }
    return request;
}, error => {
    return Promise.reject(error);
});

api.interceptors.response.use(
    response => response,
    async error => {
        const originalRequest = error.config;
        if (error.response.status === 401 && !originalRequest._retry) {
            originalRequest._retry = true;
            
            try {
                // If there's already a refresh in progress, wait for it
                if (refreshTokenPromise) {
                    const newAccessToken = await refreshTokenPromise;
                    accessToken = newAccessToken;
                    api.defaults.headers.common['Authorization'] = `Bearer ${accessToken}`;
                    return api(originalRequest);
                }
                
                // Start a new refresh token request
                refreshTokenPromise = axios.post(`${API_URL}/auth/refresh-token`, {},
                    { withCredentials: true })
                    .then(response => {
                        const { accessToken: newAccessToken } = response.data;
                        return newAccessToken;
                    });

                const newAccessToken = await refreshTokenPromise;
                accessToken = newAccessToken;
                refreshTokenPromise = null; 

                api.defaults.headers.common['Authorization'] = `Bearer ${accessToken}`;
                return api(originalRequest);
            } catch (refreshError) {
                refreshTokenPromise = null; 

                accessToken = ""
                await api.post(`${API_URL}/auth/logout`, {}, { withCredentials: true })
                return Promise.reject(refreshError);
            }
        }
        return Promise.reject(error); 
    }
);