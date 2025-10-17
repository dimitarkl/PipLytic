import { createContext, useContext, useEffect, useState, ReactNode } from 'react';

type SymbolContextType = {
    symbol: string;
    setSymbol: (symbol: string) => void;
};

const SymbolContext = createContext<SymbolContextType | undefined>(undefined);

const STORAGE_KEY = 'selectedSymbol';
const DEFAULT_SYMBOL = 'IBM';

export function SymbolProvider({ children }: { children: ReactNode }) {
    const [symbol, setSymbolState] = useState<string>(DEFAULT_SYMBOL);
    const [isHydrated, setIsHydrated] = useState(false);

    useEffect(() => {
        const stored = localStorage.getItem(STORAGE_KEY);
        if (stored) {
            setSymbolState(stored);
        }
        setIsHydrated(true);
    }, []);

    useEffect(() => {
        if (isHydrated) {
            localStorage.setItem(STORAGE_KEY, symbol);
        }
    }, [symbol, isHydrated]);

    const setSymbol = (newSymbol: string) => {
        setSymbolState(newSymbol);
    };

    return (
        <SymbolContext.Provider value={{ symbol, setSymbol }}>
            {children}
        </SymbolContext.Provider>
    );
}

export function useSymbol() {
    const context = useContext(SymbolContext);
    if (context === undefined) {
        throw new Error('useSymbol must be used within a SymbolProvider');
    }
    return context;
}
