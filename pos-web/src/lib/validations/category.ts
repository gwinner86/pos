import * as z from "zod"

export const categorySchema = z.object({
    categoryName: z.string().min(1, "Category name is required"),
    parentCategoryId: z.string().optional().nullable(),
    locationId: z.string().optional().nullable(),
    isActive: z.boolean().default(true),
})

export type CategoryFormValues = z.infer<typeof categorySchema>
