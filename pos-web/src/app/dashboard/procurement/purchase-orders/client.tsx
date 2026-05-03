"use client"

import { useEffect, useState } from "react"
import { PurchaseOrder } from "@/types/crm"
import { supplierService } from "@/services/supplier-service"
import Swal from 'sweetalert2'
import { DataTable } from "@/components/ui/data-table"
import { getColumns } from "./columns"
import { PurchaseOrderDetailsModal } from "./purchase-order-details-modal"
import { Button } from "@/components/ui/button"
import { Plus } from "lucide-react"
import { Separator } from "@/components/ui/separator"
import { toast } from "sonner"
import { useRouter } from "next/navigation"

export const PurchaseOrderClient = () => {
    const router = useRouter()
    const [orders, setOrders] = useState<PurchaseOrder[]>([])
    const [loading, setLoading] = useState(false)
    const [selectedPo, setSelectedPo] = useState<PurchaseOrder | null>(null)
    const [detailsOpen, setDetailsOpen] = useState(false)

    const loadOrders = async () => {
        try {
            const data = await supplierService.getPurchaseOrders()
            // If endpoint doesn't exist yet, we might get 404.
            // Assuming it exists or returns empty.
            if (Array.isArray(data)) {
                // Filter out Received POs so they appear removed from active purchase orders
                setOrders(data.filter(po => (po.status as string) !== 'Received' && (po.status as string) !== 'Completed'));
            }
        } catch (error) {
            console.error("Failed to load POs", error)
            // toast.error("Failed to load Purchase Orders") // Suppress for now if API not ready
        }
    }

    useEffect(() => {
        loadOrders()
    }, [])

    const handleCreate = () => {
        router.push("/dashboard/procurement/purchase-orders/create")
    }

    const columns = getColumns({
        onViewDetails: (po) => {
            setSelectedPo(po)
            setDetailsOpen(true)
        },
        onEdit: (po) => {
            router.push(`/dashboard/procurement/purchase-orders/edit/${po.id}`)
        },
        onReceive: async (po) => {
            const result = await Swal.fire({
                title: 'Receive Goods?',
                text: "Stock levels will be updated automatically.",
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#10b981', // green-500
                cancelButtonColor: '#ef4444', // red-500
                confirmButtonText: 'Yes, receive it!'
            })

            if (result.isConfirmed) {
                try {
                    await supplierService.receivePurchaseOrder(po.id)
                    Swal.fire(
                        'Received!',
                        'Purchase Order has been received.',
                        'success'
                    )
                    loadOrders() // Refresh list
                } catch (error: any) {
                    console.error(error)
                    const errorMessage = error.response?.data?.message || error.message || "Failed to receive goods."
                    Swal.fire(
                        'Error!',
                        errorMessage,
                        'error'
                    )
                }
            }
        }
    })

    return (
        <div className="flex flex-col gap-5 w-full">
            <div className="flex items-center justify-between w-full">
                <div className="flex flex-col gap-1">
                    <h2 className="text-3xl font-bold tracking-tight text-foreground">
                        Purchase Orders
                    </h2>
                    <p className="text-sm text-muted-foreground">
                        Manage procurement and stock replenishment ({orders.length})
                    </p>
                </div>
                <Button onClick={handleCreate} className="flex items-center gap-2">
                    <Plus className="h-4 w-4" /> Create PO
                </Button>
            </div>

            <div className="bg-card rounded-md border shadow-sm">
                <DataTable
                    searchKey="poNumber"
                    columns={columns}
                    data={orders}
                />
            </div>

            <PurchaseOrderDetailsModal
                po={selectedPo}
                open={detailsOpen}
                onOpenChange={setDetailsOpen}
            />
        </div>
    )
}
