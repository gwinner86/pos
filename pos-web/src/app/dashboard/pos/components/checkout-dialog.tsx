"use client";

import { useState, useEffect } from "react";
import {
    Dialog,
    DialogContent,
    DialogDescription,
    DialogFooter,
    DialogHeader,
    DialogTitle,
    DialogTrigger,
} from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { Label } from "@/components/ui/label";
import { Input } from "@/components/ui/input";
import { usePosStore } from "@/store/pos-store";
import { salesService } from "@/services/sales-service";
import { CreateSaleDto, Sale } from "@/types/sales";
import { toast } from "sonner";
import { formatCurrency } from "@/lib/utils";
import { useAuth } from "@/hooks/use-auth";
import { CreditCard, DollarSign, Wallet } from "lucide-react";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { ReceiptTemplate } from "./receipt-template";

interface CheckoutDialogProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
}

export function CheckoutDialog({ open, onOpenChange }: CheckoutDialogProps) {
    const { cart, selectedCustomer, getTotals, clearCart, triggerRefresh } = usePosStore();
    const { selectedLocation, selectedCompany } = useAuth(); // Get location and company from Auth context
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [changeDue, setChangeDue] = useState<number | null>(null);
    const [tenderedAmount, setTenderedAmount] = useState("");
    const [paymentMethod, setPaymentMethod] = useState("Cash");
    const [lastSale, setLastSale] = useState<Sale | null>(null);

    const totals = getTotals();

    useEffect(() => {
        if (open) {
            setTenderedAmount(totals.total.toString());
        }
    }, [open, totals.total]);

    const handleCheckout = async () => {
        if (cart.length === 0) return;

        if (!selectedLocation?.id) {
            toast.error("No location selected. Please re-login or select a location.");
            return;
        }

        setIsSubmitting(true);
        try {
            const saleData: CreateSaleDto = {
                locationId: selectedLocation.id,
                customerId: selectedCustomer?.id || null, // Allow null if not selected
                saleDate: new Date().toISOString(),
                paymentMethod: paymentMethod,
                details: cart.map(item => ({
                    productVariantId: item.variantId,
                    quantity: item.quantity,
                    unitPrice: item.price,
                    discount: item.discount
                }))
            };

            const result = await salesService.createSale(saleData);
            setLastSale(result);

            triggerRefresh(); // Refresh product stock
            toast.success("Sale completed successfully!");

            // Calculate change if cash
            if (paymentMethod === "Cash" && tenderedAmount) {
                const tender = parseFloat(tenderedAmount);
                if (!isNaN(tender) && tender >= totals.total) {
                    setChangeDue(tender - totals.total);
                    // Open Success Dialog
                } else {
                    setChangeDue(0); // Show success/print dialog anyway
                }
            } else {
                setChangeDue(0); // Show success/print dialog
            }

        } catch (error: any) {
            console.error("Checkout failed details:", error.response?.data || error);
            const errorMessage = error.response?.data?.message || error.response?.data?.Message || error.message || "Unknown error occurred";
            // Show error message
            toast.error(`Checkout failed: ${errorMessage}`);
        } finally {
            setIsSubmitting(false);
        }
    };

    const handleClose = () => {
        // Reset everything
        clearCart();
        setLastSale(null);
        setChangeDue(null);
        setTenderedAmount("");
        onOpenChange(false);
    };

    const handlePrint = () => {
        if (lastSale) {
            window.print();
        }
    }

    if (changeDue !== null) {
        return (
            <Dialog open={open} onOpenChange={(val) => !val && handleClose()}>
                <DialogContent className="sm:max-w-md text-center print:hidden">
                    <DialogHeader>
                        <DialogTitle className="text-2xl text-green-600">Payment Successful</DialogTitle>
                        <DialogDescription>
                            {changeDue > 0 ? "Change Due" : "Transaction Complete"}
                        </DialogDescription>
                    </DialogHeader>
                    {changeDue > 0 && (
                        <div className="py-6">
                            <span className="text-4xl font-bold">{formatCurrency(changeDue)}</span>
                        </div>
                    )}
                    <DialogFooter className="flex-col gap-2 sm:gap-0 sm:flex-row sm:space-x-2">
                        <Button className="w-full flex-1" onClick={handlePrint} variant="outline">Print Receipt</Button>
                        <Button className="w-full flex-1" onClick={handleClose}>Done</Button>
                    </DialogFooter>
                </DialogContent>

                {/* Hidden Receipt Component */}
                <div className="hidden print:block fixed inset-0 z-[9999] bg-white p-0 m-0 w-full h-full">
                    <div className="h-full w-full flex items-start justify-center pt-8">
                        <ReceiptTemplate sale={lastSale} companyName={selectedCompany?.companyName} />
                    </div>
                </div>
            </Dialog>
        );
    }

    return (
        <Dialog open={open} onOpenChange={onOpenChange}>
            <DialogContent className="sm:max-w-md">
                <DialogHeader>
                    <DialogTitle>Complete Sale</DialogTitle>
                    <DialogDescription>
                        Total Amount Due: <span className="font-bold text-foreground">{formatCurrency(totals.total)}</span>
                    </DialogDescription>
                </DialogHeader>

                <Tabs defaultValue="Cash" className="w-full" onValueChange={setPaymentMethod}>
                    <TabsList className="grid w-full grid-cols-3">
                        <TabsTrigger value="Cash">Cash</TabsTrigger>
                        <TabsTrigger value="Card">Card</TabsTrigger>
                        <TabsTrigger value="Mobile">Mobile</TabsTrigger>
                    </TabsList>

                    <div className="py-4 space-y-4">
                        <div className="grid gap-2">
                            <Label htmlFor="tendered">Tendered Amount</Label>
                            <div className="relative">
                                {/* <span className="absolute left-3 top-2.5 text-muted-foreground">$</span> */}
                                <Input
                                    id="tendered"
                                    className="text-lg"
                                    type="number"
                                    placeholder="0.00"
                                    value={tenderedAmount}
                                    onChange={(e) => setTenderedAmount(e.target.value)}
                                    autoFocus
                                />
                            </div>
                        </div>

                        {/* Quick Cash Buttons (Optional) */}
                        <div className="flex gap-2">
                            {[10, 20, 50, 100, 200].map(amt => (
                                <Button
                                    key={amt}
                                    variant="outline"
                                    size="sm"
                                    onClick={() => setTenderedAmount(amt.toString())}
                                >
                                    {amt}
                                </Button>
                            ))}
                        </div>
                    </div>
                </Tabs>

                <DialogFooter className="sm:justify-between">
                    <Button variant="ghost" onClick={() => onOpenChange(false)}>Cancel</Button>
                    <Button onClick={handleCheckout} disabled={isSubmitting || (paymentMethod === 'Cash' && parseFloat(tenderedAmount || '0') < totals.total)}>
                        {isSubmitting ? "Processing..." : "Pay Now"}
                    </Button>
                </DialogFooter>
            </DialogContent>
        </Dialog>
    );
}
