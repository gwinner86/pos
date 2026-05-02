import { type ClassValue, clsx } from "clsx"
import { twMerge } from "tailwind-merge"

export function cn(...inputs: ClassValue[]) {
    return twMerge(clsx(inputs))
}

export function formatCurrency(amount: number, currencyCode?: string, currencySymbol?: string) {
    if (typeof window !== 'undefined') {
        if (!currencySymbol && !currencyCode) {
            try {
                const loc = localStorage.getItem('location');
                if (loc) {
                    const locationData = JSON.parse(loc);
                    currencySymbol = locationData.currencySymbol || 'GH₵';
                    currencyCode = locationData.currencyCode || 'GHS';
                }
            } catch (e) { }
        }
    }

    const code = currencyCode || 'GHS';
    const symbol = currencySymbol || 'GH₵';

    try {
        return new Intl.NumberFormat('en-GH', {
            style: 'currency',
            currency: code,
        }).format(amount)
    } catch (e) {
        // Fallback for invalid/custom currency codes
        return `${symbol}${amount.toFixed(2)}`;
    }
}
