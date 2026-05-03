import { Dialog, DialogContent, DialogHeader, DialogTitle } from "@/components/ui/dialog"
import { GoodsReceipt } from "@/services/goods-receipt-service"
import { formatCurrency } from "@/lib/utils"
import { format } from "date-fns"
import { Badge } from "@/components/ui/badge"
import {
    Table,
    TableBody,
    TableCell,
    TableHead,
    TableHeader,
    TableRow,
} from "@/components/ui/table"

interface GoodsReceiptDetailsModalProps {
    receipt: GoodsReceipt | null
    open: boolean
    onOpenChange: (open: boolean) => void
}

export function GoodsReceiptDetailsModal({ receipt, open, onOpenChange }: GoodsReceiptDetailsModalProps) {
    if (!receipt) return null

    return (
        <Dialog open={open} onOpenChange={onOpenChange}>
            <DialogContent className="max-w-4xl">
                <DialogHeader>
                    <div className="flex items-center justify-between mr-8">
                        <DialogTitle>Goods Receipt Details</DialogTitle>
                        <Badge variant={receipt.status === "Approved" ? "default" : "secondary"}>
                            {receipt.status || "Pending"}
                        </Badge>
                    </div>
                    <div className="text-sm text-muted-foreground flex items-center gap-2">
                        <span>{receipt.supplierName || "Internal / Walk-in"}</span>
                        <span>•</span>
                        <span>{format(new Date(receipt.receiptDate), "MMM dd, yyyy")}</span>
                        {receipt.locationName && (
                            <>
                                <span>•</span>
                                <span>{receipt.locationName}</span>
                            </>
                        )}
                    </div>
                </DialogHeader>

                <div className="space-y-6">
                    <Table>
                        <TableHeader>
                            <TableRow>
                                <TableHead>Product</TableHead>
                                <TableHead>Variant</TableHead>
                                <TableHead className="text-right">Qty</TableHead>
                                <TableHead className="text-right">Cost</TableHead>
                                <TableHead className="text-right">Total</TableHead>
                            </TableRow>
                        </TableHeader>
                        <TableBody>
                            {receipt.details.map((item) => (
                                <TableRow key={item.id}>
                                    <TableCell>{item.productName || item.productId}</TableCell>
                                    <TableCell>{item.variantName || "Standard"}</TableCell>
                                    <TableCell className="text-right">{item.quantityReceived}</TableCell>
                                    <TableCell className="text-right">{formatCurrency(item.unitCost)}</TableCell>
                                    <TableCell className="text-right">{formatCurrency(item.quantityReceived * item.unitCost)}</TableCell>
                                </TableRow>
                            ))}
                        </TableBody>
                    </Table>

                    <div className="flex justify-end space-x-2 text-sm">
                        <span className="font-semibold">Total Value:</span>
                        <span>{formatCurrency(receipt.totalReceivedAmount)}</span>
                    </div>

                    {receipt.approvedBy && (
                        <div className="text-xs text-muted-foreground text-right mt-4">
                            Approved At: {format(new Date(receipt.approvedAt || receipt.createdAt), "MMM dd, yyyy HH:mm")}
                        </div>
                    )}
                </div>
            </DialogContent>
        </Dialog>
    )
}
