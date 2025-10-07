import { Navigate, Route, Routes } from "react-router-dom";
import { Login } from "./pages/auth/Login";
import { Register } from "./pages/auth/Register";
import { ReactNode, useContext } from "react";
import { UserContext } from "./contexts/UserContext";
import Home from "./pages/home/Home";
import Spinner from "./components/ui/spinner";
import UserPage from "./pages/user-page/UserPage";

export default function Router() {

    const { user } = useContext(UserContext)

    const authGuard = (children: ReactNode) => {
        return user
            ? children
            : <Navigate to="/login" />
    }

    const guestGuard = (children: ReactNode) => {
        return user === null
            ? children
            : <Navigate to="/" />
    };

    return (
        <>{user !== undefined
            ? <Routes>
                <Route path="/login" element={guestGuard(<Login />)} />
                <Route path="/" element={authGuard(<Home />)} />
                <Route path="/register" element={guestGuard(<Register />)} />
                <Route path="/user" element={authGuard(<UserPage />)} />
            </Routes>
            : <Spinner />
        }
        </>
    )
}