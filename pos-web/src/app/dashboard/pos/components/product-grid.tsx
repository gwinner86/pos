"use client";

import { useEffect, useState, useMemo } from "react";
import { ProductCard } from "./product-card";
import { productService } from "@/services/product-service";
import { categoryService } from "@/services/product-service";
import { Product, Category, Variant } from "@/types/inventory";
import { vatService } from "@/services/vat-service";
import { Input } from "@/components/ui/input";
import { Tabs, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Search } from "lucide-react";
import { usePosStore } from "@/store/pos-store";
import { ScrollArea, ScrollBar } from "@/components/ui/scroll-area"
import { formatCurrency } from "@/lib/utils";
import { useAuth } from "@/hooks/use-auth";
import { toast } from "sonner";
import {
    Dialog,
    DialogContent,
    DialogHeader,
    DialogTitle,
} from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";

export function ProductGrid() {
    const { selectedLocation } = useAuth();
    const [products, setProducts] = useState<Product[]>([]);
    const [categories, setCategories] = useState<Category[]>([]);
    const [loading, setLoading] = useState(true);
    const [searchQuery, setSearchQuery] = useState("");
    const [selectedCategory, setSelectedCategory] = useState("all");

    // Variant Selection State
    const [isVariantModalOpen, setIsVariantModalOpen] = useState(false);
    const [selectedProductForVariant, setSelectedProductForVariant] = useState<Product | null>(null);

    const { addToCart, refreshKey, setVats } = usePosStore();

    useEffect(() => {
        const loadData = async () => {
            setLoading(true);
            try {
                // Pass selectedLocation.id if available
                const [productsData, categoriesData, vatsData] = await Promise.all([
                    productService.getProducts(selectedLocation?.id),
                    categoryService.getCategories(),
                    vatService.getVATs()
                ]);
                setProducts(productsData);
                setCategories(categoriesData);

                // Store active VATs in POS state
                if (vatsData && Array.isArray(vatsData)) {
                    setVats(vatsData.filter(v => v.isActive).map(v => ({ name: v.name, rate: v.rate })));
                }

                // Initialize VAT calculation type from the selected location (1=Exclusive, 2=Inclusive)
                if (selectedLocation?.vatCalculationType) {
                    usePosStore.getState().setVatCalculationType(selectedLocation.vatCalculationType);
                }

            } catch (error) {
                console.error("Failed to load POS data", error);
            } finally {
                setLoading(false);
            }
        };
        if (selectedLocation?.id) {
            loadData();
        }
    }, [selectedLocation?.id, refreshKey]);

    const filteredProducts = useMemo(() => {
        return products.filter(p => {
            const matchesSearch = p.productName.toLowerCase().includes(searchQuery.toLowerCase()) ||
                p.sku?.toLowerCase().includes(searchQuery.toLowerCase());
            const matchesCategory = selectedCategory === "all" || p.categoryId === selectedCategory;

            return matchesSearch && matchesCategory;
        });
    }, [products, searchQuery, selectedCategory]);

    const handleProductClick = (product: Product) => {
        if (product.variants && product.variants.length > 1) {
            setSelectedProductForVariant(product);
            setIsVariantModalOpen(true);
        } else {
            // Add default variant (usually the single one)
            const variantId = product.variants?.[0]?.variantId || product.productId;
            const existingItem = usePosStore.getState().cart.find(i => i.variantId === variantId);
            const stockLimit = product.variants?.[0]?.stockLevel || 0;

            if (existingItem && existingItem.quantity >= stockLimit) {
                toast.warning(`Cannot add more. Only ${stockLimit} in stock!`);
                return;
            }

            if (stockLimit <= 0) {
                toast.error("Out of stock!");
                return;
            }

            addToCart(product);
        }
    };

    const handleVariantSelect = (variant: Variant) => {
        if (selectedProductForVariant) {
            const existingItem = usePosStore.getState().cart.find(i => i.variantId === variant.variantId);

            if (existingItem && existingItem.quantity >= variant.stockLevel) {
                toast.warning(`Cannot add more. Only ${variant.stockLevel} in stock!`);
                return;
            }

            addToCart(selectedProductForVariant, variant.variantId);
            setIsVariantModalOpen(false);
            setSelectedProductForVariant(null);
        }
    };

    if (loading) return <div className="p-4">Loading Products...</div>;

    return (
        <div className="flex flex-col h-full gap-4">
            {/* Search and Filter */}
            <div className="space-y-3">
                <div className="relative">
                    <Search className="absolute left-2.5 top-2.5 h-4 w-4 text-muted-foreground" />
                    <Input
                        type="search"
                        placeholder="Search products by name or SKU..."
                        className="pl-8 bg-white"
                        value={searchQuery}
                        onChange={(e) => setSearchQuery(e.target.value)}
                    />
                </div>

                <Tabs defaultValue="all" value={selectedCategory} onValueChange={setSelectedCategory}>
                    <div className="w-full overflow-x-auto pb-2">
                        <TabsList className="w-auto inline-flex h-9 items-center justify-start rounded-none border-b bg-transparent p-0 text-muted-foreground">
                            <TabsTrigger
                                value="all"
                                className="relative rounded-none border-b-2 border-transparent px-4 pb-2 pt-2 font-semibold data-[state=active]:border-primary data-[state=active]:text-foreground"
                            >
                                All Items
                            </TabsTrigger>
                            {categories.map(cat => (
                                <TabsTrigger
                                    key={cat.categoryId}
                                    value={cat.categoryId || "unknown"}
                                    className="relative rounded-none border-b-2 border-transparent px-4 pb-2 pt-2 font-semibold data-[state=active]:border-primary data-[state=active]:text-foreground"
                                >
                                    {cat.categoryName}
                                </TabsTrigger>
                            ))}
                        </TabsList>
                    </div>
                </Tabs>
            </div>

            {/* Grid */}
            <ScrollArea className="flex-1 -mr-4 pr-4">
                <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 xl:grid-cols-5 gap-4 pb-4">
                    {filteredProducts.map(product => (
                        <ProductCard
                            key={product.productId}
                            product={product}
                            onClick={handleProductClick}
                        />
                    ))}
                    {filteredProducts.length === 0 && (
                        <div className="col-span-full py-10 text-center text-muted-foreground">
                            No products found.
                        </div>
                    )}
                </div>
            </ScrollArea>

            {/* Variant Selection Modal */}
            <Dialog open={isVariantModalOpen} onOpenChange={setIsVariantModalOpen}>
                <DialogContent>
                    <DialogHeader>
                        <DialogTitle>Select Variant: {selectedProductForVariant?.productName}</DialogTitle>
                    </DialogHeader>
                    <div className="grid gap-4 py-4">
                        {selectedProductForVariant?.variants?.map((variant) => (
                            <Button
                                key={variant.variantId}
                                variant="outline"
                                className="justify-between h-auto py-3"
                                onClick={() => handleVariantSelect(variant)}
                                disabled={variant.stockLevel <= 0}
                            >
                                <div className="flex flex-col items-start">
                                    <span className="font-semibold">{variant.variantName}</span>
                                    <span className="text-xs text-muted-foreground">SKU: {variant.sku}</span>
                                </div>
                                <div className="text-right flex flex-col items-end shrink-0 pl-2">
                                    <div className="font-bold">{formatCurrency(variant.price || 0)}</div>
                                    <div className={`text-[10px] whitespace-nowrap mt-1 ${variant.stockLevel > 0 ? 'text-green-600' : 'text-red-500'}`}>
                                        {variant.stockLevel > 0 ? `${variant.stockLevel} In Stock` : 'Out of Stock'}
                                    </div>
                                </div>
                            </Button>
                        ))}
                    </div>
                </DialogContent>
            </Dialog>
        </div>
    );
}
