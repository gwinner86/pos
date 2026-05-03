import api from "@/lib/api";
import { SetupExpenseType, CreateSetupExpenseTypeRequest, UpdateSetupExpenseTypeRequest } from "../types/settings";

export const expenseTypeService = {
    getExpenseTypes: async (): Promise<SetupExpenseType[]> => {
        const response = await api.get('/api/SetupExpenseType');
        return response.data;
    },

    getExpenseTypeById: async (id: string): Promise<SetupExpenseType> => {
        const response = await api.get(`/api/SetupExpenseType/${id}`);
        return response.data;
    },

    createExpenseType: async (data: CreateSetupExpenseTypeRequest): Promise<SetupExpenseType> => {
        const response = await api.post('/api/SetupExpenseType', data);
        return response.data;
    },

    updateExpenseType: async (id: string, data: UpdateSetupExpenseTypeRequest): Promise<SetupExpenseType> => {
        const response = await api.put(`/api/SetupExpenseType/${id}`, data);
        return response.data;
    },

    deleteExpenseType: async (id: string): Promise<void> => {
        const response = await api.delete(`/api/SetupExpenseType/${id}`);
        return response.data;
    }
};
