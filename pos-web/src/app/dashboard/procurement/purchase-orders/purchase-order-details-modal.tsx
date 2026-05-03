import { Dialog, DialogContent, DialogHeader, DialogTitle } from "@/components/ui/dialog"
import { PurchaseOrder } from "@/types/crm"
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

interface PurchaseOrderDetailsModalProps {
    po: PurchaseOrder | null
    open: boolean
    onOpenChange: (open: boolean) => void
}

export function PurchaseOrderDetailsModal({ po, open, onOpenChange }: PurchaseOrderDetailsModalProps) {
    if (!po) return null

    return (
        <Dialog open={open} onOpenChange={onOpenChange}>
            <DialogContent className="max-w-4xl">
                <DialogHeader>
                    <div className="flex items-center justify-between mr-8">
                        <DialogTitle>Purchase Order {po.poNumber}</DialogTitle>
                        <Badge variant={po.status === "Received" ? "default" : "secondary"}>
                            {po.status}
                        </Badge>
                    </div>
                    <div className="text-sm text-muted-foreground">
                        {po.supplierName} • {format(new Date(po.orderDate), "MMM dd, yyyy")}
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
                            {po.details.map((item) => (
                                <TableRow key={item.id}>
                                    <TableCell>{item.productName}</TableCell>
                                    <TableCell>{item.variantName}</TableCell>
                                    <TableCell className="text-right">{item.quantity}</TableCell>
                                    <TableCell className="text-right">{formatCurrency(item.unitCost)}</TableCell>
                                    <TableCell className="text-right">{formatCurrency(item.lineTotal)}</TableCell>
                                </TableRow>
                            ))}
                        </TableBody>
                    </Table>

                    <div className="flex justify-end space-x-2 text-sm">
                        <span className="font-semibold">Total Amount:</span>
                        <span>{formatCurrency(po.totalAmount)}</span>
                    </div>
                </div>
            </DialogContent>
        </Dialog>
    )
}
