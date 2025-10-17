import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { useState } from "react";
import { InvestPoint } from "../../Home";
import { Card } from "@/components/ui/card";

type InvestSectionProps = {
    balance: number;
    investPoint?: InvestPoint;
    currentValue?: number;
    profitLoss?: number;
    onStartTrade: (amount: number, type: 'short' | 'long') => Promise<void> | void;
    onCloseTrade: () => Promise<void> | void;
}

export default function InvestSection(props: InvestSectionProps) {

    const { balance, investPoint, currentValue, profitLoss, onStartTrade, onCloseTrade } = props
    const [amount, setAmount] = useState<number>(0)

    const handleStartTrade = async (type: 'short' | 'long') => {
        if (amount <= 0) return

        try {
            await onStartTrade(amount, type)
            setAmount(0)
        } catch (error) {
            console.error('Failed to start trade', error)
        }
    }

    const getPLColor = () => {
        if (profitLoss === undefined) return ''

        return profitLoss >= 0 ? 'text-green-500' : 'text-red-500'
    }

    const getPL = () => {
        if (profitLoss === undefined) return '--'

        const prefix = profitLoss >= 0 ? '+' : ''
        return `${prefix}${profitLoss.toFixed(2)}`
    }

    const getFormattedCurrentValue = () => {
        if (currentValue === undefined) return '--'

        return currentValue.toLocaleString(undefined, {
            minimumFractionDigits: 2,
            maximumFractionDigits: 2,
        })
    }

    return (
        <>
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
                    {investPoint && (
                        <div className="mb-4 sm:mb-6 p-3 sm:p-4 bg-muted/50 rounded-lg border">
                            <div className="text-center space-y-2 sm:space-y-3">
                                <div>
                                    <p className="text-xs sm:text-sm text-muted-foreground mb-1">Current Value</p>
                                    <p className="text-xl sm:text-2xl font-bold text-foreground">${getFormattedCurrentValue()}</p>
                                </div>

                                <div>
                                    <p className="text-xs sm:text-sm text-muted-foreground mb-1">Profit & Loss</p>
                                    <p className={`text-lg sm:text-xl font-bold ${getPLColor()}`}>
                                        ${getPL()}
                                    </p>
                                </div>

                                <div className="flex justify-center">
                                    <span className={`px-2 sm:px-3 py-1 rounded-full text-xs font-medium ${investPoint.type === 'long'
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
                                onClick={() => onCloseTrade()}
                                className="w-full py-3 sm:py-4 text-base sm:text-lg font-semibold"
                                variant="destructive"
                            >
                                Close Position
                            </Button>
                        ) : (
                            <div className="flex flex-col sm:grid sm:grid-cols-2 gap-3">
                                <Button
                                    onClick={() => handleStartTrade('short')}
                                    disabled={amount <= 0 || balance < amount}
                                    className="py-3 sm:py-4 bg-red-500 hover:bg-red-600 text-white font-semibold text-base order-2 sm:order-1"
                                >
                                    <span className="flex items-center justify-center gap-2">
                                        📉 <span>Short</span>
                                    </span>
                                </Button>
                                <Button
                                    onClick={() => handleStartTrade('long')}
                                    disabled={amount <= 0 || balance < amount}
                                    className="py-3 sm:py-4 bg-green-500 hover:bg-green-600 text-white font-semibold text-base order-1 sm:order-2"
                                >
                                    <span className="flex items-center justify-center gap-2">
                                        📈 <span>Long</span>
                                    </span>
                                </Button>
                            </div>
                        )}

                    </div>
                </Card>
            </div>
        </>
    )
}