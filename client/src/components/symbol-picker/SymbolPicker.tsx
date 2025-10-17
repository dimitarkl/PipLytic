import { useState, useEffect } from 'react';
import { Dialog, DialogContent, DialogHeader, DialogTitle } from '@/components/ui/dialog';
import { Command, CommandEmpty, CommandGroup, CommandInput, CommandItem, CommandList } from '@/components/ui/command';
import { Button } from '@/components/ui/button';
import { Search, TrendingUp, Star } from 'lucide-react';
import { useSymbol } from '@/contexts/SymbolContext';
import { api } from '@/lib/api';

type Symbol = {
    id: string;
    symbol: string;
    name: string;
};

export default function SymbolPicker() {
    const [open, setOpen] = useState(false);
    const [search, setSearch] = useState('');
    const [symbols, setSymbols] = useState<Symbol[]>([]);
    const { symbol, setSymbol } = useSymbol();

    const getStockColor = (symbol: string) => {
        const colors = [
            'from-green-500 to-green-600',
            'from-green-400 to-green-500',
            'from-lime-500 to-green-500',
            'from-yellow-500 to-lime-500',
            'from-yellow-500 to-yellow-600',
            'from-amber-500 to-yellow-500',
            'from-orange-500 to-amber-500',
            'from-orange-500 to-orange-600',
            'from-red-500 to-orange-500',
            'from-red-500 to-red-600',
        ];
        const hash = symbol.split('').reduce((acc, char) => acc + char.charCodeAt(0), 0);
        return colors[hash % colors.length];
    };

    useEffect(() => {
        const getSymbols = async () => {
            try {
                const response = await api.get('/companies');
                setSymbols(response.data);
            } catch (err) {
                //Error handling
            }
        };
        getSymbols();
    }, []);

    const handleSelectStock = (newSymbol: string) => {
        setSymbol(newSymbol);
        setOpen(false);
        setSearch('');
    };

    const displayStocks = symbols.filter(
        (stock) =>
            stock.symbol.toLowerCase().includes(search.toLowerCase()) ||
            stock.name.toLowerCase().includes(search.toLowerCase())
    );

    return (
        <>
            {/* Trigger Button */}
            <Button
                variant="outline"
                onClick={() => setOpen(true)}
                className="group relative overflow-hidden border-2 hover:border-primary transition-all duration-200 h-12 px-3"
            >
                <div className="flex items-center gap-2.5">
                    <div className="flex items-center justify-center w-8 h-8 rounded-full bg-primary/10 group-hover:bg-primary/20 transition-colors">
                        <TrendingUp className="w-4 h-4 text-primary" />
                    </div>
                    <div className="flex flex-col items-start">
                        <span className="text-[10px] text-muted-foreground font-medium">Trading</span>
                        <span className="text-base font-bold text-foreground leading-tight">{symbol}</span>
                    </div>
                </div>
            </Button>

            {/* Dialog */}
            <Dialog open={open} onOpenChange={setOpen}>
                <DialogContent className="w-[calc(100vw-2rem)] max-w-2xl p-0 gap-0 sm:w-full">
                    <Command className="rounded-lg border-0 shadow-none" shouldFilter={false}>
                        <DialogHeader className="px-6 pt-5 pb-3">
                            <DialogTitle className="text-xl font-bold">Select a Stock</DialogTitle>
                        </DialogHeader>
                        <div className="border-b px-4">
                            <CommandInput
                                placeholder="Search stocks by symbol or name..."
                                value={search}
                                onValueChange={setSearch}
                                className="border-0 focus:ring-0 h-12"
                            />
                        </div>

                        <CommandList className="max-h-[400px] overflow-y-auto p-2">
                            <CommandEmpty className="py-8 text-center text-sm text-muted-foreground">
                                <div className="flex flex-col items-center gap-2">
                                    <Search className="w-8 h-8 text-muted-foreground/50" />
                                    <p>No stocks found.</p>
                                    <p className="text-xs">Try searching for AAPL, MSFT, or TSLA</p>
                                </div>
                            </CommandEmpty>

                            {displayStocks.length > 0 && (
                                <CommandGroup
                                    heading={
                                        <div className="flex items-center gap-2 text-xs font-semibold text-muted-foreground uppercase">
                                            <Search className="w-3 h-3" />
                                            {search ? `Search Results (${displayStocks.length})` : 'All Stocks'}
                                        </div>
                                    }
                                >
                                    {displayStocks.map((stock) => (
                                        <CommandItem
                                            key={`symbol-${stock.symbol}`}
                                            value={`symbol-${stock.symbol}`}
                                            onSelect={() => handleSelectStock(stock.symbol)}
                                            className="flex items-center justify-between px-3 py-2.5 cursor-pointer hover:bg-accent rounded-lg"
                                        >
                                            <div className="flex items-center gap-3">
                                                <div
                                                    className={`flex items-center justify-center w-10 h-10 rounded-full bg-gradient-to-br ${getStockColor(stock.symbol)} text-white font-bold text-sm shadow-sm`}
                                                >
                                                    {stock.symbol.charAt(0)}
                                                </div>
                                                <div>
                                                    <p className="font-semibold text-foreground">{stock.symbol}</p>
                                                    <p className="text-xs text-muted-foreground">{stock.name}</p>
                                                </div>
                                            </div>
                                            {symbol === stock.symbol && (
                                                <div className="flex items-center gap-1 text-xs font-medium text-primary bg-primary/10 px-2 py-1 rounded">
                                                    <Star className="w-3 h-3 fill-current" />
                                                    Active
                                                </div>
                                            )}
                                        </CommandItem>
                                    ))}
                                </CommandGroup>
                            )}
                        </CommandList>
                    </Command>
                </DialogContent>
            </Dialog>
        </>
    );
}
