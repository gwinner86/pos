"use client"

import { Button } from "@/components/ui/button"
import {
    Card,
    CardContent,
    CardDescription,
    CardHeader,
    CardTitle,
    CardFooter
} from "@/components/ui/card"
import { Checkbox } from "@/components/ui/checkbox"
import { Label } from "@/components/ui/label"

export function NotificationSettings() {
    return (
        <div className="grid gap-6">
            <Card>
                <CardHeader>
                    <CardTitle>Email Notifications</CardTitle>
                    <CardDescription>
                        Select when you want to receive email alerts.
                    </CardDescription>
                </CardHeader>
                <CardContent className="space-y-4">
                    <div className="flex items-center space-x-2">
                        <Checkbox id="marketing" checked />
                        <Label htmlFor="marketing">Low stock alerts</Label>
                    </div>
                    <div className="flex items-center space-x-2">
                        <Checkbox id="social" checked />
                        <Label htmlFor="social">Daily sales summary</Label>
                    </div>
                    <div className="flex items-center space-x-2">
                        <Checkbox id="security" checked />
                        <Label htmlFor="security">New device login</Label>
                    </div>
                </CardContent>
                <CardFooter className="border-t px-6 py-4">
                    <Button variant="outline">Save Preferences</Button>
                </CardFooter>
            </Card>
        </div>
    )
}
