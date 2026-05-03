"use client"

import { useState, useEffect } from "react"
import { useForm } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { InventoryAdjustmentFormValues, inventoryAdjustmentSchema } from "@/lib/validations/inventory"
import { inventoryService } from "@/services/inventory-service"
import { productService } from "@/services/product-service"
import { Product, Variant } from "@/types/inventory"
import { useAuth } from "@/hooks/use-auth"
import { Button } from "@/components/ui/button"
import {
    Form,
    FormControl,
    FormDescription,
    FormField,
    FormItem,
    FormLabel,
    FormMessage,
} from "@/components/ui/form"
import { Input } from "@/components/ui/input"
import {
    Select,
    SelectContent,
    SelectItem,
    SelectTrigger,
    SelectValue,
} from "@/components/ui/select"
import { Textarea } from "@/components/ui/textarea"
import { toast } from "sonner"
import {
    Command,
    CommandEmpty,
    CommandGroup,
    CommandInput,
    CommandItem,
    CommandList,
} from "@/components/ui/command"
import {
    Popover,
    PopoverContent,
    PopoverTrigger,
} from "@/components/ui/popover"
import { Check, ChevronsUpDown } from "lucide-react"
import { cn } from "@/lib/utils"

export function AdjustmentForm() {
    const { selectedLocation } = useAuth()
    const [loading, setLoading] = useState(false)
    const [products, setProducts] = useState<Product[]>([])
    const [selectedProduct, setSelectedProduct] = useState<Product | null>(null)
    const [openProductSelect, setOpenProductSelect] = useState(false)

    // Form definition
    const form = useForm({
        resolver: zodResolver(inventoryAdjustmentSchema),
        defaultValues: {
            productVariantId: "",
            locationId: selectedLocation?.id || "",
            quantity: 0,
            type: "StockIn", // Default
            reason: ""
        },
    })

    // Load products on mount
    useEffect(() => {
        const loadProducts = async () => {
            try {
                const data = await productService.getProducts()
                setProducts(data)
            } catch (error) {
                console.error("Failed to load products", error)
                toast.error("Failed to load products")
            }
        }
        loadProducts()
    }, [])

    // Update locationId if selectedLocation changes (though it shouldn't often)
    useEffect(() => {
        if (selectedLocation) {
            form.setValue("locationId", selectedLocation.id)
        }
    }, [selectedLocation, form])

    const onSubmit = async (data: InventoryAdjustmentFormValues) => {
        if (!selectedLocation) {
            toast.error("No location selected. Please select a location globally.")
            return
        }

        setLoading(true)
        try {
            await inventoryService.adjustInventory({
                productVariantId: data.productVariantId,
                locationId: selectedLocation.id,
                // Rereading logic: backend DTO has "AdjustmentQuantity".
                // If the user selects "StockOut", we probably want to send a negative number OR backend uses TransactionType to determine sign.
                // Usually "Adjustment" endpoints expect the signed delta or the backend handles based on type.
                // DTO comment says "Can be negative". This implies WE should control the sign.
                // Let's implement sign flipping on frontend for safety/clarity.
                adjustmentQuantity: (data.type === "StockOut" || data.type === "Damage" || data.type === "Return")
                    ? -Math.abs(data.quantity)
                    : Math.abs(data.quantity),
                transactionType: data.type,
                reason: data.reason
            })
            toast.success("Inventory adjusted successfully")
            form.reset({
                productVariantId: "",
                locationId: selectedLocation.id,
                quantity: 0,
                type: "StockIn",
                reason: ""
            })
            setSelectedProduct(null)
        } catch (error) {
            console.error("Failed to adjust inventory", error)
            toast.error("Failed to adjust inventory")
        } finally {
            setLoading(false)
        }
    }

    return (
        <Form {...form}>
            <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-6">
                <div className="bg-card rounded-xl border bg-card text-card-foreground shadow">
                    <div className="flex flex-col space-y-1.5 p-6">
                        <h3 className="font-semibold leading-none tracking-tight">Adjustment Details</h3>
                        <p className="text-sm text-muted-foreground">Enter details to adjust inventory.</p>
                    </div>
                    <div className="p-6 pt-0 grid grid-cols-1 md:grid-cols-2 gap-6">
                        {/* Product Selection */}
                        <FormField
                            control={form.control}
                            name="productVariantId"
                            render={({ field }) => (
                                <FormItem className="flex flex-col">
                                    <FormLabel>Product / Variant</FormLabel>
                                    <Popover open={openProductSelect} onOpenChange={setOpenProductSelect}>
                                        <PopoverTrigger asChild>
                                            <FormControl>
                                                <Button
                                                    variant="outline"
                                                    role="combobox"
                                                    aria-expanded={openProductSelect}
                                                    className={cn(
                                                        "w-full justify-between",
                                                        !field.value && "text-muted-foreground"
                                                    )}
                                                >
                                                    {field.value
                                                        ? (() => {
                                                            for (const p of products) {
                                                                const v = (p.variants || []).find(v => v.variantId === field.value);
                                                                if (v) {
                                                                    const isDefault = v.variantName === "Default";
                                                                    return isDefault ? p.productName : `${p.productName} - ${v.variantName}`;
                                                                }
                                                            }
                                                            return "Select Product...";
                                                        })()
                                                        : "Select product..."}
                                                    <ChevronsUpDown className="ml-2 h-4 w-4 shrink-0 opacity-50" />
                                                </Button>
                                            </FormControl>
                                        </PopoverTrigger>
                                        <PopoverContent className="w-[--radix-popover-trigger-width] p-0">
                                            <Command>
                                                <CommandInput placeholder="Search product..." />
                                                <CommandList>
                                                    <CommandEmpty>No product found.</CommandEmpty>
                                                    <CommandGroup>
                                                        {products.map((product) => (
                                                            (product.variants || []).map((variant) => {
                                                                const isDefault = variant.variantName === "Default";
                                                                const displayName = isDefault ? product.productName : `${product.productName} - ${variant.variantName}`;
                                                                const key = variant.variantId || `${product.productId}-${variant.sku}`;

                                                                return (
                                                                    <CommandItem
                                                                        key={key}
                                                                        value={`${displayName} ${variant.sku}`}
                                                                        onSelect={() => {
                                                                            form.setValue("productVariantId", variant.variantId || "")
                                                                            setSelectedProduct(product)
                                                                            setOpenProductSelect(false)
                                                                        }}
                                                                    >
                                                                        <Check
                                                                            className={cn(
                                                                                "mr-2 h-4 w-4",
                                                                                field.value === variant.variantId ? "opacity-100" : "opacity-0"
                                                                            )}
                                                                        />
                                                                        <div className="flex flex-col">
                                                                            <span>{displayName}</span>
                                                                            <span className="text-xs text-muted-foreground">SKU: {variant.sku}</span>
                                                                        </div>
                                                                    </CommandItem>
                                                                );
                                                            })
                                                        ))}
                                                    </CommandGroup>
                                                </CommandList>
                                            </Command>
                                        </PopoverContent>
                                    </Popover>
                                    <FormMessage />
                                </FormItem>
                            )}
                        />

                        {/* Transaction Type */}
                        <FormField
                            control={form.control}
                            name="type"
                            render={({ field }) => (
                                <FormItem>
                                    <FormLabel>Adjustment Type</FormLabel>
                                    <Select onValueChange={field.onChange} defaultValue={field.value}>
                                        <FormControl>
                                            <SelectTrigger>
                                                <SelectValue placeholder="Select type" />
                                            </SelectTrigger>
                                        </FormControl>
                                        <SelectContent>
                                            <SelectItem value="Damage">Damage (Remove)</SelectItem>
                                            <SelectItem value="Return">Return (Restock)</SelectItem>
                                            <SelectItem value="Audit">Audit (Correction)</SelectItem>
                                        </SelectContent>
                                    </Select>
                                    <FormMessage />
                                </FormItem>
                            )}
                        />

                        {/* Quantity */}
                        <FormField
                            control={form.control}
                            name="quantity"
                            render={({ field }) => (
                                <FormItem>
                                    <FormLabel>Quantity</FormLabel>
                                    <FormControl>
                                        <Input
                                            type="number"
                                            min="0"
                                            step="1"
                                            placeholder="0"
                                            {...field}
                                            value={(field.value as any) || ""}
                                            onChange={e => field.onChange(parseFloat(e.target.value))}
                                        />
                                    </FormControl>
                                    <FormMessage />
                                </FormItem>
                            )}
                        />

                        {/* Empty placeholder to align grid if needed, or Current Location info */}
                        <div className="flex flex-col justify-end pb-2">
                            <div className="text-sm text-muted-foreground">
                                Current Location: <span className="font-medium text-foreground">{selectedLocation?.locationName || "None"}</span>
                            </div>
                        </div>

                        {/* Reason */}
                        <div className="col-span-1 md:col-span-2">
                            <FormField
                                control={form.control}
                                name="reason"
                                render={({ field }) => (
                                    <FormItem>
                                        <FormLabel>Reason</FormLabel>
                                        <FormControl>
                                            <Textarea
                                                placeholder="Explain why this adjustment is being made..."
                                                className="resize-none min-h-[100px]"
                                                {...field}
                                            />
                                        </FormControl>
                                        <FormMessage />
                                    </FormItem>
                                )}
                            />
                        </div>
                    </div>
                </div>

                <div className="flex justify-end gap-4">
                    <Button type="button" variant="outline" onClick={() => form.reset()}>Reset</Button>
                    <Button type="submit" disabled={loading}>
                        {loading ? "Processing..." : "Submit Adjustment"}
                    </Button>
                </div>
            </form>
        </Form>
    )
}
