"use client"

import { useEffect, useState } from "react"
import { useForm } from "react-hook-form"
import * as z from "zod"
import { zodResolver } from "@hookform/resolvers/zod"
import { Plus, CreditCard } from "lucide-react"
import { toast } from "sonner"
import { settingsService, PaymentMethod } from "@/services/settings-service"

import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import {
    Table,
    TableBody,
    TableCell,
    TableHead,
    TableHeader,
    TableRow,
} from "@/components/ui/table"
import {
    Dialog,
    DialogContent,
    DialogHeader,
    DialogTitle,
    DialogTrigger,
    DialogFooter,
} from "@/components/ui/dialog"
import {
    Form,
    FormControl,
    FormField,
    FormItem,
    FormLabel,
    FormMessage,
} from "@/components/ui/form"
import { Badge } from "@/components/ui/badge"

const paymentSchema = z.object({
    methodName: z.string().min(2, "Name is required"),
    paymentType: z.string().min(2, "Type is required"),
})

export function PaymentMethodSettings() {
    const [methods, setMethods] = useState<PaymentMethod[]>([])
    const [loading, setLoading] = useState(true)
    useEffect(() => {
        loadData()
    }, [])

    const loadData = async () => {
        try {
            const data = await settingsService.getPaymentMethods()
            setMethods(data || [])
        } catch (error) {
            console.error("Failed to load payment methods", error)
        } finally {
            setLoading(false)
        }
    }

    return (
        <div className="space-y-4">
            <div className="flex justify-between items-center">
                <div>
                    <h3 className="text-lg font-medium">Payment Methods</h3>
                    <p className="text-sm text-muted-foreground">Configure payment options for sales.</p>
                </div>
            </div>

            <div className="border rounded-md bg-white">
                <Table>
                    <TableHeader>
                        <TableRow>
                            <TableHead>Method Name</TableHead>
                            <TableHead>Type</TableHead>
                            <TableHead>Status</TableHead>
                        </TableRow>
                    </TableHeader>
                    <TableBody>
                        {loading ? (
                            <TableRow>
                                <TableCell colSpan={3} className="h-24 text-center">Loading...</TableCell>
                            </TableRow>
                        ) : methods.length === 0 ? (
                            <TableRow>
                                <TableCell colSpan={3} className="h-24 text-center">No payment methods found.</TableCell>
                            </TableRow>
                        ) : (
                            methods.map((method) => (
                                <TableRow key={method.paymentMethodId}>
                                    <TableCell className="font-medium flex items-center gap-2">
                                        <CreditCard className="h-4 w-4 text-muted-foreground" />
                                        {method.methodName}
                                    </TableCell>
                                    <TableCell>{method.paymentType}</TableCell>
                                    <TableCell>
                                        <Badge variant={method.isActive ? "default" : "secondary"}>
                                            {method.isActive ? "Active" : "Inactive"}
                                        </Badge>
                                    </TableCell>
                                </TableRow>
                            ))
                        )}
                    </TableBody>
                </Table>
            </div>
        </div>
    )
}
