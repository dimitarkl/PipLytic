
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
import { Eye, EyeOff } from "lucide-react"
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
import { handleError } from "@/utils/errors"

type FormValues = {
    email: string
    password: string
    confirmPassword: string
}

export function Register() {
    const { register: registerUser } = useContext(UserContext)
    const navigate = useNavigate()

    const [showConfirmPassword, setShowConfirmPassword] = useState(false)
    const [responseError, setResponseError] = useState<string | undefined>()
    const form = useForm<FormValues>({
        defaultValues: {
            email: "",
            password: "",
            confirmPassword: "",
        },
        mode: "onChange",
    })

    const onSubmit = async (values: FormValues) => {
        setResponseError(undefined)
        try {
            await registerUser(values.email, values.password)
            navigate('/home')
        } catch (error) {
            setResponseError(handleError(error) ?? "An unexpected error occurred. Please try again.")

        }
    }

    return (
        <div className="flex flex-col gap-6 max-w-md w-full">
            <Card>
                <CardHeader>
                    <CardTitle>Register</CardTitle>
                    <CardDescription>
                        Enter email and password below to register
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
                                        message: "Enter a valid email",
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

                            <PasswordField form={form} isLogin={false} />

                            <FormField
                                control={form.control}
                                name="confirmPassword"
                                rules={{
                                    required: "Please confirm your password",
                                    validate: (value: string) =>
                                        value === form.getValues("password") || "Passwords do not match",
                                }}
                                render={({ field, fieldState }) => {
                                    return (
                                        <FormItem>
                                            <FormLabel>Confirm Password</FormLabel>
                                            <FormControl>
                                                <div className="relative">
                                                    <Input
                                                        placeholder="*********"
                                                        type={showConfirmPassword ? "text" : "password"}
                                                        {...field}
                                                        autoComplete="new-password"
                                                        className="pr-10"
                                                        aria-invalid={!!fieldState.error}
                                                    />
                                                    <Button
                                                        variant="ghost"
                                                        type="button"
                                                        onClick={() => setShowConfirmPassword(prev => !prev)}
                                                        aria-label={showConfirmPassword ? "Hide password" : "Show password"}
                                                        className="absolute right-2 top-1/2 -translate-y-1/2 h-8 w-8 p-0 inline-flex items-center justify-center"
                                                    >
                                                        {showConfirmPassword ? <EyeOff className="h-4 w-4" /> : <Eye className="h-4 w-4" />}
                                                    </Button>
                                                </div>
                                            </FormControl>
                                            <FormMessage />
                                        </FormItem>
                                    )
                                }}
                            />
                            {responseError && <FormError errorMessage={responseError} />}
                            <Button
                                type="submit"
                                className="w-full"
                                disabled={!form.formState.isValid}
                                variant={form.formState.isValid ? "default" : "secondary"}
                            >
                                Register
                            </Button>

                            <div className="text-center text-sm">
                                Already have an account?{" "}
                                <Link to="/login" className="underline underline-offset-4">
                                    Log in
                                </Link>
                            </div>
                        </form>
                    </Form>
                </CardContent>
            </Card>
        </div>
    )
}
