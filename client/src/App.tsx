import './App.css'
import { useEffect } from 'react'
import { Login } from './pages/auth/Login'
import { UserProvider } from './contexts/UserContext'
import { Route, Routes } from 'react-router-dom'
import { Register } from './pages/auth/Register'

function App() {

    useEffect(() => {
        // Force dark theme for shadcn components by adding the `dark` class
        // to the document root on mount.
        document.documentElement.classList.add('dark')
    }, [])

    return (
        <UserProvider>
            <div className="app-root">
                <div className="app-container">
                    <Routes>
                        <Route path='/login' element={<Login />} />
                        <Route path='/register' element={<Register />} />
                    </Routes>
                </div>
            </div>
        </UserProvider>
    )
}

export default App
