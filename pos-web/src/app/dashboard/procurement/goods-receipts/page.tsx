"use client"

import { useEffect, useState } from "react"
import { useRouter } from "next/navigation"
import { GoodsReceipt, goodsReceiptService } from "@/services/goods-receipt-service"
import { getColumns } from "./components/columns"
import { DataTable } from "@/components/ui/data-table"
import { useAuth } from "@/hooks/use-auth"
import { Button } from "@/components/ui/button"
import { Plus } from "lucide-react"
import { toast } from "sonner"
import Swal from "sweetalert2"
import { GoodsReceiptDetailsModal } from "./components/goods-receipt-details-modal"

export default function GoodsReceiptsPage() {
    const router = useRouter()
    const { selectedCompany } = useAuth()
    const [receipts, setReceipts] = useState<GoodsReceipt[]>([])
    const [loading, setLoading] = useState(true)
    const [selectedReceipt, setSelectedReceipt] = useState<GoodsReceipt | null>(null)
    const [isViewModalOpen, setIsViewModalOpen] = useState(false)

    const loadReceipts = async () => {
        if (!selectedCompany) return;
        setLoading(true)
        try {
            const data = await goodsReceiptService.getReceipts(selectedCompany.companyId)
            setReceipts(data)
        } catch (error) {
            console.error("Failed to load Goods Receipts", error)
        } finally {
            setLoading(false)
        }
    }

    useEffect(() => {
        if (selectedCompany) {
            loadReceipts()
        }
    }, [selectedCompany])

    const handleView = (receipt: GoodsReceipt) => {
        setSelectedReceipt(receipt);
        setIsViewModalOpen(true);
    }

    const handleEdit = (receipt: GoodsReceipt) => {
        router.push(`/dashboard/procurement/goods-receipts/${receipt.id}/edit`);
    }

    const handleApprove = async (receipt: GoodsReceipt) => {
        const result = await Swal.fire({
            title: "Approve Goods Receipt?",
            text: "Are you sure you want to approve this Goods Receipt? This action will permanently increase your inventory stock.",
            icon: "warning",
            showCancelButton: true,
            confirmButtonColor: "#3085d6",
            cancelButtonColor: "#d33",
            confirmButtonText: "Yes, approve it!"
        });

        if (!result.isConfirmed) {
            return;
        }

        try {
            await goodsReceiptService.approveReceipt(receipt.id);
            toast.success("Goods Receipt approved successfully!");
            loadReceipts();
        } catch (error: any) {
            console.error("Failed to approve Goods Receipt", error);
            toast.error(error.response?.data?.message || "Failed to approve Goods Receipt.");
        }
    }

    const columns = getColumns({
        onView: handleView,
        onEdit: handleEdit,
        onApprove: handleApprove
    })

    return (
        <div className="flex flex-col gap-5 w-full">
            <div className="flex items-center justify-between w-full">
                <div className="flex flex-col gap-1">
                    <h2 className="text-3xl font-bold tracking-tight text-foreground">
                        Goods Receipts
                    </h2>
                    <p className="text-sm text-muted-foreground">
                        View and manage incoming inventory
                    </p>
                </div>
                <Button onClick={() => router.push('/dashboard/procurement/goods-receipts/create')} className="flex items-center gap-2">
                    <Plus className="h-4 w-4" /> Create Receipt
                </Button>
            </div>

            {loading ? (
                <div className="flex items-center justify-center p-8">
                    <div className="text-muted-foreground">Loading Receipts...</div>
                </div>
            ) : (
                <div className="bg-card rounded-md border shadow-sm">
                    <DataTable columns={columns} data={receipts} searchKey="supplierName" />
                </div>
            )}

            <GoodsReceiptDetailsModal
                receipt={selectedReceipt}
                open={isViewModalOpen}
                onOpenChange={setIsViewModalOpen}
            />
        </div>
    )
}
