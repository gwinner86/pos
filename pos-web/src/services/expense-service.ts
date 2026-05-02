import api from "@/lib/api";
import { CreateExpenseDto, Expense } from "@/types/expense";

export const expenseService = {
    getExpenses: async (startDate?: string, endDate?: string) => {
        const params = new URLSearchParams();
        if (startDate) params.append("startDate", startDate);
        if (endDate) params.append("endDate", endDate);
        return api.get<Expense[]>(`/api/Expenses?${params.toString()}`).then(res => res.data);
    },

    createExpense: async (data: CreateExpenseDto) => {
        return api.post<Expense>("/api/Expenses", data).then(res => res.data);
    }
};
