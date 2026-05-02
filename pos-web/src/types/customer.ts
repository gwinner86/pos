export interface Customer {
    id: string;
    companyId: string;
    customerCode: string;
    firstName: string;
    lastName?: string;
    companyName?: string;
    email?: string;
    phone?: string;
    addressLine1?: string;
    city?: string;
    loyaltyPoints: number;
    isActive: boolean;
    createdAt: string;
}

export interface CreateCustomerDto {
    firstName: string;
    lastName?: string;
    companyName?: string;
    email?: string;
    phone?: string;
    addressLine1?: string;
    city?: string;
    customerCode: string;
}
