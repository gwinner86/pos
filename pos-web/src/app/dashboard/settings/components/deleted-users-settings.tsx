"use client"

import { useEffect, useState } from "react"
import {
    Card,
    CardContent,
    CardDescription,
    CardHeader,
    CardTitle,
} from "@/components/ui/card"
import {
    Table,
    TableBody,
    TableCell,
    TableHead,
    TableHeader,
    TableRow,
} from "@/components/ui/table"
import { Badge } from "@/components/ui/badge"
import { Loader2 } from "lucide-react"
import { toast } from "sonner"
import { settingsService, UserResponse } from "@/services/settings-service"

export function DeletedUsersSettings() {
    const [users, setUsers] = useState<UserResponse[]>([])
    const [loading, setLoading] = useState(true)

    const loadData = async () => {
        setLoading(true)
        try {
            const data = await settingsService.getDeletedUsers()
            setUsers(data)
        } catch (error) {
            toast.error("Failed to load deleted users")
        } finally {
            setLoading(false)
        }
    }

    useEffect(() => {
        loadData()
    }, [])

    return (
        <Card>
            <CardHeader>
                <CardTitle>Deleted Users</CardTitle>
                <CardDescription>
                    History of deleted users and who deleted them.
                </CardDescription>
            </CardHeader>
            <CardContent>
                {loading ? (
                    <div className="flex justify-center p-4"><Loader2 className="h-6 w-6 animate-spin" /></div>
                ) : (
                    <Table>
                        <TableHeader>
                            <TableRow>
                                <TableHead>Name</TableHead>
                                <TableHead>Email</TableHead>
                                <TableHead>Deleted By</TableHead>
                                <TableHead>Deleted At</TableHead>
                            </TableRow>
                        </TableHeader>
                        <TableBody>
                            {users.length === 0 ? (
                                <TableRow>
                                    <TableCell colSpan={4} className="text-center">No deleted users found</TableCell>
                                </TableRow>
                            ) : (
                                users.map((u) => (
                                    <TableRow key={u.userId}>
                                        <TableCell>{u.firstName} {u.lastName}</TableCell>
                                        <TableCell>{u.email}</TableCell>
                                        <TableCell>{u.deletedByName || "Unknown"}</TableCell>
                                        <TableCell>{u.deletedAt ? new Date(u.deletedAt).toLocaleString() : "-"}</TableCell>
                                    </TableRow>
                                ))
                            )}
                        </TableBody>
                    </Table>
                )}
            </CardContent>
        </Card>
    )
}
