
import AreaChart from '@/components/charts/area-chart/AreaChart';
import { Button } from '@/components/ui/button';
import { Card } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { UserContext } from '@/contexts/UserContext';
import { api, API_URL } from '@/lib/api';
import { ChartData } from '@/types/chartData';
import { Pause, Play } from 'lucide-react';
import { useContext, useEffect, useState } from 'react';

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

const UPDATE_SPEED = 500


export default function Home() {

    const { user } = useContext(UserContext)

    const [tradeId, setTradeId] = useState()
    const [interval, setInterval] = useState<"5min" | "15min" | "1h">("5min")
    const [reactive, setReactive] = useState(false);
    const [chartData, setChartData] = useState<ChartData[]>()
    const [afterData, setAfterData] = useState<ChartData[]>()
    const [meta, setMeta] = useState<Meta>()
    const [investPoint, setInvestPoint] = useState<{
        amount: number,
        point: ChartData,
        type: 'short' | 'long'
    }>()
    const [amount, setAmount] = useState<number>(0)
    const [balance, setBalance] = useState(1000)
    const [updateSpeed, setUpdateSpeed] = useState<number>(UPDATE_SPEED)

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
                    tempChartData = response.data.values.filter(v => v.time < chartData[chartData.length - 1].time)
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
        if (afterData && afterData?.length < 1)
            endTrade()
    }, [afterData])

    const startTrade = async (type: 'short' | 'long') => {
        if (balance - amount < 0) {
            alert('Not enought balance')
            return
        }

        if (chartData && chartData.length > 0 && amount > 0) {
            const response = await api.post(`${API_URL}/users/${user?.id}/trades/start`,
                {
                    amountInvested: amount,
                    type,
                    symbol: meta?.symbol
                }
            )
            console.log(response.data.id)
            setTradeId(response.data.id)
            setInvestPoint({
                amount: amount,
                point: chartData[chartData.length - 1],
                type
            })
            setReactive(true)
            setBalance(balance - amount)

        }


    }

    const getCurrentValue = () => {
        if (!investPoint || !chartData) return
        const investPrice = investPoint.point.close;
        const currentPrice = chartData[chartData.length - 1].close;
        const investedAmount = investPoint.amount;

        if (investPoint.type === 'long') {
            // Long: profit when price goes up
            return (investedAmount * (currentPrice / investPrice)).toFixed(2);
        } else {
            // Short: profit when price goes down
            return (investedAmount * (2 - currentPrice / investPrice)).toFixed(2);
        }
    }

    const getPLColor = () => {
        if (!investPoint || !chartData) return

        const investPrice = investPoint.point.close;
        const currentPrice = chartData[chartData.length - 1].close;
        const profitLoss = investPoint.type === 'long'
            ? ((currentPrice - investPrice) / investPrice) * investPoint.amount
            : ((investPrice - currentPrice) / investPrice) * investPoint.amount;
        return profitLoss >= 0 ? 'text-green-500' : 'text-red-500';
    }

    const getPL = () => {
        if (!investPoint || !chartData) return

        const investPrice = investPoint.point.close;
        const currentPrice = chartData[chartData.length - 1].close;
        const profitLoss = investPoint.type === 'long'
            ? ((currentPrice - investPrice) / investPrice) * investPoint.amount
            : ((investPrice - currentPrice) / investPrice) * investPoint.amount;
        return (profitLoss >= 0 ? '+' : '') + profitLoss.toFixed(2);

    }

    const endTrade = async () => {
        const response = await api.post(`${API_URL}/users/${user?.id}/trades/end`, {
            tradeId,
            amountFinal: Number(getCurrentValue())
        })

        console.log(response)

        setReactive(false)
        setBalance(balance + Number(getCurrentValue()))
        setInvestPoint(undefined)
    }

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
                
                {/* Time Interval Controls - Full width on mobile */}
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

            <div className="flex flex-col lg:grid lg:grid-cols-1 xl:grid-cols-3 gap-4 sm:gap-6">
                {/* Chart Section */}
                <div className="xl:col-span-2 order-1 lg:order-1">
                    <Card className="p-3 sm:p-6 min-h-[450px]">
                        <div className="mb-3 sm:mb-4">
                            <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-3 sm:gap-0 mb-3 sm:mb-4">
                                <h2 className="text-base sm:text-lg font-semibold">Price Chart</h2>
                                <div className="flex flex-col sm:flex-row items-stretch sm:items-center gap-2">
                                    <Button
                                        variant={reactive ? "default" : "outline"}
                                        size="sm"
                                        onClick={() => setReactive(!reactive)}
                                        className="flex items-center justify-center gap-2 w-full sm:w-auto"
                                    >
                                        {reactive ? <Pause className="w-4 h-4" /> : <Play className="w-4 h-4" />}
                                        <span className="sm:hidden">{reactive ? 'Pause Stream' : 'Start Stream'}</span>
                                        <span className="hidden sm:inline">{reactive ? 'Pause' : 'Play'}</span>
                                    </Button>
                                    
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
                        
                        <div className="h-64 sm:h-80 lg:h-96">
                            <AreaChart
                                reactive={reactive}
                                data={chartData || []}
                                investPoint={investPoint}
                            />
                        </div>

                        {afterData && afterData.length < 1 && (
                            <div className="mt-3 sm:mt-4 p-3 bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 rounded-lg">
                                <p className="text-red-600 dark:text-red-400 text-sm font-medium text-center sm:text-left">
                                    📊 Market data has ended
                                </p>
                            </div>
                        )}
                    </Card>
                </div>

                {/* Trading Panel */}
                <div className="xl:col-span-1 order-2 lg:order-2">
                    <Card className="p-4 sm:p-6 h-fit">
                        <div className="text-center mb-4 sm:mb-6">
                            <h2 className="text-xl sm:text-2xl font-bold text-foreground mb-1 sm:mb-2">Trading Panel</h2>
                            <p className="text-muted-foreground text-xs sm:text-sm">Take a position on the market</p>
                        </div>

                        {/* Investment Amount Section */}
                        <div className="mb-4 sm:mb-6">
                            <div className="flex flex-col sm:flex-row sm:items-center justify-between mb-2 sm:mb-3 gap-1">
                                <label className="text-sm font-medium text-foreground">Investment Amount</label>
                                {investPoint?.amount && (
                                    <span className="text-xs sm:text-sm text-muted-foreground">
                                        Invested: ${investPoint.amount.toLocaleString()}
                                    </span>
                                )}
                            </div>
                            
                            {!investPoint?.amount && (
                                <div className="space-y-3">
                                    <div className="flex gap-2">
                                        <Input
                                            type="number"
                                            placeholder="Enter amount"
                                            value={amount || ''}
                                            onChange={e => setAmount(Number(e.target.value))}
                                            className="flex-1 text-base sm:text-sm"
                                        />
                                        <Button
                                            variant="outline"
                                            onClick={() => setAmount(balance)}
                                            className="whitespace-nowrap px-3 sm:px-4"
                                            size="sm"
                                        >
                                            MAX
                                        </Button>
                                    </div>
                                    <div className="grid grid-cols-3 gap-2">
                                        <Button
                                            variant="outline"
                                            size="sm"
                                            onClick={() => setAmount(Math.floor(balance * 0.25))}
                                            className="text-xs py-2"
                                        >
                                            25%
                                        </Button>
                                        <Button
                                            variant="outline"
                                            size="sm"
                                            onClick={() => setAmount(Math.floor(balance * 0.5))}
                                            className="text-xs py-2"
                                        >
                                            50%
                                        </Button>
                                        <Button
                                            variant="outline"
                                            size="sm"
                                            onClick={() => setAmount(Math.floor(balance * 0.75))}
                                            className="text-xs py-2"
                                        >
                                            75%
                                        </Button>
                                    </div>
                                </div>
                            )}
                        </div>

                        {/* Active Position Info */}
                        {investPoint && chartData && (
                            <div className="mb-4 sm:mb-6 p-3 sm:p-4 bg-muted/50 rounded-lg border">
                                <div className="text-center space-y-2 sm:space-y-3">
                                    <div>
                                        <p className="text-xs sm:text-sm text-muted-foreground mb-1">Current Value</p>
                                        <p className="text-xl sm:text-2xl font-bold text-foreground">${getCurrentValue()}</p>
                                    </div>
                                    
                                    <div>
                                        <p className="text-xs sm:text-sm text-muted-foreground mb-1">Profit & Loss</p>
                                        <p className={`text-lg sm:text-xl font-bold ${getPLColor()}`}>
                                            ${getPL()}
                                        </p>
                                    </div>
                                    
                                    <div className="flex justify-center">
                                        <span className={`px-2 sm:px-3 py-1 rounded-full text-xs font-medium ${
                                            investPoint.type === 'long' 
                                                ? 'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200' 
                                                : 'bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-200'
                                        }`}>
                                            {investPoint.type.toUpperCase()} Position
                                        </span>
                                    </div>
                                </div>
                            </div>
                        )}

                        {/* Action Buttons */}
                        <div className="space-y-3">
                            {investPoint ? (
                                <Button
                                    onClick={() => endTrade()}
                                    className="w-full py-3 sm:py-4 text-base sm:text-lg font-semibold"
                                    variant="destructive"
                                >
                                    Close Position
                                </Button>
                            ) : (
                                <div className="flex flex-col sm:grid sm:grid-cols-2 gap-3">
                                    <Button
                                        onClick={() => startTrade('short')}
                                        disabled={amount <= 0 || balance < amount}
                                        className="py-3 sm:py-4 bg-red-500 hover:bg-red-600 text-white font-semibold text-base order-2 sm:order-1"
                                    >
                                        <span className="flex items-center justify-center gap-2">
                                            📉 <span>Short</span>
                                        </span>
                                    </Button>
                                    <Button
                                        onClick={() => startTrade('long')}
                                        disabled={amount <= 0 || balance < amount}
                                        className="py-3 sm:py-4 bg-green-500 hover:bg-green-600 text-white font-semibold text-base order-1 sm:order-2"
                                    >
                                        <span className="flex items-center justify-center gap-2">
                                            📈 <span>Long</span>
                                        </span>
                                    </Button>
                                </div>
                            )}
                            
                            {amount > balance && (
                                <p className="text-xs sm:text-sm text-red-500 text-center">
                                    Insufficient balance
                                </p>
                            )}
                        </div>
                    </Card>
                </div>
            </div>
        </div>
    )
};
