import { AlertCircleIcon } from "lucide-react";
import { Alert, AlertTitle } from "../ui/alert";

export default function FormError({ errorMessage }: { errorMessage: string }) {

    return (
        <Alert variant="destructive">
            <AlertCircleIcon />
            <AlertTitle>{errorMessage}</AlertTitle>
        </Alert>
    )
}