import { ColumnDef } from "@tanstack/react-table"
import { PurchaseOrder } from "@/types/crm"
import { Button } from "@/components/ui/button"
import { MoreHorizontal } from "lucide-react"
import { formatCurrency } from "@/lib/utils"
import { Badge } from "@/components/ui/badge"
import {
    DropdownMenu,
    DropdownMenuContent,
    DropdownMenuItem,
    DropdownMenuLabel,
    DropdownMenuSeparator,
    DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu"
import { format } from "date-fns"

interface ColumnsProps {
    onViewDetails: (po: PurchaseOrder) => void
    onEdit: (po: PurchaseOrder) => void
    onReceive: (po: PurchaseOrder) => void
}

export const getColumns = ({ onViewDetails, onEdit, onReceive }: ColumnsProps): ColumnDef<PurchaseOrder>[] => [
    {
        accessorKey: "poNumber",
        header: "PO Number",
    },
    {
        accessorKey: "supplierName",
        header: "Supplier",
    },
    {
        accessorKey: "orderDate",
        header: "Date",
        cell: ({ row }) => {
            const dateVal = row.getValue("orderDate") as string
            if (!dateVal) return "N/A"
            try {
                return format(new Date(dateVal), "MMM dd, yyyy")
            } catch {
                return "Invalid Date"
            }
        },
    },
    {
        accessorKey: "totalAmount",
        header: () => <div className="text-right">Total</div>,
        cell: ({ row }) => (
            <div className="text-right font-medium">
                {formatCurrency(row.getValue("totalAmount"))}
            </div>
        ),
    },
    {
        accessorKey: "status",
        header: "Status",
        cell: ({ row }) => {
            const status = row.getValue("status") as string
            let variant: "default" | "secondary" | "destructive" | "outline" = "default"
            if (status === "Draft" || status === "Pending") variant = "secondary"
            if (status === "Received") variant = "default"
            if (status === "Cancelled") variant = "destructive"

            return <Badge variant={variant}>{status}</Badge>
        }
    },
    {
        id: "actions",
        cell: ({ row }) => {
            const po = row.original

            return (
                <DropdownMenu>
                    <DropdownMenuTrigger asChild>
                        <Button variant="ghost" className="h-8 w-8 p-0">
                            <span className="sr-only">Open menu</span>
                            <MoreHorizontal className="h-4 w-4" />
                        </Button>
                    </DropdownMenuTrigger>
                    <DropdownMenuContent align="end">
                        <DropdownMenuLabel>Actions</DropdownMenuLabel>
                        <DropdownMenuItem onClick={() => navigator.clipboard.writeText(po.poNumber)}>
                            Copy PO Number
                        </DropdownMenuItem>
                        <DropdownMenuSeparator />
                        <DropdownMenuItem onClick={() => onViewDetails(po)}>
                            View Details
                        </DropdownMenuItem>
                        {(po.status === 'Draft' || po.status === 'Pending') && (
                            <DropdownMenuItem onClick={() => onEdit(po)}>Edit</DropdownMenuItem>
                        )}
                        {(po.status === 'Draft' || po.status === 'Pending') && (
                            <DropdownMenuItem className="text-green-600" onClick={() => onReceive(po)}>Receive Goods</DropdownMenuItem>
                        )}
                    </DropdownMenuContent>
                </DropdownMenu>
            )
        },
    },
]
