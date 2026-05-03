"use client"

import { ColumnDef } from "@tanstack/react-table"
import { Sale } from "@/types/sales"
import { Button } from "@/components/ui/button"
import { ArrowUpDown, MoreHorizontal, Eye } from "lucide-react"
import { formatCurrency } from "@/lib/utils"
import {
    DropdownMenu,
    DropdownMenuContent,
    DropdownMenuItem,
    DropdownMenuLabel,
    DropdownMenuSeparator,
    DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu"

interface ColumnsProps {
    onView: (sale: Sale) => void;
}

export const getColumns = ({ onView }: ColumnsProps): ColumnDef<Sale>[] => [
    {
        accessorKey: "saleNumber",
        header: "Sale #",
    },
    {
        accessorKey: "saleDate",
        header: ({ column }) => {
            return (
                <Button
                    variant="ghost"
                    onClick={() => column.toggleSorting(column.getIsSorted() === "asc")}
                >
                    Date
                    <ArrowUpDown className="ml-2 h-4 w-4" />
                </Button>
            )
        },
        cell: ({ row }) => <div>{new Date(row.getValue("saleDate")).toLocaleDateString()}</div>,
    },
    {
        accessorKey: "customerName",
        header: "Customer",
    },
    {
        accessorKey: "locationName",
        header: "Location",
    },
    {
        accessorKey: "paymentMethod",
        header: "Payment",
    },
    {
        accessorKey: "status",
        header: "Status",
        cell: ({ row }) => (
            <div className={`font-medium ${row.getValue("status") === "Completed" ? "text-green-600" : "text-yellow-600"
                }`}>
                {row.getValue("status")}
            </div>
        )
    },
    {
        accessorKey: "totalAmount",
        header: () => <div className="text-right">Total</div>,
        cell: ({ row }) => (
            <div className="text-right font-bold">
                {formatCurrency(row.getValue("totalAmount"))}
            </div>
        ),
    },
    {
        id: "actions",
        cell: ({ row }) => {
            const sale = row.original

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
                        <DropdownMenuItem onClick={() => onView(sale)}>
                            <Eye className="mr-2 h-4 w-4" />
                            View Details
                        </DropdownMenuItem>
                        <DropdownMenuSeparator />
                    </DropdownMenuContent>
                </DropdownMenu>
            )
        },
    },
]
