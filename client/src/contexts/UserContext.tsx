import { api, setAccessToken } from "@/lib/api"
import { createContext, useEffect, useState } from "react"

type User = {
    id: string
    email: string,
} | null | undefined
type UserContext = {
    user: User,
    login: (email: string, password: string) => Promise<void>,
    register: (email: string, password: string) => Promise<void>,
    logout: () => Promise<void>
}
export const UserContext = createContext<UserContext>({
    user: undefined,
    login: () => { throw new Error('Not implemented') },
    register: () => { throw new Error('Not implemented') },
    logout: () => { throw new Error('Not implemented') }
})
export const UserProvider = ({ children }: { children: React.ReactNode }) => {
    const [user, setUser] = useState<User>(undefined)

    useEffect(() => {
        const getUser = async () => {

            try {
                const response = await api.get('/users/me')
                if (response.data.user)
                    setUser(response.data)
                    
            } catch (err) {
                setUser(null)
            }
        }
        getUser()
    }, [])

    const login = async (email: string, password: string) => {
        const response = await api.post(`/auth/login`, {
            email,
            password
        })
        setAccessToken(response.data)
        //TODO FIX With ACC user
        setUser(response.data)
    }
    const register = async (email: string, password: string) => {
        const response = await api.post(`/auth/register`, {
            email,
            password
        })
        setAccessToken(response.data)
        //TODO FIX With ACC user
        setUser(response.data)
    }
    const logout = async () => {
        await api.post(`/auth/logout`)
        setAccessToken('')
    }
    return (
        <UserContext.Provider value={{ user, login, register, logout }}>
            {children}
        </UserContext.Provider>
    );
}