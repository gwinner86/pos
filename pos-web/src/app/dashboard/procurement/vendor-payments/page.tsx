"use client"

import { useEffect, useState } from "react"
import { SupplierInvoice, vendorInvoiceService } from "@/services/vendor-invoice-service"
import { getColumns } from "./components/columns"
import { DataTable } from "@/components/ui/data-table"
import { useAuth } from "@/hooks/use-auth"
import { CreatePaymentModal } from "./components/create-payment-modal"

export default function VendorPaymentsPage() {
    const { selectedCompany } = useAuth()
    const [invoices, setInvoices] = useState<SupplierInvoice[]>([])
    const [loading, setLoading] = useState(true)
    const [selectedInvoice, setSelectedInvoice] = useState<SupplierInvoice | null>(null)
    const [isPaymentModalOpen, setIsPaymentModalOpen] = useState(false)

    const loadInvoices = async () => {
        if (!selectedCompany) return;
        setLoading(true)
        try {
            const data = await vendorInvoiceService.getInvoices(selectedCompany.companyId)
            setInvoices(data)
        } catch (error) {
            console.error("Failed to load Vendor Invoices", error)
        } finally {
            setLoading(false)
        }
    }

    useEffect(() => {
        if (selectedCompany) {
            loadInvoices()
        }
    }, [selectedCompany])

    const handlePay = (invoice: SupplierInvoice) => {
        setSelectedInvoice(invoice)
        setIsPaymentModalOpen(true)
    }

    const columns = getColumns({ onPay: handlePay })

    return (
        <div className="flex flex-col gap-5 w-full">
            <div className="flex items-center justify-between w-full">
                <div className="flex flex-col gap-1">
                    <h2 className="text-3xl font-bold tracking-tight text-foreground">
                        Vendor Payments
                    </h2>
                    <p className="text-sm text-muted-foreground">
                        Select an invoice to record a payment
                    </p>
                </div>
            </div>

            {loading ? (
                <div className="flex items-center justify-center p-8">
                    <div className="text-muted-foreground">Loading Invoices...</div>
                </div>
            ) : (
                <div className="bg-card rounded-md border shadow-sm">
                    <DataTable columns={columns} data={invoices} searchKey="invoiceNumber" />
                </div>
            )}

            <CreatePaymentModal
                invoice={selectedInvoice}
                open={isPaymentModalOpen}
                onOpenChange={setIsPaymentModalOpen}
                onSuccess={loadInvoices}
            />
        </div>
    )
}
