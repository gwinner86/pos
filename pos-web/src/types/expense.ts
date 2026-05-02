export interface Expense {
    id: string;
    expenseDate: string;
    description: string;
    referenceNumber?: string;
    amount: number;
    expenseTypeId: string;
    expenseTypeName: string;
    paymentAccountId: string;
    paymentAccountName: string;
    status: string;
}

export interface CreateExpenseDto {
    locationId: string;
    expenseDate: string;
    description: string;
    referenceNumber?: string;
    amount: number;
    expenseTypeId: string;
    paymentAccountId: string;
}
