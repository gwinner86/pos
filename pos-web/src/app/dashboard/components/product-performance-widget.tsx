import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { formatCurrency } from "@/lib/utils";
import { TrendingDown, TrendingUp } from "lucide-react";

export interface ProductPerformance {
    productId: string;
    productName: string;
    sku: string;
    quantitySold: number;
    totalRevenue: number;
}

interface ProductPerformanceWidgetProps {
    title: string;
    data: ProductPerformance[];
    type: "fast" | "slow";
}

export function ProductPerformanceWidget({ title, data, type }: ProductPerformanceWidgetProps) {
    return (
        <Card>
            <CardHeader className="flex flex-row items-center justify-between pb-2">
                <CardTitle className="text-base font-semibold">{title}</CardTitle>
                {type === "fast" ? (
                    <TrendingUp className="h-4 w-4 text-emerald-500" />
                ) : (
                    <TrendingDown className="h-4 w-4 text-rose-500" />
                )}
            </CardHeader>
            <CardContent>
                <div className="space-y-4">
                    {data?.map((product, index) => (
                        <div key={`${product.productId}-${index}`} className="flex items-center justify-between">
                            <div className="space-y-1">
                                <p className="text-sm font-medium leading-none line-clamp-1">{product.productName}</p>
                                <p className="text-xs text-muted-foreground">SKU: {product.sku}</p>
                            </div>
                            <div className="text-right">
                                <p className="text-sm font-medium">{product.quantitySold} sold</p>
                                <p className="text-xs text-muted-foreground">{formatCurrency(product.totalRevenue)}</p>
                            </div>
                        </div>
                    ))}
                    {(!data || data.length === 0) && (
                        <p className="text-sm text-muted-foreground text-center py-4">No data available.</p>
                    )}
                </div>
            </CardContent>
        </Card>
    );
}
