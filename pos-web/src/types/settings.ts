export interface SetupVAT {
    id: string;
    tenantId: string;
    companyId: string;
    locationId?: string;
    name: string;
    rate: number;
    description?: string;
    countryCode?: string;
    makerId?: string;
    makeDate?: string;
    isActive: boolean;
    createdAt: string;
    updatedAt?: string;
}

export interface CreateSetupVATRequest {
    companyId?: string; // Optional if derived from context
    locationId?: string;
    name: string;
    rate: number;
    description?: string;
    countryCode?: string;
    isActive: boolean;
}

export type UpdateSetupVATRequest = CreateSetupVATRequest;

export interface SetupCountry {
    id: string;
    tenantId: string;
    companyId: string;
    locationId?: string;
    countryName: string;
    countryCode: string;
    isActive: boolean;
    createdAt: string;
    updatedAt?: string;
}

export interface CreateSetupCountryRequest {
    tenantId: string;
    companyId: string;
    locationId?: string;
    countryName: string;
    countryCode: string;
}

export interface UpdateSetupCountryRequest {
    countryName: string;
    countryCode: string;
    isActive: boolean;
}

export interface SetupExpenseType {
    id: string;
    tenantId: string;
    companyId: string;
    name: string;
    description?: string;
    expenseAccountId: string;
    expenseAccountName?: string;
    isActive: boolean;
    createdAt: string;
    updatedAt?: string;
}

export interface CreateSetupExpenseTypeRequest {
    name: string;
    description?: string;
    expenseAccountId: string;
    isActive: boolean;
}

export type UpdateSetupExpenseTypeRequest = CreateSetupExpenseTypeRequest;
