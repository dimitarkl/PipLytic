import { FormControl, FormField, FormItem, FormLabel, FormMessage } from "@/components/ui/form"
import { Input } from "@/components/ui/input"
import { Button } from "@/components/ui/button"
import { UseFormReturn } from "react-hook-form"
import { useState } from "react"
import { Eye, EyeOff } from "lucide-react"

type PasswordFieldProps = {
    form: UseFormReturn<any, any, any>
    isLogin?: boolean
}

export default function PasswordField({ form, isLogin = false }: PasswordFieldProps) {
    const [showPassword, setShowPassword] = useState(false)

    const getValidationRules = () => {
        if (isLogin) {
            return {
                required: "Password is required",
                minLength: { value: 8, message: "Password must be at least 8 characters" }
            }
        }

        return {
            validate: (value: string) => {
                if (!value) return "Password is required"
                
                const checks = [
                    { ok: value.length >= 8, msg: "Minimum 8 characters (recommend 12+ for strong security)." },
                    { ok: /[A-Z]/.test(value), msg: "At least one uppercase letter (A–Z)." },
                    { ok: /[a-z]/.test(value), msg: "At least one lowercase letter (a–z)." },
                    { ok: /[0-9]/.test(value), msg: "At least one number (0–9)." },
                ]

                const failed = checks.filter(c => !c.ok).map(c => c.msg)
                return failed.length === 0 ? true : failed.join("\n\n")
            },
        }
    }

    const renderPasswordStrengthIndicators = (value: string) => {
        if (isLogin || value.length === 0) return null

        const lengthOk = value.length >= 8
        const upperOk = /[A-Z]/.test(value)
        const lowerOk = /[a-z]/.test(value)
        const numberOk = /[0-9]/.test(value)

        return (
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
        )
    }

    return (
        <FormField
            control={form.control}
            name="password"
            rules={getValidationRules()}
            render={({ field, fieldState }) => {
                const value = field.value || ""

                return (
                    <FormItem>
                        <div className="flex items-center">
                            <FormLabel>Password</FormLabel>
                            {isLogin && (
                                <a
                                    href="#"
                                    className="ml-auto inline-block text-sm underline-offset-4 hover:underline"
                                >
                                    Forgot your password?
                                </a>
                            )}
                        </div>

                        <FormControl>
                            <div className="relative">
                                <Input 
                                    placeholder="*********" 
                                    type={showPassword ? "text" : "password"} 
                                    {...field} 
                                    autoComplete={isLogin ? "current-password" : "new-password"}
                                    className="pr-10"
                                    aria-invalid={!!fieldState.error}
                                />
                                <Button
                                    variant="ghost"
                                    type="button"
                                    onClick={() => setShowPassword(prev => !prev)}
                                    aria-label={showPassword ? "Hide password" : "Show password"}
                                    className="absolute right-2 top-1/2 -translate-y-1/2 h-8 w-8 p-0 inline-flex items-center justify-center"
                                >
                                    {showPassword ? <EyeOff className="h-4 w-4" /> : <Eye className="h-4 w-4" />}
                                </Button>
                            </div>
                        </FormControl>

                        {isLogin && <FormMessage />}
                        {renderPasswordStrengthIndicators(value)}
                    </FormItem>
                )
            }}
        />
    )
}