import api from "@/lib/api"

export interface IncomeStatementItem {
    accountId: string
    accountNumber: string
    accountName: string
    amount: number
}

export interface IncomeStatement {
    startDate: string
    endDate: string
    revenues: IncomeStatementItem[]
    totalRevenue: number
    costOfGoodsSold: IncomeStatementItem[]
    totalCOGS: number
    grossProfit: number
    expenses: IncomeStatementItem[]
    totalExpenses: number
    netIncome: number
}

export interface BalanceSheet {
    asOfDate: string
    assets: IncomeStatementItem[]
    totalAssets: number
    liabilities: IncomeStatementItem[]
    totalLiabilities: number
    equity: IncomeStatementItem[]
    totalEquity: number
}

export interface TrialBalanceItem {
    accountNumber: string
    accountName: string
    debit: number
    credit: number
}

export interface TrialBalance {
    asOfDate: string
    accounts: TrialBalanceItem[]
    totalDebit: number
    totalCredit: number
}

export const reportService = {
    getIncomeStatement: async (startDate?: string, endDate?: string) => {
        const response = await api.get<IncomeStatement>("/api/Reports/income-statement", {
            params: { startDate, endDate }
        })
        return response.data
    },

    getBalanceSheet: async (asOfDate?: string) => {
        const response = await api.get<BalanceSheet>("/api/Reports/balance-sheet", {
            params: { asOfDate }
        })
        return response.data
    },

    getTrialBalance: async (asOfDate?: string) => {
        const response = await api.get<TrialBalance>("/api/Reports/trial-balance", {
            params: { asOfDate }
        })
        return response.data
    }
}
