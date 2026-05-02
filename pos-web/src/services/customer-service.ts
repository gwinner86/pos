import api from "@/lib/api";
import { CreateCustomerRequest, Customer } from "@/types/crm";

export const customerService = {
    getCustomers: async (query?: string) => {
        const response = await api.get('/api/Customers', { params: { search: query } });
        return response.data.data as Customer[];
    },

    getCustomer: async (id: string) => {
        const response = await api.get(`/api/Customers/${id}`);
        return response.data.data as Customer;
    },

    createCustomer: async (data: CreateCustomerRequest) => {
        const response = await api.post('/api/Customers', data);
        return response.data.data as Customer;
    },

    updateCustomer: async (id: string, data: Partial<CreateCustomerRequest>) => {
        const response = await api.put(`/api/Customers/${id}`, data);
        return response.data.data as Customer;
    },

    deleteCustomer: async (id: string) => {
        await api.delete(`/api/Customers/${id}`);
    }
};
