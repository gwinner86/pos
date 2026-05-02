"use client"

import { ColumnDef } from "@tanstack/react-table"
import { Product } from "@/types/inventory"
import { Button } from "@/components/ui/button"
import { formatCurrency } from "@/lib/utils"

import { GitBranch, MoreHorizontal } from "lucide-react"
import {
    DropdownMenu,
    DropdownMenuContent,
    DropdownMenuItem,
    DropdownMenuLabel,
    DropdownMenuSeparator,
    DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu"

interface ProductActions {
    onEdit: (product: Product) => void;
    onView: (product: Product) => void;
    onDelete: (product: Product) => void;
    onManageBranches: (product: Product) => void;
}

export const getColumns = ({ onEdit, onView, onDelete, onManageBranches }: ProductActions): ColumnDef<Product>[] => [
    {
        accessorKey: "productName",
        header: "Name",
    },
    {
        accessorKey: "sku",
        header: "SKU",
    },
    {
        accessorKey: "categoryName",
        header: "Category",
    },
    {
        accessorKey: "price",
        header: () => <div className="text-right">Price</div>,
        cell: ({ row }) => {
            const val = parseFloat(row.getValue("price"))
            const amount = isNaN(val) ? 0 : val
            return <div className="text-right font-medium">{formatCurrency(amount)}</div>
        },
    },
    {
        accessorKey: "stockLevel",
        header: "Stock",
        cell: ({ row }) => {
            const val = parseFloat(row.getValue("stockLevel"))
            const stock = isNaN(val) ? 0 : val
            return (
                <div className={stock <= 10 ? "text-red-500 font-bold" : ""}>
                    {stock}
                </div>
            )
        }
    },
    {
        accessorKey: "isActive",
        header: "Status",
        cell: ({ row }) => (
            <div className={row.getValue("isActive") ? "text-green-500" : "text-gray-500"}>
                {row.getValue("isActive") ? "Active" : "Inactive"}
            </div>
        )
    },
    {
        id: "actions",
        cell: ({ row }) => {
            const product = row.original

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
                        <DropdownMenuItem onClick={() => navigator.clipboard.writeText(product.productId)}>
                            Copy ID
                        </DropdownMenuItem>
                        <DropdownMenuSeparator />
                        <DropdownMenuItem onClick={() => onView(product)}>View Details</DropdownMenuItem>
                        <DropdownMenuItem onClick={() => onEdit(product)}>Edit Product</DropdownMenuItem>
                        <DropdownMenuItem
                            className="text-blue-600 focus:text-blue-600"
                            onClick={() => onManageBranches(product)}
                        >
                            <GitBranch className="h-4 w-4 mr-2" />
                            Manage Branches
                        </DropdownMenuItem>
                        <DropdownMenuSeparator />
                        <DropdownMenuItem className="text-red-600" onClick={() => onDelete(product)}>
                            Delete Product
                        </DropdownMenuItem>
                    </DropdownMenuContent>
                </DropdownMenu>
            )
        },
    },
]
