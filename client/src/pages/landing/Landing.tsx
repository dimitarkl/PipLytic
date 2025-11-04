import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import { Link } from "react-router-dom";
import { TrendingUp, BarChart3, DollarSign, Shield, Zap } from "lucide-react";

export default function Landing() {
    return (
        <div className="min-h-screen bg-gradient-to-b from-background to-muted/20">
            {/* Hero Section */}
            <div className="container mx-auto px-4 py-12 sm:py-16">
                <div className="text-center max-w-4xl mx-auto space-y-6">
                    <div className="space-y-3">
                        <h1 className="text-4xl sm:text-5xl md:text-6xl font-bold tracking-tight">
                            Master Trading with{" "}
                            <span className="text-primary">PipLytic</span>
                        </h1>
                        <p className="text-lg sm:text-xl text-muted-foreground max-w-2xl mx-auto">
                            Practice trading with real market data. Build confidence before risking real money.
                        </p>
                    </div>

                    <div className="flex flex-col sm:flex-row gap-3 justify-center items-center">
                        <Link to="/register">
                            <Button size="lg" className="px-8 w-full sm:w-auto">
                                Get Started Free
                            </Button>
                        </Link>
                        <Link to="/demo">
                            <Button size="lg" variant="outline" className="px-8 w-full sm:w-auto">
                                Try Demo
                            </Button>
                        </Link>
                    </div>

                    <div className="flex flex-wrap justify-center gap-6 pt-6 text-sm text-muted-foreground">
                        <div className="flex items-center gap-2">
                            <Shield className="w-4 h-4" />
                            <span>Risk-Free</span>
                        </div>
                        <div className="flex items-center gap-2">
                            <TrendingUp className="w-4 h-4" />
                            <span>Real Data</span>
                        </div>
                        <div className="flex items-center gap-2">
                            <Zap className="w-4 h-4" />
                            <span>Instant</span>
                        </div>
                    </div>
                </div>
            </div>

            {/* Features Section */}
            <div className="container mx-auto px-4 py-12">
                <div className="text-center mb-10">
                    <h2 className="text-2xl sm:text-3xl font-bold mb-3">
                        Everything You Need to Learn Trading
                    </h2>
                    <p className="text-muted-foreground max-w-2xl mx-auto">
                        Complete trading simulations with ease
                    </p>
                </div>

                <div className="grid grid-cols-1 md:grid-cols-3 gap-4 max-w-5xl mx-auto">
                    <Card className="border-2 hover:border-primary/50 transition-colors">
                        <CardContent className="pt-6 space-y-3">
                            <div className="flex gap-2 items-center">
                                <div className="w-10 h-10 rounded-lg bg-primary/10 flex items-center justify-center">
                                    <TrendingUp className="w-5 h-5 text-primary" />
                                </div>
                                <h3 className="text-lg font-semibold">Real-Time Charts</h3>
                            </div>
                            <p className="text-sm text-muted-foreground">
                                Professional charts with multiple timeframes and live price tracking.
                            </p>
                        </CardContent>
                    </Card>

                    <Card className="border-2 hover:border-primary/50 transition-colors">
                        <CardContent className="pt-6 space-y-3">
                            <div className="flex gap-2 items-center">
                                <div className="w-10 h-10 rounded-lg bg-primary/10 flex items-center justify-center">
                                    <DollarSign className="w-5 h-5 text-primary" />
                                </div>
                                <h3 className="text-lg font-semibold">Long & Short Trading</h3>
                            </div>
                            <p className="text-sm text-muted-foreground">
                                Practice both long and short positions without financial risk.
                            </p>
                        </CardContent>
                    </Card>

                    <Card className="border-2 hover:border-primary/50 transition-colors">
                        <CardContent className="pt-6 space-y-3">
                            <div className="flex gap-2 items-center">
                                <div className="w-10 h-10 rounded-lg bg-primary/10 flex items-center justify-center">
                                    <BarChart3 className="w-5 h-5 text-primary" />
                                </div>
                                <h3 className="text-lg font-semibold">Trading History</h3>
                            </div>
                            <p className="text-sm text-muted-foreground">
                                Track performance and review trades to improve your strategy.
                            </p>
                        </CardContent>
                    </Card>
                </div>
            </div>

            {/* CTA Section */}
            <div className="container mx-auto px-4 py-12">
                <Card className="max-w-3xl mx-auto border-2 border-primary/20">
                    <CardContent className="pt-10 pb-10 text-center space-y-5">
                        <h2 className="text-2xl sm:text-3xl font-bold">
                            Ready to Start Trading?
                        </h2>
                        <p className="text-muted-foreground max-w-xl mx-auto">
                            Join PipLytic today and learn to trade with confidence. No risk, just learning.
                        </p>
                        <div className="flex flex-col sm:flex-row gap-3 justify-center items-center pt-2">
                            <Link to="/register">
                                <Button size="lg" className="px-8 w-full sm:w-auto">
                                    Sign Up Now
                                </Button>
                            </Link>
                            <Link to="/login">
                                <Button size="lg" variant="outline" className="px-8 w-full sm:w-auto">
                                    Login
                                </Button>
                            </Link>
                        </div>
                    </CardContent>
                </Card>
            </div>

            {/* Footer */}
            <footer className="border-t">
                <div className="container mx-auto px-4 py-6">
                    <div className="text-center text-sm text-muted-foreground">
                        <p>&copy; 2025 PipLytic. All rights reserved.</p>
                    </div>
                </div>
            </footer>
        </div>
    );
}
