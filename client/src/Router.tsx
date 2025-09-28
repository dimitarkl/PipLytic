import { Navigate, Route, Routes } from "react-router-dom";
import { Login } from "./pages/auth/Login";
import { Register } from "./pages/auth/Register";
import { useContext } from "react";
import { UserContext } from "./contexts/UserContext";
import Home from "./pages/home/Home";
import Spinner from "./components/ui/spinner";
import UserPage from "./pages/user-page/UserPage";

export default function Router() {

    const { user } = useContext(UserContext)

    return (
        <>{user !== undefined
            ? <Routes>
                <Route path="/login" element={user ? <Navigate to="/" /> : <Login />} />
                <Route path="/" element={user ? <Home /> : <Navigate to="/login" />} />
                <Route path="/register" element={<Register />} />
                <Route path="/user" element={<UserPage />} />
            </Routes>
            : <Spinner />
        }
        </>
    )
}