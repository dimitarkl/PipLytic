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
        console.log(error)
        if (error.response.status === 401 && !originalRequest._retry) {
            originalRequest._retry = true; // Mark the request as retried to avoid infinite loops.
            try {
                // Make a request to your auth server to refresh the token.
                const response = await axios.post(`${API_URL}/auth/refresh`, {},
                    { withCredentials: true });

                const { accessToken: newAccessToken } = response.data;
                accessToken = newAccessToken;

                api.defaults.headers.common['Authorization'] = `Bearer ${accessToken}`;
                return api(originalRequest);
            } catch (refreshError) {

                accessToken = ""
                await api.post(`${API_URL}/auth/logout`, {}, { withCredentials: true })
                return Promise.reject(refreshError);
            }
        }
        return Promise.reject(error); // For all other errors, return the error as is.
    }
);