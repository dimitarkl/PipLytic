
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
import { useState } from "react"
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
import { Link } from "react-router-dom"


export function Register() {
    const { register: registerUser } = useContext(UserContext)
    const [showPassword, setShowPassword] = useState(false)
    const [showConfirm, setShowConfirm] = useState(false)

    type FormValues = {
        email: string
        password: string
        confirmPassword: string
    }

    const form = useForm<FormValues>({
        defaultValues: {
            email: "",
            password: "",
            confirmPassword: "",
        },
    })

    const onSubmit = async (values: FormValues) => {
        if (values.password !== values.confirmPassword) {
            form.setError("confirmPassword", { message: "Passwords do not match" })
            return
        }

        await registerUser(values.email, values.password)
    }

    return (
        <div className="flex flex-col gap-6 max-w-md w-full">
            <Card>
                <CardHeader>
                    <CardTitle>Register</CardTitle>
                    <CardDescription>Enter email and password below to register</CardDescription>
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

                                <FormField
                                    control={form.control}
                                    name="password"
                                    rules={{
                                        validate: (value: string) => {
                                            const checks = [
                                                { ok: (value?.length ?? 0) >= 8, msg: "Minimum 8 characters (recommend 12+ for strong security)." },
                                                { ok: /[A-Z]/.test(value || ""), msg: "At least one uppercase letter (A–Z)." },
                                                { ok: /[a-z]/.test(value || ""), msg: "At least one lowercase letter (a–z)." },
                                                { ok: /[0-9]/.test(value || ""), msg: "At least one number (0–9)." },
                                            ]

                                            const failed = checks.filter(c => !c.ok).map(c => c.msg)

                                            if (failed.length === 0) return true

                                            // return the requested error text as a single string
                                            return failed.join("\n\n")
                                        },
                                    }}
                                    render={({ field }) => {
                                        const value = field.value || ""
                                        const lengthOk = value.length >= 8
                                        const upperOk = /[A-Z]/.test(value)
                                        const lowerOk = /[a-z]/.test(value)
                                        const numberOk = /[0-9]/.test(value)

                                        return (
                                            <FormItem>
                                                <FormLabel>Password</FormLabel>
                                                <FormControl>
                                                    <div className="relative">
                                                        <Input
                                                            placeholder="*********"
                                                            type={showPassword ? "text" : "password"}
                                                            {...field}
                                                            autoComplete="new-password"
                                                            className="pr-10"
                                                        />
                                                            <Button
                                                                variant="ghost"
                                                                type="button"
                                                                onClick={() => setShowPassword(s => !s)}
                                                                aria-label={showPassword ? "Hide password" : "Show password"}
                                                                className="absolute right-2 top-1/2 -translate-y-1/2 h-8 w-8 p-0 inline-flex items-center justify-center"
                                                            >
                                                                {showPassword ? <EyeOff className="h-4 w-4" /> : <Eye className="h-4 w-4" />}
                                                            </Button>
                                                    </div>
                                                </FormControl>
                                                <FormMessage />

                                                {value.length > 0 && (
                                                    <ul className="mt-2 list-disc pl-5 space-y-1 text-sm">
                                                        <li className={lengthOk ? "text-green-600" : "text-destructive"}>
                                                            Minimum 8 characters (recommend 12+ for strong security).
                                                        </li>
                                                        <li className={upperOk ? "text-green-600" : "text-destructive"}>
                                                            At least one uppercase letter (A–Z).
                                                        </li>
                                                        <li className={lowerOk ? "text-green-600" : "text-destructive"}>
                                                            At least one lowercase letter (a–z).
                                                        </li>
                                                        <li className={numberOk ? "text-green-600" : "text-destructive"}>
                                                            At least one number (0–9).
                                                        </li>
                                                    </ul>
                                                )}
                                            </FormItem>
                                        )
                                    }}
                                />

                                <FormField
                                    control={form.control}
                                    name="confirmPassword"
                                    rules={{
                                        required: "Please confirm your password",
                                        validate: (value: string) =>
                                            value === form.getValues("password") || "Passwords do not match",
                                    }}
                                    render={({ field }) => (
                                        <FormItem>
                                            <FormLabel>Repeat password</FormLabel>
                                            <FormControl>
                                                <div className="relative">
                                                    <Input
                                                        placeholder="*********"
                                                        type={showConfirm ? "text" : "password"}
                                                        {...field}
                                                        autoComplete="new-password"
                                                        className="pr-10"
                                                    />
                                                    <Button
                                                        variant="ghost"
                                                        type="button"
                                                        onClick={() => setShowConfirm(s => !s)}
                                                        aria-label={showConfirm ? "Hide password" : "Show password"}
                                                        className="absolute right-2 top-1/2 -translate-y-1/2 h-8 w-8 p-0 inline-flex items-center justify-center"
                                                    >
                                                        {showConfirm ? <EyeOff className="h-4 w-4" /> : <Eye className="h-4 w-4" />}
                                                    </Button>
                                                </div>
                                            </FormControl>
                                            <FormMessage />
                                        </FormItem>
                                    )}
                                />

                                <div className="flex flex-col gap-3">
                                    <Button type="submit" className="w-full">
                                        Register
                                    </Button>
                                </div>
                            </div>

                            <div className="mt-4 text-center text-sm">
                                Already have an account?{" "}
                                <Link to="/login"className="underline underline-offset-4">
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
