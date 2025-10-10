
import AreaChart from '@/components/charts/area-chart/AreaChart';
import AiChat from '@/components/ai-chat/AiChat';
import { Button } from '@/components/ui/button';
import { Card } from '@/components/ui/card';
import { UserContext } from '@/contexts/UserContext';
import { api, API_URL } from '@/lib/api';
import { ChartData } from '@/types/chartData';
import { Pause, Play } from 'lucide-react';
import { useCallback, useContext, useEffect, useMemo, useState } from 'react';
import InvestSection from './invest-section/InvestSection';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';

type Meta = {
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


const calculateCurrentValue = (investPoint?: InvestPoint, chartData?: ChartData[]) => {
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

const calculateProfitLoss = (investPoint?: InvestPoint, chartData?: ChartData[]) => {
    if (!investPoint || !chartData || chartData.length === 0) return undefined

    const investPrice = investPoint.point.close
    if (investPrice === 0) return undefined

    const currentPrice = chartData[chartData.length - 1].close

    const profitLoss = investPoint.type === 'long'
        ? ((currentPrice - investPrice) / investPrice) * investPoint.amount
        : ((investPrice - currentPrice) / investPrice) * investPoint.amount

    return Number(profitLoss.toFixed(2))
}


export default function Home() {

    const { user } = useContext(UserContext)

    const [tradeId, setTradeId] = useState<string | undefined>()
    const [interval, setInterval] = useState<"5min" | "15min" | "1h">("5min")
    const [reactive, setReactive] = useState(false);
    const [chartData, setChartData] = useState<ChartData[]>()
    const [afterData, setAfterData] = useState<ChartData[]>()
    const [meta, setMeta] = useState<Meta>()
    const [investPoint, setInvestPoint] = useState<InvestPoint>()
    const [balance, setBalance] = useState(1000)
    const [updateSpeed, setUpdateSpeed] = useState<number>(UPDATE_SPEED)

    const handleStartTrade = useCallback(async (amount: number, type: 'short' | 'long') => {
        if (amount <= 0) return

        if (balance < amount) {
            alert('Not enough balance')
            return
        }

        if (!chartData || chartData.length === 0) {
            alert('Market data is not available yet')
            return
        }

        if (!meta?.symbol) {
            alert('Trading symbol unavailable')
            return
        }

        if (!user?.id) {
            alert('Please sign in to trade')
            return
        }

        try {
            const { data } = await api.post<{ id: string }>(`${API_URL}/users/${user.id}/trades/start`, {
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
        } catch (error) {
            console.error('Failed to start trade', error)
            alert('Failed to start trade')
        }
    }, [balance, chartData, meta, user])

    const endTrade = useCallback(async () => {
        if (!investPoint || !tradeId) return

        if (!user?.id) {
            alert('Please sign in to close trades')
            return
        }

        const currentValue = calculateCurrentValue(investPoint, chartData)
        if (currentValue === undefined) return

        try {
            await api.post(`${API_URL}/users/${user.id}/trades/end`, {
                tradeId,
                amountFinal: currentValue,
            })

            setReactive(false)
            setBalance(prev => prev + currentValue)
            setInvestPoint(undefined)
            setTradeId(undefined)
        } catch (error) {
            console.error('Failed to end trade', error)
            alert('Failed to end trade')
        }
    }, [chartData, investPoint, tradeId, user])

    useEffect(() => {
        const getChartData = async () => {
            try {
                const response: SearchResponse = await api.post('/market/stocks/search', {
                    symbol: "IBM",
                    interval: interval,
                })
                let tempChartData;
                let tempAfterData;

                if (chartData) {
                    tempChartData = response.data.values.filter(v => v.time <= chartData[chartData.length - 1].time)
                    tempAfterData = response.data.values.filter(v => v.time > chartData[chartData.length - 1].time)
                } else {
                    tempChartData = response.data.values.slice(0, response.data.values.length / 2)
                    tempAfterData = response.data.values.slice(response.data.values.length / 2, response.data.values.length)
                }

                setMeta(response.data.meta)
                setChartData(tempChartData)
                setAfterData(tempAfterData)
            } catch (err) {
                console.log(err)
            }
        }
        getChartData()
    }, [interval])

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

    const currentValue = useMemo(() => calculateCurrentValue(investPoint, chartData), [investPoint, chartData])
    const profitLoss = useMemo(() => calculateProfitLoss(investPoint, chartData), [investPoint, chartData])

    return (
        <div className="container mx-auto px-2 sm:px-4 py-4 sm:py-6 max-w-7xl">

            {/* Header Section */}
            <div className="flex flex-col space-y-4 mb-4 sm:mb-6">
                {/* Title and Balance Row */}
                <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-3">
                    <div>
                        <h1 className="text-2xl sm:text-3xl font-bold text-foreground">{meta?.symbol ?? 'Loading...'}</h1>
                        <p className="text-sm sm:text-base text-muted-foreground">Trading Dashboard</p>
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
                {/*Chart */}
                <div className="xl:col-start-1 xl:col-span-2 order-1 lg:order-1 space-y-4 sm:space-y-6">
                    <Card className="p-3 sm:p-6 min-h-[450px]">
                        <div className="mb-3 sm:mb-4">
                            <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-3 sm:gap-0 mb-3 sm:mb-4">
                                <h2 className="text-base sm:text-lg font-semibold">Price Chart</h2>
                                <div className="flex flex-col sm:flex-row items-stretch sm:items-center gap-2">
                                    {afterData && afterData.length < 1
                                        ? <div className="mt-3 sm:mt-4 p-3 bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 rounded-lg">
                                            <p className="text-red-600 dark:text-red-400 text-sm font-medium text-center sm:text-left">
                                                📊 Market data has ended
                                            </p>
                                        </div>
                                        : <Button
                                            variant={reactive ? "default" : "outline"}
                                            size="sm"
                                            onClick={() => setReactive(!reactive)}
                                            className="flex items-center justify-center gap-2 w-full sm:w-auto"
                                        >
                                            {reactive ? <Pause className="w-4 h-4" /> : <Play className="w-4 h-4" />}
                                            <span className="sm:hidden">{reactive ? 'Pause Stream' : 'Start Stream'}</span>
                                            <span className="hidden sm:inline">{reactive ? 'Pause' : 'Live'}</span>
                                        </Button>}

                                    {reactive && (
                                        <div className="flex bg-muted p-1 rounded-md">
                                            <Button
                                                variant={updateSpeed === UPDATE_SPEED ? 'default' : 'ghost'}
                                                size="sm"
                                                onClick={() => setUpdateSpeed(UPDATE_SPEED)}
                                                className="text-xs px-2 py-1 flex-1 sm:flex-none"
                                            >
                                                1x
                                            </Button>
                                            <Button
                                                variant={updateSpeed === UPDATE_SPEED / 2 ? 'default' : 'ghost'}
                                                size="sm"
                                                onClick={() => setUpdateSpeed(UPDATE_SPEED / 2)}
                                                className="text-xs px-2 py-1 flex-1 sm:flex-none"
                                            >
                                                2x
                                            </Button>
                                            <Button
                                                variant={updateSpeed === UPDATE_SPEED / 5 ? 'default' : 'ghost'}
                                                size="sm"
                                                onClick={() => setUpdateSpeed(UPDATE_SPEED / 5)}
                                                className="text-xs px-2 py-1 flex-1 sm:flex-none"
                                            >
                                                5x
                                            </Button>
                                        </div>
                                    )}
                                </div>
                            </div>
                        </div>

                        <div className=" flex sm:h-80 lg:h-96">
                            <AreaChart
                                reactive={reactive}
                                data={chartData || []}
                                investPoint={investPoint}
                            />
                        </div>
                    </Card>
                </div>

                <div className="xl:col-start-3 order-2 lg:order-2 flex flex-col">
                    <Tabs defaultValue='investSection' className="flex flex-col h-[522px]">
                        <TabsList className="flex-shrink-0 w-full">
                            <TabsTrigger value="investSection">Trading Panel</TabsTrigger>
                            <TabsTrigger value="aiSection" className="relative">
                                <span>Chat With AI</span>

                                {user?.userType === 0
                                    && <span className="ml-1.5 inline-flex items-center gap-1 px-2 py-0.5 rounded-full text-[10px] font-semibold bg-gradient-to-r from-purple-500 to-pink-500 text-white shadow-sm">
                                        <svg className="w-2.5 h-2.5" fill="currentColor" viewBox="0 0 20 20">
                                            <path d="M9.049 2.927c.3-.921 1.603-.921 1.902 0l1.07 3.292a1 1 0 00.95.69h3.462c.969 0 1.371 1.24.588 1.81l-2.8 2.034a1 1 0 00-.364 1.118l1.07 3.292c.3.921-.755 1.688-1.54 1.118l-2.8-2.034a1 1 0 00-1.175 0l-2.8 2.034c-.784.57-1.838-.197-1.539-1.118l1.07-3.292a1 1 0 00-.364-1.118L2.98 8.72c-.783-.57-.38-1.81.588-1.81h3.461a1 1 0 00.951-.69l1.07-3.292z" />
                                        </svg>
                                        TRIAL
                                    </span>}
                            </TabsTrigger>
                        </TabsList>
                        <TabsContent value="investSection" className="flex-1 mt-2 min-h-0">
                            <InvestSection
                                balance={balance}
                                investPoint={investPoint}
                                currentValue={currentValue}
                                profitLoss={profitLoss}
                                onStartTrade={handleStartTrade}
                                onCloseTrade={endTrade}
                            />
                        </TabsContent>
                        <TabsContent value="aiSection" className="flex-1 mt-2 min-h-0">
                            <AiChat />
                        </TabsContent>
                    </Tabs>
                </div>
            </div>
        </div>
    )
};
