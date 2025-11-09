import { LogIn, LogOut, User, BarChart3 } from "lucide-react";
import { Button } from "../ui/button";
import { useContext } from "react";
import { UserContext } from "@/contexts/UserContext";
import { Link, useNavigate } from "react-router-dom";
import { ThemeToggle } from "../theme-toggle/ThemToggle";

export default function Header() {
    const { user, logout } = useContext(UserContext)
    const navigate = useNavigate()

    const logoutUser = async () => {
        await logout()
        navigate('')
    }

    return (
        <header className="w-full fixed px-2 sm:px-4 top-0 z-50 border-b bg-background/95 backdrop-blur supports-[backdrop-filter]:bg-background/60 flex h-14 items-center">
            {/* Logo/Brand */}
            <Link to="/" className="mr-2 sm:mr-6 flex items-center space-x-2">
                {/* <BarChart3 className="h-5 w-5 sm:h-6 sm:w-6 text-primary" /> */}
                <div className="h-10 w-10">
                    <img src="/piplytic-nobg.png" alt="PipLytic Logo" className="w-full" />
                </div>
                <span className="font-bold text-xl inline-block">
                    PipLytic
                </span>
            </Link>
            {user && <nav className="hidden md:flex items-center gap-6 text-sm">
                <Link
                    to="/"
                    className="transition-colors hover:text-foreground/80 text-foreground/60"
                >
                    Home
                </Link>
            </nav>
            }

            <div className="flex flex-1 items-center justify-end space-x-1 sm:space-x-2">
                {/* Auth Buttons */}
                <ThemeToggle />
                <div className="flex items-center space-x-1 sm:space-x-2">
                    {!user ? (
                        <>
                            <Link to="/login">
                                <Button variant="ghost" size="sm" className="h-9 px-2 sm:px-3">
                                    <LogIn className="h-4 w-4 sm:mr-2" />
                                    <span className="hidden sm:inline">Login</span>
                                </Button>
                            </Link>
                        </>
                    ) : (
                        <div className="flex items-center space-x-1 sm:space-x-2">
                            <Link to="/user" className="hidden sm:inline text-sm text-muted-foreground underline truncate max-w-[120px] md:max-w-[200px]">
                                Welcome, {user.email}
                            </Link>
                            <Link to="/user" className="sm:hidden">
                                <Button variant="ghost" size="sm" className="h-9 px-2">
                                    <User className="h-4 w-4" />
                                </Button>
                            </Link>
                            <Button
                                variant="ghost"
                                size="sm"
                                onClick={logoutUser}
                                className="h-9 px-2 sm:px-3"
                            >
                                <LogOut className="h-4 w-4 sm:mr-2" />
                                <span className="hidden sm:inline">Logout</span>
                            </Button>
                        </div>
                    )}

                </div>
            </div>
        </header>
    )
}