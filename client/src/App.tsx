import './App.css'
import { useEffect } from 'react'
import { UserProvider } from './contexts/UserContext'
import Router from './Router'
import Header from './components/header/Header'

function App() {

    useEffect(() => {
        // Force dark theme for shadcn components by adding the `dark` class
        // to the document root on mount.
        document.documentElement.classList.add('dark')
    }, [])

    return (
        <UserProvider>
            <div className="app-root">
                <Header />
                <main className="app-container">
                    <Router />
                </main>
            </div>
        </UserProvider>
    )
}

export default App
