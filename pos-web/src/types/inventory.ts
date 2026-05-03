export interface Variant {
    variantId?: string; // Optional for new variants
    sku: string;
    variantName: string; // e.g. "Size: L, Color: Red"
    attributes: Record<string, string>; // { "Size": "L", "Color": "Red" }
    price: number;
    stockLevel: number;
    minStockLevel?: number;
    cost: number; // Required for creation
}

export interface Product {
    productId: string;
    productName: string;
    sku: string;
    barcode?: string;
    description?: string;
    categoryId: string;
    categoryName?: string;
    price: number;
    cost: number;
    stockLevel: number;
    minStockLevel: number;
    isActive: boolean;
    isVatExcluded?: boolean;
    image1?: string;
    supplierId?: string; // Added
    supplierName?: string; // Added (Display)
    tenantId: string;
    hasVariants: boolean;
    newStock?: number;
    variants?: Variant[];
}

export interface Category {
    categoryId: string;
    categoryName: string;
    parentCategoryId?: string | null;
    parentCategoryName?: string | null;
    locationId?: string | null;
    isActive: boolean;
    createdAt?: string;
    subCategories?: Category[];
}

export interface CreateCategoryRequest {
    categoryName: string;
    parentCategoryId?: string | null;
    locationId?: string | null;
    isActive: boolean;
}

export interface AdjustInventoryRequest {
    productVariantId: string;
    locationId: string;
    adjustmentQuantity: number;
    transactionType: "StockIn" | "StockOut" | "Audit" | "Return" | "Damage";
    reason: string;
}

export interface CreateProductRequest {
    productName: string;
    sku: string;
    barcode?: string;
    categoryId: string;
    supplierId?: string; // Added
    // Global display values (optional/calculated)
    price?: number;
    cost?: number;
    stockLevel?: number;
    minStockLevel?: number;
    description?: string;
    image1?: string;
    isActive: boolean;
    isVatExcluded?: boolean;
    variants: Variant[]; // Required now
}
