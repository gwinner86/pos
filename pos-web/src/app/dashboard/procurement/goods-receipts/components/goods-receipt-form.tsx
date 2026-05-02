"use client";

import { useState, useEffect } from "react";
import { useForm, useFieldArray } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import * as z from "zod";
import { useRouter } from "next/navigation";
import { Button } from "@/components/ui/button";
import {
    Form,
    FormControl,
    FormField,
    FormItem,
    FormLabel,
    FormMessage,
} from "@/components/ui/form";
import { Input } from "@/components/ui/input";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Trash, Plus } from "lucide-react";
import { toast } from "sonner";
import { supplierService } from "@/services/supplier-service";
import { productService } from "@/services/product-service";
import { goodsReceiptService } from "@/services/goods-receipt-service";
import { Supplier } from "@/types/crm";
import { Product } from "@/types/inventory";
import { formatCurrency } from "@/lib/utils";
import { useAuth } from "@/hooks/use-auth";
import { GoodsReceipt } from "@/services/goods-receipt-service";

const receiptSchema = z.object({
    supplierId: z.string().optional(),
    receiptDate: z.string(), // ISO date string or YYYY-MM-DD
    items: z.array(z.object({
        productId: z.string().min(1, "Product is required"),
        variantId: z.string().optional(),
        quantityReceived: z.coerce.number().positive("Quantity must be > 0"),
        unitCost: z.coerce.number().min(0, "Cost must be >= 0"),
    })).min(1, "Add at least one item"),
});

type ReceiptFormValues = z.infer<typeof receiptSchema>;

interface GoodsReceiptFormProps {
    initialData?: GoodsReceipt | null;
}

export function GoodsReceiptForm({ initialData }: GoodsReceiptFormProps) {
    const router = useRouter();
    const { selectedLocation, selectedCompany } = useAuth();
    const [loading, setLoading] = useState(false);
    const [suppliers, setSuppliers] = useState<Supplier[]>([]);
    const [products, setProducts] = useState<Product[]>([]);

    useEffect(() => {
        const load = async () => {
            try {
                const [supData, prodData] = await Promise.all([
                    supplierService.getSuppliers(),
                    productService.getProducts()
                ]);
                setSuppliers(supData);
                setProducts(prodData);
            } catch (err) {
                console.error(err);
                toast.error("Failed to load suppliers or products");
            }
        };
        load();
    }, []);

    const form = useForm<ReceiptFormValues>({
        resolver: zodResolver(receiptSchema) as any,
        defaultValues: initialData ? {
            supplierId: initialData.supplierId || "none",
            receiptDate: initialData.receiptDate ? new Date(initialData.receiptDate).toISOString().split('T')[0] : new Date().toISOString().split('T')[0],
            items: initialData.details.map(d => ({
                id: d.id,
                productId: d.productId,
                variantId: d.productVariantId,
                quantityReceived: d.quantityReceived,
                unitCost: d.unitCost
            }))
        } : {
            supplierId: "none",
            receiptDate: new Date().toISOString().split('T')[0],
            items: [{ productId: "", quantityReceived: 1, unitCost: 0 }]
        },
    });

    const { fields, append, remove } = useFieldArray({
        control: form.control,
        name: "items"
    });

    const items = form.watch("items");
    const totalAmount = items.reduce((sum, item) => sum + (item.quantityReceived * item.unitCost), 0);

    const onSubmit = async (data: ReceiptFormValues) => {
        if (!selectedLocation) {
            toast.error("Please select a location first");
            return;
        }

        setLoading(true);
        try {
            const payload: any = {
                locationId: selectedLocation.id,
                receiptDate: new Date(data.receiptDate).toISOString(),
                totalReceivedAmount: totalAmount,
                details: data.items.map(i => {
                    const product = products.find(p => p.productId === i.productId);
                    let variantId = i.variantId;
                    if (!variantId || variantId === "none" || variantId === "") {
                        if (product && product.variants && product.variants.length > 0) {
                            variantId = product.variants[0].variantId;
                        }
                    }
                    if (!variantId) throw new Error("Variant ID missing");

                    return {
                        productId: i.productId, // Interface compatibility
                        productVariantId: variantId,
                        quantityReceived: Number(i.quantityReceived),
                        unitCost: Number(i.unitCost)
                    };
                })
            };

            if (data.supplierId && data.supplierId !== "none") {
                payload.supplierId = data.supplierId;
            }

            if (initialData) {
                // Update mapping
                const updatePayload = {
                    receiptDate: payload.receiptDate,
                    details: data.items.map(i => {
                        const product = products.find(p => p.productId === i.productId);
                        let variantId = i.variantId;
                        if (!variantId || variantId === "none" || variantId === "") {
                            if (product && product.variants && product.variants.length > 0) {
                                variantId = product.variants[0].variantId;
                            }
                        }
                        return {
                            id: (i as any).id, // Pass existing ID if present
                            productVariantId: variantId as string,
                            quantityReceived: Number(i.quantityReceived),
                            unitCost: Number(i.unitCost)
                        };
                    })
                };
                await goodsReceiptService.updateReceipt(initialData.id, updatePayload);
                toast.success("Goods Receipt updated successfully");
            } else {
                await goodsReceiptService.createReceipt(payload);
                toast.success("Goods Receipt created successfully");
            }

            router.push("/dashboard/procurement/goods-receipts");
            router.refresh();
        } catch (error: any) {
            console.error(error);
            toast.error(error.message || "Failed to create Goods Receipt");
        } finally {
            setLoading(false);
        }
    };

    return (
        <Form {...form}>
            <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-6">
                <Card>
                    <CardHeader>
                        <CardTitle>Receipt Details</CardTitle>
                    </CardHeader>
                    <CardContent className="grid grid-cols-2 gap-4">
                        <FormField
                            control={form.control}
                            name="supplierId"
                            render={({ field }) => (
                                <FormItem>
                                    <FormLabel>Supplier (Optional)</FormLabel>
                                    <Select onValueChange={field.onChange} value={field.value}>
                                        <FormControl>
                                            <SelectTrigger>
                                                <SelectValue placeholder="Select Supplier (Optional)" />
                                            </SelectTrigger>
                                        </FormControl>
                                        <SelectContent>
                                            <SelectItem value="none">No Supplier</SelectItem>
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
                            name="receiptDate"
                            render={({ field }) => (
                                <FormItem>
                                    <FormLabel>Receipt Date</FormLabel>
                                    <FormControl>
                                        <Input type="date" {...field} />
                                    </FormControl>
                                    <FormMessage />
                                </FormItem>
                            )}
                        />
                    </CardContent>
                </Card>

                <Card>
                    <CardHeader className="flex flex-row items-center justify-between">
                        <CardTitle>Items Received</CardTitle>
                        <Button type="button" variant="outline" size="sm" onClick={() => append({ productId: "", quantityReceived: 1, unitCost: 0 })}>
                            <Plus className="mr-2 h-4 w-4" /> Add Item
                        </Button>
                    </CardHeader>
                    <CardContent className="space-y-4">
                        {fields.map((field, index) => {
                            const selectedProductId = form.watch(`items.${index}.productId`);
                            const selectedProduct = products.find(p => p.productId === selectedProductId);

                            return (
                                <div key={field.id} className="grid grid-cols-12 gap-2 items-end border p-3 rounded-md bg-muted/20">
                                    <div className="col-span-7">
                                        <FormField
                                            control={form.control}
                                            name={`items.${index}.variantId`}
                                            render={({ field }) => {
                                                const currentProductId = form.watch(`items.${index}.productId`);
                                                const compositeValue = (currentProductId && field.value) ? `${currentProductId}|${field.value}` : "";

                                                return (
                                                    <FormItem>
                                                        <FormLabel className="text-xs">Product Variant</FormLabel>
                                                        <Select onValueChange={(val) => {
                                                            const [pId, vId] = val.split('|');
                                                            form.setValue(`items.${index}.productId`, pId);
                                                            field.onChange(vId);

                                                            const p = products.find(prod => prod.productId === pId);
                                                            if (p) {
                                                                if (vId && vId !== 'none' && p.variants) {
                                                                    const v = p.variants.find(vr => vr.variantId === vId);
                                                                    if (v && v.cost) form.setValue(`items.${index}.unitCost`, v.cost);
                                                                } else {
                                                                    if (p.cost) form.setValue(`items.${index}.unitCost`, p.cost);
                                                                }
                                                            }
                                                        }} value={compositeValue}>
                                                            <FormControl>
                                                                <SelectTrigger className="h-8">
                                                                    <SelectValue placeholder="Select Product Variant" />
                                                                </SelectTrigger>
                                                            </FormControl>
                                                            <SelectContent>
                                                                {products.map(p => {
                                                                    if (!p.hasVariants || !p.variants || p.variants.length === 0) {
                                                                        return (
                                                                            <SelectItem key={p.productId} value={`${p.productId}|none`}>
                                                                                {p.productName} (Stock: {p.stockLevel || 0})
                                                                            </SelectItem>
                                                                        );
                                                                    }
                                                                    return p.variants.map(v => (
                                                                        <SelectItem key={v.variantId || `temp-${p.productId}`} value={`${p.productId}|${v.variantId}`}>
                                                                            {p.productName} - {v.variantName || v.sku} (Stock: {v.stockLevel || 0})
                                                                        </SelectItem>
                                                                    ));
                                                                })}
                                                            </SelectContent>
                                                        </Select>
                                                    </FormItem>
                                                );
                                            }}
                                        />
                                    </div>

                                    <div className="col-span-2">
                                        <FormField
                                            control={form.control}
                                            name={`items.${index}.quantityReceived`}
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
                            );
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
                        {loading ? "Saving..." : (initialData ? "Update Goods Receipt" : "Create Goods Receipt")}
                    </Button>
                </div>
            </form>
        </Form>
    );
}
