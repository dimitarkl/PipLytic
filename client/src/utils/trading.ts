import { InvestPoint } from "@/pages/home/Home"
import { ChartData } from "@/types/chartData"

export const calculateCurrentValue = (investPoint?: InvestPoint, chartData?: ChartData[]) => {
    if (!investPoint || !chartData || chartData.length === 0) return undefined

    const investPrice = investPoint.point.close
    if (investPrice === 0) return undefined

    const currentPrice = chartData[chartData.length - 1].close
    const investedAmount = investPoint.amount

    const value = investPoint.type === 'long'
        ? investedAmount * (currentPrice / investPrice)
        : investedAmount * (2 - currentPrice / investPrice)

    return Number(value.toFixed(2))
}

export const calculateProfitLoss = (investPoint?: InvestPoint, chartData?: ChartData[]) => {
    if (!investPoint || !chartData || chartData.length === 0) return undefined

    const investPrice = investPoint.point.close
    if (investPrice === 0) return undefined

    const currentPrice = chartData[chartData.length - 1].close

    const profitLoss = investPoint.type === 'long'
        ? ((currentPrice - investPrice) / investPrice) * investPoint.amount
        : ((investPrice - currentPrice) / investPrice) * investPoint.amount

    return Number(profitLoss.toFixed(2))
}

