import api from "@/lib/api";

// --- Types ---
export interface Company {
    companyId: string;
    companyName: string;
    description?: string;
    addressLine1?: string;
    taxId?: string;
    logoUrl?: string; // Optional
    currencyId?: string;
}

export interface Currency {
    id: string;
    currencyName: string;
    currencyCode: string;
    currencySymbol: string;
    isDefault: boolean;
}

export interface Location {
    id: string;
    locationName: string;
    locationType: string;
    addressLine1?: string;
    currencyId?: string;
    vatCalculationType: number;
}

export interface CreateLocationRequest {
    locationName: string;
    locationType: string;
    addressLine1?: string;
    currencyId?: string;
    vatCalculationType: number;
}

export interface PaymentMethod {
    id: string; // Note: DTO says PaymentMethodId (int), but frontend interface used string. I should verify response mapping. Assumed match, but DTO has int.
    // Let's check getPaymentMethods response. If it returns int, this string type might be wrong unless JS converts it.
    // I will use 'any' or check usage. Wait, DTO says PaymentMethodId. Frontend mapped it.
    // Let's check getPaymentMethods in service. It casts to PaymentMethod[].
    // If backend sends PaymentMethodId, I should probably match it.
    // User request: "methodName": "string", "paymentType": "string", "isActive": true.
    // I will update interface to match DTO: PaymentMethodId
    paymentMethodId: number;
    methodName: string;
    paymentType: string;
    isActive: boolean;
}

export interface Role {
    roleId: number;
    roleName: string;
    description?: string;
}

// --- Service ---
export const settingsService = {
    // Company
    getCompany: async (id: string) => {
        const response = await api.get(`/api/Companies/${id}`);
        // Backend usually wraps in { data: ... }
        return response.data.data as Company;
    },
    updateCompany: async (id: string, data: Partial<Company>) => {
        const response = await api.put(`/api/Companies/${id}`, data);
        return response.data.data as Company;
    },

    // Currencies
    getCurrencies: async () => {
        const response = await api.get<any>("/api/Currency");
        const data = response.data;
        // Robustly handle if backend returns List or Single object or null
        return Array.isArray(data) ? data : (data ? [data as Currency] : []);
    },
    createCurrency: async (data: Omit<Currency, "id">) => {
        const response = await api.post("/api/Currency", data);
        return response.data;
    },
    updateCurrency: async (data: Partial<Currency> & { id: string }) => {
        const response = await api.put(`/api/Currency/${data.id}`, data);
        return response.data;
    },

    // Locations
    getLocations: async () => {
        const response = await api.get<any>("/api/Locations");
        return response.data.data as Location[];
    },
    createLocation: async (data: CreateLocationRequest) => {
        const response = await api.post("/api/Locations", data);
        return response.data;
    },
    updateLocation: async (id: string, data: CreateLocationRequest) => {
        const response = await api.put(`/api/Locations/${id}`, data);
        return response.data;
    },

    // Payment Methods
    getPaymentMethods: async () => {
        const response = await api.get<any>("/api/PaymentMethods");
        // Endpoint returns List<PaymentMethodDto>, no wrapper
        return response.data as PaymentMethod[];
    },
    createPaymentMethod: async (data: { methodName: string }) => {
        const response = await api.post("/api/PaymentMethods", data);
        return response.data;
    },

    // Roles
    getRoles: async () => {
        const response = await api.get<any>("/api/Roles");
        return response.data.data as Role[];
    },
    createRole: async (data: { roleName: string; description?: string }) => {
        const response = await api.post("/api/Roles", data);
        return response.data;
    },

    // Permissions
    getAllPermissions: async () => {
        const response = await api.get<any>("/api/Roles/permissions");
        return response.data.data as Permission[];
    },
    getRolePermissions: async (roleId: number) => {
        const response = await api.get<any>(`/api/Roles/${roleId}/permissions`);
        return response.data.data as Permission[];
    },
    assignPermissions: async (roleId: number, permissionIds: number[]) => {
        const response = await api.put(`/api/Roles/${roleId}/permissions`, permissionIds);
        return response.data;
    },

    // Users
    getUsers: async (includeDeleted: boolean = false) => {
        const response = await api.get<any>(`/api/Auth/users?includeDeleted=${includeDeleted}`);
        return response.data.data as UserResponse[];
    },
    getDeletedUsers: async () => {
        const response = await api.get<any>("/api/Auth/users/deleted");
        return response.data.data as UserResponse[];
    },
    createUser: async (data: any) => {
        const response = await api.post("/api/Auth/register-user", data);
        return response.data;
    },
    deleteUser: async (userId: string) => {
        const response = await api.delete(`/api/Auth/users/${userId}`);
        return response.data;
    },
    toggleUserStatus: async (userId: string, isActive: boolean) => {
        const response = await api.patch(`/api/Auth/users/${userId}/status?isActive=${isActive}`);
        return response.data;
    },
    resetUserPassword: async (userId: string) => {
        const response = await api.post(`/api/Auth/users/${userId}/reset-password`);
        return response.data;
    },
    changePassword: async (data: any) => {
        const response = await api.post("/api/Auth/change-password", data);
        return response.data;
    }
};

export interface UserResponse {
    userId: string;
    firstName: string;
    lastName: string;
    email: string;
    roleName: string;
    isActive: boolean;
    requiresPasswordChange: boolean;
    tenantId: string;
    companyId?: string;
    deletedAt?: string;
    deletedByName?: string;
}

export interface Permission {
    permissionId: number;
    name: string;
    description?: string;
    group?: string;
}
