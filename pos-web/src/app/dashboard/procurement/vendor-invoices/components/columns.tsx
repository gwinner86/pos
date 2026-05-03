"use client"

import { ColumnDef } from "@tanstack/react-table"
import { SupplierInvoice } from "@/services/vendor-invoice-service"
import { format } from "date-fns"
import { Button } from "@/components/ui/button"
import { Eye } from "lucide-react"
import { formatCurrency } from "@/lib/utils"

interface ColumnsProps {
    onView: (invoice: SupplierInvoice) => void;
}

export const getColumns = ({ onView }: ColumnsProps): ColumnDef<SupplierInvoice>[] => [
    {
        accessorKey: "invoiceNumber",
        header: "Invoice #",
    },
    {
        accessorKey: "invoiceDate",
        header: "Date",
        cell: ({ row }) => {
            return format(new Date(row.original.invoiceDate), "MMM dd, yyyy")
        },
    },
    {
        accessorKey: "supplierName",
        header: "Supplier",
        cell: ({ row }) => {
            return row.original.supplierName || "Unknown"
        },
    },
    {
        accessorKey: "totalAmount",
        header: "Total Amount",
        cell: ({ row }) => {
            const amount = parseFloat(row.getValue("totalAmount"))
            return <div className="font-medium">{formatCurrency(amount)}</div>
        },
    },
    {
        accessorKey: "totalPaid",
        header: "Amount Paid",
        cell: ({ row }) => {
            const amount = parseFloat(row.getValue("totalPaid"))
            return <div className="font-medium text-green-600">{formatCurrency(amount)}</div>
        },
    },
    {
        accessorKey: "outstandingAmount",
        header: "Outstanding Amount",
        cell: ({ row }) => {
            const amount = parseFloat(row.getValue("outstandingAmount"))
            return <div className="font-medium text-red-600">{formatCurrency(amount)}</div>
        },
    },
    {
        accessorKey: "status",
        header: "Status",
        cell: ({ row }) => {
            const status = row.original.status;
            let colorClass = "bg-gray-100 text-gray-800";
            if (status === "Paid") colorClass = "bg-green-100 text-green-800";
            if (status === "Partial") colorClass = "bg-yellow-100 text-yellow-800";
            if (status === "Unpaid") colorClass = "bg-red-100 text-red-800";

            return (
                <span className={`px-2 py-1 rounded-full text-xs font-semibold ${colorClass}`}>
                    {status}
                </span>
            )
        },
    },
    {
        id: "actions",
        cell: ({ row }) => {
            const invoice = row.original

            return (
                <div className="flex items-center gap-2">
                    <Button
                        variant="ghost"
                        size="icon"
                        onClick={() => onView(invoice)}
                        title="View Details"
                    >
                        <Eye className="h-4 w-4" />
                    </Button>
                </div>
            )
        },
    },
]
