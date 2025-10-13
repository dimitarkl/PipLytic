import axios from "axios"

export const handleError = (err: unknown): string | null => {
    if (axios.isAxiosError(err) && err.response) {
        return err.response.data.message

    } else
        return null
}