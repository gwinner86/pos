"use client"

import { useState, useEffect } from "react"
import { useForm, useFieldArray } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { useRouter } from "next/navigation"
import { ProductFormValues, productSchema } from "@/lib/validations/product"
import { productService, categoryService } from "@/services/product-service"
import { supplierService } from "@/services/supplier-service"
import { Supplier } from "@/types/crm" // Import Supplier from types
import { Category, Product, Variant } from "@/types/inventory"
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
import { Textarea } from "@/components/ui/textarea"
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs"
import {
    Select,
    SelectContent,
    SelectItem,
    SelectTrigger,
    SelectValue,
} from "@/components/ui/select"
import { Checkbox } from "@/components/ui/checkbox"
import { Label } from "@/components/ui/label"
import { toast } from "sonner"
import { useAuth } from "@/hooks/use-auth"

interface ProductFormProps {
    initialData?: Product;
    onSuccess?: () => void;
}

export function ProductForm({ initialData, onSuccess }: ProductFormProps) {
    const router = useRouter()
    const { selectedLocation } = useAuth()
    const [categories, setCategories] = useState<Category[]>([])
    const [suppliers, setSuppliers] = useState<Supplier[]>([])
    const [loading, setLoading] = useState(false)

    const form = useForm<ProductFormValues>({
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        resolver: zodResolver(productSchema) as any,
        defaultValues: {
            productName: "",
            sku: "",
            categoryId: "",
            supplierId: "",
            isActive: true,
            isVatExcluded: false,
            barcode: "",
            description: "",
            image1: "",
            variants: []
        },
    })

    const { fields, append, remove } = useFieldArray({
        control: form.control,
        name: "variants"
    })

    useEffect(() => {
        if (initialData) {
            form.reset({
                productName: initialData.productName,
                sku: initialData.sku,
                categoryId: initialData.categoryId || "",
                supplierId: initialData.supplierId || "", // Map supplier
                isActive: initialData.isActive,
                isVatExcluded: initialData.isVatExcluded || false,
                barcode: initialData.barcode || "",
                description: initialData.description || "",
                image1: (initialData as any).image1 || "",
                variants: initialData.variants?.map((v: Variant) => ({
                    variantId: v.variantId,
                    variantName: v.variantName,
                    sku: v.sku,
                    price: v.price || 0,
                    cost: v.cost || 0,
                    stockLevel: v.stockLevel || 0,
                    minStockLevel: v.minStockLevel || 0,
                    attributes: v.attributes || {}
                })) || []
            });
        }
    }, [initialData, form]);

    useEffect(() => {
        loadCategories()
        loadSuppliers()
    }, [])

    const loadCategories = async () => {
        try {
            const data = await categoryService.getCategories()
            console.log("DEBUG: Loaded Categories:", data);
            setCategories(data)
        } catch (error) {
            console.error("Failed to load categories", error)
        }
    }

    const loadSuppliers = async () => {
        try {
            const data = await supplierService.getSuppliers()
            setSuppliers(data)
        } catch (error) {
            console.error("Failed to load suppliers", error)
        }
    }
    const onSubmit = async (data: ProductFormValues) => {
        setLoading(true)

        if (!selectedLocation?.id && data.variants.some(v => (v.stockLevel || 0) > 0)) {
            toast.warning("No location selected. Stock levels will not be saved to inventory.");
        }

        // Enforce unique variant SKUs
        const variantSkus = data.variants.map(v => v.sku.trim().toLowerCase()).filter(s => s.length > 0);
        if (new Set(variantSkus).size !== variantSkus.length) {
            toast.error("Validation Error: Each variant must have a unique SKU.");
            setLoading(false);
            return;
        }

        try {
            const payload: any = {
                ...data,
                locationId: selectedLocation?.id, // Pass Location ID
                sku: data.sku || (data.variants && data.variants.length > 0 ? data.variants[0].sku : "GEN-" + Date.now()),
            };

            if (initialData) {
                await productService.updateProduct(initialData.productId, payload);
                toast.success("Product updated successfully");
                router.refresh();
                if (onSuccess) onSuccess();
            } else {
                await productService.createProduct(payload);
                toast.success("Product created successfully");
                // router.push("/dashboard/inventory/products"); // Removed as we are in a sheet/modal often
                router.refresh();
                form.reset();
                if (onSuccess) onSuccess();
            }
        } catch (error: any) {
            console.error("Product Form Error:", error);
            const serverError = error.response?.data?.Message || error.response?.data?.message || error.message || "Unknown error";
            toast.error(`${initialData ? "Update" : "Creation"} failed: ${serverError}`);
        } finally {
            setLoading(false);
        }
    }

    return (
        <Form {...form}>
            <form onSubmit={form.handleSubmit(onSubmit, (errors) => {
                let errorMessage = "Please check the form for errors";
                if (errors.variants && Array.isArray(errors.variants)) {
                    const variantErrorIndex = errors.variants.findIndex(v => v !== undefined);
                    if (variantErrorIndex !== -1) {
                        const vError = errors.variants[variantErrorIndex];
                        if (vError) {
                            const fieldName = Object.keys(vError)[0];
                            errorMessage = `Variant ${variantErrorIndex + 1}: ${vError[fieldName as keyof typeof vError]?.message}`;
                        }
                    }
                } else {
                    const firstError = Object.values(errors)[0];
                    errorMessage = firstError?.message as string || errorMessage;
                }
                console.error("Form Validation Errors:", errors);
                toast.error(`Validation Error: ${errorMessage}`);
            })} className="space-y-8">
                <Tabs defaultValue="general" className="w-full">
                    <TabsList>
                        <TabsTrigger value="general">General</TabsTrigger>
                        <TabsTrigger value="variants">Variants (Pricing & Stock)</TabsTrigger>
                    </TabsList>

                    <TabsContent value="general" className="space-y-4 py-4">
                        <FormField
                            control={form.control}
                            name="productName"
                            render={({ field }) => (
                                <FormItem>
                                    <FormLabel>Product Name</FormLabel>
                                    <FormControl>
                                        <Input placeholder="e.g. Wireless Mouse" {...field} />
                                    </FormControl>
                                    <FormMessage />
                                </FormItem>
                            )}
                        />
                        <div className="grid grid-cols-2 gap-4">
                            <FormField
                                control={form.control}
                                name="categoryId"
                                render={({ field }) => (
                                    <FormItem>
                                        <FormLabel>Category ({categories.length})</FormLabel>
                                        <FormControl>
                                            <select
                                                className="flex h-10 w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background file:border-0 file:bg-transparent file:text-sm file:font-medium placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50"
                                                value={field.value || ""}
                                                onChange={(e) => field.onChange(e.target.value)}
                                            >
                                                <option value="" disabled>Select a category</option>
                                                {categories.map((category) => (
                                                    <option key={category.categoryId} value={category.categoryId}>
                                                        {category.categoryName}
                                                    </option>
                                                ))}
                                            </select>
                                        </FormControl>
                                        <FormMessage />
                                    </FormItem>
                                )}
                            />
                            <FormField
                                control={form.control}
                                name="supplierId"
                                render={({ field }) => (
                                    <FormItem>
                                        <FormLabel>Supplier ({suppliers.length})</FormLabel>
                                        <FormControl>
                                            <select
                                                className="flex h-10 w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background file:border-0 file:bg-transparent file:text-sm file:font-medium placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50"
                                                value={field.value || ""}
                                                onChange={(e) => field.onChange(e.target.value)}
                                            >
                                                <option value="" disabled>Select a supplier</option>
                                                {suppliers.map((s) => (
                                                    <option key={s.id} value={s.id}>
                                                        {s.supplierName}
                                                    </option>
                                                ))}
                                            </select>
                                        </FormControl>
                                        <FormMessage />
                                    </FormItem>
                                )}
                            />
                        </div>
                        <FormField
                            control={form.control}
                            name="isActive"
                            render={({ field }) => (
                                <FormItem className="flex flex-row items-start space-x-3 space-y-0 rounded-md border p-4">
                                    <FormControl>
                                        <Checkbox
                                            checked={field.value}
                                            onCheckedChange={field.onChange}
                                        />
                                    </FormControl>
                                    <div className="space-y-1 leading-none">
                                        <FormLabel>
                                            Active Status
                                        </FormLabel>
                                        <FormDescription>
                                            This product will be visible in POS and other lists.
                                        </FormDescription>
                                    </div>
                                </FormItem>
                            )}
                        />
                        <FormField
                            control={form.control}
                            name="isVatExcluded"
                            render={({ field }) => (
                                <FormItem className="flex flex-row items-start space-x-3 space-y-0 rounded-md border p-4">
                                    <FormControl>
                                        <Checkbox
                                            checked={field.value}
                                            onCheckedChange={field.onChange}
                                        />
                                    </FormControl>
                                    <div className="space-y-1 leading-none">
                                        <FormLabel>
                                            Exclude from VAT
                                        </FormLabel>
                                        <FormDescription>
                                            If checked, this product will NOT be charged VAT during sales.
                                        </FormDescription>
                                    </div>
                                </FormItem>
                            )}
                        />
                        <FormField
                            control={form.control}
                            name="description"
                            render={({ field }) => (
                                <FormItem>
                                    <FormLabel>Description</FormLabel>
                                    <FormControl>
                                        <Textarea placeholder="Product description..." className="resize-none" {...field} />
                                    </FormControl>
                                    <FormMessage />
                                </FormItem>
                            )}
                        />
                        <div className="grid w-full max-w-sm items-center gap-1.5">
                            <Label htmlFor="picture">Product Image</Label>
                            <Input
                                id="picture"
                                type="file"
                                onChange={async (e) => {
                                    const file = e.target.files?.[0];
                                    if (file) {
                                        try {
                                            const url = await productService.uploadImage(file);
                                            form.setValue('image1', url);
                                            toast.success("Image uploaded!");
                                        } catch (err) {
                                            toast.error("Failed to upload image");
                                        }
                                    }
                                }}
                            />
                            {form.watch('image1') && (
                                <img
                                    src={form.watch('image1')?.startsWith('http')
                                        ? form.watch('image1')
                                        : `http://localhost:5047${form.watch('image1')?.startsWith('/') ? '' : '/'}${form.watch('image1')}`
                                    }
                                    alt="Preview"
                                    className="h-20 w-20 object-cover mt-2 rounded-md"
                                />
                            )}
                        </div>
                    </TabsContent>

                    <TabsContent value="variants" className="space-y-4 py-4">
                        <div className="flex justify-end mb-4">
                            <Button
                                type="button"
                                variant="outline"
                                onClick={() => append({
                                    sku: "",
                                    variantName: "",
                                    price: 0,
                                    cost: 0,
                                    stockLevel: 0,
                                    minStockLevel: 0,
                                    attributes: {}
                                })}
                            >
                                Add Variant
                            </Button>
                        </div>
                        {fields.length === 0 && (
                            <div className="text-sm text-red-500 mb-4">
                                At least one variant is required (e.g. "Default").
                            </div>
                        )}
                        <div className="space-y-4">
                            {fields.map((field, index) => (
                                <div key={field.id} className="grid grid-cols-6 gap-4 items-end border p-4 rounded-md">
                                    <input
                                        type="hidden"
                                        {...form.register(`variants.${index}.variantId`)}
                                        defaultValue={field.variantId}
                                    />

                                    <div className="col-span-2">
                                        <FormField
                                            control={form.control}
                                            name={`variants.${index}.variantName`}
                                            render={({ field }) => (
                                                <FormItem>
                                                    <FormLabel>Name (e.g. Size L)</FormLabel>
                                                    <FormControl>
                                                        <Input placeholder="Default" {...field} />
                                                    </FormControl>
                                                    <FormMessage />
                                                </FormItem>
                                            )}
                                        />
                                    </div>

                                    <div className="col-span-2">
                                        <FormField
                                            control={form.control}
                                            name={`variants.${index}.sku`}
                                            render={({ field }) => (
                                                <FormItem>
                                                    <FormLabel>SKU</FormLabel>
                                                    <FormControl>
                                                        <Input placeholder="SKU-001" {...field} />
                                                    </FormControl>
                                                    <FormMessage />
                                                </FormItem>
                                            )}
                                        />
                                    </div>

                                    <div className="col-span-1">
                                        <FormField
                                            control={form.control}
                                            name={`variants.${index}.cost`}
                                            render={({ field }) => (
                                                <FormItem>
                                                    <FormLabel>Cost</FormLabel>
                                                    <FormControl>
                                                        <Input
                                                            type="number"
                                                            step="0.01"
                                                            {...field}
                                                            value={Number.isNaN(field.value) ? '' : field.value}
                                                            onChange={e => field.onChange(e.target.valueAsNumber)}
                                                        />
                                                    </FormControl>
                                                    <FormMessage />
                                                </FormItem>
                                            )}
                                        />
                                    </div>

                                    <div className="col-span-1">
                                        <FormField
                                            control={form.control}
                                            name={`variants.${index}.price`}
                                            render={({ field }) => (
                                                <FormItem>
                                                    <FormLabel>Selling Price</FormLabel>
                                                    <FormControl>
                                                        <Input
                                                            type="number"
                                                            step="0.01"
                                                            {...field}
                                                            value={Number.isNaN(field.value) ? '' : field.value}
                                                            onChange={e => field.onChange(e.target.valueAsNumber)}
                                                        />
                                                    </FormControl>
                                                    <FormMessage />
                                                </FormItem>
                                            )}
                                        />
                                    </div>

                                    <div className="col-span-1">
                                        <FormField
                                            control={form.control}
                                            name={`variants.${index}.stockLevel`}
                                            render={({ field }) => (
                                                <FormItem>
                                                    <FormLabel>Qty In Stock</FormLabel>
                                                    <FormControl>
                                                        <Input
                                                            type="number"
                                                            {...field}
                                                            value={Number.isNaN(field.value) ? '' : field.value}
                                                            onChange={e => field.onChange(e.target.valueAsNumber)}
                                                        />
                                                    </FormControl>
                                                    <FormMessage />
                                                </FormItem>
                                            )}
                                        />
                                    </div>

                                    <div className="col-span-1">
                                        <FormField
                                            control={form.control}
                                            name={`variants.${index}.minStockLevel`}
                                            render={({ field }) => (
                                                <FormItem>
                                                    <FormLabel>Low Stock</FormLabel>
                                                    <FormControl>
                                                        <Input
                                                            type="number"
                                                            {...field}
                                                            value={Number.isNaN(field.value) ? '' : field.value}
                                                            onChange={e => field.onChange(e.target.valueAsNumber)}
                                                        />
                                                    </FormControl>
                                                    <FormMessage />
                                                </FormItem>
                                            )}
                                        />
                                    </div>


                                    <div className="col-span-6 flex justify-end">
                                        <Button
                                            type="button"
                                            variant="destructive"
                                            size="sm"
                                            onClick={() => remove(index)}
                                        >
                                            Remove
                                        </Button>
                                    </div>
                                </div>
                            ))}
                        </div>
                    </TabsContent>
                </Tabs>

                <div className="flex justify-end">
                    <Button type="submit" disabled={loading}>
                        {initialData
                            ? (loading ? "Updating..." : "Update Product")
                            : (loading ? "Creating..." : "Create Product")
                        }
                    </Button>
                </div>
            </form>
        </Form >
    )
}
