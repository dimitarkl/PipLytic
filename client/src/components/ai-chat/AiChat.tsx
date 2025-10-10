import { Button } from '@/components/ui/button';
import { Card } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { api, API_URL } from '@/lib/api';
import { Send } from 'lucide-react';
import { useState, useRef, useEffect } from 'react';
import ReactMarkdown from 'react-markdown';

type Message = {
    index: number,
    role: 'user' | 'assistant';
    message: string;
};
type AiChatProps = {
    symbol?: string;
    endDate?: number;
};

export default function AiChat({ symbol = "IBM", endDate }: AiChatProps) {
    const [messages, setMessages] = useState<Message[]>([
        {
            index: -1,
            role: 'assistant',
            message: `Hello! I'm your AI trading assistant. I can help you analyze market trends for ${symbol}. Ask me anything about the stock!`,
        }
    ]);
    const [inputMessage, setInputMessage] = useState('');
    const [isLoading, setIsLoading] = useState(false);
    const messagesEndRef = useRef<HTMLDivElement>(null);

    const scrollToBottom = () => {
        messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
    };

    useEffect(() => {
        const getChatHistory = async () => {
            const response = await api.get(`${API_URL}/ai-chat`);
            if (response.data.length > 0)
                setMessages(response.data)
        }
        getChatHistory()
    }, [])

    useEffect(() => scrollToBottom(), [messages]);

    const handleSendMessage = async () => {
        if (!inputMessage.trim() || isLoading) return;

        const messageText = inputMessage;

        setMessages(prev => {
            const userMessage: Message = {
                index: prev.length,
                role: 'user',
                message: messageText,
            };
            return [...prev, userMessage];
        });

        setInputMessage('');
        setIsLoading(true);

        try {
            const response = await api.post<Message>(`${API_URL}/ai-chat`, {
                symbol: symbol,
                message: messageText,
                endDate: endDate || Date.now()
            });

            setMessages(prev => {
                const assistantIndex = response?.data?.index ?? prev.length;
                const assistantMessage: Message = {
                    index: assistantIndex,
                    role: 'assistant',
                    message: response.data.message || 'Sorry, I could not process that request.',
                };
                return [...prev, assistantMessage];
            });
        } catch (error: any) {
            setMessages(prev => {
                const errorMessage: Message = {
                    index: prev.length,
                    role: 'assistant',
                    message: 'Error: ' + error.response.data.message || 'Sorry, I encountered an error. Please try again.',
                };
                return [...prev, errorMessage];
            });
        } finally {
            setIsLoading(false);
        }
    };

    const handleKeyPress = (e: React.KeyboardEvent) => {
        if (e.key === 'Enter' && !e.shiftKey) {
            e.preventDefault();
            handleSendMessage();
        }
    };
    const quickPrompts = [
        "What's the trend?",
        "Explain this chart in one sentence",
        "Why did volume spike?",
        "Key support level?",
        "Key resistance level?",
        "One quick action I can take"
    ];

    const handleQuickPrompt = (prompt: string) => {
        setInputMessage(prompt);
    };
    return (
        <Card className="p-3 sm:p-6 flex flex-col h-full">
            {/* Messages Area */}
            <div className="flex-1 space-y-3 sm:space-y-4 mb-3 sm:mb-4 min-h-0 overflow-y-auto">
                {messages.map((message) => (
                    <div
                        key={message.index}
                        className={
                            `flex ${message.role === 'user'
                                ? 'justify-end'
                                : 'justify-start'}`}
                    >
                        <div
                            className={`max-w-[85%] rounded-lg p-3 ${message.role === 'user'
                                ? 'bg-primary text-primary-foreground'
                                : 'bg-muted border border-border'
                                }`}
                        >
                            {message.role === 'assistant' ? (
                                <div className="prose text-sm">
                                    <ReactMarkdown>{message.message}</ReactMarkdown>
                                </div>
                            ) : (
                                <p className="text-sm whitespace-pre-wrap break-words">{message.message}</p>
                            )}

                        </div>
                    </div>
                ))}
                {isLoading && (
                    <div className="flex justify-start">
                        <div className="bg-muted border border-border rounded-lg p-3">
                            <div className="flex space-x-2">
                                <div className="w-2 h-2 bg-primary rounded-full animate-bounce" style={{ animationDelay: '0ms' }}></div>
                                <div className="w-2 h-2 bg-primary rounded-full animate-bounce" style={{ animationDelay: '150ms' }}></div>
                                <div className="w-2 h-2 bg-primary rounded-full animate-bounce" style={{ animationDelay: '300ms' }}></div>
                            </div>
                        </div>
                    </div>
                )}
                <div ref={messagesEndRef} />
            </div>

            {/* Quick Prompts */}
            {messages.length <= 1 && (
                <div className="mb-3 sm:mb-4 pb-3 border-b flex-shrink-0">
                    <p className="text-xs text-muted-foreground mb-2">Quick prompts:</p>
                    <div className="flex flex-wrap gap-2">
                        {quickPrompts.map((prompt, index) => (
                            <Button
                                key={index}
                                variant="outline"
                                size="sm"
                                onClick={() => handleQuickPrompt(prompt)}
                                className="text-xs h-7"
                            >
                                {prompt}
                            </Button>
                        ))}
                    </div>
                </div>
            )}

            {/* Input Area */}
            <div className="mt-auto flex-shrink-0">
                <div className="flex gap-2">
                    <Input
                        value={inputMessage}
                        onChange={(e) => setInputMessage(e.target.value)}
                        onKeyPress={handleKeyPress}
                        placeholder="Ask about trends, analysis..."
                        className="flex-1"
                        disabled={isLoading}
                    />
                    <Button
                        onClick={handleSendMessage}
                        disabled={!inputMessage.trim() || isLoading}
                        size="icon"
                    >
                        <Send className="h-4 w-4" />
                    </Button>
                </div>
            </div>
        </Card>
    );
}
