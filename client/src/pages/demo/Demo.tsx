import { Button } from '@/components/ui/button';
import { ChartData } from '@/types/chartData';
import { useCallback, useEffect, useMemo, useState } from 'react';
import TradingPanel from '../home/trading-panel/TradingPanel';
import ChartDisplay from '../home/chart-display/ChartDisplay';
import { calculateCurrentValue, calculateProfitLoss } from '@/utils/trading';
import { splitChartData } from '@/utils/charts';
import Error from '@/components/error/Error';
import Toaster from '@/components/toaster/Toaster';
import { DUMMY_STOCK_DATA } from '@/utils/dummyData';

export type Meta = {
    "symbol": string,
    "interval": "5min" | "15min",
    "currency": string,
    "exchange_timezone": string
}

export type InvestPoint = {
    amount: number,
    point: ChartData,
    type: 'short' | 'long'
}

const UPDATE_SPEED = 500

export default function Demo() {
    const [interval, setInterval] = useState<"5min" | "15min" | "1h">("5min")
    const [reactive, setReactive] = useState(false);
    const [chartData, setChartData] = useState<ChartData[]>()
    const [afterData, setAfterData] = useState<ChartData[]>()
    const [meta] = useState<Meta>({
        symbol: "DEMO",
        interval: "5min",
        currency: "USD",
        exchange_timezone: "America/New_York"
    })
    const [investPoint, setInvestPoint] = useState<InvestPoint>()
    const [balance, setBalance] = useState(1000)
    const [updateSpeed, setUpdateSpeed] = useState<number>(UPDATE_SPEED)

    const [error, setError] = useState<string | null>(null)
    const [toaster, setToaster] = useState<string | null>(null)

    const handleStartTrade = useCallback((amount: number, type: 'short' | 'long') => {
        if (amount <= 0) return

        if (balance < amount) {
            setError('Insufficient balance')
            return
        }

        if (!chartData || chartData.length === 0) {
            setError('Market data not available')
            return
        }

        setInvestPoint({
            amount,
            point: chartData[chartData.length - 1],
            type,
        })
        setReactive(true)
        setBalance(prev => prev - amount)
        setToaster("Demo trade started successfully")
    }, [balance, chartData])

    const endTrade = useCallback(() => {
        if (!investPoint) return

        const currentValue = calculateCurrentValue(investPoint, chartData)
        if (currentValue === undefined) return

        setReactive(false)
        setBalance(prev => prev + currentValue)
        setInvestPoint(undefined)
        setToaster("Demo trade ended successfully")
    }, [chartData, investPoint])

    // Transform data based on interval
    const transformDataByInterval = useCallback((data: ChartData[], targetInterval: "5min" | "15min" | "1h"): ChartData[] => {
        if (targetInterval === "5min") {
            return data; // Base data is 5min
        }

        const multiplier = targetInterval === "15min" ? 3 : 12; // 15min = 3x5min, 1h = 12x5min
        const transformed: ChartData[] = [];

        for (let i = 0; i < data.length; i += multiplier) {
            const chunk = data.slice(i, i + multiplier);
            if (chunk.length === 0) break;

            const open = chunk[0].open;
            const close = chunk[chunk.length - 1].close;
            const high = Math.max(...chunk.map(d => d.high));
            const low = Math.min(...chunk.map(d => d.low));
            const volume = chunk.reduce((sum, d) => sum + (d.volume || 0), 0);
            const time = chunk[0].time;

            transformed.push({ time, open, high, low, close, volume });
        }

        return transformed;
    }, []);

    // Initialize chart data on mount and when interval changes
    useEffect(() => {
        if (DUMMY_STOCK_DATA.length === 0) {
            setError("No dummy data available. Please add data to DUMMY_STOCK_DATA in utils/dummyData.ts")
            return
        }

        // Transform data based on selected interval
        const transformedData = transformDataByInterval(DUMMY_STOCK_DATA, interval);

        // Calculate the current position as a percentage of the full data
        const currentPosition = chartData ? chartData.length / (chartData.length + (afterData?.length || 0)) : 0.5;

        // Split data maintaining the current position
        if (currentPosition > 0 && chartData && chartData.length > 0) {
            const splitIndex = Math.floor(transformedData.length * currentPosition);
            const tempChartData = transformedData.slice(0, splitIndex);
            const tempAfterData = transformedData.slice(splitIndex);
            
            setChartData(tempChartData);
            setAfterData(tempAfterData);
        } else {
            // Initial load - split in half
            const { tempChartData, tempAfterData } = splitChartData(transformedData, undefined);
            setChartData(tempChartData);
            setAfterData(tempAfterData);
        }
    }, [interval, transformDataByInterval])

    // Handle reactive data streaming
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
        } else {
            setUpdateSpeed(UPDATE_SPEED)
        }

        return () => {
            if (intervalId) {
                window.clearInterval(intervalId);
            }
        };
    }, [reactive, updateSpeed])

    // Auto-close trade when data ends
    useEffect(() => {
        if (investPoint && afterData && afterData.length < 1)
            endTrade()
    }, [afterData, endTrade, investPoint])

    const getNextMonthData = () => {
        setError("Demo mode: No additional data available")
    }

    const refreshData = () => {
        if (DUMMY_STOCK_DATA.length === 0) {
            setError("No dummy data available. Please add data to DUMMY_STOCK_DATA in utils/dummyData.ts")
            return
        }

        // Transform and reset data
        const transformedData = transformDataByInterval(DUMMY_STOCK_DATA, interval);
        const { tempChartData, tempAfterData } = splitChartData(transformedData, undefined)
        setChartData(tempChartData)
        setAfterData(tempAfterData)
        setToaster("Demo data reset")
    }

    const currentValue = useMemo(() => calculateCurrentValue(investPoint, chartData), [investPoint, chartData])
    const profitLoss = useMemo(() => calculateProfitLoss(investPoint, chartData), [investPoint, chartData])

    return (
        <div className="container mx-auto px-2 sm:px-4 py-4 sm:py-6 max-w-7xl">
            <div className="flex flex-col space-y-4 mb-4 sm:mb-6">
                <div className="flex flex-row sm:flex-row justify-between items-start sm:items-center gap-3">
                    <div className="flex items-center gap-4">
                        <div>
                            <h1 className="text-2xl font-bold">Demo Trading</h1>
                            <p className="text-sm text-muted-foreground">{meta.symbol} - {meta.exchange_timezone}</p>
                        </div>
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
                    demo
                />

                {toaster && <Toaster message={toaster} onClose={() => setToaster(null)} />}
                {error && <Error message={error} onClose={() => setError(null)} />}
            </div>
        </div>
    )
}
