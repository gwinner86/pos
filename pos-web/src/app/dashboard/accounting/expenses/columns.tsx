"use client"

import { ColumnDef } from "@tanstack/react-table"
import { Expense } from "@/types/expense"
import { format } from "date-fns"
import { formatCurrency } from "@/lib/utils"
import { Badge } from "@/components/ui/badge"

export const columns: ColumnDef<Expense>[] = [
    {
        accessorKey: "expenseDate",
        header: "Date",
        cell: ({ row }) => format(new Date(row.getValue("expenseDate")), "MMM dd, yyyy"),
    },
    {
        accessorKey: "description",
        header: "Description",
    },
    {
        accessorKey: "expenseTypeName",
        header: "Expense Type",
        cell: ({ row }) => (
            <Badge variant="secondary">
                {row.getValue("expenseTypeName")}
            </Badge>
        ),
    },
    {
        accessorKey: "amount",
        header: () => <div className="text-right">Amount</div>,
        cell: ({ row }) => {
            const amount = parseFloat(row.getValue("amount"))
            return <div className="text-right font-medium">{formatCurrency(amount)}</div>
        },
    },
    {
        accessorKey: "status",
        header: "Status",
        cell: ({ row }) => (
            <Badge
                variant={row.getValue("status") === "Posted" ? "secondary" : "default"}
                className={row.getValue("status") === "Posted" ? "bg-emerald-100 text-emerald-800 hover:bg-emerald-100" : ""}
            >
                {row.getValue("status") || "Draft"}
            </Badge>
        ),
    },
]
