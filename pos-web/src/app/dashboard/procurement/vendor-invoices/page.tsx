"use client"

import { useEffect, useState } from "react"
import { SupplierInvoice, vendorInvoiceService } from "@/services/vendor-invoice-service"
import { getColumns } from "./components/columns"
import { DataTable } from "@/components/ui/data-table"
import { useAuth } from "@/hooks/use-auth"
import { Button } from "@/components/ui/button"
import { Plus } from "lucide-react"
import { Dialog, DialogContent, DialogHeader, DialogTitle } from "@/components/ui/dialog"
import { VendorInvoiceForm } from "./components/vendor-invoice-form"

export default function VendorInvoicesPage() {
    const { selectedCompany } = useAuth()
    const [invoices, setInvoices] = useState<SupplierInvoice[]>([])
    const [loading, setLoading] = useState(true)
    const [isCreateModalOpen, setIsCreateModalOpen] = useState(false)

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

    const handleView = (invoice: SupplierInvoice) => {
        // Implement view dialog later
        console.log("Viewing invoice", invoice)
    }

    const columns = getColumns({ onView: handleView })

    return (
        <div className="flex flex-col gap-5 w-full">
            <div className="flex items-center justify-between w-full">
                <div className="flex flex-col gap-1">
                    <h2 className="text-3xl font-bold tracking-tight text-foreground">
                        Vendor Invoices
                    </h2>
                    <p className="text-sm text-muted-foreground">
                        Track and manage supplier bills and payables
                    </p>
                </div>
                <Button onClick={() => setIsCreateModalOpen(true)} className="flex items-center gap-2">
                    <Plus className="h-4 w-4" /> Create Invoice
                </Button>
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

            <Dialog open={isCreateModalOpen} onOpenChange={setIsCreateModalOpen}>
                <DialogContent className="sm:max-w-[600px]">
                    <DialogHeader>
                        <DialogTitle>Create Vendor Invoice</DialogTitle>
                    </DialogHeader>
                    <VendorInvoiceForm
                        onSuccess={() => {
                            setIsCreateModalOpen(false)
                            loadInvoices()
                        }}
                        onCancel={() => setIsCreateModalOpen(false)}
                    />
                </DialogContent>
            </Dialog>
        </div>
    )
}
