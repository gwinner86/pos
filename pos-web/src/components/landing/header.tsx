import Link from "next/link"
import { Button } from "@/components/ui/button"

export function Header() {
    return (
        <header className="sticky top-0 z-50 w-full border-b bg-background/95 backdrop-blur supports-[backdrop-filter]:bg-background/60">
            <div className="container flex h-16 items-center px-4 md:px-6">
                <Link href="/" className="mr-6 flex items-center space-x-2">
                    <span className="text-xl font-bold">Wintech POS</span>
                </Link>
                <nav className="hidden md:flex flex-1 items-center gap-6 text-sm font-medium">
                    <Link href="#features" className="transition-colors hover:text-foreground/80 text-foreground/60">Features</Link>
                    <Link href="#pricing" className="transition-colors hover:text-foreground/80 text-foreground/60">Pricing</Link>
                </nav>
                <div className="flex items-center gap-4">
                    <Link href="/login">
                        <Button variant="ghost" size="sm">Login</Button>
                    </Link>
                    <Link href="#pricing">
                        <Button size="sm">Get Started</Button>
                    </Link>
                </div>
            </div>
        </header>
    )
}
