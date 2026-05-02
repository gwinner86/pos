"use client"

import { ColumnDef } from "@tanstack/react-table"
import { JournalEntry } from "@/types/accounting"
import { format } from "date-fns"
import { formatCurrency } from "@/lib/utils"
import { Badge } from "@/components/ui/badge"
import { Button } from "@/components/ui/button"
import { Eye } from "lucide-react"

interface GetColumnsProps {
    onView: (entry: JournalEntry) => void
}

export const getColumns = ({ onView }: GetColumnsProps): ColumnDef<JournalEntry>[] => [
    {
        accessorKey: "entryDate",
        header: "Date",
        cell: ({ row }) => format(new Date(row.getValue("entryDate")), "MMM dd, yyyy"),
    },
    {
        accessorKey: "id", // Or sourceId if available
        header: "Transaction ID",
        cell: ({ row }) => {
            const id = row.original.sourceId || row.original.id;
            return <span className="font-mono text-xs text-muted-foreground">TXN-{id.substring(0, 8).toUpperCase()}</span>
        }
    },
    {
        accessorKey: "sourceTable",
        header: "Type",
        cell: ({ row }) => (
            <Badge variant="outline" className="capitalize">
                {row.getValue("sourceTable") || "Manual"}
            </Badge>
        ),
    },
    {
        accessorKey: "description",
        header: "Reference / Description",
        cell: ({ row }) => (
            <div className="max-w-[300px] truncate" title={row.getValue("description")}>
                {row.getValue("description")}
            </div>
        ),
    },
    {
        accessorKey: "amount",
        header: () => <div className="text-right">Amount</div>,
        cell: ({ row }) => {
            // Calculate display amount
            // For Sales: Prioritize Revenue (4000) Credit or Cash (1000) Debit to show actual Sale Value, not Gross (Sale+Cost).
            const details = row.original.details;
            const revenue = details.find(d => d.accountNumber === '4000');
            const cash = details.find(d => d.accountNumber === '1000');

            let displayAmount = 0;
            if (row.original.sourceTable === 'Sales' && revenue) {
                displayAmount = revenue.creditAmount;
            } else if (row.original.sourceTable === 'Sales' && cash) {
                displayAmount = cash.debitAmount;
            } else {
                displayAmount = details.reduce((sum, d) => sum + d.debitAmount, 0);
            }

            return <div className="text-right font-medium">{formatCurrency(displayAmount)}</div>
        },
    },
    {
        accessorKey: "isPosted",
        header: "Status",
        cell: ({ row }) => (
            <Badge
                variant={row.getValue("isPosted") ? "secondary" : "default"}
                className={row.getValue("isPosted") ? "bg-emerald-100 text-emerald-800 hover:bg-emerald-100" : ""}
            >
                {row.getValue("isPosted") ? "Posted" : "Draft"}
            </Badge>
        ),
    },
    {
        id: "actions",
        cell: ({ row }) => {
            return (
                <Button variant="ghost" size="icon" onClick={() => onView(row.original)}>
                    <Eye className="h-4 w-4" />
                </Button>
            )
        },
    },
]
