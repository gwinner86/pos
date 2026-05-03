import api from "@/lib/api";
import { CreateSaleDto, Sale } from "@/types/sales";

export const salesService = {
    createSale: async (data: CreateSaleDto): Promise<Sale> => {
        const response = await api.post("/api/Sales", data);
        return response.data.data;
    },

    getSales: async (customerId?: string, userId?: string, startDate?: string, endDate?: string): Promise<Sale[]> => {
        const params = new URLSearchParams();
        if (customerId) params.append("customerId", customerId);
        if (userId) params.append("userId", userId);
        if (startDate) params.append("startDate", startDate);
        if (endDate) params.append("endDate", endDate);

        const response = await api.get(`/api/Sales?${params.toString()}`);
        return response.data.data;
    },

    getSale: async (id: string): Promise<Sale> => {
        const response = await api.get(`/api/Sales/${id}`);
        return response.data.data;
    },
};
