import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { Trade } from "@/pages/user-page/UserPage"


type TradingHistoryProps = {
    trades: Trade[]
}



const formatDate = (date: Date) => {
    return date.toLocaleDateString('en-US', {
        month: 'short',
        day: 'numeric',
        year: 'numeric',
        hour: '2-digit',
        minute: '2-digit'
    })
}

const formatCurrency = (amount: number) => {
    return new Intl.NumberFormat('en-US', {
        style: 'currency',
        currency: 'USD'
    }).format(amount)
}



const getTypeColor = (type: string) => {
    return type === 'long'
        ? 'text-green-600 dark:text-green-400'
        : 'text-red-600 dark:text-red-400'
}

const getPnLColor = (pnl?: number) => {
    if (!pnl) return 'text-muted-foreground'
    return pnl >= 0 ? 'text-green-600 dark:text-green-400' : 'text-red-600 dark:text-red-400'
}

export default function TradingHistory({ trades }: TradingHistoryProps) {
    const completedTrades = trades.filter(trade => trade.endDate > 0)

    const totalProfit = completedTrades
        .reduce((acc, trade) => acc + (trade.amountFinal - trade.amountInvested),0)

    const totalTrades = completedTrades.length

    return (
        <div className="space-y-6">
            {/* Trading Summary Cards */}
            <div className="grid grid-cols-3 md:grid-cols-3 gap-2 sm:gap-4">
                <Card>
                    <CardContent className="pt-3 pb-3 sm:pt-6 sm:pb-6 px-3 sm:px-6">
                        <div className="text-lg sm:text-2xl font-bold">{totalTrades}</div>
                        <p className="text-xs text-muted-foreground">Trades</p>
                    </CardContent>
                </Card>
                <Card>
                    <CardContent className="pt-3 pb-3 sm:pt-6 sm:pb-6 px-3 sm:px-6">
                        <div className={`text-lg sm:text-2xl font-bold ${totalProfit >= 0 ? 'text-green-600' : 'text-red-600'}`}>
                            {formatCurrency(totalProfit)}
                        </div>
                        <p className="text-xs text-muted-foreground">
                            {totalProfit >= 0 ? 'Profit' : 'Loss'}
                        </p>
                    </CardContent>
                </Card>
                <Card>
                    <CardContent className="pt-3 pb-3 sm:pt-6 sm:pb-6 px-3 sm:px-6">
                        <div className="text-lg sm:text-2xl font-bold">
                            {formatCurrency(completedTrades.reduce((acc, trade) =>
                                acc + trade.amountInvested, 0
                            ))}
                        </div>
                        <p className="text-xs text-muted-foreground">Invested</p>
                    </CardContent>
                </Card>
            </div>

            {/* Trading History Table */}
            <Card>
                <CardHeader>
                    <CardTitle>Recent Trades</CardTitle>
                </CardHeader>
                <CardContent>
                    <div className="overflow-x-auto">
                        <table className="w-full">
                            <thead>
                                <tr className="border-b">
                                    <th className="text-left py-2 px-2 font-medium text-muted-foreground">Symbol</th>
                                    <th className="text-left py-2 px-2 font-medium text-muted-foreground">Type</th>
                                    <th className="text-right py-2 px-2 font-medium text-muted-foreground">Invested</th>
                                    <th className="text-right py-2 px-2 font-medium text-muted-foreground">Final</th>
                                    <th className="text-right py-2 px-2 font-medium text-muted-foreground">P&L</th>
                                    <th className="text-left py-2 px-2 font-medium text-muted-foreground">Start Date</th>
                                </tr>
                            </thead>
                            <tbody>
                                {trades.map((trade) => {
                                    const pnl = trade.endDate > 0 ? trade.amountFinal - trade.amountInvested : 0

                                    return (
                                        <tr key={trade.id} className="border-b hover:bg-muted/50 transition-colors">
                                            <td className="py-3 px-2">
                                                <div className="font-medium">{trade.symbol}</div>
                                            </td>
                                            <td className="py-3 px-2">
                                                <span className={`font-medium uppercase ${getTypeColor(trade.type)}`}>
                                                    {trade.type}
                                                </span>
                                            </td>
                                            <td className="py-3 px-2 text-right font-medium">
                                                {formatCurrency(trade.amountInvested)}
                                            </td>
                                            <td className="py-3 px-2 text-right font-medium">
                                                {trade.endDate > 0 ? formatCurrency(trade.amountFinal) : '-'}
                                            </td>
                                            <td className="py-3 px-2 text-right font-medium">
                                                {trade.endDate > 0 ? (
                                                    <span className={getPnLColor(pnl)}>
                                                        {pnl >= 0 ? '+' : ''}{formatCurrency(pnl)}
                                                    </span>
                                                ) : (
                                                    '-'
                                                )}
                                            </td>
                                            <td className="py-3 px-2 text-sm text-muted-foreground">
                                                {formatDate(new Date(trade.executedAt * 1000))}
                                            </td>
                                        </tr>
                                    )
                                })}
                            </tbody>
                        </table>
                    </div>
                </CardContent>
            </Card>
        </div>
    )
}