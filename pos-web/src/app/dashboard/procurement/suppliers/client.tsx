"use client"

import { useEffect, useState } from "react"
import { Supplier } from "@/types/crm"
import { supplierService } from "@/services/supplier-service"
import { SupplierForm } from "./supplier-form"
import { DataTable } from "@/components/ui/data-table"
import { getColumns } from "./columns"
import { Button } from "@/components/ui/button"
import { Plus } from "lucide-react"
import { Separator } from "@/components/ui/separator" // Assuming reuse
import Swal from 'sweetalert2'
import {
    Dialog,
    DialogContent,
    DialogDescription,
    DialogHeader,
    DialogTitle,
    DialogTrigger,
} from "@/components/ui/dialog"
import { toast } from "sonner"

export const SupplierClient = () => {
    const [suppliers, setSuppliers] = useState<Supplier[]>([])
    const [loading, setLoading] = useState(true)
    const [isDialogOpen, setIsDialogOpen] = useState(false)
    const [selectedSupplier, setSelectedSupplier] = useState<Supplier | null>(null)

    const loadSuppliers = async () => {
        setLoading(true)
        try {
            const data = await supplierService.getSuppliers()
            setSuppliers(data)
        } catch (error) {
            console.error(error)
            toast.error("Failed to load suppliers")
        } finally {
            setLoading(false)
        }
    }

    useEffect(() => {
        loadSuppliers()
    }, [])

    const handleEdit = (supplier: Supplier) => {
        setSelectedSupplier(supplier)
        setIsDialogOpen(true)
    }


    // ...

    const handleDelete = async (supplier: Supplier) => {
        const result = await Swal.fire({
            title: 'Delete Supplier?',
            text: "You won't be able to revert this!",
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#d33',
            cancelButtonColor: '#3085d6',
            confirmButtonText: 'Yes, delete it!'
        })

        if (result.isConfirmed) {
            try {
                await supplierService.deleteSupplier(supplier.id)
                Swal.fire(
                    'Deleted!',
                    'Supplier has been deleted.',
                    'success'
                )
                loadSuppliers()
            } catch (error) {
                console.error(error)
                Swal.fire(
                    'Error!',
                    'Failed to delete supplier.',
                    'error'
                )
            }
        }
    }

    const handleAdd = () => {
        setSelectedSupplier(null)
        setIsDialogOpen(true)
    }

    const columns = getColumns({
        onEdit: handleEdit,
        onDelete: handleDelete
    })

    return (
        <div className="flex flex-col gap-5 w-full">
            <div className="flex items-center justify-between w-full">
                <div className="flex flex-col gap-1">
                    <h2 className="text-3xl font-bold tracking-tight text-foreground">
                        Suppliers
                    </h2>
                    <p className="text-sm text-muted-foreground">
                        Manage your vendor/supplier list ({suppliers.length})
                    </p>
                </div>
                <div className="flex items-center gap-2">
                    <Dialog open={isDialogOpen} onOpenChange={(open) => {
                        setIsDialogOpen(open);
                        if (!open) setSelectedSupplier(null);
                    }}>
                        <DialogTrigger asChild>
                            <Button className="flex items-center gap-2" onClick={handleAdd}>
                                <Plus className="h-4 w-4" /> Add Supplier
                            </Button>
                        </DialogTrigger>
                        <DialogContent className="sm:max-w-[500px] max-h-[90vh] overflow-y-auto">
                            <DialogHeader>
                                <DialogTitle>{selectedSupplier ? "Edit Supplier" : "Add Supplier"}</DialogTitle>
                                <DialogDescription>
                                    {selectedSupplier ? "Update supplier details." : "Add a new supplier to your list."}
                                </DialogDescription>
                            </DialogHeader>
                            <div className="py-2">
                                <SupplierForm
                                    initialData={selectedSupplier}
                                    onSuccess={() => {
                                        setIsDialogOpen(false);
                                        loadSuppliers();
                                    }}
                                />
                            </div>
                        </DialogContent>
                    </Dialog>
                </div>
            </div>

            {loading ? (
                <div className="flex items-center justify-center p-8">
                    <div className="text-muted-foreground">Loading suppliers...</div>
                </div>
            ) : (
                <div className="bg-card rounded-md border shadow-sm">
                    <DataTable
                        searchKey="supplierName"
                        columns={columns}
                        data={suppliers}
                    />
                </div>
            )}
        </div>
    )
}
