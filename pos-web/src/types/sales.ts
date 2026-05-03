export interface SaleDetail {
    id: string;
    productId: string;
    productName: string;
    productVariantId: string;
    variantName: string;
    quantity: number;
    unitPrice: number;
    discount: number;
    lineTotal: number;
    vatDetails?: string;
}

export interface Sale {
    id: string;
    companyId: string;
    locationId: string;
    locationName: string;
    customerId?: string;
    customerName: string;
    saleNumber: string;
    saleDate: string;
    totalAmount: number;
    status: string;
    paymentMethod: string;
    createdByUserId: string;
    createdByUserName: string;
    createdAt: string;
    details: SaleDetail[];
}

export interface CreateSaleDetailDto {
    productVariantId: string;
    quantity: number;
    unitPrice: number;
    discount: number;
}

export interface CreateSaleDto {
    locationId: string;
    customerId?: string | null;
    saleDate?: string;
    paymentMethod: string;
    details: CreateSaleDetailDto[];
}
