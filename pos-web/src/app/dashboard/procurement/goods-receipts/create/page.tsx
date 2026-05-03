"use client";

import { GoodsReceiptForm } from "../components/goods-receipt-form";
import { ArrowLeft } from "lucide-react";
import Link from "next/link";
import { Button } from "@/components/ui/button";

export default function CreateGoodsReceiptPage() {
    return (
        <div className="flex flex-col gap-5 w-full max-w-4xl mx-auto pb-10">
            <div className="flex items-center gap-4">
                <Button variant="ghost" size="icon" asChild>
                    <Link href="/dashboard/procurement/goods-receipts">
                        <ArrowLeft className="h-5 w-5" />
                    </Link>
                </Button>
                <div className="flex flex-col gap-1">
                    <h2 className="text-3xl font-bold tracking-tight text-foreground">
                        Create Goods Receipt
                    </h2>
                    <p className="text-sm text-muted-foreground">
                        Record directly received inventory without a formal PO.
                    </p>
                </div>
            </div>

            <GoodsReceiptForm />
        </div>
    );
}
