"use client"

import { useTheme } from "next-themes"
import { Button } from "@/components/ui/button"
import {
    Card,
    CardContent,
    CardDescription,
    CardHeader,
    CardTitle,
} from "@/components/ui/card"
import { Label } from "@/components/ui/label"
import { Moon, Sun } from "lucide-react"
import { cn } from "@/lib/utils"

export function DisplaySettings() {
    const { setTheme, theme } = useTheme()

    return (
        <div className="grid gap-6">
            <Card>
                <CardHeader>
                    <CardTitle>Appearance</CardTitle>
                    <CardDescription>
                        Customize the look and feel of the application.
                    </CardDescription>
                </CardHeader>
                <CardContent className="space-y-4">
                    <div className="space-y-2">
                        <Label>Theme</Label>
                        <div className="flex gap-4">
                            <Button
                                variant="outline"
                                className={cn("w-32 justify-start gap-2", theme === "light" && "border-primary")}
                                onClick={() => setTheme("light")}
                            >
                                <Sun className="h-4 w-4" />
                                Light
                            </Button>
                            <Button
                                variant="outline"
                                className={cn("w-32 justify-start gap-2", theme === "dark" && "border-primary")}
                                onClick={() => setTheme("dark")}
                            >
                                <Moon className="h-4 w-4" />
                                Dark
                            </Button>
                            <Button
                                variant="outline"
                                className={cn("w-32 justify-start gap-2", theme === "system" && "border-primary")}
                                onClick={() => setTheme("system")}
                            >
                                <span className="h-4 w-4 flex items-center justify-center font-bold text-xs">A</span>
                                System
                            </Button>
                        </div>
                    </div>
                </CardContent>
            </Card>
        </div>
    )
}
