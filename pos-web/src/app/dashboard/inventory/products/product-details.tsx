import { Product } from "@/types/inventory";
import { formatCurrency } from "@/lib/utils";
import {
    Dialog,
    DialogContent,
    DialogHeader,
    DialogTitle,
    DialogDescription,
} from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";

interface ProductDetailsProps {
    product: Product | null;
    open: boolean;
    onOpenChange: (open: boolean) => void;
    onEdit?: (product: Product) => void;
}

export function ProductDetails({ product, open, onOpenChange, onEdit }: ProductDetailsProps) {
    if (!product) return null;

    return (
        <Dialog open={open} onOpenChange={onOpenChange}>
            <DialogContent className="max-w-3xl">
                <DialogHeader>
                    <div className="flex justify-between items-center pr-8">
                        <div>
                            <DialogTitle>Product Details</DialogTitle>
                            <DialogDescription>Viewing details for {product.productName}</DialogDescription>
                        </div>
                        {onEdit && (
                            <Button variant="outline" onClick={() => onEdit(product)}>
                                Edit Product
                            </Button>
                        )}
                    </div>
                </DialogHeader>

                <div className="grid grid-cols-1 md:grid-cols-2 gap-6 mt-4">
                    {/* Left Column: Image & Basic Info */}
                    <div className="space-y-4">
                        <div className="border rounded-md p-2 flex items-center justify-center bg-gray-50 h-64">
                            {(product as any).image1 ? (
                                <img
                                    src={(product as any).image1.startsWith('http')
                                        ? (product as any).image1
                                        : `http://localhost:5047${(product as any).image1.startsWith('/') ? '' : '/'}${(product as any).image1}`}
                                    alt={product.productName}
                                    className="max-h-full max-w-full object-contain rounded-md"
                                />
                            ) : (
                                <div className="text-gray-400">No Image Available</div>
                            )}
                        </div>
                        <div>
                            <h3 className="text-lg font-semibold">{product.productName}</h3>
                            <p className="text-sm text-gray-500">{product.sku}</p>
                            <Badge variant={product.isActive ? "default" : "secondary"} className="mt-2">
                                {product.isActive ? "Active" : "Inactive"}
                            </Badge>
                        </div>
                        <div className="grid grid-cols-2 gap-4 text-sm">
                            <div>
                                <span className="font-medium text-gray-500">Category:</span>
                                <p>{product.categoryName || 'N/A'}</p>
                            </div>
                            <div>
                                <span className="font-medium text-gray-500">Barcode:</span>
                                <p>{product.barcode || 'N/A'}</p>
                            </div>
                            <div>
                                <span className="font-medium text-gray-500">Supplier:</span>
                                <p>{product.supplierName || 'N/A'}</p>
                            </div>
                        </div>
                        <div>
                            <span className="font-medium text-gray-500 block mb-1">Description:</span>
                            <p className="text-sm bg-gray-50 p-2 rounded-md">{product.description || 'No description provided.'}</p>
                        </div>
                    </div>

                    {/* Right Column: Pricing, Stock, Variants */}
                    <div className="space-y-6">
                        {/* Pricing & Stock Card */}
                        <div className="border rounded-md p-4 space-y-3">
                            <h4 className="font-semibold border-b pb-2">Pricing & Inventory</h4>
                            <div className="grid grid-cols-2 gap-y-2 text-sm">
                                <div><span className="text-gray-500">Selling Price:</span></div>
                                <div className="font-bold">{formatCurrency(product.price)}</div>

                                <div><span className="text-gray-500">Cost Price:</span></div>
                                <div>{formatCurrency(product.cost)}</div>

                                <div><span className="text-gray-500">Current Stock:</span></div>
                                <div className={product.stockLevel <= (product.variants?.length ? product.variants.reduce((a, b) => a + (b.minStockLevel || 0), 0) : product.minStockLevel) ? "text-red-500 font-bold" : "text-green-600 font-medium"}>
                                    {product.stockLevel}
                                </div>

                                <div><span className="text-gray-500">New Stock:</span></div>
                                <div>{product.newStock || 0}</div>

                                <div><span className="text-gray-500">Low Stock Alert:</span></div>
                                <div>{product.variants?.length ? product.variants.reduce((a, b) => a + (b.minStockLevel || 0), 0) : product.minStockLevel}</div>
                            </div>
                        </div>

                        {/* Variants */}
                        {product.variants && product.variants.length > 0 && (
                            <div className="border rounded-md p-4 space-y-3">
                                <h4 className="font-semibold border-b pb-2">Variants ({product.variants.length})</h4>
                                <div className="max-h-48 overflow-y-auto space-y-2">
                                    {product.variants.map((v, i) => (
                                        <div key={i} className="flex justify-between items-center text-sm p-2 bg-gray-50 rounded-sm">
                                            <div>
                                                <span className="font-medium">{v.variantName}</span>
                                                <span className="text-xs text-gray-400 block">{v.sku}</span>
                                            </div>
                                            <div className="text-right">
                                                <div className="font-medium">{formatCurrency(v.price)}</div>
                                                <div className="text-xs text-gray-500">Qty: {v.stockLevel}</div>
                                            </div>
                                        </div>
                                    ))}
                                </div>
                            </div>
                        )}
                    </div>
                </div>
            </DialogContent>
        </Dialog>
    );
}
