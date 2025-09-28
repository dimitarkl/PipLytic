import { LogIn, LogOut, User, BarChart3 } from "lucide-react";
import { Button } from "../ui/button";
import { useContext } from "react";
import { UserContext } from "@/contexts/UserContext";
import { Link } from "react-router-dom";
import { ThemeToggle } from "../theme-toggle/ThemToggle";

export default function Header() {
    const { user, logout } = useContext(UserContext)
    
    return (
        <header className="w-full fixed px-4 top-0 z-50 border-b bg-background/95 backdrop-blur supports-[backdrop-filter]:bg-background/60 flex h-14 items-center">
            {/* Logo/Brand */}
            <Link to="/" className="mr-6 flex items-center space-x-2">
                <BarChart3 className="h-6 w-6 text-primary" />
                <span className="hidden font-bold sm:inline-block">
                    PipLytic
                </span>
            </Link>

            {/* Navigation */}
            <nav className="flex items-center gap-6 text-sm">
                <Link
                    to="/"
                    className="transition-colors hover:text-foreground/80 text-foreground/60"
                >
                    Home
                </Link>
            </nav>

            <div className="flex flex-1 items-center justify-between space-x-2 md:justify-end">
                {/* Auth Buttons */}
                <ThemeToggle />
                <div className="flex items-center space-x-2">
                    {!user ? (
                        <>
                            <Link to="/login">
                                <Button variant="ghost" size="sm">
                                    <LogIn className="h-4 w-4 mr-2" />
                                    Login
                                </Button>
                            </Link>
                            <Link to="/register">
                                <Button size="sm">
                                    <User className="h-4 w-4 mr-2" />
                                    Register
                                </Button>
                            </Link>
                        </>
                    ) : (
                        <div className="flex items-center space-x-2">
                            <Link to="/user" className="text-sm text-muted-foreground underline">
                                Welcome, {user.email}
                            </Link >
                            <Button
                                variant="ghost"
                                size="sm"
                                onClick={logout}
                            >
                                <LogOut className="h-4 w-4 mr-2" />
                                Logout
                            </Button>
                        </div>
                    )}

                </div>
            </div>
        </header>
    )
}