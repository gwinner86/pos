import { Card, CardContent, CardFooter } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Product } from "@/types/inventory";
import { formatCurrency } from "@/lib/utils"; // Assuming this exists, or I will use Intl
import { Package } from "lucide-react";

interface ProductCardProps {
    product: Product;
    onClick: (product: Product) => void;
}

export function ProductCard({ product, onClick }: ProductCardProps) {
    // Determine display price. Use range if multiple variants with different prices?
    // For now, use the first variant's price or 0.
    const displayPrice = product.variants && product.variants.length > 0
        ? (product.variants[0].price || product.price) // Use variant price, or fallback to product price if 0 (optional logic, or just use variant price)
        : product.price;

    // Actually, if variant has price 0, it might be intentional? 
    // But user complained "prices are showing 0". 
    // If variant price is 0, it's likely bad data. Fallback to product price is safer for now.
    // Better logic: use variant[0].price if > 0, else product.price.
    const effectivePrice = (product.variants && product.variants.length > 0)
        ? (product.variants[0].price > 0 ? product.variants[0].price : product.price)
        : product.price;

    const displayStock = product.stockLevel; // total stock

    return (
        <Card
            className="w-full h-full cursor-pointer hover:border-primary transition-colors flex flex-col justify-between"
            onClick={() => onClick(product)}
        >
            <CardContent className="p-4 flex flex-col items-center justify-center flex-grow space-y-2">
                {/* Place holder for Image - or actual image if available */}
                <div className="w-24 h-24 bg-muted rounded-md flex items-center justify-center text-muted-foreground">
                    {product.image1 ? (
                        <img src={`http://localhost:5047${product.image1}`} alt={product.productName} className="object-cover w-full h-full rounded-md" />
                    ) : (
                        <Package className="w-10 h-10" />
                    )}
                </div>

                <div className="text-center w-full">
                    <h3 className="font-semibold text-sm line-clamp-2 leading-tight" title={product.productName}>
                        {product.productName}
                    </h3>
                    {product.variants && product.variants.length > 1 && (
                        <span className="text-xs text-muted-foreground">{product.variants.length} Variants</span>
                    )}
                </div>
            </CardContent>
            <CardFooter className="p-3 bg-muted/50 flex justify-between items-center text-sm relative">
                <span className="font-bold">{formatCurrency(effectivePrice)}</span>
                {displayStock > 0 ? (
                    <div className="flex flex-col items-center justify-center rounded-full border border-border shadow-sm w-11 h-11 bg-background shrink-0">
                        <span className="text-green-600 font-bold text-xs leading-none mb-0.5">{displayStock}</span>
                        <span className="text-[8px] text-muted-foreground leading-none">In Stock</span>
                    </div>
                ) : (
                    <div className="flex flex-col items-center justify-center rounded-full border border-destructive/30 bg-destructive/10 w-11 h-11 shrink-0">
                        <span className="text-destructive font-bold text-[8px] leading-tight text-center">Out of<br />Stock</span>
                    </div>
                )}
            </CardFooter>
        </Card>
    );
}
