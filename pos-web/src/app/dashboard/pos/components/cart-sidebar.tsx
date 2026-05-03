"use client";

import { usePosStore } from "@/store/pos-store";
import { formatCurrency } from "@/lib/utils";
import { Button } from "@/components/ui/button";
import { ScrollArea } from "@/components/ui/scroll-area";
import { Separator } from "@/components/ui/separator";
import { Minus, Plus, Trash2, ShoppingCart } from "lucide-react";
import { CustomerSelector } from "./customer-selector";
import { CheckoutDialog } from "./checkout-dialog"; // Assume this is created
import { useState } from "react";
import { toast } from "sonner";

export function CartSidebar() {
    const { cart, updateQuantity, removeFromCart, getTotals, clearCart } = usePosStore();
    const totals = getTotals();
    const [checkoutOpen, setCheckoutOpen] = useState(false);

    return (
        <div className="flex flex-col h-full bg-card border-l">
            {/* Header / Customer Select */}
            <div className="p-4 border-b space-y-4">
                <div className="flex items-center justify-between">
                    <h2 className="font-semibold text-lg flex items-center gap-2">
                        <ShoppingCart className="w-5 h-5" />
                        Current Ticket
                    </h2>
                    <Button variant="ghost" size="sm" onClick={clearCart} disabled={cart.length === 0} className="text-destructive hover:text-destructive">
                        Clear
                    </Button>
                </div>
                <CustomerSelector />
            </div>

            {/* Cart Items List */}
            <ScrollArea className="flex-1">
                <div className="p-4 space-y-4">
                    {cart.map((item) => (
                        <div key={item.id} className="flex gap-3 py-2 border-b last:border-0 border-dashed">
                            <div className="flex-1 space-y-1">
                                <div className="flex justify-between">
                                    <span className="font-medium text-sm">{item.productName}</span>
                                    <span className="font-semibold text-sm">{formatCurrency(item.price * item.quantity)}</span>
                                </div>
                                <div className="text-xs text-muted-foreground">
                                    {item.variantName !== "Default" && item.variantName}
                                    {/* @ {formatCurrency(item.price)} */}
                                </div>

                                <div className="flex items-center gap-3 mt-2">
                                    <div className="flex items-center border rounded-md">
                                        <Button
                                            variant="ghost"
                                            size="icon"
                                            className="h-7 w-7 rounded-none"
                                            onClick={() => {
                                                if (item.quantity <= 1) removeFromCart(item.variantId);
                                                else updateQuantity(item.variantId, item.quantity - 1);
                                            }}
                                        >
                                            <Minus className="h-3 w-3" />
                                        </Button>
                                        <span className="w-8 text-center text-sm">{item.quantity}</span>
                                        <Button
                                            variant="ghost"
                                            size="icon"
                                            className="h-7 w-7 rounded-none"
                                            onClick={() => {
                                                if (item.quantity >= item.maxStock) {
                                                    toast.warning(`Cannot add more. Only ${item.maxStock} in stock!`);
                                                    return;
                                                }
                                                updateQuantity(item.variantId, item.quantity + 1);
                                            }}
                                        >
                                            <Plus className="h-3 w-3" />
                                        </Button>
                                    </div>
                                    <Button
                                        variant="ghost"
                                        size="icon"
                                        className="h-7 w-7 text-muted-foreground hover:text-destructive ml-auto"
                                        onClick={() => removeFromCart(item.variantId)}
                                    >
                                        <Trash2 className="h-3 w-3" />
                                    </Button>
                                </div>
                            </div>
                        </div>
                    ))}
                    {cart.length === 0 && (
                        <div className="text-center py-10 text-muted-foreground text-sm">
                            Cart is empty.<br />Select products to begin.
                        </div>
                    )}
                </div>
            </ScrollArea>

            {/* Footer / Summary */}
            <div className="p-4 bg-muted/30 border-t space-y-3">
                <div className="space-y-1 text-sm">
                    <div className="flex justify-between">
                        <span className="text-muted-foreground">Subtotal</span>
                        <span>{formatCurrency(totals.subtotal)}</span>
                    </div>
                    {totals.discount > 0 && (
                        <div className="flex justify-between items-center text-sm text-green-600">
                            <span>Discount</span>
                            <span>-{formatCurrency(totals.discount)}</span>
                        </div>
                    )}
                    <div className="flex justify-between">
                        <span className="text-muted-foreground">Tax</span>
                        <span>{formatCurrency(totals.tax)}</span>
                    </div>
                    <Separator className="my-2" />
                    <div className="flex justify-between items-center font-bold text-lg">
                        <span>Total</span>
                        <span>{formatCurrency(totals.total)}</span>
                    </div>
                </div>

                <div className="grid grid-cols-2 gap-2 pt-2">
                    <Button variant="outline" disabled={cart.length === 0}>Hold</Button>
                    <Button
                        size="lg"
                        disabled={cart.length === 0}
                        className="w-full"
                        onClick={() => setCheckoutOpen(true)}
                    >
                        Pay
                    </Button>
                </div>
            </div>

            <CheckoutDialog open={checkoutOpen} onOpenChange={setCheckoutOpen} />
        </div>
    );
}
