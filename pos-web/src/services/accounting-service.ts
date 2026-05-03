import api from "@/lib/api";
import { GLAccount, JournalEntry, CreateJournalEntryRequest } from "@/types/accounting";

export const accountingService = {
    getGLAccounts: async () => {
        const response = await api.get('/api/Accounting/accounts');
        // Handle different response structures (direct array or wrapper)
        const data = response.data?.data || response.data;
        return Array.isArray(data) ? data : [];
    },

    getJournalEntries: async () => {
        const response = await api.get('/api/Accounting/journals');
        const data = response.data?.data || response.data;
        return Array.isArray(data) ? data : [] as JournalEntry[];
    },

    createJournalEntry: async (data: CreateJournalEntryRequest) => {
        const response = await api.post('/api/Accounting/journals', data);
        return response.data.data;
    }
};
