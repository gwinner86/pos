import api from "@/lib/api";
import { AdjustInventoryRequest } from "@/types/inventory";

export const inventoryService = {
    adjustInventory: async (data: AdjustInventoryRequest) => {
        const response = await api.post('/api/Inventory/adjust', data);
        return response.data.data;
    },

    // Assuming a GET endpoint exists or will be useful
    getInventory: async () => {
        const response = await api.get('/api/Inventory');
        return response.data.data;
    }
};
