import * as z from "zod"

export const inventoryAdjustmentSchema = z.object({
    productVariantId: z.string().min(1, "Product variant is required"),
    locationId: z.string().min(1, "Location is required"),
    quantity: z.coerce.number().min(0.0001, "Quantity must be greater than 0"),
    type: z.enum(["StockIn", "StockOut", "Audit", "Return", "Damage"]),
    reason: z.string().min(3, "Reason must be at least 3 characters"),
})

export type InventoryAdjustmentFormValues = z.infer<typeof inventoryAdjustmentSchema>
