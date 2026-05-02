import api from '@/lib/api';
import { SetupCountry, CreateSetupCountryRequest, UpdateSetupCountryRequest } from '@/types/settings';

export const countryService = {
    getCountries: async () => {
        const response = await api.get<SetupCountry[]>('/api/SetupCountry');
        return response.data;
    },

    getCountryById: async (id: string) => {
        const response = await api.get<SetupCountry>(`/api/SetupCountry/${id}`);
        return response.data;
    },

    createCountry: async (data: CreateSetupCountryRequest) => {
        const response = await api.post<SetupCountry>('/api/SetupCountry', data);
        return response.data;
    },

    updateCountry: async (id: string, data: UpdateSetupCountryRequest) => {
        const response = await api.put<SetupCountry>(`/api/SetupCountry/${id}`, data);
        return response.data;
    },

    deleteCountry: async (id: string) => {
        const response = await api.delete(`/api/SetupCountry/${id}`);
        return response.data;
    }
};
