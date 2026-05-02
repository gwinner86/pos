import api from '@/lib/api';

export interface SupplierInvoice {
    id: string;
    goodsReceiptId?: string;
    supplierId: string;
    supplierName?: string;
    companyId: string;
    invoiceNumber: string;
    invoiceDate: string;
    dueDate: string;
    totalAmount: number;
    totalPaid: number;
    outstandingAmount: number;
    status: string; // "Unpaid", "Partial", "Paid"
    createdAt: string;
}

export interface CreateSupplierInvoiceDto {
    goodsReceiptId?: string;
    supplierId: string;
    invoiceNumber: string;
    invoiceDate: string;
    dueDate: string;
    totalAmount: number;
}

export const vendorInvoiceService = {
    getInvoices: async (companyId?: string): Promise<SupplierInvoice[]> => {
        const response = await api.get('/api/SupplierInvoices', {
            params: { companyId }
        });
        return response.data.data;
    },

    getInvoiceById: async (id: string): Promise<SupplierInvoice> => {
        const response = await api.get(`/api/SupplierInvoices/${id}`);
        return response.data.data;
    },

    createInvoice: async (data: CreateSupplierInvoiceDto): Promise<SupplierInvoice> => {
        const response = await api.post('/api/SupplierInvoices', data);
        return response.data.data;
    }
};
