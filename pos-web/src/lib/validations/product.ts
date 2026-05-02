import * as z from "zod"

export const productSchema = z.object({
    productName: z.string().min(2, "Name must be at least 2 characters"),
    sku: z.string().optional(),
    barcode: z.string().optional(),
    categoryId: z.string().min(1, "Category is required"),
    supplierId: z.string().min(1, "Supplier is required"),
    description: z.string().optional(),
    image1: z.string().optional(),
    isActive: z.boolean().default(true),
    isVatExcluded: z.boolean().default(false),
    // Variants are now required and hold the inventory/pricing data
    variants: z.array(z.object({
        variantId: z.string().optional(),
        sku: z.string().min(1, "SKU is required"),
        variantName: z.string().min(1, "Name is required"),
        price: z.coerce.number().min(0, "Price must be non-negative"), // Selling Price
        cost: z.coerce.number().min(0, "Cost must be non-negative"),   // Cost Price
        stockLevel: z.coerce.number().int().min(0, "Stock must be non-negative"), // Quantity
        minStockLevel: z.coerce.number().int().min(0).default(0),
        attributes: z.record(z.string(), z.string()).optional()
    })).min(1, "At least one variant is required"),
})

export type ProductFormValues = z.infer<typeof productSchema>
