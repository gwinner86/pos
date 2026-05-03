export interface GLAccount {
    id: string;
    accountNumber: string;
    accountName: string;
    accountType: string;
    debitIncreases: boolean;
    isActive: boolean;
    balance: number;
}

export interface JournalEntryDetail {
    id?: string;
    glAccountId?: string;
    accountNumber: string; // Used for creation
    accountName?: string; // Display
    debitAmount: number;
    creditAmount: number;
}

export interface JournalEntry {
    id: string;
    entryDate: string;
    description: string;
    sourceTable?: string;
    sourceId?: string;
    locationId?: string;
    isPosted: boolean;
    postedDate?: string;
    details: JournalEntryDetail[];
}

export interface CreateJournalEntryRequest {
    entryDate: string;
    description: string;
    sourceTable?: string;
    sourceId?: string;
    details: {
        accountNumber: string;
        debitAmount: number;
        creditAmount: number;
    }[];
}
