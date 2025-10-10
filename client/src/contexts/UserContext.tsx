import { api, setAccessToken } from "@/lib/api"
import { createContext, useEffect, useState } from "react"
import { AxiosResponse } from "axios"

type User = {
    id: string
    email: string,
    userType: number
} | null | undefined



type UserContext = {
    user: User,
    login: (email: string, password: string) => Promise<AxiosResponse<any, any>>,
    register: (email: string, password: string) => Promise<AxiosResponse<any, any>>,
    logout: () => Promise<void>
}
export const UserContext = createContext<UserContext>({
    user: undefined,
    login: async () => { throw new Error('Not implemented') },
    register: async () => { throw new Error('Not implemented') },
    logout: async () => { throw new Error('Not implemented') }
})
export const UserProvider = ({ children }: { children: React.ReactNode }) => {
    const [user, setUser] = useState<User>(undefined)

    const getUser = async () => {
        try {
            const response = await api.get('/users/me')
            if (response.data.user)
                setUser(response.data.user)
        } catch (err) {
            setUser(null)
        }
    }

    useEffect(() => {

        getUser()
    }, [])

    const login = async (email: string, password: string) => {
        const response = await api.post(`/auth/login`, {
            email,
            password
        })
        setAccessToken(response.data.accessToken)
        getUser()
        return response
    }
    const register = async (email: string, password: string) => {
        const response = await api.post(`/auth/register`, {
            email,
            password
        })
        setAccessToken(response.data.accessToken)
        getUser()
        return response
    }
    const logout = async () => {
        await api.post(`/auth/logout`)
        setAccessToken('')
        setUser(null)
    }
    return (
        <UserContext.Provider value={{ user, login, register, logout }}>
            {children}
        </UserContext.Provider>
    );
}