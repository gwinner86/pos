"use client"

import { useState, useEffect } from "react"
import { useForm, useFieldArray } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import * as z from "zod"
import { useRouter } from "next/navigation"
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
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select"
import { Textarea } from "@/components/ui/textarea"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { Trash, Plus } from "lucide-react"
import { toast } from "sonner"
import { supplierService } from "@/services/supplier-service"
import { productService } from "@/services/product-service"
import { Supplier, PurchaseOrder } from "@/types/crm"
import { Product } from "@/types/inventory"
import { formatCurrency } from "@/lib/utils"
import { useAuth } from "@/hooks/use-auth"

const poSchema = z.object({
    supplierId: z.string().min(1, "Supplier is required"),
    date: z.string(), // ISO date string or YYYY-MM-DD
    notes: z.string().optional(),
    items: z.array(z.object({
        id: z.string().optional(), // For updates
        productVariantId: z.string().min(1, "Product Variant is required"),
        quantity: z.coerce.number().positive("Quantity must be > 0"),
        unitCost: z.coerce.number().min(0, "Cost must be >= 0"),
    })).min(1, "Add at least one item"),
    status: z.string().optional()
})

type PoFormValues = z.infer<typeof poSchema>

interface PurchaseOrderFormProps {
    initialData?: PurchaseOrder
}

export function PurchaseOrderForm({ initialData }: PurchaseOrderFormProps) {
    console.log("PurchaseOrderForm initialData:", initialData)
    const router = useRouter()
    const { selectedLocation } = useAuth()
    const [loading, setLoading] = useState(false)
    const [suppliers, setSuppliers] = useState<Supplier[]>([])
    const [products, setProducts] = useState<Product[]>([])

    // Load Data
    useEffect(() => {
        const load = async () => {
            try {
                const [supData, prodData] = await Promise.all([
                    supplierService.getSuppliers(),
                    productService.getProducts()
                ])
                setSuppliers(supData)
                setProducts(prodData)
            } catch (err) {
                console.error(err)
                toast.error("Failed to load suppliers or products")
            }
        }
        load()
    }, [])

    const defaultValues: PoFormValues = initialData ? {
        supplierId: initialData.supplierId,
        // If initialData.expectedDeliveryDate exists, use it, else default to today
        date: initialData.expectedDeliveryDate ? new Date(initialData.expectedDeliveryDate).toISOString().split('T')[0] : new Date().toISOString().split('T')[0],
        notes: initialData.notes || "",
        status: initialData.status,
        items: initialData.details.map(d => ({
            id: d.id,
            productVariantId: d.productVariantId || d.productId, // Fallback to productId if no variant ID
            quantity: d.quantity,
            unitCost: d.unitCost
        }))
    } : {
        supplierId: "",
        date: new Date().toISOString().split('T')[0],
        notes: "",
        items: [{ productVariantId: "", quantity: 1, unitCost: 0 }]
    }

    const form = useForm<PoFormValues>({
        resolver: zodResolver(poSchema) as any,
        defaultValues,
    })

    const { fields, append, remove } = useFieldArray({
        control: form.control,
        name: "items"
    })

    // Calculate Total (Client Side)
    const items = form.watch("items")
    const totalAmount = items.reduce((sum, item) => sum + (item.quantity * item.unitCost), 0)

    const onSubmit = async (data: PoFormValues) => {
        if (!selectedLocation) {
            toast.error("Please select a location first")
            return
        }

        setLoading(true)
        try {
            if (initialData) {
                // UPDATE
                const payload = {
                    expectedDeliveryDate: new Date(data.date).toISOString(),
                    status: data.status, // Not explicit in form but preserved
                    notes: data.notes,
                    details: data.items.map(i => {
                        return {
                            id: i.id, // Include ID for existing items
                            productVariantId: i.productVariantId,
                            quantity: Number(i.quantity),
                            unitCost: Number(i.unitCost)
                        }
                    })
                }
                console.log("Updating PO Payload:", payload)
                await supplierService.updatePurchaseOrder(initialData.id, payload)
                toast.success("Purchase Order updated")
            } else {
                // CREATE
                const payload = {
                    locationId: selectedLocation.id,
                    supplierId: data.supplierId,
                    expectedDeliveryDate: new Date(data.date).toISOString(),
                    notes: data.notes,
                    details: data.items.map(i => {
                        return {
                            productVariantId: i.productVariantId,
                            quantity: Number(i.quantity),
                            unitCost: Number(i.unitCost)
                        }
                    })
                }
                console.log("Creating PO Payload:", payload)
                await supplierService.createPurchaseOrder(payload)
                toast.success("Purchase Order created")
            }
            router.push("/dashboard/procurement/purchase-orders")
            router.refresh()
        } catch (error: any) {
            console.error(error)
            toast.error(error.message || "Failed to save Purchase Order")
        } finally {
            setLoading(false)
        }
    }

    return (
        <Form {...form}>
            <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-6">
                {/* Header Info */}
                <Card>
                    <CardHeader>
                        <CardTitle>{initialData ? "Edit Order Details" : "Order Details"}</CardTitle>
                    </CardHeader>
                    <CardContent className="grid grid-cols-2 gap-4">
                        <FormField
                            control={form.control}
                            name="supplierId"
                            render={({ field }) => (
                                <FormItem>
                                    <FormLabel>Supplier</FormLabel>
                                    <Select onValueChange={field.onChange} value={field.value} disabled={!!initialData}>
                                        <FormControl>
                                            <SelectTrigger>
                                                <SelectValue placeholder="Select Supplier" />
                                            </SelectTrigger>
                                        </FormControl>
                                        <SelectContent>
                                            {suppliers.map(s => (
                                                <SelectItem key={s.id} value={s.id}>{s.supplierName}</SelectItem>
                                            ))}
                                        </SelectContent>
                                    </Select>
                                    <FormMessage />
                                </FormItem>
                            )}
                        />
                        <FormField
                            control={form.control}
                            name="date"
                            render={({ field }) => (
                                <FormItem>
                                    <FormLabel>Expected Delivery Date</FormLabel>
                                    <FormControl>
                                        <Input type="date" {...field} />
                                    </FormControl>
                                    <FormMessage />
                                </FormItem>
                            )}
                        />
                        <div className="col-span-2">
                            <FormField
                                control={form.control}
                                name="notes"
                                render={({ field }) => (
                                    <FormItem>
                                        <FormLabel>Notes</FormLabel>
                                        <FormControl>
                                            <Textarea placeholder="Delivery instructions..." {...field} />
                                        </FormControl>
                                        <FormMessage />
                                    </FormItem>
                                )}
                            />
                        </div>
                    </CardContent>
                </Card>

                {/* Line Items */}
                <Card>
                    <CardHeader className="flex flex-row items-center justify-between">
                        <CardTitle>Items</CardTitle>
                        <Button type="button" variant="outline" size="sm" onClick={() => append({ productVariantId: "", quantity: 1, unitCost: 0 })}>
                            <Plus className="mr-2 h-4 w-4" /> Add Item
                        </Button>
                    </CardHeader>
                    <CardContent className="space-y-4">
                        {fields.map((field, index) => {
                            return (
                                <div key={field.id} className="grid grid-cols-12 gap-2 items-end border p-3 rounded-md bg-muted/20">
                                    <div className="col-span-7">
                                        <FormField
                                            control={form.control}
                                            name={`items.${index}.productVariantId`}
                                            render={({ field }) => (
                                                <FormItem>
                                                    <FormLabel className="text-xs">Product Variant</FormLabel>
                                                    <Select onValueChange={(val) => {
                                                        field.onChange(val)
                                                        // Auto-fill cost
                                                        const prod = products.find(p => p.variants?.some(v => v.variantId === val) || (!p.hasVariants && p.productId === val));
                                                        if (prod) {
                                                            const variant = prod.variants?.find(v => v.variantId === val);
                                                            const cost = variant?.cost || prod.cost || 0;
                                                            form.setValue(`items.${index}.unitCost`, cost);
                                                        }
                                                    }} value={field.value}>
                                                        <FormControl>
                                                            <SelectTrigger className="h-8">
                                                                <SelectValue placeholder="Select Product Variant" />
                                                            </SelectTrigger>
                                                        </FormControl>
                                                        <SelectContent>
                                                            {products.map(p => {
                                                                if (!p.hasVariants || !p.variants || p.variants.length === 0) {
                                                                    // Map to productId if no variants
                                                                    return (
                                                                        <SelectItem key={p.productId} value={p.productId}>
                                                                            {p.productName} (Stock: {p.stockLevel || 0})
                                                                        </SelectItem>
                                                                    )
                                                                }
                                                                return p.variants.map(v => (
                                                                    <SelectItem key={v.variantId || `temp-${p.productId}`} value={v.variantId || ""}>
                                                                        {p.productName} - {v.variantName || v.sku} (Stock: {v.stockLevel || 0})
                                                                    </SelectItem>
                                                                ))
                                                            })}
                                                        </SelectContent>
                                                    </Select>
                                                </FormItem>
                                            )}
                                        />
                                    </div>

                                    <div className="col-span-2">
                                        <FormField
                                            control={form.control}
                                            name={`items.${index}.quantity`}
                                            render={({ field }) => (
                                                <FormItem>
                                                    <FormLabel className="text-xs">Qty</FormLabel>
                                                    <FormControl>
                                                        <Input type="number" className="h-8" {...field} />
                                                    </FormControl>
                                                </FormItem>
                                            )}
                                        />
                                    </div>
                                    <div className="col-span-2">
                                        <FormField
                                            control={form.control}
                                            name={`items.${index}.unitCost`}
                                            render={({ field }) => (
                                                <FormItem>
                                                    <FormLabel className="text-xs">Cost</FormLabel>
                                                    <FormControl>
                                                        <Input type="number" step="0.01" className="h-8" {...field} />
                                                    </FormControl>
                                                </FormItem>
                                            )}
                                        />
                                    </div>
                                    <div className="col-span-1">
                                        <Button type="button" variant="ghost" size="sm" className="h-8 w-8 p-0 text-destructive" onClick={() => remove(index)}>
                                            <Trash className="h-4 w-4" />
                                        </Button>
                                    </div>
                                </div>
                            )
                        })}
                        <div className="flex justify-end pt-4">
                            <div className="text-lg font-bold">
                                Total: {formatCurrency(totalAmount)}
                            </div>
                        </div>
                    </CardContent>
                </Card>

                <div className="flex justify-end gap-4">
                    <Button type="button" variant="outline" onClick={() => router.back()}>Cancel</Button>
                    <Button type="submit" disabled={loading}>
                        {loading ? "Saving..." : (initialData ? "Update Purchase Order" : "Create Purchase Order")}
                    </Button>
                </div>
            </form>
        </Form>
    )
}
