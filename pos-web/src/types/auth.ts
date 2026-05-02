export interface Tenant {
    id: string;
    tenantName: string;
    featureId: number;
    isActive: boolean;
    createdAt: string;
}

export interface User {
    id: string;
    email: string;
    firstName: string;
    lastName: string;
    role: string;
    tenantId: string;
    companyId: string;
    companyEmailAddress?: string;
    permissions?: string[];
    requiresPasswordChange?: boolean;
}

export interface RegisterTenantRequest {
    tenantName: string;
    companyName: string;
    firstName: string;
    lastName: string;
    email: string;
    password: string;
    featureId: number;
    companyPrimaryPhoneNumber: string;
    companyEmailAddress?: string;
    locationName: string;
}

export interface LoginResponse {
    token: string;
    refreshToken: string;
    user: User;
    expiration: string;
}

export interface AuthState {
    user: User | null;
    tenant: Tenant | null;
    isAuthenticated: boolean;
    isLoading: boolean;
    verifyTenant: (name: string) => Promise<Tenant>;
    selectedCompany: Company | null;
    selectedLocation: Location | null;
    setSelectedCompany: (company: Company) => void;
    setSelectedLocation: (location: Location) => void;
}

export interface Company {
    companyId: string;
    tenantId: string;
    companyName: string;
    isActive: boolean;
}

export interface Location {
    id: string;
    locationName: string;
    companyId: string;
    locationType: string;
    vatCalculationType?: number;
    currencyId?: string;
    currencySymbol?: string;
    currencyCode?: string;
}
