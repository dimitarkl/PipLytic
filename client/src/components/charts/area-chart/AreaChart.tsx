import { CandlestickSeries, Chart, HistogramSeries, Pane } from 'lightweight-charts-react-components';
import { chartColors } from '@/lib/utils';
import { useTheme } from '@/contexts/ThemeProvider';
import { ChartData } from '@/types/chartData';
import { UTCTimestamp } from 'lightweight-charts';

type AreaChartProps = {
    data: ChartData
}

export default function AreaChart(props: AreaChartProps) {
    const { data } = props
    const { theme } = useTheme();
    const isDark = theme === 'dark' || (theme === 'system' && window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches);

    const chartOptions = {
        width: 600,
        height: 400,
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
                time: Number(d.time) as UTCTimestamp,
                value: d.volume ?? 0,
                color: d.close >= d.open ? chartColors.series.up : chartColors.series.down,
            }))
        : [];

    return (
        <Chart options={chartOptions}>
            <Pane
                stretchFactor={2}
            >
                <CandlestickSeries
                    data={data}
                    options={candlestickSeriesOptions}
                />
            </Pane>
            <Pane>
                <HistogramSeries
                    data={volumeData} />
            </Pane>
        </Chart>
    )
};
