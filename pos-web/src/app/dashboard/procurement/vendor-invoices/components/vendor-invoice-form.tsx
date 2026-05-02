"use client"

import { useState, useEffect } from "react"
import { format } from "date-fns"
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
import { supplierService } from "@/services/supplier-service"
import { vendorInvoiceService, CreateSupplierInvoiceDto } from "@/services/vendor-invoice-service"
import { Supplier } from "@/types/crm"
import {
    Select,
    SelectContent,
    SelectItem,
    SelectTrigger,
    SelectValue,
} from "@/components/ui/select"

const formSchema = z.object({
    supplierId: z.string().min(1, 'Supplier is required'),
    goodsReceiptId: z.string().optional(),
    invoiceNumber: z.string().min(1, 'Invoice Number is required'),
    invoiceDate: z.string().min(1, 'Invoice Date is required'),
    dueDate: z.string().min(1, 'Due Date is required'),
    totalAmount: z.number().min(0.01, 'Total Amount must be greater than 0'),
})

interface VendorInvoiceFormProps {
    onSuccess: () => void
    onCancel: () => void
}

export function VendorInvoiceForm({ onSuccess, onCancel }: VendorInvoiceFormProps) {
    const [loading, setLoading] = useState(false)
    const [suppliers, setSuppliers] = useState<Supplier[]>([])
    const [goodsReceipts, setGoodsReceipts] = useState<any[]>([])

    const form = useForm<z.infer<typeof formSchema>>({
        resolver: zodResolver(formSchema),
        defaultValues: {
            supplierId: "",
            goodsReceiptId: "none",
            invoiceNumber: "",
            invoiceDate: new Date().toISOString().split('T')[0],
            dueDate: new Date().toISOString().split('T')[0],
            totalAmount: 0,
        },
    })

    const formatDate = (dateString: string) => {
        try {
            return format(new Date(dateString), "MMM dd, yyyy");
        } catch {
            return dateString;
        }
    }

    useEffect(() => {
        const fetchSuppliers = async () => {
            try {
                const data = await supplierService.getSuppliers()
                setSuppliers(data)
            } catch (error) {
                console.error("Failed to load suppliers:", error)
            }
        }
        fetchSuppliers()
    }, [])

    // Fetch goods receipts when a supplier is selected
    const selectedSupplierId = form.watch("supplierId");

    useEffect(() => {
        const fetchReceipts = async () => {
            try {
                // To fetch goods receipts, normally we have a service. Let's import it here.
                const { goodsReceiptService } = await import('@/services/goods-receipt-service');
                const receipts = await goodsReceiptService.getReceipts();
                // Filter for Approved receipts that belong to this supplier and are NOT invoiced yet
                const pendingReceipts = receipts.filter(r => r.supplierId === selectedSupplierId && r.status === "Approved" && r.isInvoicedYesNo !== "YES");
                setGoodsReceipts(pendingReceipts);
            } catch (error) {
                console.error("Failed to load goods receipts:", error);
            }
        }

        if (selectedSupplierId) {
            fetchReceipts();
        } else {
            setGoodsReceipts([]);
        }
    }, [selectedSupplierId]);

    const handleGoodsReceiptChange = (value: string) => {
        form.setValue("goodsReceiptId", value);
        if (value && value !== "none") {
            const selectedGr = goodsReceipts.find(gr => gr.id === value);
            if (selectedGr) {
                form.setValue("totalAmount", selectedGr.totalReceivedAmount);
            }
        } else {
            form.setValue("totalAmount", 0);
        }
    }

    const onSubmit = async (values: z.infer<typeof formSchema>) => {
        setLoading(true)
        try {
            const dto: CreateSupplierInvoiceDto = {
                supplierId: values.supplierId,
                goodsReceiptId: values.goodsReceiptId !== "none" ? values.goodsReceiptId : undefined,
                invoiceNumber: values.invoiceNumber,
                invoiceDate: new Date(values.invoiceDate).toISOString(),
                dueDate: new Date(values.dueDate).toISOString(),
                totalAmount: values.totalAmount
            }
            await vendorInvoiceService.createInvoice(dto)
            toast.success("Vendor Invoice created successfully")
            onSuccess()
        } catch (error: any) {
            console.error("Submission error:", error)
            toast.error(error.response?.data?.message || "Failed to create invoice")
        } finally {
            setLoading(false)
        }
    }

    return (
        <Form {...form}>
            <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-6">
                <div className="grid grid-cols-2 gap-4">
                    <FormField
                        control={form.control}
                        name="supplierId"
                        render={({ field }) => (
                            <FormItem>
                                <FormLabel>Supplier</FormLabel>
                                <Select onValueChange={field.onChange} defaultValue={field.value}>
                                    <FormControl>
                                        <SelectTrigger>
                                            <SelectValue placeholder="Select a supplier" />
                                        </SelectTrigger>
                                    </FormControl>
                                    <SelectContent>
                                        {suppliers.map((s) => (
                                            <SelectItem key={s.id} value={s.id}>
                                                {s.supplierName}
                                            </SelectItem>
                                        ))}
                                    </SelectContent>
                                </Select>
                                <FormMessage />
                            </FormItem>
                        )}
                    />

                    <FormField
                        control={form.control}
                        name="goodsReceiptId"
                        render={({ field }) => (
                            <FormItem>
                                <FormLabel>Goods Receipt (Optional)</FormLabel>
                                <Select onValueChange={handleGoodsReceiptChange} value={field.value || "none"} disabled={!selectedSupplierId}>
                                    <FormControl>
                                        <SelectTrigger>
                                            <SelectValue placeholder="Select a goods receipt" />
                                        </SelectTrigger>
                                    </FormControl>
                                    <SelectContent>
                                        <SelectItem value="none">None - Direct Invoice</SelectItem>
                                        {goodsReceipts.map((gr) => (
                                            <SelectItem key={gr.id} value={gr.id}>
                                                {formatDate(gr.receiptDate)} - {gr.locationName} - Value: {gr.totalReceivedAmount}
                                            </SelectItem>
                                        ))}
                                    </SelectContent>
                                </Select>
                                <FormMessage />
                            </FormItem>
                        )}
                    />

                    <FormField
                        control={form.control}
                        name="invoiceNumber"
                        render={({ field }) => (
                            <FormItem>
                                <FormLabel>Invoice Number</FormLabel>
                                <FormControl>
                                    <Input placeholder="INV-001" {...field} />
                                </FormControl>
                                <FormMessage />
                            </FormItem>
                        )}
                    />

                    <FormField
                        control={form.control}
                        name="invoiceDate"
                        render={({ field }) => (
                            <FormItem>
                                <FormLabel>Invoice Date</FormLabel>
                                <FormControl>
                                    <Input type="date" {...field} />
                                </FormControl>
                                <FormMessage />
                            </FormItem>
                        )}
                    />

                    <FormField
                        control={form.control}
                        name="dueDate"
                        render={({ field }) => (
                            <FormItem>
                                <FormLabel>Due Date</FormLabel>
                                <FormControl>
                                    <Input type="date" {...field} />
                                </FormControl>
                                <FormMessage />
                            </FormItem>
                        )}
                    />

                    <FormField
                        control={form.control}
                        name="totalAmount"
                        render={({ field }) => (
                            <FormItem>
                                <FormLabel>Total Amount</FormLabel>
                                <FormControl>
                                    <Input
                                        type="number"
                                        step="0.01"
                                        min="0"
                                        placeholder="0.00"
                                        {...field}
                                        onChange={e => field.onChange(parseFloat(e.target.value) || 0)}
                                    />
                                </FormControl>
                                <FormMessage />
                            </FormItem>
                        )}
                    />
                </div>

                <div className="flex justify-end space-x-2 pt-4 border-t">
                    <Button type="button" variant="outline" onClick={onCancel}>
                        Cancel
                    </Button>
                    <Button type="submit" disabled={loading}>
                        {loading ? "Saving..." : "Create Invoice"}
                    </Button>
                </div>
            </form>
        </Form>
    )
}
