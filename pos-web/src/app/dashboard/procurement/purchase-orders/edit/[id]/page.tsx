"use client"

import { useEffect, useState } from "react"
import { useParams, useRouter } from "next/navigation"
import { PurchaseOrderForm } from "../../components/purchase-order-form"
import { supplierService } from "@/services/supplier-service"
import { PurchaseOrder } from "@/types/crm"
import { toast } from "sonner"
import { Loader2 } from "lucide-react"

export default function EditPurchaseOrderPage() {
    const params = useParams()
    const router = useRouter()
    const [po, setPo] = useState<PurchaseOrder | null>(null)
    const [loading, setLoading] = useState(true)

    useEffect(() => {
        const load = async () => {
            try {
                if (typeof params.id !== "string") return
                const data = await supplierService.getPurchaseOrder(params.id)
                setPo(data)
            } catch (error) {
                console.error(error)
                toast.error("Failed to load Purchase Order")
                router.push("/dashboard/procurement/purchase-orders")
            } finally {
                setLoading(false)
            }
        }
        load()
    }, [params.id, router])

    if (loading) {
        return <div className="flex justify-center p-8"><Loader2 className="animate-spin h-8 w-8" /></div>
    }

    if (!po) return null

    return (
        <div className="p-8 pt-6 space-y-6 max-w-5xl mx-auto">
            <div className="flex items-center justify-between">
                <h2 className="text-3xl font-bold tracking-tight">Edit Purchase Order</h2>
                <div className="text-muted-foreground">{po.poNumber}</div>
            </div>
            <PurchaseOrderForm initialData={po} />
        </div>
    )
}
