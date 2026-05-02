"use client"

import { useState } from "react"
import { useForm } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { CategoryFormValues, categorySchema } from "@/lib/validations/category"
import { Category } from "@/types/inventory"
import { categoryService } from "@/services/product-service"
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
import { Checkbox } from "@/components/ui/checkbox"
import { toast } from "sonner"

interface CategoryFormProps {
    initialData?: Category | null
    onSuccess: () => void
}

export function CategoryForm({ initialData, onSuccess }: CategoryFormProps) {
    const [loading, setLoading] = useState(false)

    const form = useForm({
        resolver: zodResolver(categorySchema),
        defaultValues: {
            categoryName: initialData?.categoryName || "",
            isActive: initialData?.isActive ?? true,
            parentCategoryId: initialData?.parentCategoryId || null,
            locationId: initialData?.locationId || null
        },
    })

    const onSubmit = async (data: CategoryFormValues) => {
        setLoading(true)
        try {
            if (initialData) {
                await categoryService.updateCategory(initialData.categoryId, data)
                toast.success("Category updated successfully")
            } else {
                await categoryService.createCategory(data)
                toast.success("Category created successfully")
            }
            onSuccess()
        } catch (error) {
            toast.error(initialData ? "Failed to update category" : "Failed to create category")
            console.error(error)
        } finally {
            setLoading(false)
        }
    }

    return (
        <Form {...form}>
            <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
                <FormField
                    control={form.control}
                    name="categoryName"
                    render={({ field }) => (
                        <FormItem>
                            <FormLabel>Category Name</FormLabel>
                            <FormControl>
                                <Input placeholder="e.g. Electronics" {...field} />
                            </FormControl>
                            <FormMessage />
                        </FormItem>
                    )}
                />

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
                                    This category will be visible in lists.
                                </FormDescription>
                            </div>
                        </FormItem>
                    )}
                />

                <div className="flex justify-end pt-4">
                    <Button type="submit" disabled={loading}>
                        {loading ? "Saving..." : (initialData ? "Save Changes" : "Create Category")}
                    </Button>
                </div>
            </form>
        </Form>
    )
}
