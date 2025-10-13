import { UserContext } from "@/contexts/UserContext"
import { useContext, useEffect, useState } from "react"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import TradingHistory from "@/components/trading-history/TradingHistory"
import { api, API_URL } from "@/lib/api"
import { handleError } from "@/utils/errors"
import Error from "@/components/error/Error"

export type Trade = {
    amountFinal: number
    amountInvested: number
    endDate: number
    id: number
    startDate: number
    symbol: string
    type: string
    userId: string
    executedAt: number
}


export default function UserPage() {

    const { user } = useContext(UserContext)
    const [trades, setTrades] = useState<Trade[]>([])

    const [error, setError] = useState<string | null>(null)

    useEffect(() => {
        const getTrades = async () => {
            try {
                const response = await api.get(`${API_URL}/users/${user?.id}/trades`)
                setTrades(response.data)
            } catch (err) {
                const error = handleError(err) ?? "Failed Fetching Stocks Data"
                setError(error)
            }
        }
        getTrades()
    }, [])

    return (
        <div className="container mx-auto px-4 py-8 max-w-4xl">
            {/* User Profile Card */}
            <Card className="mb-8">
                <CardHeader>
                    <CardTitle>User Profile</CardTitle>
                </CardHeader>
                <CardContent>
                    <div className="flex items-start space-x-6">
                        {/* Profile Image */}
                        <div className="flex-shrink-0">
                            <div className="w-24 h-24 bg-gradient-to-br from-blue-400 to-purple-500 rounded-full flex items-center justify-center">
                                <span className="text-white text-2xl font-bold">
                                    {user?.email.charAt(0).toUpperCase()}
                                </span>
                            </div>
                        </div>

                        {/* User Info */}
                        <div className="flex-1 min-w-0">
                            <h2 className="text-2xl font-bold text-foreground mb-2">
                                {user?.email.split('@')[0]}
                            </h2>
                            <p className="text-muted-foreground mb-4">
                                {user?.email}
                            </p>
                            {/* <div className="flex flex-wrap gap-2">
                                <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-200">
                                    Active User
                                </span>
                                <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200">
                                    Member since 2025
                                </span>
                            </div> */}
                        </div>
                    </div>
                </CardContent>
            </Card>
            {error && <Error message={error} onClose={() => setError(null)} />}
            <TradingHistory trades={trades} />
        </div>
    )
}