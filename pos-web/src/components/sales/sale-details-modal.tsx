import {
    Dialog,
    DialogContent,
    DialogDescription,
    DialogFooter,
    DialogHeader,
    DialogTitle,
} from "@/components/ui/dialog"
import { Button } from "@/components/ui/button"
import { Printer } from "lucide-react"
import {
    Table,
    TableBody,
    TableCell,
    TableHead,
    TableHeader,
    TableRow,
} from "@/components/ui/table"
import { Sale } from "@/types/sales"
import { formatCurrency } from "@/lib/utils"
import { ReceiptTemplate } from "@/app/dashboard/pos/components/receipt-template"
import { useAuth } from "@/hooks/use-auth"

interface SaleDetailsModalProps {
    open: boolean
    onOpenChange: (open: boolean) => void
    sale: Sale | null
}

export function SaleDetailsModal({ open, onOpenChange, sale }: SaleDetailsModalProps) {
    const { selectedCompany } = useAuth()

    if (!sale) return null

    const handlePrint = () => {
        window.print()
    }

    return (
        <Dialog open={open} onOpenChange={onOpenChange}>
            <DialogContent className="sm:max-w-[700px] max-h-[80vh] overflow-y-auto print:hidden">
                <DialogHeader>
                    <DialogTitle>Sale Details: {sale.saleNumber}</DialogTitle>
                    <DialogDescription>
                        View the items and details for this transaction.
                    </DialogDescription>
                </DialogHeader>

                <div className="grid grid-cols-2 gap-4 py-4">
                    <div>
                        <div className="text-sm font-medium text-muted-foreground">Date</div>
                        <div>{new Date(sale.saleDate).toLocaleDateString()} {new Date(sale.saleDate).toLocaleTimeString()}</div>
                    </div>
                    <div>
                        <div className="text-sm font-medium text-muted-foreground">Status</div>
                        <div className={sale.status === "Completed" ? "text-green-600 font-medium" : ""}>{sale.status}</div>
                    </div>
                    <div>
                        <div className="text-sm font-medium text-muted-foreground">Payment Method</div>
                        <div>{sale.paymentMethod}</div>
                    </div>
                    <div>
                        <div className="text-sm font-medium text-muted-foreground">Location</div>
                        <div>{sale.locationName}</div>
                    </div>
                </div>

                <div className="rounded-md border">
                    <Table>
                        <TableHeader>
                            <TableRow>
                                <TableHead>Product</TableHead>
                                <TableHead className="text-right">Qty</TableHead>
                                <TableHead className="text-right">Price</TableHead>
                                <TableHead className="text-right">Total</TableHead>
                            </TableRow>
                        </TableHeader>
                        <TableBody>
                            {sale.details.map((detail) => (
                                <TableRow key={detail.id}>
                                    <TableCell>
                                        <div className="font-medium">{detail.productName}</div>
                                        {detail.variantName && detail.variantName !== "Default" && (
                                            <div className="text-sm text-muted-foreground">{detail.variantName}</div>
                                        )}
                                    </TableCell>
                                    <TableCell className="text-right">{detail.quantity}</TableCell>
                                    <TableCell className="text-right">{formatCurrency(detail.unitPrice)}</TableCell>
                                    <TableCell className="text-right">{formatCurrency(detail.lineTotal)}</TableCell>
                                </TableRow>
                            ))}
                            <TableRow>
                                <TableCell colSpan={3} className="text-right font-bold">Total Amount</TableCell>
                                <TableCell className="text-right font-bold">{formatCurrency(sale.totalAmount)}</TableCell>
                            </TableRow>
                        </TableBody>
                    </Table>
                </div>
                <DialogFooter className="mt-4">
                    <Button variant="outline" onClick={() => onOpenChange(false)}>
                        Close
                    </Button>
                    <Button onClick={handlePrint} className="flex items-center gap-2">
                        <Printer className="h-4 w-4" />
                        Print Receipt
                    </Button>
                </DialogFooter>
            </DialogContent>

            {/* Hidden Receipt Component exclusively for Printing */}
            <div className="hidden print:block fixed inset-0 z-[9999] bg-white p-0 m-0 w-full h-full">
                <div className="h-full w-full flex items-start justify-center pt-8">
                    <ReceiptTemplate sale={sale} companyName={selectedCompany?.companyName} />
                </div>
            </div>
        </Dialog>
    )
}
