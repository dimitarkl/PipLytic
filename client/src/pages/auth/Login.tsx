
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
import { useContext, useState } from "react"
import { useForm } from "react-hook-form"
import {
    Form,
    FormField,
    FormItem,
    FormLabel,
    FormControl,
    FormMessage,
} from "@/components/ui/form"
import { Link, useNavigate } from "react-router-dom"
import PasswordField from "./password-field/PasswordField"
import FormError from "@/components/form-error/FormError"
import { AxiosError } from "axios"

type FormValues = {
    email: string
    password: string
}

export function Login() {
    const { login } = useContext(UserContext)
    const navigate = useNavigate()

    const form = useForm<FormValues>({
        defaultValues: { email: "", password: "" },
        mode: "onChange",
    })

    const [responseError, setResponseError] = useState<string | undefined>()

    const onSubmit = async (values: FormValues) => {
        setResponseError(undefined)
        try {
            await login(values.email, values.password)
            navigate('/')
        } catch (error) {
            if (error instanceof AxiosError && error.response?.status === 400) {
                setResponseError(error.response.data)
            } else {
                setResponseError("An unexpected error occurred. Please try again.")
            }
        }
    }

    return (
        <div className="flex flex-col gap-6 max-w-md w-full">
            <Card>
                <CardHeader>
                    <CardTitle>Login to your account</CardTitle>
                    <CardDescription>
                        Enter your email and password below to login to your account
                    </CardDescription>
                </CardHeader>

                <CardContent>
                    <Form {...form}>
                        <form onSubmit={form.handleSubmit(onSubmit)} className="flex flex-col gap-6">
                            <FormField
                                control={form.control}
                                name="email"
                                rules={{
                                    required: "Email is required",
                                    pattern: {
                                        value: /^[^\s@]+@[^\s@]+\.[^\s@]+$/,
                                        message: "Enter a valid email"
                                    },
                                }}
                                render={({ field }) => (
                                    <FormItem>
                                        <FormLabel>Email</FormLabel>
                                        <FormControl>
                                            <Input
                                                placeholder="m@example.com"
                                                {...field}
                                                autoComplete="email"
                                            />
                                        </FormControl>
                                        <FormMessage />
                                    </FormItem>
                                )}
                            />

                            <PasswordField form={form} isLogin={true} />
                            {responseError && <FormError errorMessage={responseError} />}
                            <Button
                                type="submit"
                                className="w-full"
                                disabled={!form.formState.isValid}
                                variant={form.formState.isValid ? "default" : "secondary"}
                            >
                                Login
                            </Button>

                            <div className="text-center text-sm">
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
