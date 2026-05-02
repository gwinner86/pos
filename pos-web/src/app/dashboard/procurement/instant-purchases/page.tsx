"use client"

import { useEffect, useState } from "react"
import { useAuth } from "@/hooks/use-auth"
import { useRouter } from "next/navigation"
import api from "@/lib/api"

import { supplierService } from "@/services/supplier-service"
import { Supplier } from "@/types/crm"
import { productService } from "@/services/product-service"
import { Product } from "@/types/inventory"
import { Button } from "@/components/ui/button"
import { format } from "date-fns"
import { Calendar as CalendarIcon, Trash2, Plus } from "lucide-react"
import { Popover, PopoverContent, PopoverTrigger } from "@/components/ui/popover"
import { Calendar } from "@/components/ui/calendar"
import { cn, formatCurrency } from "@/lib/utils"
// Use Select components from the UI library
import {
    Select,
    SelectContent,
    SelectItem,
    SelectTrigger,
    SelectValue,
} from "@/components/ui/select"
import { Input } from "@/components/ui/input"
import { Textarea } from "@/components/ui/textarea"
import { toast } from "sonner"

interface OrderLine {
    id: string; // temp UI id
    productVariantId: string;
    quantity: number;
    unitCost: number;
}

export default function InstantPurchasesPage() {
    const { selectedCompany, selectedLocation } = useAuth()
    const router = useRouter()

    const [loading, setLoading] = useState(false)
    const [submitting, setSubmitting] = useState(false)

    // Lookups
    const [suppliers, setSuppliers] = useState<Supplier[]>([])
    const [products, setProducts] = useState<Product[]>([])

    // Form State
    const [supplierId, setSupplierId] = useState("")
    const [expectedDate, setExpectedDate] = useState<Date>(new Date())
    const [notes, setNotes] = useState("")
    const [lines, setLines] = useState<OrderLine[]>([])

    useEffect(() => {
        if (selectedCompany && selectedLocation) {
            loadLookups()
        }
    }, [selectedCompany, selectedLocation])

    const loadLookups = async () => {
        setLoading(true)
        try {
            const [suppsRes, productsRes] = await Promise.all([
                supplierService.getSuppliers(),
                productService.getProducts(selectedLocation?.id || "")
            ])
            setSuppliers(suppsRes)
            setProducts(productsRes)
        } catch (error) {
            console.error("Failed to load lookups", error)
            toast.error("Failed to load requisite data.")
        } finally {
            setLoading(false)
        }
    }

    const handleAddLine = () => {
        setLines([...lines, { id: crypto.randomUUID(), productVariantId: "", quantity: 1, unitCost: 0 }])
    }

    const handleRemoveLine = (id: string) => {
        setLines(lines.filter(l => l.id !== id))
    }

    const handleLineChange = (id: string, field: keyof OrderLine, value: any) => {
        setLines(lines.map(line => {
            if (line.id === id) {
                // If variant changed, auto-populate cost if available
                if (field === 'productVariantId') {
                    const prod = products.find(p => p.variants?.some(v => v.variantId === value));
                    const variant = prod?.variants?.find(v => v.variantId === value);
                    return { ...line, [field]: value, unitCost: variant?.cost || prod?.cost || 0 }
                }
                return { ...line, [field]: value }
            }
            return line
        }))
    }

    const calculateTotal = () => {
        return lines.reduce((sum, line) => sum + (line.quantity * line.unitCost), 0)
    }

    const handleSubmit = async () => {
        if (!selectedLocation) {
            toast.error("Please select a location first")
            return
        }

        if (lines.length === 0) {
            toast.error("Please add at least one line item")
            return
        }
        if (lines.some(l => !l.productVariantId || l.quantity <= 0 || l.unitCost <= 0)) {
            toast.error("Please completely fill out all line items with valid quantities/costs")
            return
        }

        setSubmitting(true)
        try {
            const payload: any = {
                locationId: selectedLocation.id,
                expectedDeliveryDate: expectedDate.toISOString(),
                notes: notes,
                details: lines.map(l => ({
                    productVariantId: l.productVariantId,
                    quantity: l.quantity,
                    unitCost: l.unitCost
                }))
            }

            if (supplierId && supplierId !== "none") {
                payload.supplierId = supplierId;
            }

            await supplierService.createInstantPurchase(payload)
            toast.success("Instant Purchase completed. Stock and Invoice generated.")

            // Reset form or navigate away
            setSupplierId("")
            setNotes("")
            setLines([])
            router.push("/dashboard/procurement/goods-receipts") // Route them to receipts to view it
        } catch (error: any) {
            console.error("Submission failed", error)
            toast.error(error.response?.data?.message || "Failed to process instant purchase.")
        } finally {
            setSubmitting(false)
        }
    }

    if (loading) {
        return <div className="p-8">Loading...</div>
    }

    return (
        <div className="flex flex-col gap-5 w-full max-w-5xl mx-auto">
            <div className="flex flex-col gap-1">
                <h2 className="text-3xl font-bold tracking-tight text-foreground">
                    Instant Purchase
                </h2>
                <p className="text-sm text-muted-foreground">
                    Bypass standard workflow. Instantly receive stock and generate an unpaid Vendor Invoice.
                </p>
            </div>

            <div className="grid grid-cols-1 md:grid-cols-2 gap-6 bg-card p-6 rounded-md border shadow-sm">
                <div className="flex flex-col gap-2">
                    <label className="text-sm font-medium">Supplier (Optional)</label>
                    <Select value={supplierId} onValueChange={setSupplierId}>
                        <SelectTrigger>
                            <SelectValue placeholder="Select Supplier (Optional)" />
                        </SelectTrigger>
                        <SelectContent>
                            <SelectItem value="none">No Supplier</SelectItem>
                            {suppliers.map(s => (
                                <SelectItem key={s.id} value={s.id}>{s.supplierName}</SelectItem>
                            ))}
                        </SelectContent>
                    </Select>
                </div>

                <div className="flex flex-col gap-2">
                    <label className="text-sm font-medium">Date</label>
                    <Popover>
                        <PopoverTrigger asChild>
                            <Button
                                variant={"outline"}
                                className={cn("w-full justify-start text-left font-normal", !expectedDate && "text-muted-foreground")}
                            >
                                <CalendarIcon className="mr-2 h-4 w-4" />
                                {expectedDate ? format(expectedDate, "PPP") : <span>Pick a date</span>}
                            </Button>
                        </PopoverTrigger>
                        <PopoverContent className="w-auto p-0" align="start">
                            <Calendar
                                mode="single"
                                selected={expectedDate}
                                onSelect={(d) => d && setExpectedDate(d)}
                                initialFocus
                            />
                        </PopoverContent>
                    </Popover>
                </div>

                <div className="flex flex-col gap-2 md:col-span-2">
                    <label className="text-sm font-medium">Notes</label>
                    <Textarea
                        placeholder="Internal notes about this purchase..."
                        value={notes}
                        onChange={(e) => setNotes(e.target.value)}
                    />
                </div>
            </div>

            <div className="flex flex-col gap-4 bg-card p-6 rounded-md border shadow-sm mt-2">
                <div className="flex items-center justify-between">
                    <h3 className="text-lg font-semibold">Line Items</h3>
                    <Button variant="outline" size="sm" onClick={handleAddLine} className="flex gap-2 items-center">
                        <Plus className="h-4 w-4" /> Add Item
                    </Button>
                </div>

                <div className="space-y-4">
                    {lines.map((line, index) => (
                        <div key={line.id} className="grid grid-cols-12 gap-4 items-end border-b pb-4">
                            <div className="col-span-5 flex flex-col gap-2">
                                <label className="text-xs font-semibold text-muted-foreground uppercase">Product Variant</label>
                                <Select value={line.productVariantId} onValueChange={(v) => handleLineChange(line.id, 'productVariantId', v)}>
                                    <SelectTrigger>
                                        <SelectValue placeholder="Select product variant" />
                                    </SelectTrigger>
                                    <SelectContent>
                                        {products.map(p => {
                                            if (!p.hasVariants || !p.variants || p.variants.length === 0) {
                                                // We mapped to productVariantId for the backend handling in instant purchase. 
                                                // So if no variants, we just use the productId as the variant ID.
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
                            </div>

                            <div className="col-span-2 flex flex-col gap-2">
                                <label className="text-xs font-semibold text-muted-foreground uppercase">Quantity</label>
                                <Input
                                    type="number"
                                    min="1"
                                    value={line.quantity}
                                    onChange={(e) => handleLineChange(line.id, 'quantity', parseFloat(e.target.value) || 0)}
                                />
                            </div>

                            <div className="col-span-2 flex flex-col gap-2">
                                <label className="text-xs font-semibold text-muted-foreground uppercase">Unit Cost</label>
                                <Input
                                    type="number"
                                    min="0"
                                    step="0.01"
                                    value={line.unitCost}
                                    onChange={(e) => handleLineChange(line.id, 'unitCost', parseFloat(e.target.value) || 0)}
                                />
                            </div>

                            <div className="col-span-2 flex flex-col gap-2">
                                <label className="text-xs font-semibold text-muted-foreground uppercase">Line Total</label>
                                <div className="h-10 flex items-center font-medium bg-muted/50 px-3 rounded-md border border-transparent">
                                    {formatCurrency(line.quantity * line.unitCost)}
                                </div>
                            </div>

                            <div className="col-span-1 flex justify-end pb-1">
                                <Button variant="ghost" size="icon" className="text-destructive hover:text-destructive hover:bg-destructive/10" onClick={() => handleRemoveLine(line.id)}>
                                    <Trash2 className="h-4 w-4" />
                                </Button>
                            </div>
                        </div>
                    ))}

                    {lines.length === 0 && (
                        <div className="text-center py-8 text-sm text-muted-foreground italic border rounded-md">
                            No items added yet. Click "Add Item" to begin.
                        </div>
                    )}
                </div>

                <div className="flex justify-between items-center mt-6 pt-4 border-t">
                    <div className="text-lg font-bold">
                        Grand Total: <span className="text-primary">{formatCurrency(calculateTotal())}</span>
                    </div>
                    <Button onClick={handleSubmit} disabled={submitting || lines.length === 0}>
                        {submitting ? "Processing..." : "Finish Purchase"}
                    </Button>
                </div>
            </div>
        </div>
    )
}
