namespace POS.Application.DTOs.Accounting
{
    public class IncomeStatementDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        
        // Revenue
        public List<IncomeStatementItemDto> Revenues { get; set; } = new();
        public decimal TotalRevenue { get; set; }

        // COGS
        public List<IncomeStatementItemDto> CostOfGoodsSold { get; set; } = new();
        public decimal TotalCOGS { get; set; }

        public decimal GrossProfit => TotalRevenue - TotalCOGS;

        // Operating Expenses
        public List<IncomeStatementItemDto> Expenses { get; set; } = new();
        public decimal TotalExpenses { get; set; }

        public decimal NetIncome => GrossProfit - TotalExpenses;
    }

    public class IncomeStatementItemDto
    {
        public Guid AccountId { get; set; }
        public string AccountNumber { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;
        public decimal Amount { get; set; } // Always positive for display, specific meaning depends on section
    }

    public class BalanceSheetDto
    {
        public DateTime AsOfDate { get; set; }

        public List<IncomeStatementItemDto> Assets { get; set; } = new();
        public decimal TotalAssets { get; set; }

        public List<IncomeStatementItemDto> Liabilities { get; set; } = new();
        public decimal TotalLiabilities { get; set; }

        public List<IncomeStatementItemDto> Equity { get; set; } = new();
        public decimal TotalEquity { get; set; }
    }

    public class TrialBalanceDto
    {
        public DateTime AsOfDate { get; set; }
        public List<TrialBalanceItemDto> Accounts { get; set; } = new();
        public decimal TotalDebit { get; set; }
        public decimal TotalCredit { get; set; }
    }

    public class TrialBalanceItemDto
    {
        public string AccountNumber { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
    }
}
