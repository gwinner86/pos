import api from "@/lib/api";
import { Sale } from "@/types/sales";

export interface DashboardStats {
    totalRevenue: number;
    salesCount: number;
    activeProductsCount: number;
    activeCustomersCount: number;
    recentSales: any[];
    salesTrend: { date: string; totalSales: number }[];
    fastMovingProducts: any[];
    slowMovingProducts: any[];
    paymentMethodStats: any[];
}

export const dashboardService = {
    getStats: async (filter?: string) => {
        const queryParams = new URLSearchParams();
        if (filter) {
            queryParams.append("filter", filter);
        }
        const response = await api.get(`/api/Dashboard/stats?${queryParams.toString()}`);
        return response.data.data as DashboardStats;
    }
};
