import api from "@/lib/api";
import { CreateSupplierRequest, Supplier, CreatePurchaseOrderRequest, PurchaseOrder } from "@/types/crm";

export const supplierService = {
    getSuppliers: async () => {
        const response = await api.get('/api/Suppliers');
        return response.data.data as Supplier[];
    },

    getSupplier: async (id: string) => {
        const response = await api.get(`/api/Suppliers/${id}`);
        return response.data.data as Supplier;
    },

    createSupplier: async (data: CreateSupplierRequest) => {
        const response = await api.post('/api/Suppliers', data);
        return response.data.data as Supplier;
    },

    updateSupplier: async (id: string, data: Partial<CreateSupplierRequest>) => {
        const response = await api.put(`/api/Suppliers/${id}`, data);
        return response.data.data as Supplier;
    },

    deleteSupplier: async (id: string) => {
        await api.delete(`/api/Suppliers/${id}`);
    },

    // Purchase Orders
    createPurchaseOrder: async (data: CreatePurchaseOrderRequest) => {
        const response = await api.post('/api/PurchaseOrders', data);
        return response.data.data as PurchaseOrder;
    },

    getPurchaseOrders: async () => {
        const response = await api.get('/api/PurchaseOrders');
        return response.data.data as PurchaseOrder[];
    },

    getPurchaseOrder: async (id: string) => {
        const response = await api.get(`/api/PurchaseOrders/${id}`);
        return response.data.data as PurchaseOrder;
    },

    updatePurchaseOrder: async (id: string, data: any) => { // using any or UpdatePurchaseOrderRequest
        const response = await api.put(`/api/PurchaseOrders/${id}`, data);
        return response.data.data as PurchaseOrder;
    },

    receivePurchaseOrder: async (id: string) => {
        const response = await api.post(`/api/PurchaseOrders/${id}/receive`);
        return response.data.data as PurchaseOrder;
    },

    createInstantPurchase: async (data: any) => {
        const response = await api.post('/api/PurchaseOrders/instant', data);
        return response.data.data as PurchaseOrder;
    }
};
