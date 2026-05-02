"use client"

import { useEffect, useState } from "react"
import { Customer } from "@/types/crm"
import { customerService } from "@/services/customer-service"
import Swal from 'sweetalert2'
import { CustomerForm } from "./customer-form"
import { DataTable } from "@/components/ui/data-table"
import { getColumns } from "./columns"
import { Button } from "@/components/ui/button"
import { Plus } from "lucide-react"
import { Separator } from "@/components/ui/separator"
import { toast } from "sonner"
import { useRouter } from "next/navigation"
import {
    Dialog,
    DialogContent,
    DialogDescription,
    DialogHeader,
    DialogTitle,
    DialogTrigger,
} from "@/components/ui/dialog"

export const CustomerClient = () => {
    const router = useRouter()
    const [customers, setCustomers] = useState<Customer[]>([])
    const [loading, setLoading] = useState(true)
    const [dialogOpen, setDialogOpen] = useState(false)
    const [selectedCustomer, setSelectedCustomer] = useState<Customer | null>(null)

    const loadCustomers = async () => {
        try {
            const data = await customerService.getCustomers()
            setCustomers(data)
        } catch (error) {
            console.error(error)
            toast.error("Failed to load customers")
        } finally {
            setLoading(false)
        }
    }

    useEffect(() => {
        loadCustomers()
    }, [])

    const handleEdit = (customer: Customer) => {
        setSelectedCustomer(customer)
        setDialogOpen(true)
    }

    const handleDelete = async (customer: Customer) => {
        const result = await Swal.fire({
            title: 'Delete Customer?',
            text: "This action cannot be undone.",
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#d33',
            cancelButtonColor: '#3085d6',
            confirmButtonText: 'Yes, delete it!'
        })

        if (result.isConfirmed) {
            try {
                await customerService.deleteCustomer(customer.id)
                Swal.fire(
                    'Deleted!',
                    'Customer has been deleted.',
                    'success'
                )
                loadCustomers()
            } catch (error) {
                console.error(error)
                Swal.fire(
                    'Error!',
                    'Failed to delete customer.',
                    'error'
                )
            }
        }
    }

    const handleView = (customer: Customer) => {
        router.push(`/dashboard/customers/${customer.id}`)
    }

    const handleAdd = () => {
        setSelectedCustomer(null)
        setDialogOpen(true)
    }

    const columns = getColumns({
        onEdit: handleEdit,
        onDelete: handleDelete,
        onView: handleView
    })

    return (
        <div className="flex flex-col gap-5 w-full h-full p-4">
            <div className="flex items-center justify-between w-full">
                <div className="flex flex-col gap-1">
                    <h2 className="text-3xl font-bold tracking-tight text-foreground">
                        Customers
                    </h2>
                    <p className="text-sm text-muted-foreground">
                        Manage your customer database ({customers.length})
                    </p>
                </div>
                <Dialog open={dialogOpen} onOpenChange={(open) => {
                    setDialogOpen(open);
                    if (!open) setSelectedCustomer(null);
                }}>
                    <DialogTrigger asChild>
                        <Button className="flex items-center gap-2" onClick={handleAdd}>
                            <Plus className="h-4 w-4" /> Add Customer
                        </Button>
                    </DialogTrigger>
                    <DialogContent className="sm:max-w-[500px] max-h-[90vh] overflow-y-auto">
                        <DialogHeader>
                            <DialogTitle>{selectedCustomer ? "Edit Customer" : "Add Customer"}</DialogTitle>
                            <DialogDescription>
                                {selectedCustomer ? "Update customer details." : "Create a new customer profile."}
                            </DialogDescription>
                        </DialogHeader>
                        <div className="py-2">
                            <CustomerForm
                                initialData={selectedCustomer}
                                onSuccess={() => {
                                    setDialogOpen(false);
                                    loadCustomers();
                                }}
                            />
                        </div>
                    </DialogContent>
                </Dialog>
            </div>

            {loading ? (
                <div className="flex items-center justify-center p-8">
                    <div className="text-muted-foreground">Loading customers...</div>
                </div>
            ) : (
                <div className="bg-card rounded-md border shadow-sm flex-1">
                    <DataTable
                        searchKey="firstName"
                        columns={columns}
                        data={customers}
                    />
                </div>
            )}
        </div>
    )
}
