import { LogIn, LogOut, User, BarChart3 } from "lucide-react";
import { Button } from "../ui/button";
import { useContext } from "react";
import { UserContext } from "@/contexts/UserContext";
import { Link } from "react-router-dom";

export default function Header() {
    const { user, logout } = useContext(UserContext)

    return (
        <header className="border-b border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900">
            <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
                <div className="flex justify-between items-center h-16">
                    {/* Logo/Brand */}
                    <Link to="/" className="flex items-center space-x-2">
                        <BarChart3 className="h-8 w-8 text-blue-600" />
                        <span className="text-xl font-bold text-gray-900 dark:text-white">
                            Chart Choice
                        </span>
                    </Link>

                    {/* Navigation */}
                    <nav className="hidden md:flex items-center space-x-8">
                        <Link 
                            to="/" 
                            className="text-gray-700 dark:text-gray-300 hover:text-blue-600 dark:hover:text-blue-400 transition-colors"
                        >
                            Home
                        </Link>
                        {user && (
                            <Link 
                                to="/dashboard" 
                                className="text-gray-700 dark:text-gray-300 hover:text-blue-600 dark:hover:text-blue-400 transition-colors"
                            >
                                Dashboard
                            </Link>
                        )}
                    </nav>

                    {/* Auth Buttons */}
                    <div className="flex items-center space-x-4">
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
                            <div className="flex items-center space-x-4">
                                <span className="text-sm text-gray-700 dark:text-gray-300">
                                    Welcome, {user.email}
                                </span>
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
            </div>
        </header>
    )
}