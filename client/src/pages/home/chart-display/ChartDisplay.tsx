import AreaChart from "@/components/charts/area-chart/AreaChart";
import { Button } from "@/components/ui/button";
import { Pause, Play } from "lucide-react";
import { Card } from '@/components/ui/card';
import { ChartData } from "@/types/chartData";
import type { InvestPoint } from '../Home';

type ChartDisplayProps = {
    chartData: ChartData[];
    afterData: ChartData[]
    reactive: boolean;
    updateSpeed: number;
    setReactive: (v: boolean) => void;
    setUpdateSpeed: (n: number) => void;
    investPoint?: InvestPoint;
}

export default function ChartDisplay(props: ChartDisplayProps) {
    const {
        chartData,
        afterData,
        reactive,
        updateSpeed,
        setReactive,
        setUpdateSpeed,
        investPoint } = props;

    return (
        <div className="xl:col-start-1 xl:col-span-2 order-1 lg:order-1 space-y-4 sm:space-y-6">
            <Card className="p-3 sm:p-6 min-h-[450px]">
                <div className="mb-3 sm:mb-4">
                    <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-3 sm:gap-0 mb-3 sm:mb-4">
                        <h2 className="text-base sm:text-lg font-semibold">Price Chart</h2>
                        <div className="flex flex-col sm:flex-row items-stretch sm:items-center gap-2">
                            {chartData && afterData.length < 1
                                ? <div className="p-2 bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 rounded-lg w-full">
                                    <div className="flex flex-col sm:flex-row sm:items-center gap-3">
                                        <div>
                                            <p className="text-red-600 dark:text-red-400 text-sm font-medium">📊 Market data has ended</p>
                                            <p className="text-xs text-muted-foreground mt-1">No more historical points in this month.</p>
                                        </div>


                                    </div>
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
                                        variant={updateSpeed === 500 ? 'default' : 'ghost'}
                                        size="sm"
                                        onClick={() => setUpdateSpeed(500)}
                                        className="text-xs px-2 py-1 flex-1 sm:flex-none"
                                    >
                                        1x
                                    </Button>
                                    <Button
                                        variant={updateSpeed === 250 ? 'default' : 'ghost'}
                                        size="sm"
                                        onClick={() => setUpdateSpeed(250)}
                                        className="text-xs px-2 py-1 flex-1 sm:flex-none"
                                    >
                                        2x
                                    </Button>
                                    <Button
                                        variant={updateSpeed === 100 ? 'default' : 'ghost'}
                                        size="sm"
                                        onClick={() => setUpdateSpeed(100)}
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
    )
}