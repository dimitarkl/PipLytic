import { CandlestickSeries, Chart, HistogramSeries, Markers, Pane } from 'lightweight-charts-react-components';
import { chartColors } from '@/lib/utils';
import { useTheme } from '@/contexts/ThemeProvider';
import { ChartData } from '@/types/chartData';
import { SeriesMarker, Time, UTCTimestamp } from 'lightweight-charts';
import { useMemo, useEffect, useRef, useState } from 'react';

type AreaChartProps = {
    data: ChartData[]
    reactive: boolean,
    investPoint?: {
        amount: number,
        point: ChartData,
        type: 'short' | 'long'
    }
}

export default function AreaChart(props: AreaChartProps) {
    const { data, investPoint, reactive } = props
    const { theme } = useTheme();
    const isDark = theme === 'dark' || (theme === 'system' && window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches);
    
    const containerRef = useRef<HTMLDivElement>(null);
    const [dimensions, setDimensions] = useState({ width: 750, height: 400 });

    useEffect(() => {
        const updateDimensions = () => {
            if (containerRef.current) {
                const { offsetWidth, offsetHeight } = containerRef.current;
                setDimensions({
                    width: offsetWidth || 750,
                    height: offsetHeight || 400
                });
            }
        };

        updateDimensions();
        
        const resizeObserver = new ResizeObserver(updateDimensions);
        if (containerRef.current) {
            resizeObserver.observe(containerRef.current);
        }

        return () => {
            resizeObserver.disconnect();
        };
    }, []);

    const chartOptions = {
        width: dimensions.width,
        height: dimensions.height,
        layout: {
            background: { color: isDark ? chartColors.dark.background : chartColors.light.background },
            textColor: isDark ? chartColors.dark.text : chartColors.light.text,
        },
        grid: {
            vertLines: { color: isDark ? chartColors.dark.grid : chartColors.light.grid },
            horzLines: { color: isDark ? chartColors.dark.grid : chartColors.light.grid },
        },
        timeScale: {
            timeVisible: true,
            secondsVisible: false,
        },
        priceScale: {
            borderColor: isDark ? chartColors.dark.grid : chartColors.light.grid,
        },
    };

    const candlestickSeriesOptions = {
        upColor: chartColors.series.up, downColor: chartColors.series.down, borderVisible: chartColors.series.borderVisible as any,
        wickUpColor: chartColors.series.wickUp, wickDownColor: chartColors.series.wickDown,
    };

    const volumeData = data
        ? data
            .map(d => ({
                time: d.time as UTCTimestamp,
                value: d.volume ?? 0,
                color: d.close >= d.open ? chartColors.series.up : chartColors.series.down,
            }))
        : [];
    const markers = useMemo(() => {
        if (!investPoint) return [];
        return [{
            time: investPoint.point.time,
            color: investPoint.type === 'long' ? "green" : "red",
            shape: investPoint.type === 'long' ? "arrowUp" : "arrowDown",
            text: investPoint.type,
            position: investPoint.type === 'long' ? "belowBar" : "aboveBar",
            size: 2,
        }] satisfies SeriesMarker<Time>[];
    }, [investPoint]);

    return (
        <div ref={containerRef} className="w-full h-full min-h-[300px]">
            <Chart options={chartOptions}>
                <Pane
                    stretchFactor={2}
                >
                    <CandlestickSeries
                        data={data}
                        options={candlestickSeriesOptions}
                        reactive={true}
                    >
                        {!reactive ? null : <Markers markers={markers} />}
                    </CandlestickSeries>

                </Pane>
                <Pane>
                    <HistogramSeries
                        data={volumeData} />
                </Pane>
            </Chart>
        </div>
    )
};
