"use client";

import { useEffect, useState } from "react";
import { useParams, useRouter } from "next/navigation";
import { GoodsReceiptForm } from "../../components/goods-receipt-form";
import { GoodsReceipt, goodsReceiptService } from "@/services/goods-receipt-service";
import { Button } from "@/components/ui/button";
import { ArrowLeft } from "lucide-react";
import { toast } from "sonner";

export default function EditGoodsReceiptPage() {
    const params = useParams();
    const router = useRouter();
    const [receipt, setReceipt] = useState<GoodsReceipt | null>(null);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        const loadReceipt = async () => {
            if (!params.id) return;
            try {
                const data = await goodsReceiptService.getReceiptById(params.id as string);
                setReceipt(data);
            } catch (error) {
                console.error(error);
                toast.error("Failed to load Goods Receipt details.");
                router.push("/dashboard/procurement/goods-receipts");
            } finally {
                setLoading(false);
            }
        };
        loadReceipt();
    }, [params.id, router]);

    if (loading) {
        return <div className="p-8 text-center text-muted-foreground">Loading receipt details...</div>;
    }

    if (!receipt) {
        return null;
    }

    return (
        <div className="flex flex-col gap-5 w-full max-w-5xl mx-auto">
            <div className="flex items-center gap-4 w-full">
                <Button variant="ghost" size="icon" onClick={() => router.back()}>
                    <ArrowLeft className="h-4 w-4" />
                </Button>
                <div className="flex flex-col gap-1">
                    <h2 className="text-3xl font-bold tracking-tight text-foreground">
                        Edit Goods Receipt
                    </h2>
                    <p className="text-sm text-muted-foreground">
                        Update the details of the pending goods receipt block
                    </p>
                </div>
            </div>

            <GoodsReceiptForm initialData={receipt} />
        </div>
    );
}
