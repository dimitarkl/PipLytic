import { clsx, type ClassValue } from "clsx"
import { twMerge } from "tailwind-merge"

export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs))
}

export const chartColors = {
  light: {
    background: '#ffffff', // oklch(1 0 0)
    text: '#262626', // oklch(0.145 0 0)
    grid: '#e8e8e8', // oklch(0.922 0 0)
  },
  dark: {
    background: '#171717', // oklch(0.145 0 0)
    text: '#fafafa', // oklch(0.985 0 0)
    grid: 'rgba(255, 255, 255, 0.1)', // oklch(1 0 0 / 10%)
  },
  series: {
    up: '#22c55e', // chart-2 light: oklch(0.6 0.118 184.704), dark: oklch(0.696 0.17 162.48)
    down: '#ef4444', // destructive light: oklch(0.577 0.245 27.325), dark: oklch(0.704 0.191 22.216)
    borderVisible: false,
    wickUp: '#22c55e',
    wickDown: '#ef4444',
    primary: '#f97316', // chart-1 light: oklch(0.646 0.222 41.116), dark: oklch(0.488 0.243 264.376)
    secondary: '#3b82f6', // chart-3 light: oklch(0.398 0.07 227.392), dark: oklch(0.769 0.188 70.08)
    accent: '#eab308', // chart-4 light: oklch(0.828 0.189 84.429), dark: oklch(0.627 0.265 303.9)
    highlight: '#f59e0b', // chart-5 light: oklch(0.769 0.188 70.08), dark: oklch(0.645 0.246 16.439)
  },
};
