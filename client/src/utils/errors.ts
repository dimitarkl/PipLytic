import axios from "axios"

export const handleError = (err: unknown): string | null => {
    if (axios.isAxiosError(err) && err.response) {
        const data = err.response.data
        if (typeof data === 'string') {
            return data
        }
        if (typeof data === 'object' && data !== null) {
            if ('message' in data && typeof data.message === 'string') {
                return data.message
            }
            return JSON.stringify(data)
        }
        return 'An error occurred'
    } else
        return null
}