import { ChartData } from "@/types/chartData";

export const splitChartData = (values: ChartData[], chartData: ChartData[] | undefined) => {
    let tempChartData;
    let tempAfterData;

    if (chartData) {
        tempChartData = values.filter(v => v.time <= chartData[chartData.length - 1].time)
        tempAfterData = values.filter(v => v.time > chartData[chartData.length - 1].time)
    } else {
        tempChartData = values.slice(0, values.length / 2)
        tempAfterData = values.slice(values.length / 2, values.length)
    }
    return {
        tempChartData,
        tempAfterData
    }
}