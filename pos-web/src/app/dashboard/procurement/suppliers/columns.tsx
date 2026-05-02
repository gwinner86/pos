"use client"

import { ColumnDef } from "@tanstack/react-table"
import { Supplier } from "@/types/crm"
import { Button } from "@/components/ui/button"
import { MoreHorizontal } from "lucide-react"
import {
    DropdownMenu,
    DropdownMenuContent,
    DropdownMenuItem,
    DropdownMenuLabel,
    DropdownMenuSeparator,
    DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu"

interface SupplierActions {
    onEdit: (supplier: Supplier) => void;
    onDelete: (supplier: Supplier) => void;
}

export const getColumns = ({ onEdit, onDelete }: SupplierActions): ColumnDef<Supplier>[] => [
    {
        accessorKey: "supplierName",
        header: "Supplier Name",
        cell: ({ row }) => <div className="font-medium">{row.getValue("supplierName")}</div>
    },
    {
        accessorKey: "contactName",
        header: "Contact Person",
    },
    {
        accessorKey: "phone",
        header: "Phone",
    },
    {
        accessorKey: "contactEmail",
        header: "Email",
    },
    {
        accessorKey: "locationName",
        header: "Branch",
        cell: ({ row }) => {
            const val = row.getValue("locationName") as string;
            return <div className="text-muted-foreground">{val || "Global"}</div>
        }
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
            const supplier = row.original

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
                        <DropdownMenuItem onClick={() => navigator.clipboard.writeText(supplier.id)}>
                            Copy ID
                        </DropdownMenuItem>
                        <DropdownMenuSeparator />
                        <DropdownMenuItem onClick={() => onEdit(supplier)}>Edit Supplier</DropdownMenuItem>
                        <DropdownMenuItem className="text-red-600" onClick={() => onDelete(supplier)}>
                            Delete Supplier
                        </DropdownMenuItem>
                    </DropdownMenuContent>
                </DropdownMenu>
            )
        },
    },
]
