
import { Button } from "@/components/ui/button"
import {
    Card,
    CardContent,
    CardDescription,
    CardHeader,
    CardTitle,
} from "@/components/ui/card"
import { Input } from "@/components/ui/input"
import { UserContext } from "@/contexts/UserContext"
import { useContext } from "react"
import { useForm } from "react-hook-form"
import Home from "../home/Home"
import {
    Form,
    FormField,
    FormItem,
    FormLabel,
    FormControl,
    FormMessage,
} from "@/components/ui/form"
import { Link } from "react-router-dom"
import PasswordField from "./password-field/PasswordField"


export function Login() {
    const { login, user } = useContext(UserContext)

    type FormValues = {
        email: string
        password: string
    }

    const form = useForm<FormValues>({
        defaultValues: { email: "", password: "" },
    })

    const onSubmit = async (values: FormValues) => {
        await login(values.email, values.password)
    }

    return (
        <div className="flex flex-col gap-6">
            {user && <Home />}
            <Card>
                <CardHeader>
                    <CardTitle>Login to your account</CardTitle>
                    <CardDescription>
                        Enter your email and password below to login to your account
                    </CardDescription>
                </CardHeader>
                <CardContent>
                    <Form {...form}>
                        <form onSubmit={form.handleSubmit(onSubmit)}>
                            <div className="flex flex-col gap-6">
                                <FormField
                                    control={form.control}
                                    name="email"
                                    rules={{
                                        required: "Email is required",
                                        pattern: { value: /^[^\s@]+@[^\s@]+\.[^\s@]+$/, message: "Enter a valid email" },
                                    }}
                                    render={({ field }) => (
                                        <FormItem>
                                            <FormLabel>Email</FormLabel>
                                            <FormControl>
                                                <Input placeholder="m@example.com" {...field} autoComplete="email" />
                                            </FormControl>
                                            <FormMessage />
                                        </FormItem>
                                    )}
                                />

                                <PasswordField
                                    form={form}
                                />

                                <div className="flex flex-col gap-3">
                                    <Button type="submit" className="w-full">
                                        Login
                                    </Button>
                                </div>
                            </div>

                            <div className="mt-4 text-center text-sm">
                                Don&apos;t have an account?{" "}
                                <Link to="/register" className="underline underline-offset-4">
                                    Sign up
                                </Link>
                            </div>
                        </form>
                    </Form>
                </CardContent>
            </Card>
        </div>
    )
}
