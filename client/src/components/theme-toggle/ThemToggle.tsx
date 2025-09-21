"use client"

import * as React from "react"
import { Moon, Sun } from "lucide-react"
import { useTheme } from "@/contexts/ThemeProvider"
import { Switch } from "@/components/ui/switch"
import { Label } from "@/components/ui/label"

export function ThemeToggle() {
    const { theme, setTheme } = useTheme()
    const [mounted, setMounted] = React.useState(false)

    React.useEffect(() => {
        setMounted(true)
    }, [])

    if (!mounted) {
        return null
    }

    const isDark = theme === "dark"

    const handleToggle = (checked: boolean) => {
        setTheme(checked ? "dark" : "light")
    }

    return (
        <div className="flex items-center space-x-2">
            <Sun className="h-4 w-4" />
            <Switch
                id="theme-toggle"
                checked={isDark}
                onCheckedChange={handleToggle}
                aria-label="Toggle theme"
            />
            <Moon className="h-4 w-4" />
            <Label htmlFor="theme-toggle" className="sr-only">
                Toggle between light and dark theme
            </Label>
        </div>
    )
}