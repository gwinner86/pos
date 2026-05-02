import api from '@/lib/api';

export interface InvoicePayment {
    id: string;
    supplierInvoiceId: string;
    supplierOrCustomer: string; // usually "SUPPLIER"
    paymentDate: string;
    amountPaid: number;
    paymentMethod: string;
    createdAt: string;
}

export interface CreateInvoicePaymentDto {
    supplierInvoiceId: string;
    amountPaid: number;
    paymentMethod: string;
}

export const vendorPaymentService = {
    getPaymentsByInvoice: async (invoiceId: string): Promise<InvoicePayment[]> => {
        const response = await api.get(`/api/InvoicePayments/invoice/${invoiceId}`);
        return response.data.data;
    },

    createPayment: async (data: CreateInvoicePaymentDto): Promise<InvoicePayment> => {
        const response = await api.post('/api/InvoicePayments', data);
        return response.data.data;
    }
};
