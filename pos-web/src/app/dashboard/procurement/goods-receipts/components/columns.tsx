"use client"

import { ColumnDef } from "@tanstack/react-table"
import { GoodsReceipt } from "@/services/goods-receipt-service"
import { format } from "date-fns"
import { Button } from "@/components/ui/button"
import { Eye, Edit, CheckCircle } from "lucide-react"
import { Badge } from "@/components/ui/badge"
import { formatCurrency } from "@/lib/utils"

interface ColumnsProps {
    onView: (receipt: GoodsReceipt) => void;
    onEdit?: (receipt: GoodsReceipt) => void;
    onApprove?: (receipt: GoodsReceipt) => void;
}

export const getColumns = ({ onView, onEdit, onApprove }: ColumnsProps): ColumnDef<GoodsReceipt>[] => [
    {
        accessorKey: "receiptDate",
        header: "Date",
        cell: ({ row }) => {
            return format(new Date(row.original.receiptDate), "MMM dd, yyyy")
        },
    },
    {
        accessorKey: "supplierName",
        header: "Supplier",
        cell: ({ row }) => {
            return row.original.supplierName || "Internal / Walk-in"
        },
    },
    {
        accessorKey: "locationName",
        header: "Location",
        cell: ({ row }) => {
            return row.original.locationName || "Current Location"
        }
    },
    {
        accessorKey: "totalReceivedAmount",
        header: "Value",
        cell: ({ row }) => {
            const amount = parseFloat(row.getValue("totalReceivedAmount"))
            return <div className="font-medium">{formatCurrency(amount)}</div>
        },
    },
    {
        accessorKey: "status",
        header: "Status",
        cell: ({ row }) => {
            const status = row.original.status || "Pending"
            return (
                <Badge variant={status === "Approved" ? "default" : "secondary"}>
                    {status}
                </Badge>
            )
        }
    },
    {
        accessorKey: "isInvoicedYesNo",
        header: "Invoiced",
        cell: ({ row }) => {
            const isInvoiced = row.original.isInvoicedYesNo || "NO"
            return (
                <Badge variant={isInvoiced === "YES" ? "default" : "outline"} className={isInvoiced === "YES" ? "bg-green-100 text-green-800 hover:bg-green-100" : ""}>
                    {isInvoiced}
                </Badge>
            )
        }
    },
    {
        id: "actions",
        cell: ({ row }) => {
            const receipt = row.original

            return (
                <div className="flex items-center gap-2">
                    <Button
                        variant="ghost"
                        size="icon"
                        onClick={() => onView(receipt)}
                        title="View Details"
                    >
                        <Eye className="h-4 w-4" />
                    </Button>

                    {receipt.status !== "Approved" && onEdit && (
                        <Button
                            variant="ghost"
                            size="icon"
                            onClick={() => onEdit(receipt)}
                            title="Edit Receipt"
                        >
                            <Edit className="h-4 w-4 text-blue-500" />
                        </Button>
                    )}

                    {receipt.status !== "Approved" && onApprove && (
                        <Button
                            variant="ghost"
                            size="icon"
                            onClick={() => onApprove(receipt)}
                            title="Approve Receipt"
                        >
                            <CheckCircle className="h-4 w-4 text-green-500" />
                        </Button>
                    )}
                </div>
            )
        },
    },
]
