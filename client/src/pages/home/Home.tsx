
import { Button } from '@/components/ui/button';
import { UserContext } from '@/contexts/UserContext';
import { useSymbol } from '@/contexts/SymbolContext';
import { api, API_URL } from '@/lib/api';
import { ChartData } from '@/types/chartData';
import { useCallback, useContext, useEffect, useMemo, useState } from 'react';
import TradingPanel from './trading-panel/TradingPanel';
import ChartDisplay from './chart-display/ChartDisplay';
import { calculateCurrentValue, calculateProfitLoss } from '@/utils/trading';
import { splitChartData } from '@/utils/charts';
import Error from '@/components/error/Error';
import { handleError } from '@/utils/errors';
import Toaster from '@/components/toaster/Toaster';
import SymbolPicker from '@/components/symbol-picker/SymbolPicker';

export type Meta = {
    "symbol": string,
    "interval": "5min" | "15min",
    "currency": string,
    "exchange_timezone": string
}
type StocksDataResponse = {
    "meta": Meta,
    "values": ChartData[],
}
type SearchResponse = {
    data: StocksDataResponse
}

export type InvestPoint = {
    amount: number,
    point: ChartData,
    type: 'short' | 'long'
}
const UPDATE_SPEED = 500


export default function Home() {

    const { user } = useContext(UserContext)
    const { symbol } = useSymbol()

    const [tradeId, setTradeId] = useState<string | undefined>()
    const [interval, setInterval] = useState<"5min" | "15min" | "1h">("5min")
    const [reactive, setReactive] = useState(false);
    const [chartData, setChartData] = useState<ChartData[]>()
    const [afterData, setAfterData] = useState<ChartData[]>()
    const [meta, setMeta] = useState<Meta>()
    const [investPoint, setInvestPoint] = useState<InvestPoint>()
    const [balance, setBalance] = useState(1000)
    const [updateSpeed, setUpdateSpeed] = useState<number>(UPDATE_SPEED)

    const [error, setError] = useState<string | null>(null)
    const [toaster, setToaster] = useState<string | null>(null)

    const handleStartTrade = useCallback(async (amount: number, type: 'short' | 'long') => {
        if (amount <= 0) return

        if (balance < amount) {
            setError('Insufficient balance')
            return
        }

        if (!chartData || chartData.length === 0) {
            setError('Market data not available')
            return
        }

        if (!meta?.symbol) {
            setError('Trading symbol is unavailable')
            return
        }

        if (!user?.id) {
            setError('Please sign in to trade')
            return
        }

        try {
            const { data } = await api.post<{ id: string }>(`${API_URL}/users/trades`, {
                amountInvested: amount,
                type,
                symbol: meta.symbol
            })

            setTradeId(data.id)
            setInvestPoint({
                amount,
                point: chartData[chartData.length - 1],
                type,
            })
            setReactive(true)
            setBalance(prev => prev - amount)
        } catch (err) {
            const error = handleError(err) ?? "Failed to start trade"
            setError(error)
        }
    }, [balance, chartData, meta, user])

    const endTrade = useCallback(async () => {
        if (!investPoint || !tradeId) return

        if (!user?.id) {
            setError('Please sign in to close trades')
            return
        }

        const currentValue = calculateCurrentValue(investPoint, chartData)
        if (currentValue === undefined) return

        try {
            await api.patch(`${API_URL}/users/trades/`, {
                tradeId,
                amountFinal: currentValue,
            })

            setReactive(false)
            setBalance(prev => prev + currentValue)
            setInvestPoint(undefined)
            setTradeId(undefined)
            setToaster("Trade ended successfully")
        } catch (err) {
            const error = handleError(err) ?? "Failed to end trade"
            setError(error)
        }
    }, [chartData, investPoint, tradeId, user])

    useEffect(() => {
        const getChartData = async () => {
            try {
                const response: SearchResponse = await api.post('/market/stocks/search', {
                    symbol: symbol,
                    interval: interval,
                })
                const { tempChartData, tempAfterData } = splitChartData(response.data.values, undefined)

                setMeta(response.data.meta)
                setChartData(tempChartData)
                setAfterData(tempAfterData)
            } catch (err) {
                const error = handleError(err) ?? "Failed to fetch stock data"
                setError(error)
            }
        }
        getChartData()
    }, [interval, symbol])

    useEffect(() => {
        let intervalId: number | null = null;

        if (reactive) {
            intervalId = window.setInterval(() => {
                setAfterData(prevAfterData => {
                    if (!prevAfterData || prevAfterData.length === 0) {
                        setReactive(false)
                        return prevAfterData;
                    }

                    const [firstElement, ...rest] = prevAfterData;

                    setChartData(prevChartData => {
                        if (!prevChartData) return [firstElement];

                        const lastItem = prevChartData[prevChartData.length - 1];
                        if (lastItem && lastItem.time === firstElement.time) {
                            return prevChartData;
                        }

                        return [...prevChartData, firstElement];
                    });

                    return rest;
                });
            }, updateSpeed);
        } else
            setUpdateSpeed(UPDATE_SPEED)


        return () => {
            if (intervalId) {
                window.clearInterval(intervalId);
            }
        };
    }, [reactive, updateSpeed])

    useEffect(() => {
        if (investPoint && afterData && afterData.length < 1)
            endTrade()
    }, [afterData, endTrade, investPoint])

    const getNextMonthData = async () => {
        const lastDate = chartData && chartData.length > 0 ? chartData[chartData.length - 1].time : 0

        try {
            const response: SearchResponse = await api.post('/market/stocks/continue', {
                symbol: symbol,
                interval: interval,
                lastDate
            })

            setMeta(response.data.meta)
            setAfterData(prev => [...(prev || []), ...response.data.values])
        } catch (err) {
            const error = handleError(err) ?? "Failed to fetch next month's stock data"
            setError(error)
        }

    }

    const refreshData = async () => {
        try {
            const response: SearchResponse = await api.post('/market/stocks/refresh', {
                symbol: symbol,
                interval: interval,
            })

            const { tempChartData, tempAfterData } = splitChartData(response.data.values, undefined)

            setMeta(response.data.meta)
            setChartData(tempChartData)
            setAfterData(tempAfterData)
        } catch (err) {
            const error = handleError(err) ?? "Failed to refresh stock data"
            setError(error)
        }

    }

    const currentValue = useMemo(() => calculateCurrentValue(investPoint, chartData), [investPoint, chartData])
    const profitLoss = useMemo(() => calculateProfitLoss(investPoint, chartData), [investPoint, chartData])
    return (
        <div className="container mx-auto px-2 sm:px-4 py-4 sm:py-6 max-w-7xl">

            {/* Header Section */}
            <div className="flex flex-col space-y-4 mb-4 sm:mb-6">
                {/* Title and Balance Row */}
                <div className="flex flex-row sm:flex-row justify-between items-start sm:items-center gap-3">
                    <div className="flex items-center gap-4">
                        <SymbolPicker />
                        {/* <div className="hidden sm:block">
                            <p className="text-sm text-muted-foreground">Trading Dashboard</p>
                            <p className="text-xs text-muted-foreground/60">{meta?.exchange_timezone}</p>
                        </div> */}
                    </div>
                    <div className="bg-primary/10 px-3 py-1.5 rounded-full">
                        <span className="text-sm font-medium text-primary">Balance: ${balance.toLocaleString()}</span>
                    </div>
                </div>

                {/* Time Interval Controls */}
                <div className="flex bg-muted p-1 rounded-lg w-full sm:w-auto">
                    <Button
                        variant={interval === "5min" ? "default" : "ghost"}
                        size="sm"
                        onClick={() => setInterval("5min")}
                        className="rounded-md flex-1 sm:flex-none"
                    >
                        5min
                    </Button>
                    <Button
                        variant={interval === "15min" ? "default" : "ghost"}
                        size="sm"
                        onClick={() => setInterval("15min")}
                        className="rounded-md flex-1 sm:flex-none"
                    >
                        15min
                    </Button>
                    <Button
                        variant={interval === "1h" ? "default" : "ghost"}
                        size="sm"
                        onClick={() => setInterval("1h")}
                        className="rounded-md flex-1 sm:flex-none"
                    >
                        1h
                    </Button>
                </div>
            </div>

            <div className="flex flex-col lg:grid lg:grid-cols-1 xl:grid-cols-3 gap-4 sm:gap-6 ">
                <ChartDisplay
                    afterData={afterData || []}
                    chartData={chartData || []}
                    reactive={reactive}
                    updateSpeed={updateSpeed}
                    setReactive={setReactive}
                    setUpdateSpeed={setUpdateSpeed}
                    investPoint={investPoint}
                    getNextMonthData={getNextMonthData}
                    refreshData={refreshData}
                />

                <TradingPanel
                    balance={balance}
                    investPoint={investPoint}
                    currentValue={currentValue}
                    profitLoss={profitLoss}
                    onStartTrade={handleStartTrade}
                    onCloseTrade={endTrade}
                />
                {toaster && <Toaster message={toaster} onClose={() => setToaster(null)} />}

                {error && <Error message={error} onClose={() => setError(null)} />}
            </div>
        </div>
    )
};
