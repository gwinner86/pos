export interface Customer {
    id: string;
    firstName: string;
    lastName?: string;
    companyName?: string;
    email?: string;
    phone?: string;
    addressLine1?: string;
    city?: string;
    customerCode: string;
    loyaltyPoints: number;
    isActive: boolean;
    createdAt: string;
}

export interface CreateCustomerRequest {
    firstName: string;
    lastName?: string;
    companyName?: string;
    email?: string;
    phone?: string; // Backend expects 'Phone'
    addressLine1?: string; // Backend 'AddressLine1'
    city?: string;
    customerCode: string; // Required by backend
}

export interface Supplier {
    id: string;
    supplierName: string;
    contactName?: string;
    contactEmail?: string;
    phone?: string;
    terms?: string;
    isActive: boolean;
    locationId?: string;
    locationName?: string;
    createdAt?: string;
}

export interface CreateSupplierRequest {
    supplierName: string;
    contactPerson?: string;
    email?: string;
    phoneNumber: string;
    address?: string;
}

export interface PurchaseOrder {
    id: string;
    supplierId: string;
    supplierName: string;
    poNumber: string;
    orderDate: string;
    expectedDeliveryDate?: string;
    status: "Draft" | "Pending" | "Submitted" | "PartiallyReceived" | "Received" | "Cancelled";
    totalAmount: number;
    details: PurchaseOrderDetail[];
    notes?: string;
    createdAt: string;
}

export interface PurchaseOrderDetail {
    id: string;
    productId: string;
    productName: string;
    productVariantId: string;
    variantName: string;
    quantity: number;
    unitCost: number;
    lineTotal: number;
    quantityReceived: number;
}

export interface CreatePurchaseOrderDetailRequest {
    productVariantId: string;
    quantity: number;
    unitCost: number;
}

// ... existing code ...
export interface CreatePurchaseOrderRequest {
    locationId: string;
    supplierId: string;
    expectedDeliveryDate?: string;
    details: CreatePurchaseOrderDetailRequest[];
    notes?: string;
}

export interface UpdatePurchaseOrderDetailRequest {
    id?: string;
    productVariantId: string;
    quantity: number;
    unitCost: number;
}

export interface UpdatePurchaseOrderRequest {
    expectedDeliveryDate?: string;
    status?: string;
    details: UpdatePurchaseOrderDetailRequest[];
    notes?: string;
}
