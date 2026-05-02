"use client"

import { useEffect, useState } from "react"
import { Category } from "@/types/inventory"
import { getColumns } from "./columns"
import { DataTable } from "@/components/ui/data-table"
import { categoryService } from "@/services/product-service"
import { Button } from "@/components/ui/button"
import { Plus } from "lucide-react"
import {
    Sheet,
    SheetContent,
    SheetDescription,
    SheetHeader,
    SheetTitle,
    SheetTrigger,
} from "@/components/ui/sheet"
import { CategoryForm } from "./category-form"
import { toast } from "sonner"
import {
    AlertDialog,
    AlertDialogAction,
    AlertDialogCancel,
    AlertDialogContent,
    AlertDialogDescription,
    AlertDialogFooter,
    AlertDialogHeader,
    AlertDialogTitle,
} from "@/components/ui/alert-dialog"

export default function CategoriesPage() {
    const [categories, setCategories] = useState<Category[]>([])
    const [loading, setLoading] = useState(true)
    const [isSheetOpen, setIsSheetOpen] = useState(false)
    const [editingCategory, setEditingCategory] = useState<Category | null>(null)
    const [deletingCategory, setDeletingCategory] = useState<Category | null>(null)

    useEffect(() => {
        loadCategories()
    }, [])

    const loadCategories = async () => {
        try {
            const data = await categoryService.getCategories()
            setCategories(data)
        } catch (error) {
            console.error("Failed to load categories", error)
            toast.error("Failed to load categories")
        } finally {
            setLoading(false)
        }
    }

    const handleSuccess = () => {
        setIsSheetOpen(false)
        setEditingCategory(null)
        loadCategories()
    }

    const handleEdit = (category: Category) => {
        setEditingCategory(category)
        setIsSheetOpen(true)
    }

    const handleDelete = async () => {
        if (!deletingCategory) return

        try {
            await categoryService.deleteCategory(deletingCategory.categoryId)
            toast.success("Category deleted successfully")
            loadCategories()
        } catch (error) {
            console.error("Failed to delete category", error)
            toast.error("Failed to delete category")
        } finally {
            setDeletingCategory(null)
        }
    }

    const columns = getColumns({
        onEdit: handleEdit,
        onDelete: setDeletingCategory
    })

    return (
        <div className="flex flex-col gap-5 w-full">
            <div className="flex items-center justify-between w-full">
                <div className="flex flex-col gap-1">
                    <h2 className="text-3xl font-bold tracking-tight text-foreground">
                        Categories
                    </h2>
                    <p className="text-sm text-muted-foreground">
                        Manage your product categories
                    </p>
                </div>
                <div className="flex items-center gap-2">
                    <Sheet open={isSheetOpen} onOpenChange={(open) => {
                        setIsSheetOpen(open)
                        if (!open) setEditingCategory(null)
                    }}>
                        <SheetTrigger asChild>
                            <Button className="flex items-center gap-2" variant="default">
                                <Plus className="h-4 w-4" />
                                <span>Add Category</span>
                            </Button>
                        </SheetTrigger>
                        <SheetContent className="w-[500px] sm:w-[540px] overflow-y-auto">
                            <SheetHeader>
                                <SheetTitle>{editingCategory ? "Edit Category" : "Create Category"}</SheetTitle>
                                <SheetDescription>
                                    {editingCategory ? "Update the category details." : "Add a new category to your inventory."}
                                </SheetDescription>
                            </SheetHeader>
                            <div className="py-4">
                                <CategoryForm
                                    initialData={editingCategory}
                                    onSuccess={handleSuccess}
                                />
                            </div>
                        </SheetContent>
                    </Sheet>
                </div>
            </div>

            {loading ? (
                <div className="flex items-center justify-center p-8">
                    <div className="text-muted-foreground">Loading categories...</div>
                </div>
            ) : (
                <div className="bg-card rounded-md border shadow-sm">
                    <DataTable columns={columns} data={categories} searchKey="categoryName" />
                </div>
            )}

            <AlertDialog open={!!deletingCategory} onOpenChange={(open) => !open && setDeletingCategory(null)}>
                <AlertDialogContent>
                    <AlertDialogHeader>
                        <AlertDialogTitle>Are you sure?</AlertDialogTitle>
                        <AlertDialogDescription>
                            This action cannot be undone. This will permanently delete the category
                            "{deletingCategory?.categoryName}".
                        </AlertDialogDescription>
                    </AlertDialogHeader>
                    <AlertDialogFooter>
                        <AlertDialogCancel>Cancel</AlertDialogCancel>
                        <AlertDialogAction onClick={handleDelete} className="bg-destructive text-destructive-foreground hover:bg-destructive/90">
                            Delete
                        </AlertDialogAction>
                    </AlertDialogFooter>
                </AlertDialogContent>
            </AlertDialog>
        </div>
    )
}
