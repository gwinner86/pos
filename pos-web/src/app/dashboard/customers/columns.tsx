"use client"

import { ColumnDef } from "@tanstack/react-table"
import { Customer } from "@/types/crm"
import { Button } from "@/components/ui/button"
import { MoreHorizontal, ArrowUpDown } from "lucide-react"
import {
    DropdownMenu,
    DropdownMenuContent,
    DropdownMenuItem,
    DropdownMenuLabel,
    DropdownMenuSeparator,
    DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu"
import { formatCurrency } from "@/lib/utils"

interface CustomerActions {
    onEdit: (customer: Customer) => void;
    onDelete: (customer: Customer) => void;
    onView: (customer: Customer) => void;
}

export const getColumns = ({ onEdit, onView, onDelete }: CustomerActions): ColumnDef<Customer>[] => [
    {
        accessorKey: "firstName", // Combined for display
        header: ({ column }) => {
            return (
                <Button
                    variant="ghost"
                    onClick={() => column.toggleSorting(column.getIsSorted() === "asc")}
                >
                    Name
                    <ArrowUpDown className="ml-2 h-4 w-4" />
                </Button>
            )
        },
        cell: ({ row }) => `${row.original.firstName} ${row.original.lastName || ""}`
    },
    {
        accessorKey: "customerCode",
        header: "Code",
    },
    {
        accessorKey: "email",
        header: "Email",
    },
    {
        accessorKey: "phone",
        header: "Phone",
    },
    {
        accessorKey: "loyaltyPoints",
        header: () => <div className="text-right">Loyalty Points</div>,
        cell: ({ row }) => <div className="text-right">{row.getValue("loyaltyPoints")}</div>,
    },
    {
        accessorKey: "isActive",
        header: "Status",
        cell: ({ row }) => (
            <div className={row.getValue("isActive") ? "text-green-600 font-medium" : "text-gray-500"}>
                {row.getValue("isActive") ? "Active" : "Inactive"}
            </div>
        )
    },
    {
        id: "actions",
        cell: ({ row }) => {
            const customer = row.original

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
                        <DropdownMenuItem onClick={() => navigator.clipboard.writeText(customer.id)}>
                            Copy ID
                        </DropdownMenuItem>
                        <DropdownMenuSeparator />
                        <DropdownMenuItem onClick={() => onView(customer)}>View Details & History</DropdownMenuItem>
                        <DropdownMenuItem onClick={() => onEdit(customer)}>Edit Customer</DropdownMenuItem>
                        <DropdownMenuItem className="text-red-600" onClick={() => onDelete(customer)}>
                            Delete Customer
                        </DropdownMenuItem>
                    </DropdownMenuContent>
                </DropdownMenu>
            )
        },
    },
]
