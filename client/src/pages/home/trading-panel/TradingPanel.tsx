import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import AiChat from '@/components/ai-chat/AiChat';
import InvestSection from './invest-section/InvestSection';
import type { InvestPoint } from '../Home';
//import { useContext } from 'react';
//import { UserContext } from '@/contexts/UserContext';

type TradingPanelProps = {
    balance: number;
    investPoint?: InvestPoint;
    currentValue?: number;
    profitLoss?: number;
    onStartTrade: (amount: number, type: 'short' | 'long') => Promise<void> | void;
    onCloseTrade: () => Promise<void> | void;
    defaultValue?: string;
}

export default function TradingPanel(props: TradingPanelProps) {
    //const { user } = useContext(UserContext)
    const { balance, investPoint, currentValue, profitLoss, onStartTrade, onCloseTrade, defaultValue = 'investSection' } = props

    return (
        <div className="xl:col-start-3 order-2 lg:order-2 flex flex-col">
            <Tabs defaultValue={defaultValue} className="flex flex-col h-[522px]">
                <TabsList className="flex-shrink-0 w-full">
                    <TabsTrigger value="investSection">Trading Panel</TabsTrigger>
                    <TabsTrigger value="aiSection" className="relative">
                        <span>Chat With AI</span>

                        {/* {user?.userType === 0
                            && <span className="ml-1.5 inline-flex items-center gap-1 px-2 py-0.5 rounded-full text-[10px] font-semibold bg-gradient-to-r from-purple-500 to-pink-500 text-white shadow-sm">
                                <svg className="w-2.5 h-2.5" fill="currentColor" viewBox="0 0 20 20">
                                    <path d="M9.049 2.927c.3-.921 1.603-.921 1.902 0l1.07 3.292a1 1 0 00.95.69h3.462c.969 0 1.371 1.24.588 1.81l-2.8 2.034a1 1 0 00-.364 1.118l1.07 3.292c.3.921-.755 1.688-1.54 1.118l-2.8-2.034a1 1 0 00-1.175 0l-2.8 2.034c-.784.57-1.838-.197-1.539-1.118l1.07-3.292a1 1 0 00-.364-1.118L2.98 8.72c-.783-.57-.38-1.81.588-1.81h3.461a1 1 0 00.951-.69l1.07-3.292z" />
                                </svg>
                                TRIAL
                            </span>} */}
                    </TabsTrigger>
                </TabsList>
                <TabsContent value="investSection" className="flex-1 mt-2 min-h-0">
                    <InvestSection
                        balance={balance}
                        investPoint={investPoint}
                        currentValue={currentValue}
                        profitLoss={profitLoss}
                        onStartTrade={onStartTrade}
                        onCloseTrade={onCloseTrade}
                    />
                </TabsContent>
                <TabsContent value="aiSection" className="flex-1 mt-2 min-h-0">
                    <AiChat />
                </TabsContent>
            </Tabs>
        </div >
    )
}