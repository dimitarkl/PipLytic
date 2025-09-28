import AreaChart from "@/components/charts/area-chart/AreaChart";
import { Button } from "@/components/ui/button";
import { Pause, Play } from "lucide-react";
import { useEffect, useState } from "react";
import { Meta } from "../Home";
import { ChartData } from "@/types/chartData";

type ChartDisplayProps = {
    balance: number,
    interval: string,
    meta: Meta | undefined
    chartData: ChartData[],
    reactive: boolean
}
const UPDATE_SPEED = 500

export default function ChartDisplay(props: ChartDisplayProps) {

    const { balance, interval, meta, chartData, reactive } = props;

    const [updateSpeed, setUpdateSpeed] = useState<number>(UPDATE_SPEED)

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

    return (
        <div>
            <h1>DEV:{balance}</h1>
            <h2 className='font-bold text-2xl'>{meta?.symbol ?? ''}</h2>
            <div className='flex flex-row space-x-2'>
                <Button
                    variant={interval === "5min" ? "default" : "ghost"}
                    type='button'
                    onClick={() => setInterval("5min")}
                >
                    5min
                </Button>
                <Button
                    variant={interval === "15min" ? "default" : "ghost"}
                    type='button'
                    onClick={() => setInterval("15min")}
                >
                    15min
                </Button>
                <Button
                    variant={interval === "1h" ? "default" : "ghost"}
                    type='button'
                    onClick={() => setInterval("1h")}

                >
                    1h
                </Button>
            </div>
            <AreaChart
                reactive={reactive}
                data={chartData || []}
                investPoint={investPoint}
            />
            <div className=' flex space-x-2 items-center'>
                <Button
                    type='button'
                    onClick={() => setReactive(!reactive)}
                >
                    {reactive ? <Pause /> : <Play />}
                </Button>
                {reactive &&
                    <>
                        <Button
                            type='button'
                            variant={updateSpeed === UPDATE_SPEED ? 'default' : 'outline'}
                            onClick={() => setUpdateSpeed(UPDATE_SPEED)}
                        >
                            1x
                        </Button>
                        <Button
                            type='button'
                            variant={updateSpeed === UPDATE_SPEED / 2 ? 'default' : 'outline'}
                            onClick={() => setUpdateSpeed(UPDATE_SPEED / 2)}
                        >
                            2x
                        </Button>
                        <Button
                            type='button'
                            variant={updateSpeed === UPDATE_SPEED / 5 ? 'default' : 'outline'}
                            onClick={() => setUpdateSpeed(UPDATE_SPEED / 5)}
                        >
                            5x
                        </Button>
                    </>}

            </div>
        </div>
    )
}