"use client"

import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogDescription } from "@/components/ui/dialog"
import { JournalEntry } from "@/types/accounting"
import { formatCurrency } from "@/lib/utils"
import { format } from "date-fns"
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table"
import { Badge } from "@/components/ui/badge"
import { Separator } from "@/components/ui/separator"
import { CheckCircle2 } from "lucide-react"

interface TransactionDetailsModalProps {
    open: boolean
    onOpenChange: (open: boolean) => void
    entry: JournalEntry | null
}

export function TransactionDetailsModal({ open, onOpenChange, entry }: TransactionDetailsModalProps) {
    if (!entry) return null

    const totalDebit = entry.details.reduce((sum, d) => sum + d.debitAmount, 0)
    const totalCredit = entry.details.reduce((sum, d) => sum + d.creditAmount, 0)
    const isBalanced = Math.abs(totalDebit - totalCredit) < 0.01

    return (
        <Dialog open={open} onOpenChange={onOpenChange}>
            <DialogContent className="max-w-4xl max-h-[90vh] overflow-y-auto">
                <DialogHeader>
                    <div className="flex items-center justify-between pr-8">
                        <div className="flex items-center gap-4">
                            <DialogTitle className="text-xl">TXN-{entry.sourceId ? entry.sourceId.substring(0, 8).toUpperCase() : entry.id.substring(0, 8).toUpperCase()}</DialogTitle>
                            <span className="text-muted-foreground">{format(new Date(entry.entryDate), "MMM dd, yyyy")}</span>
                        </div>
                        <div className="flex items-center gap-2">
                            <div className="font-mono text-sm text-primary bg-primary/10 px-2 py-1 rounded">
                                {entry.sourceTable || "MANUAL"}
                            </div>
                            {entry.isPosted && <Badge variant="secondary" className="bg-emerald-100 text-emerald-800 hover:bg-emerald-100">posted</Badge>}
                        </div>
                    </div>
                </DialogHeader>

                <div className="space-y-6 py-4">
                    {/* Header Info */}
                    <div className="grid gap-1">
                        <div className="text-sm text-muted-foreground">Description</div>
                        <div className="font-medium">{entry.description}</div>
                    </div>

                    <div className="border rounded-md">
                        <div className="flex items-center justify-between p-4 bg-muted/50 border-b">
                            <div className="flex items-center gap-2 text-sm font-medium">
                                <FileTextIcon className="w-4 h-4" />
                                Journal Entry
                            </div>
                            <div className="flex items-center gap-2 text-sm text-muted-foreground">
                                {/* Optional: Order Items Toggle could go here if we had item details */}
                            </div>
                        </div>

                        <Table>
                            <TableHeader>
                                <TableRow>
                                    <TableHead className="w-[50px]">#</TableHead>
                                    <TableHead>Account</TableHead>
                                    <TableHead>Description</TableHead>
                                    <TableHead className="text-right">Debit</TableHead>
                                    <TableHead className="text-right">Credit</TableHead>
                                </TableRow>
                            </TableHeader>
                            <TableBody>
                                {entry.details.map((detail, index) => (
                                    <TableRow key={index}>
                                        <TableCell className="text-muted-foreground">{index + 1}</TableCell>
                                        <TableCell>
                                            <div className="font-medium">{detail.accountName || "Unknown Account"}</div>
                                            <div className="text-xs text-muted-foreground">{detail.accountNumber}</div>
                                        </TableCell>
                                        <TableCell>
                                            <div className="text-sm text-muted-foreground">{entry.description}</div>
                                            {/* Ideally detail level description if available, currently falling back to header */}
                                        </TableCell>
                                        <TableCell className="text-right font-mono">
                                            {detail.debitAmount > 0 ? (
                                                <span className="text-blue-600 dark:text-blue-400">{formatCurrency(detail.debitAmount)}</span>
                                            ) : (
                                                <span className="text-muted-foreground">-</span>
                                            )}
                                        </TableCell>
                                        <TableCell className="text-right font-mono">
                                            {detail.creditAmount > 0 ? (
                                                <span className="text-emerald-600 dark:text-emerald-400">{formatCurrency(detail.creditAmount)}</span>
                                            ) : (
                                                <span className="text-muted-foreground">-</span>
                                            )}
                                        </TableCell>
                                    </TableRow>
                                ))}
                                <TableRow className="bg-muted/50 font-medium">
                                    <TableCell colSpan={3} className="text-right">Total</TableCell>
                                    <TableCell className="text-right font-mono">{formatCurrency(totalDebit)}</TableCell>
                                    <TableCell className="text-right font-mono">{formatCurrency(totalCredit)}</TableCell>
                                </TableRow>
                            </TableBody>
                        </Table>
                    </div>

                    <div className="flex items-center justify-between px-2">
                        <div className="flex items-center gap-2 text-sm text-muted-foreground">
                            Balance Check
                        </div>
                        {isBalanced ? (
                            <Badge variant="outline" className="text-emerald-600 border-emerald-200 bg-emerald-50 gap-1 pl-1 pr-2">
                                <CheckCircle2 className="w-3 h-3" /> Balanced
                            </Badge>
                        ) : (
                            <Badge variant="destructive">Unbalanced</Badge>
                        )}
                    </div>
                </div>
            </DialogContent>
        </Dialog>
    )
}

function FileTextIcon(props: React.SVGProps<SVGSVGElement>) {
    return (
        <svg
            {...props}
            xmlns="http://www.w3.org/2000/svg"
            width="24"
            height="24"
            viewBox="0 0 24 24"
            fill="none"
            stroke="currentColor"
            strokeWidth="2"
            strokeLinecap="round"
            strokeLinejoin="round"
        >
            <path d="M14.5 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V7.5L14.5 2z" />
            <polyline points="14 2 14 8 20 8" />
            <line x1="16" x2="8" y1="13" y2="13" />
            <line x1="16" x2="8" y1="17" y2="17" />
            <line x1="10" x2="8" y1="9" y2="9" />
        </svg>
    )
}
