import api from "@/lib/api";
import { SetupVAT, CreateSetupVATRequest, UpdateSetupVATRequest } from "../types/settings";

export const vatService = {
    getVATs: async () => {
        const response = await api.get('/api/SetupVAT');
        return response.data.data;
    },

    getVATById: async (id: string) => {
        const response = await api.get(`/api/SetupVAT/${id}`);
        return response.data.data;
    },

    createVAT: async (data: CreateSetupVATRequest) => {
        const response = await api.post('/api/SetupVAT', data);
        return response.data.data;
    },

    updateVAT: async (id: string, data: UpdateSetupVATRequest) => {
        const response = await api.put(`/api/SetupVAT/${id}`, data);
        return response.data.data;
    },

    deleteVAT: async (id: string) => {
        const response = await api.delete(`/api/SetupVAT/${id}`);
        return response.data.data;
    }
};
