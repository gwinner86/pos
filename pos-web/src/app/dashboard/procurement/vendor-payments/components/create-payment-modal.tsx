"use client"

import { useState, useEffect } from "react"
import { useForm } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import * as z from "zod"
import { Button } from "@/components/ui/button"
import {
    Form,
    FormControl,
    FormField,
    FormItem,
    FormLabel,
    FormMessage,
} from "@/components/ui/form"
import { Input } from "@/components/ui/input"
import { toast } from "sonner"
import {
    Select,
    SelectContent,
    SelectItem,
    SelectTrigger,
    SelectValue,
} from "@/components/ui/select"
import {
    Dialog,
    DialogContent,
    DialogHeader,
    DialogTitle,
    DialogDescription,
} from "@/components/ui/dialog"
import { SupplierInvoice } from "@/services/vendor-invoice-service"
import { vendorPaymentService, CreateInvoicePaymentDto } from "@/services/vendor-payment-service"
import { formatCurrency } from "@/lib/utils"

const formSchema = z.object({
    amountPaid: z.number().min(0.01, 'Amount must be greater than 0'),
    paymentMethod: z.string().min(1, 'Payment Method is required'),
})

interface CreatePaymentModalProps {
    invoice: SupplierInvoice | null
    open: boolean
    onOpenChange: (open: boolean) => void
    onSuccess: () => void
}

export function CreatePaymentModal({ invoice, open, onOpenChange, onSuccess }: CreatePaymentModalProps) {
    const [loading, setLoading] = useState(false)

    const maxPayment = invoice ? invoice.totalAmount - invoice.totalPaid : 0

    const form = useForm<z.infer<typeof formSchema>>({
        resolver: zodResolver(formSchema),
        defaultValues: {
            amountPaid: 0,
            paymentMethod: "CASH",
        },
    })

    // Reset form when modal opens with a new invoice
    useEffect(() => {
        if (open && invoice) {
            form.reset({
                amountPaid: invoice.totalAmount - invoice.totalPaid,
                paymentMethod: "CASH"
            });
        }
    }, [open, invoice, form]);

    const onSubmit = async (values: z.infer<typeof formSchema>) => {
        if (!invoice) return

        if (values.amountPaid > maxPayment) {
            toast.error("Payment amount cannot exceed the remaining balance.")
            return;
        }

        setLoading(true)
        try {
            const dto: CreateInvoicePaymentDto = {
                supplierInvoiceId: invoice.id,
                amountPaid: values.amountPaid,
                paymentMethod: values.paymentMethod
            }
            await vendorPaymentService.createPayment(dto)
            toast.success("Payment recorded successfully")
            onSuccess()
            onOpenChange(false)
            form.reset()
        } catch (error: any) {
            console.error("Submission error:", error)
            toast.error(error.response?.data?.message || "Failed to record payment")
        } finally {
            setLoading(false)
        }
    }

    if (!invoice) return null;

    return (
        <Dialog open={open} onOpenChange={onOpenChange}>
            <DialogContent className="sm:max-w-[425px]">
                <DialogHeader>
                    <DialogTitle>Record Payment</DialogTitle>
                    <DialogDescription>
                        Invoice: <span className="font-semibold text-foreground">{invoice.invoiceNumber}</span>
                        <br />
                        Remaining Balance: <span className="font-semibold text-red-600">{formatCurrency(maxPayment)}</span>
                    </DialogDescription>
                </DialogHeader>

                <Form {...form}>
                    <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4 pt-4">
                        <FormField
                            control={form.control}
                            name="amountPaid"
                            render={({ field }) => (
                                <FormItem>
                                    <FormLabel>Amount to Pay</FormLabel>
                                    <FormControl>
                                        <Input
                                            type="number"
                                            step="0.01"
                                            min="0.01"
                                            max={maxPayment}
                                            placeholder="0.00"
                                            {...field}
                                            onChange={e => field.onChange(parseFloat(e.target.value) || 0)}
                                        />
                                    </FormControl>
                                    <FormMessage />
                                </FormItem>
                            )}
                        />

                        <FormField
                            control={form.control}
                            name="paymentMethod"
                            render={({ field }) => (
                                <FormItem>
                                    <FormLabel>Payment Method</FormLabel>
                                    <Select onValueChange={field.onChange} defaultValue={field.value}>
                                        <FormControl>
                                            <SelectTrigger>
                                                <SelectValue placeholder="Select a payment method" />
                                            </SelectTrigger>
                                        </FormControl>
                                        <SelectContent>
                                            <SelectItem value="CASH">Cash</SelectItem>
                                            <SelectItem value="BANK_TRANSFER">Bank Transfer</SelectItem>
                                            <SelectItem value="MOBILE_MONEY">Mobile Money</SelectItem>
                                            <SelectItem value="CHEQUE">Cheque</SelectItem>
                                        </SelectContent>
                                    </Select>
                                    <FormMessage />
                                </FormItem>
                            )}
                        />

                        <div className="flex justify-end space-x-2 pt-4">
                            <Button type="button" variant="outline" onClick={() => onOpenChange(false)}>
                                Cancel
                            </Button>
                            <Button type="submit" disabled={loading}>
                                {loading ? "Processing..." : "Record Payment"}
                            </Button>
                        </div>
                    </form>
                </Form>
            </DialogContent>
        </Dialog>
    )
}
