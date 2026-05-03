import api from '@/lib/api';

export interface GoodsReceipt {
    id: string;
    purchaseOrderId?: string;
    companyId: string;
    locationId: string;
    locationName?: string;
    supplierId?: string;
    supplierName?: string;
    receiptDate: string;
    totalReceivedAmount: number;
    status: string;
    isInvoicedYesNo?: string;
    createdBy: string;
    updatedBy?: string;
    approvedBy?: string;
    approvedAt?: string;
    createdAt: string;
    details: GoodsReceiptDetail[];
}

export interface GoodsReceiptDetail {
    id: string;
    productId: string;
    productName?: string;
    productVariantId: string;
    variantName?: string;
    quantityReceived: number;
    unitCost: number;
    lineTotal: number;
}

export interface CreateGoodsReceiptDto {
    purchaseOrderId?: string;
    locationId: string;
    supplierId?: string;
    totalReceivedAmount: number;
    details: CreateGoodsReceiptDetailDto[];
}

export interface CreateGoodsReceiptDetailDto {
    productId: string;
    productVariantId: string;
    quantityReceived: number;
    unitCost: number;
}

export interface UpdateGoodsReceiptDto {
    receiptDate?: string;
    details: UpdateGoodsReceiptDetailDto[];
}

export interface UpdateGoodsReceiptDetailDto {
    id?: string;
    productVariantId: string;
    quantityReceived: number;
    unitCost: number;
}

export const goodsReceiptService = {
    getReceipts: async (companyId?: string): Promise<GoodsReceipt[]> => {
        const response = await api.get('/api/GoodsReceipts', {
            params: { companyId }
        });
        return response.data.data;
    },

    getReceiptById: async (id: string): Promise<GoodsReceipt> => {
        const response = await api.get(`/api/GoodsReceipts/${id}`);
        return response.data.data;
    },

    createReceipt: async (data: CreateGoodsReceiptDto): Promise<GoodsReceipt> => {
        const response = await api.post('/api/GoodsReceipts', data);
        return response.data.data;
    },

    updateReceipt: async (id: string, data: UpdateGoodsReceiptDto): Promise<GoodsReceipt> => {
        const response = await api.put(`/api/GoodsReceipts/${id}`, data);
        return response.data.data;
    },

    approveReceipt: async (id: string): Promise<GoodsReceipt> => {
        const response = await api.post(`/api/GoodsReceipts/${id}/approve`);
        return response.data.data;
    }
};
