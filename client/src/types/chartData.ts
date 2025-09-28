import { UTCTimestamp } from "lightweight-charts"

export type ChartData = {
    time: UTCTimestamp,
    open: number,
    high: number,
    low: number,
    close: number,
    volume?: number
}