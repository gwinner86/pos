"use client"

import { PurchaseOrderForm } from "../components/purchase-order-form"

export default function CreatePurchaseOrderPage() {
    return (
        <div className="p-8 pt-6 space-y-6 max-w-5xl mx-auto">
            <div className="flex items-center justify-between">
                <h2 className="text-3xl font-bold tracking-tight">Create Purchase Order</h2>
            </div>
            <PurchaseOrderForm />
        </div>
    )
}
