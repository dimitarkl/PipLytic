import './App.css'
import { useEffect } from 'react'
import { UserProvider } from './contexts/UserContext'
import Router from './Router'
import Header from './components/header/Header'
import { ThemeProvider } from './contexts/ThemeProvider'

function App() {

    useEffect(() => {
        // Force dark theme for shadcn components by adding the `dark` class
        // to the document root on mount.
        document.documentElement.classList.add('dark')
    }, [])

    return (
        <ThemeProvider defaultTheme="dark" storageKey="vite-ui-theme">
            <UserProvider>
                <div className="app-root">
                    <Header />
                    <div className="app-container">
                        <Router />
                    </div>
                </div>
            </UserProvider>
        </ThemeProvider>
    )
}

export default App
