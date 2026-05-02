"use client"

import { useEffect, useState } from "react"
import { Product } from "@/types/inventory"
import { getColumns } from "./columns" // Import getColumns instead of columns
import { DataTable } from "@/components/ui/data-table"
import { productService } from "@/services/product-service"
import { Button } from "@/components/ui/button"
import { Plus, Download, Trash2 } from "lucide-react"
import {
    Sheet,
    SheetContent,
    SheetDescription,
    SheetHeader,
    SheetTitle,
    SheetTrigger,
} from "@/components/ui/sheet"
import { ProductForm } from "./product-form"
import { ProductDetails } from "./product-details"
import Swal from "sweetalert2"
import { toast } from "sonner"
import { ProductBranchModal } from "./product-branch-modal"
import { useAuth } from "@/hooks/use-auth"

export default function ProductsPage() {
    const { selectedLocation } = useAuth() // Get location
    const [products, setProducts] = useState<Product[]>([])
    const [loading, setLoading] = useState(true)
    const [selectedProduct, setSelectedProduct] = useState<Product | null>(null)
    const [isEditOpen, setIsEditOpen] = useState(false)
    const [isViewOpen, setIsViewOpen] = useState(false)
    const [isBranchModalOpen, setIsBranchModalOpen] = useState(false)
    const [branchProduct, setBranchProduct] = useState<Product | null>(null)

    useEffect(() => {
        // Reload when location changes
        if (selectedLocation?.id) {
            loadProducts()
        } else {
            // Fallback or load global? Loading global might show 0 if logic differs.
            // Better to load global if no location, but prefer location.
            loadProducts()
        }
    }, [selectedLocation?.id])

    const loadProducts = async () => {
        setLoading(true)
        try {
            const data = await productService.getProducts(selectedLocation?.id)
            setProducts(data)
        } catch (error) {
            console.error("Failed to load products", error)
        } finally {
            setLoading(false)
        }
    }

    const handleEdit = (product: Product) => {
        setSelectedProduct(product)
        setIsEditOpen(true)
    }

    const handleView = (product: Product) => {
        setSelectedProduct(product)
        setIsViewOpen(true)
    }

    const handleDelete = async (product: Product) => {
        const result = await Swal.fire({
            title: 'Are you sure?',
            text: "Do you really want to delete this product? This action cannot be undone.",
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#d33',
            cancelButtonColor: '#3085d6',
            confirmButtonText: 'Yes, delete it!'
        })

        if (result.isConfirmed) {
            try {
                await productService.deleteProduct(product.productId)
                toast.success("Product deleted successfully")
                loadProducts()
            } catch (error) {
                console.error("Failed to delete product", error)
                toast.error("Failed to delete product")
            }
        }
    }

    const handleManageBranches = (product: Product) => {
        setBranchProduct(product)
        setIsBranchModalOpen(true)
    }

    const handleDeleteAll = async () => {
        const first = await Swal.fire({
            title: 'Delete ALL Products?',
            text: "This will permanently delete every product and all related inventory, pricing, and cost records. This action CANNOT be undone.",
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#d33',
            cancelButtonColor: '#3085d6',
            confirmButtonText: 'Yes, delete all!',
            cancelButtonText: 'Cancel'
        })

        if (!first.isConfirmed) return

        // Second confirmation for extra safety
        const second = await Swal.fire({
            title: 'Are you absolutely sure?',
            text: `Type DELETE to confirm.`,
            input: 'text',
            inputPlaceholder: 'DELETE',
            showCancelButton: true,
            confirmButtonColor: '#d33',
            confirmButtonText: 'Permanently Delete',
            preConfirm: (value) => {
                if (value !== 'DELETE') {
                    Swal.showValidationMessage('You must type DELETE to confirm')
                }
            }
        })

        if (!second.isConfirmed) return

        setLoading(true)
        try {
            const res = await productService.deleteAllProducts()
            toast.success(res.message || `All products deleted successfully`)
            setProducts([])
        } catch (error: any) {
            console.error('Failed to delete all products', error)
            const msg = error.response?.data?.message || 'Failed to delete all products.'
            toast.error(msg)
        } finally {
            setLoading(false)
        }
    }

    const handleBulkUpload = async (event: React.ChangeEvent<HTMLInputElement>) => {
        const file = event.target.files?.[0];
        if (!file) return;

        setLoading(true);
        try {
            const res = await productService.bulkUploadProducts(file);
            toast.success(res.message || "Products uploaded successfully!");
            loadProducts();
        } catch (error: any) {
            console.error("Bulk upload failed", error);
            const msg = error.response?.data?.message || "Failed to upload products.";
            toast.error(msg);
        } finally {
            setLoading(false);
            // Reset input
            event.target.value = "";
        }
    };

    const handleDownloadTemplate = () => {
        const headers = ["ProductName", "Sku", "CategoryId", "LocationId", "SupplierId", "Description", "Price", "Cost", "StockLevel", "MinStockLevel"];
        const sampleRow = ["Sample Product", "SMP-001", "", "", "", "Sample Description", "100.00", "80.00", "50", "10"];
        const csvContent = headers.join(",") + "\n" + sampleRow.join(",");

        const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.setAttribute('download', 'Product_Upload_Template.csv');
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
    };

    const columns = getColumns({
        onEdit: handleEdit,
        onView: handleView,
        onDelete: handleDelete,
        onManageBranches: handleManageBranches
    })

    return (
        <div className="flex flex-col gap-5 w-full">
            <div className="flex items-center justify-between w-full">
                <div className="flex flex-col gap-1">
                    <h2 className="text-3xl font-bold tracking-tight text-foreground">
                        Products
                    </h2>
                    <p className="text-sm text-muted-foreground">
                        Manage your product inventory
                    </p>
                </div>
                <div className="flex items-center gap-2">
                    <Button
                        className="flex items-center gap-2"
                        variant="secondary"
                        onClick={handleDownloadTemplate}
                        disabled={loading}
                    >
                        <Download className="h-4 w-4" />
                        <span>Template</span>
                    </Button>
                    <Button
                        className="flex items-center gap-2"
                        variant="outline"
                        onClick={() => document.getElementById("excel-upload")?.click()}
                        disabled={loading}
                    >
                        <Plus className="h-4 w-4" />
                        <span>Import Excel</span>
                    </Button>
                    <input
                        type="file"
                        id="excel-upload"
                        accept=".xlsx, .xls, .csv"
                        className="hidden"
                        onChange={handleBulkUpload}
                    />
                    <Sheet open={isEditOpen} onOpenChange={(open) => {
                        setIsEditOpen(open);
                        if (!open) setSelectedProduct(null); // Reset on close
                    }}>
                        <SheetTrigger asChild>
                            <Button
                                className="flex items-center gap-2"
                                variant="default"
                                onClick={() => {
                                    setSelectedProduct(null); // Ensure clean state for new product
                                    setIsEditOpen(true);
                                }}
                            >
                                <Plus className="h-4 w-4" />
                                <span>Add Product</span>
                            </Button>
                        </SheetTrigger>
                        <SheetContent className="min-w-[900px] overflow-y-auto">
                            <SheetHeader>
                                <SheetTitle>{selectedProduct ? "Edit Product" : "Create Product"}</SheetTitle>
                                <SheetDescription>
                                    {selectedProduct ? "Update product details." : "Add a new product to your inventory."}
                                </SheetDescription>
                            </SheetHeader>
                            <div className="py-4">
                                {/* Pass initialData if editing, otherwise undefined */}
                                <ProductForm
                                    initialData={selectedProduct || undefined}
                                    onSuccess={() => {
                                        setIsEditOpen(false);
                                        loadProducts();
                                    }}
                                />
                            </div>
                        </SheetContent>
                    </Sheet>
                </div>
            </div>

            {loading ? (
                <div className="flex items-center justify-center p-8">
                    <div className="text-muted-foreground">Loading products...</div>
                </div>
            ) : (
                <div className="bg-card rounded-md border shadow-sm">
                    <DataTable columns={columns} data={products} />
                </div>
            )}

            <ProductDetails
                product={selectedProduct}
                open={isViewOpen}
                onOpenChange={setIsViewOpen}
                onEdit={(product) => {
                    setIsViewOpen(false);
                    handleEdit(product);
                }}
            />

            <ProductBranchModal
                product={branchProduct}
                open={isBranchModalOpen}
                onOpenChange={(open) => {
                    setIsBranchModalOpen(open)
                    if (!open) setBranchProduct(null)
                }}
                onSuccess={loadProducts}
            />
        </div>
    )
}
