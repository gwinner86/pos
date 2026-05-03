using Microsoft.EntityFrameworkCore;
using POS.Application.DTOs.Accounting;
using POS.Application.DTOs.Expense;
using POS.Application.DTOs.GoodsReturn;
using POS.Application.DTOs.Inventory;
using POS.Application.DTOs.InvoicePayment;
using POS.Application.DTOs.Sale;
using POS.Application.DTOs.SalePayment;
using POS.Application.DTOs.SupplierInvoice;
using POS.Application.Interfaces;
using POS.Domain.Entities;
using POS.Infrastructure.Persistence;

namespace POS.Infrastructure.Services
{
    public class AccountingService : IAccountingService
    {
        private readonly ApplicationDbContext _context;

        public AccountingService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task EnsureDefaultAccountsAsync(Guid companyId, Guid userId)
        {
            // Standard Chart of Accounts
            var defaults = new List<(string Code, string Name, string Type, bool DebitIncreases)>
            {
                ("1000", "Cash on Hand", "Asset", true),
                ("1100", "Accounts Receivable", "Asset", true),
                ("1200", "Inventory Asset", "Asset", true),
                ("2000", "Accounts Payable", "Liability", false),
                ("2100", "Sales Tax Payable", "Liability", false),
                ("4000", "Sales Revenue", "Revenue", false),
                ("4100", "Sales Returns", "Revenue", true), // Contra-Revenue, naturally debit balance
                ("5000", "Cost of Goods Sold", "Expense", true),
                ("5100", "Inventory Shrinkage/Loss", "Expense", true),
                ("5200", "Operating Expense", "Expense", true)
            };

            foreach (var def in defaults)
            {
                var exists = await _context.GLAccounts
                    .AnyAsync(a => a.AccountNumber == def.Code && a.CompanyId == companyId);
                
                if (!exists)
                {
                    _context.GLAccounts.Add(new GLAccount
                    {
                        CompanyId = companyId,
                        AccountNumber = def.Code,
                        AccountName = def.Name,
                        AccountType = def.Type,
                        DebitIncreases = def.DebitIncreases,
                        IsActive = true,
                        CreatedBy = userId
                    });
                }
            }
            
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<GLAccountDto>> GetGLAccountsAsync(Guid companyId)
        {
            // Ensure defaults exist before querying
            // We need a userId for creation. Since this method doesn't take one, 
            // we might need to change the signature or use a system ID / Guid.Empty if acceptable for system-gen accounts.
            // Looking at EnsureDefaultAccountsAsync signature: (Guid companyId, Guid userId)
            // I'll check how to get userId here or if I can pass Guid.Empty for system auto-gen.
            // For now, I'll stick to Guid.Empty as "System" for auto-generation on view, 
            // or better, update the Controller to pass it.
            // Let's check Controller first.
            
            var accounts = await _context.GLAccounts
                .Where(a => a.CompanyId == companyId)
                .OrderBy(a => a.AccountNumber)
                .ToListAsync();

            // Calculate Balances
            var balances = await _context.JournalEntryDetails
                .Include(d => d.JournalEntry)
                .Where(d => d.JournalEntry.CompanyId == companyId && d.JournalEntry.IsPosted)
                .GroupBy(d => d.GLAccountId)
                .Select(g => new 
                { 
                    GLAccountId = g.Key, 
                    TotalDebit = g.Sum(x => x.DebitAmount), 
                    TotalCredit = g.Sum(x => x.CreditAmount) 
                })
                .ToDictionaryAsync(k => k.GLAccountId, v => new { v.TotalDebit, v.TotalCredit });

            return accounts.Select(a => 
            {
                decimal balance = 0;
                if (balances.ContainsKey(a.Id))
                {
                    var b = balances[a.Id];
                    // Asset/Expense: Dr - Cr
                    // Liability/Equity/Revenue: Cr - Dr
                    if (a.DebitIncreases)
                        balance = b.TotalDebit - b.TotalCredit;
                    else
                        balance = b.TotalCredit - b.TotalDebit;
                }

                return new GLAccountDto
                {
                    Id = a.Id,
                    AccountNumber = a.AccountNumber,
                    AccountName = a.AccountName,
                    AccountType = a.AccountType,
                    DebitIncreases = a.DebitIncreases,
                    IsActive = a.IsActive,
                    Balance = balance
                };
            }).ToList();
        }

        public async Task<IEnumerable<JournalEntryDto>> GetJournalEntriesAsync(Guid companyId)
        {
            var journals = await _context.JournalEntries
                .Include(j => j.Details)
                .ThenInclude(d => d.GLAccount)
                .Where(j => j.CompanyId == companyId)
                .OrderByDescending(j => j.EntryDate)
                .ToListAsync();

            return journals.Select(j => new JournalEntryDto
            {
                Id = j.Id,
                EntryDate = j.EntryDate,
                SourceTable = j.SourceTable,
                SourceId = j.SourceId,
                Description = j.Description,
                IsPosted = j.IsPosted,
                PostedDate = j.PostedDate,
                Details = j.Details.Select(d => new JournalEntryDetailDto
                {
                    GLAccountId = d.GLAccountId,
                    AccountNumber = d.GLAccount?.AccountNumber ?? "??",
                    AccountName = d.GLAccount?.AccountName ?? "Unknown",
                    DebitAmount = d.DebitAmount,
                    CreditAmount = d.CreditAmount
                }).ToList()
            }).ToList();
        }

        public async Task<JournalEntryDto> PostJournalEntryAsync(CreateJournalEntryDto dto, Guid userId, Guid companyId)
        {
            // 1. Validate Total Debits = Total Credits
            var totalDebit = dto.Details.Sum(d => d.DebitAmount);
            var totalCredit = dto.Details.Sum(d => d.CreditAmount);

            if (totalDebit != totalCredit)
                throw new InvalidOperationException($"Journal Entry is unbalanced. Debit: {totalDebit}, Credit: {totalCredit}");

            // 2. Fetch Accounts
            var accounts = await _context.GLAccounts
                .Where(a => a.CompanyId == companyId)
                .ToListAsync();

            var accountMap = accounts.ToDictionary(a => a.AccountNumber, a => a.Id);

            // 3. Create Header
            var defaultLocation = await _context.Locations.FirstOrDefaultAsync(l => l.CompanyId == companyId);
            var locationId = defaultLocation?.Id ?? Guid.Empty; 

            var entry = new JournalEntry
            {
                CompanyId = companyId,
                TenantId = (await _context.Companies.FindAsync(companyId))?.TenantId ?? Guid.Empty, // Helper fetch
                LocationId = locationId, 
                EntryDate = dto.EntryDate,
                SourceTable = dto.SourceTable,
                SourceId = dto.SourceId,
                Description = dto.Description,
                IsPosted = true,
                PostedDate = DateTime.UtcNow,
                CreatedBy = userId,
                Details = new List<JournalEntryDetail>()
            };

            // 4. Create Details
            foreach (var d in dto.Details)
            {
                if (!accountMap.ContainsKey(d.AccountNumber))
                    throw new InvalidOperationException($"GL Account {d.AccountNumber} not found.");

                entry.Details.Add(new JournalEntryDetail
                {
                    GLAccountId = accountMap[d.AccountNumber],
                    DebitAmount = d.DebitAmount,
                    CreditAmount = d.CreditAmount
                });
            }

            _context.JournalEntries.Add(entry);
            await _context.SaveChangesAsync();

            return new JournalEntryDto { Id = entry.Id, IsPosted = true };
        }

        public async Task PostSaleTransactionAsync(SaleDto sale, Guid userId)
        {
            await EnsureDefaultAccountsAsync(sale.CompanyId, userId);

            var details = new List<CreateJournalEntryDetailDto>();

            // 1. Debit Accounts Receivable (Total Amount)
            details.Add(new CreateJournalEntryDetailDto
            {
                AccountNumber = "1100", // AR
                DebitAmount = sale.TotalAmount,
                CreditAmount = 0
            });

            // 2. Credit Sales Revenue
            details.Add(new CreateJournalEntryDetailDto
            {
                AccountNumber = "4000", // Revenue
                DebitAmount = 0,
                CreditAmount = sale.TotalAmount
            });

            // 3. Cost of Goods Sold & Inventory Asset
            decimal totalCost = 0;
            
            var variantIds = sale.Details.Select(d => d.ProductVariantId).ToList();
            var variants = await _context.ProductVariants
                .Include(v => v.Costs) // Include Costs
                .Where(v => variantIds.Contains(v.Id))
                .ToListAsync();

            foreach (var line in sale.Details)
            {
                var v = variants.FirstOrDefault(x => x.Id == line.ProductVariantId);
                // Get latest cost
                var cost = v?.Costs.OrderByDescending(c => c.EffectiveDate).FirstOrDefault()?.CostValue ?? 0;
                totalCost += cost * line.Quantity;
            }

            if (totalCost > 0)
            {
                // Debit COGS
                details.Add(new CreateJournalEntryDetailDto
                {
                    AccountNumber = "5000",
                    DebitAmount = totalCost,
                    CreditAmount = 0
                });
                
                // Credit Inventory Asset
                details.Add(new CreateJournalEntryDetailDto
                {
                    AccountNumber = "1200",
                    DebitAmount = 0,
                    CreditAmount = totalCost
                });
            }

            var entry = new CreateJournalEntryDto
            {
                SourceTable = "Sales",
                SourceId = sale.Id,
                Description = $"Sale Invoice #{sale.SaleNumber}",
                EntryDate = sale.SaleDate,
                Details = details
            };

            var journal = new JournalEntry
            {
                CompanyId = sale.CompanyId,
                TenantId = (await _context.Companies.FindAsync(sale.CompanyId))?.TenantId ?? Guid.Empty,
                LocationId = sale.LocationId,
                EntryDate = entry.EntryDate,
                SourceTable = entry.SourceTable,
                SourceId = entry.SourceId,
                Description = entry.Description,
                IsPosted = true,
                PostedDate = DateTime.UtcNow,
                CreatedBy = userId,
                Details = new List<JournalEntryDetail>()
            };
            
            var accounts = await _context.GLAccounts.Where(a => a.CompanyId == sale.CompanyId).ToListAsync();
            var accountMap = accounts.ToDictionary(a => a.AccountNumber, a => a.Id);
            
            foreach(var d in details)
            {
                 if (!accountMap.ContainsKey(d.AccountNumber)) continue;
                 journal.Details.Add(new JournalEntryDetail
                 {
                     GLAccountId = accountMap[d.AccountNumber],
                     DebitAmount = d.DebitAmount,
                     CreditAmount = d.CreditAmount
                 });
            }
            
            _context.JournalEntries.Add(journal);
            await _context.SaveChangesAsync();
        }

        public async Task PostDirectSaleTransactionAsync(SaleDto sale, Guid userId)
        {
            await EnsureDefaultAccountsAsync(sale.CompanyId, userId);

            var details = new List<CreateJournalEntryDetailDto>();

            // 1. Debit Cash on Hand (1000) - Total Amount
            // Replacing Accounts Receivable with Cash on Hand as requested.
            details.Add(new CreateJournalEntryDetailDto
            {
                AccountNumber = "1000", // Cash
                DebitAmount = sale.TotalAmount,
                CreditAmount = 0
            });

            // 2. Credit Sales Revenue (4000) - Total Amount
            details.Add(new CreateJournalEntryDetailDto
            {
                AccountNumber = "4000", // Revenue
                DebitAmount = 0,
                CreditAmount = sale.TotalAmount
            });

            // 3. Cost of Goods Sold & Inventory Asset
            // Calculate detailed cost
            decimal totalCost = 0;
            var variantIds = sale.Details.Select(d => d.ProductVariantId).ToList();
            var variants = await _context.ProductVariants
                .Include(v => v.Costs)
                .Where(v => variantIds.Contains(v.Id))
                .ToListAsync();

            foreach (var line in sale.Details)
            {
                var v = variants.FirstOrDefault(x => x.Id == line.ProductVariantId);
                var cost = v?.Costs.OrderByDescending(c => c.EffectiveDate).FirstOrDefault()?.CostValue ?? 0;
                totalCost += cost * line.Quantity;
            }

            if (totalCost > 0)
            {
                // Debit COGS (5000)
                details.Add(new CreateJournalEntryDetailDto
                {
                    AccountNumber = "5000",
                    DebitAmount = totalCost,
                    CreditAmount = 0
                });
                
                // Credit Inventory Asset (1200)
                details.Add(new CreateJournalEntryDetailDto
                {
                    AccountNumber = "1200",
                    DebitAmount = 0,
                    CreditAmount = totalCost
                });
            }

            var entry = new CreateJournalEntryDto
            {
                SourceTable = "Sales",
                SourceId = sale.Id,
                Description = $"POS Sale #{sale.SaleNumber} (Cash)",
                EntryDate = sale.SaleDate,
                Details = details
            };

            var journal = new JournalEntry
            {
                CompanyId = sale.CompanyId,
                TenantId = (await _context.Companies.FindAsync(sale.CompanyId))?.TenantId ?? Guid.Empty,
                LocationId = sale.LocationId,
                EntryDate = entry.EntryDate,
                SourceTable = entry.SourceTable,
                SourceId = entry.SourceId,
                Description = entry.Description,
                IsPosted = true,
                PostedDate = DateTime.UtcNow,
                CreatedBy = userId,
                Details = new List<JournalEntryDetail>()
            };
            
            var accounts = await _context.GLAccounts.Where(a => a.CompanyId == sale.CompanyId).ToListAsync();
            var accountMap = accounts.ToDictionary(a => a.AccountNumber, a => a.Id);
            
            foreach(var d in details)
            {
                 if (!accountMap.ContainsKey(d.AccountNumber)) continue;
                 journal.Details.Add(new JournalEntryDetail
                 {
                     GLAccountId = accountMap[d.AccountNumber],
                     DebitAmount = d.DebitAmount,
                     CreditAmount = d.CreditAmount
                 });
            }
            
            _context.JournalEntries.Add(journal);
            await _context.SaveChangesAsync();
        }

        public async Task PostPaymentTransactionAsync(SalePaymentDto payment, Guid userId)
        {
             var sale = await _context.Sales.FindAsync(payment.SaleId);
             if (sale == null) return;
             
             await EnsureDefaultAccountsAsync(sale.CompanyId, userId);
             var accounts = await _context.GLAccounts.Where(a => a.CompanyId == sale.CompanyId).ToListAsync();
             var accountMap = accounts.ToDictionary(a => a.AccountNumber, a => a.Id);

             var journal = new JournalEntry
            {
                CompanyId = sale.CompanyId,
                TenantId = sale.TenantId,
                LocationId = sale.LocationId,
                EntryDate = payment.PaymentDate,
                SourceTable = "SalePayments",
                SourceId = payment.Id,
                Description = $"Payment for Sale #{sale.SaleNumber}",
                IsPosted = true,
                PostedDate = DateTime.UtcNow,
                CreatedBy = userId,
                Details = new List<JournalEntryDetail>()
            };
            
            // Debit Cash 1000
            if(accountMap.ContainsKey("1000"))
                journal.Details.Add(new JournalEntryDetail { GLAccountId = accountMap["1000"], DebitAmount = payment.Amount, CreditAmount = 0 });
                
            // Credit AR 1100
             if(accountMap.ContainsKey("1100"))
                journal.Details.Add(new JournalEntryDetail { GLAccountId = accountMap["1100"], DebitAmount = 0, CreditAmount = payment.Amount });

            _context.JournalEntries.Add(journal);
            await _context.SaveChangesAsync();
        }

        public async Task PostReturnTransactionAsync(GoodsReturnDto ret, Guid userId)
        {
            await EnsureDefaultAccountsAsync(ret.CompanyId, userId);
            var accounts = await _context.GLAccounts.Where(a => a.CompanyId == ret.CompanyId).ToListAsync();
            var accountMap = accounts.ToDictionary(a => a.AccountNumber, a => a.Id);
            
             var journal = new JournalEntry
            {
                CompanyId = ret.CompanyId,
                TenantId = (await _context.Companies.FindAsync(ret.CompanyId))?.TenantId ?? Guid.Empty,
                LocationId = ret.LocationId,
                EntryDate = ret.ReturnDate,
                SourceTable = "GoodsReturns",
                SourceId = ret.Id,
                Description = $"Return #{ret.Id} - {ret.ReturnReason}",
                IsPosted = true,
                PostedDate = DateTime.UtcNow,
                CreatedBy = userId,
                Details = new List<JournalEntryDetail>()
            };
            
            // 1. Debit Sales Returns 4100
            if(accountMap.ContainsKey("4100"))
                journal.Details.Add(new JournalEntryDetail { GLAccountId = accountMap["4100"], DebitAmount = ret.TotalRefundAmount, CreditAmount = 0 });

            // 2. Credit Cash/AR 1100 (Assuming credit note/AR reduction for now)
             if(accountMap.ContainsKey("1100"))
                journal.Details.Add(new JournalEntryDetail { GLAccountId = accountMap["1100"], DebitAmount = 0, CreditAmount = ret.TotalRefundAmount });

            _context.JournalEntries.Add(journal);
            await _context.SaveChangesAsync();
        }

        public async Task PostInventoryAdjustmentAsync(Guid inventoryId, decimal quantityChanged, string reason, Guid userId)
        {
            // If quantityChanged is 0, nothing to do
            if (quantityChanged == 0) return;

            var inventory = await _context.Inventory
                .Include(i => i.ProductVariant).ThenInclude(pv => pv.Costs)
                .Include(i => i.Location) // Use Location to find Company
                .FirstOrDefaultAsync(i => i.Id == inventoryId);

            if (inventory == null) return;

            // Resolve CompanyId from Location
            var companyId = inventory.Location?.CompanyId ?? Guid.Empty;
            if (companyId == Guid.Empty) return;

            await EnsureDefaultAccountsAsync(companyId, userId);
            var accounts = await _context.GLAccounts.Where(a => a.CompanyId == companyId).ToListAsync();
            var accountMap = accounts.ToDictionary(a => a.AccountNumber, a => a.Id);

            // Calculate Value of Adjustment
            // Need Cost Price
            var cost = inventory.ProductVariant?.Costs.OrderByDescending(c => c.EffectiveDate).FirstOrDefault()?.CostValue ?? 0;
            var totalValue = Math.Abs(quantityChanged) * cost;

            if (totalValue == 0) return; // No financial impact if cost is 0

            var journal = new JournalEntry
            {
                CompanyId = companyId,
                TenantId = inventory.Product?.TenantId ?? Guid.Empty, // Product not included in query above?
                // Wait, I removed Product include? I should add it back or use Location.TenantId if available.
                // Location usually has TenantId too. Or just use Guid.Empty if strictness not required for now since CompanyId is key.
                // Let's rely on CompanyId. TenantId is often redundant in single-DB multi-tenant if handled by Company.
                // But let's check Location entity quickly? 
                // Assuming Location has TenantId.
                LocationId = inventory.LocationId,
                EntryDate = DateTime.UtcNow,
                SourceTable = "InventoryTransactions", 
                SourceId = inventoryId, 
                Description = $"Inventory Adjustment: {reason} ({quantityChanged})",
                IsPosted = true,
                PostedDate = DateTime.UtcNow,
                CreatedBy = userId,
                Details = new List<JournalEntryDetail>()
            };
            
            // Re-fetch TenantId just to be safe if strictly required
            if (journal.TenantId == Guid.Empty)
            {
                 var company = await _context.Companies.FindAsync(companyId);
                 journal.TenantId = company?.TenantId ?? Guid.Empty;
            }

            if (quantityChanged < 0)
            {
                // LOSS / DECREASE
                // Debit Expense (Shrinkage) 5100
                // Credit Inventory Asset 1200
                
                if (accountMap.ContainsKey("5100"))
                    journal.Details.Add(new JournalEntryDetail { GLAccountId = accountMap["5100"], DebitAmount = totalValue, CreditAmount = 0 });
                
                if (accountMap.ContainsKey("1200"))
                    journal.Details.Add(new JournalEntryDetail { GLAccountId = accountMap["1200"], DebitAmount = 0, CreditAmount = totalValue });
            }
            else
            {
                // GAIN / INCREASE (Found Stock)
                // Debit Inventory Asset 1200
                // Credit Revenue/Gain? Or Reverse Expense?
                
                 if (accountMap.ContainsKey("1200"))
                    journal.Details.Add(new JournalEntryDetail { GLAccountId = accountMap["1200"], DebitAmount = totalValue, CreditAmount = 0 });

                 if (accountMap.ContainsKey("5100"))
                    journal.Details.Add(new JournalEntryDetail { GLAccountId = accountMap["5100"], DebitAmount = 0, CreditAmount = totalValue });
            }

            _context.JournalEntries.Add(journal);
            await _context.SaveChangesAsync();
        }
        public async Task PostExpenseTransactionAsync(Guid expenseId, Guid userId)
        {
            var expense = await _context.Expenses.Include(e => e.ExpenseType).FirstOrDefaultAsync(e => e.Id == expenseId);
            if (expense == null || expense.ExpenseType == null) throw new KeyNotFoundException("Expense not found");

            await EnsureDefaultAccountsAsync(expense.CompanyId, userId);
            
            var journal = new JournalEntry
            {
                CompanyId = expense.CompanyId,
                TenantId = expense.TenantId,
                LocationId = expense.LocationId,
                EntryDate = expense.ExpenseDate,
                SourceTable = "Expenses",
                SourceId = expense.Id,
                Description = $"Expense: {expense.Description} (Ref: {expense.ReferenceNumber})",
                IsPosted = true,
                PostedDate = DateTime.UtcNow,
                CreatedBy = userId,
                Details = new List<JournalEntryDetail>()
            };

            // Debit Expense Account
            journal.Details.Add(new JournalEntryDetail
            {
                GLAccountId = expense.ExpenseType.ExpenseAccountId,
                DebitAmount = expense.Amount,
                CreditAmount = 0
            });

            // Credit Payment Account (Asset)
            journal.Details.Add(new JournalEntryDetail
            {
                GLAccountId = expense.PaymentAccountId,
                DebitAmount = 0,
                CreditAmount = expense.Amount
            });

            _context.JournalEntries.Add(journal);
            await _context.SaveChangesAsync();
        }

        public async Task PostSupplierInvoiceTransactionAsync(SupplierInvoiceDto invoice, Guid userId)
        {
             var entity = await _context.SupplierInvoices.FindAsync(invoice.Id);
             if (entity == null) return;
             
             await EnsureDefaultAccountsAsync(entity.CompanyId, userId);

             var details = new List<CreateJournalEntryDetailDto>
             {
                 // Debit Inventory Asset (1200)
                 new CreateJournalEntryDetailDto
                 {
                     AccountNumber = "1200",
                     DebitAmount = invoice.TotalAmount,
                     CreditAmount = 0
                 },
                 // Credit Accounts Payable (2000)
                 new CreateJournalEntryDetailDto
                 {
                     AccountNumber = "2000",
                     DebitAmount = 0,
                     CreditAmount = invoice.TotalAmount
                 }
             };

             var entry = new CreateJournalEntryDto
             {
                 SourceTable = "SupplierInvoices",
                 SourceId = invoice.Id,
                 Description = $"Bill #{invoice.InvoiceNumber} from {invoice.SupplierName}",
                 EntryDate = invoice.InvoiceDate,
                 Details = details
             };
             
             // SupplierInvoice does not have LocationId, using Empty
             await ProcessJournalEntryAsync(entry, entity.CompanyId, Guid.Empty, userId);
        }

        public async Task PostInvoicePaymentTransactionAsync(InvoicePaymentDto payment, Guid userId)
        {
            var entity = await _context.InvoicePayments.Include(p => p.SupplierInvoice).FirstOrDefaultAsync(p => p.Id == payment.Id);
            if (entity == null || entity.SupplierInvoice == null) return;

             var details = new List<CreateJournalEntryDetailDto>
             {
                 // Debit Accounts Payable (2000)
                 new CreateJournalEntryDetailDto
                 {
                     AccountNumber = "2000",
                     DebitAmount = payment.AmountPaid,
                     CreditAmount = 0
                 },
                 // Credit Cash (1000)
                 new CreateJournalEntryDetailDto
                 {
                     AccountNumber = "1000",
                     DebitAmount = 0,
                     CreditAmount = payment.AmountPaid
                 }
             };

             var entry = new CreateJournalEntryDto
             {
                 SourceTable = "InvoicePayments",
                 SourceId = payment.Id,
                 Description = $"Payment for Bill #{entity.SupplierInvoice.InvoiceNumber}",
                 EntryDate = payment.PaymentDate,
                 Details = details
             };
             
             // SupplierInvoice does not have LocationId, using Empty
             await ProcessJournalEntryAsync(entry, entity.SupplierInvoice.CompanyId, Guid.Empty, userId);
        }

        private async Task ProcessJournalEntryAsync(CreateJournalEntryDto entry, Guid companyId, Guid locationId, Guid userId)
        {
            await EnsureDefaultAccountsAsync(companyId, userId);
            
            if (locationId == Guid.Empty)
            {
                var defaultLocation = await _context.Locations.FirstOrDefaultAsync(l => l.CompanyId == companyId);
                locationId = defaultLocation?.Id ?? Guid.Empty;
            }

            var journal = new JournalEntry
            {
                CompanyId = companyId,
                TenantId = (await _context.Companies.FindAsync(companyId))?.TenantId ?? Guid.Empty,
                LocationId = locationId,
                EntryDate = entry.EntryDate,
                SourceTable = entry.SourceTable,
                SourceId = entry.SourceId,
                Description = entry.Description,
                IsPosted = true,
                PostedDate = DateTime.UtcNow,
                CreatedBy = userId,
                Details = new List<JournalEntryDetail>()
            };

            var accounts = await _context.GLAccounts.Where(a => a.CompanyId == companyId).ToListAsync();
            var accountMap = accounts.ToDictionary(a => a.AccountNumber, a => a.Id);

            foreach (var d in entry.Details)
            {
                if (!accountMap.ContainsKey(d.AccountNumber)) continue;
                journal.Details.Add(new JournalEntryDetail
                {
                    GLAccountId = accountMap[d.AccountNumber],
                    DebitAmount = d.DebitAmount,
                    CreditAmount = d.CreditAmount
                });
            }

            _context.JournalEntries.Add(journal);
            await _context.SaveChangesAsync();
        }

        public async Task<IncomeStatementDto> GetIncomeStatementAsync(Guid companyId, DateTime startDate, DateTime endDate)
        {
            var report = new IncomeStatementDto
            {
                StartDate = startDate,
                EndDate = endDate
            };

            // Fetch all posted journal details for the period
            var details = await _context.JournalEntryDetails
                .Include(d => d.GLAccount)
                .Include(d => d.JournalEntry)
                .Where(d => d.JournalEntry.CompanyId == companyId && 
                            d.JournalEntry.IsPosted &&
                            d.JournalEntry.EntryDate >= startDate && 
                            d.JournalEntry.EntryDate <= endDate)
                .ToListAsync();

            // Group by GL Account
            var grouped = details.GroupBy(d => d.GLAccount).ToList();

            foreach (var group in grouped)
            {
                var account = group.Key;
                if (account == null) continue;

                // Calculate Net Balance for the period
                // For Revenue/Income (Credit Normal): Cr - Dr
                // For Expense (Debit Normal): Dr - Cr
                
                decimal balance = 0;
                
                // Revenue (4000 series, or Type "Revenue")
                if (account.AccountType == "Revenue")
                {
                    balance = group.Sum(x => x.CreditAmount) - group.Sum(x => x.DebitAmount);
                    if (balance != 0)
                    {
                        report.Revenues.Add(new IncomeStatementItemDto
                        {
                            AccountId = account.Id,
                            AccountNumber = account.AccountNumber,
                            AccountName = account.AccountName,
                            Amount = balance
                        });
                    }
                }
                // COGS (Usually 5000 range specifically)
                else if (account.AccountNumber.StartsWith("50")) 
                {
                    balance = group.Sum(x => x.DebitAmount) - group.Sum(x => x.CreditAmount);
                    if (balance != 0)
                    {
                        report.CostOfGoodsSold.Add(new IncomeStatementItemDto
                        {
                            AccountId = account.Id,
                            AccountNumber = account.AccountNumber,
                            AccountName = account.AccountName,
                            Amount = balance
                        });
                    }
                }
                // Operating Expenses (Other Expenses, e.g. 6000+)
                // Also catch other 'Expense' types that aren't COGS
                else if (account.AccountType == "Expense")
                {
                    balance = group.Sum(x => x.DebitAmount) - group.Sum(x => x.CreditAmount);
                    if (balance != 0)
                    {
                        report.Expenses.Add(new IncomeStatementItemDto
                        {
                            AccountId = account.Id,
                            AccountNumber = account.AccountNumber,
                            AccountName = account.AccountName,
                            Amount = balance
                        });
                    }
                }
            }

            // Summarize
            report.TotalRevenue = report.Revenues.Sum(x => x.Amount);
            report.TotalCOGS = report.CostOfGoodsSold.Sum(x => x.Amount);
            report.TotalExpenses = report.Expenses.Sum(x => x.Amount);

            return report;
        }

        public async Task<BalanceSheetDto> GetBalanceSheetAsync(Guid companyId, DateTime asOfDate)
        {
            var report = new BalanceSheetDto
            {
                AsOfDate = asOfDate
            };

            // Fetch all posted journal details up to the AsOfDate
            var details = await _context.JournalEntryDetails
                .Include(d => d.GLAccount)
                .Include(d => d.JournalEntry)
                .Where(d => d.JournalEntry.CompanyId == companyId && 
                            d.JournalEntry.IsPosted &&
                            d.JournalEntry.EntryDate <= asOfDate)
                .ToListAsync();

            var grouped = details.GroupBy(d => d.GLAccount).ToList();

            // Variables for Retained Earnings Calculation (Revenue - Expense)
            decimal retainedEarnings = 0;

            foreach (var group in grouped)
            {
                var account = group.Key;
                if (account == null) continue;

                decimal balance = 0;

                // Assets (Debit Normal): Dr - Cr
                if (account.AccountType == "Asset" || account.AccountNumber.StartsWith("1"))
                {
                    balance = group.Sum(x => x.DebitAmount) - group.Sum(x => x.CreditAmount);
                    if (balance != 0)
                    {
                        report.Assets.Add(new IncomeStatementItemDto
                        {
                            AccountId = account.Id,
                            AccountNumber = account.AccountNumber,
                            AccountName = account.AccountName,
                            Amount = balance
                        });
                    }
                }
                // Liabilities (Credit Normal): Cr - Dr
                else if (account.AccountType == "Liability" || account.AccountNumber.StartsWith("2"))
                {
                    balance = group.Sum(x => x.CreditAmount) - group.Sum(x => x.DebitAmount);
                    if (balance != 0)
                    {
                        report.Liabilities.Add(new IncomeStatementItemDto
                        {
                            AccountId = account.Id,
                            AccountNumber = account.AccountNumber,
                            AccountName = account.AccountName,
                            Amount = balance
                        });
                    }
                }
                // Equity (Credit Normal): Cr - Dr
                else if (account.AccountType == "Equity" || account.AccountNumber.StartsWith("3"))
                {
                    balance = group.Sum(x => x.CreditAmount) - group.Sum(x => x.DebitAmount);
                    if (balance != 0)
                    {
                        report.Equity.Add(new IncomeStatementItemDto
                        {
                            AccountId = account.Id,
                            AccountNumber = account.AccountNumber,
                            AccountName = account.AccountName,
                            Amount = balance
                        });
                    }
                }
                // Revenue & Expense (Calculated into Retained Earnings)
                else
                {
                    // Revenue (Credit Normal) increases Equity
                    if (account.AccountType == "Revenue" || account.AccountNumber.StartsWith("4"))
                    {
                        retainedEarnings += (group.Sum(x => x.CreditAmount) - group.Sum(x => x.DebitAmount));
                    }
                    // Expense (Debit Normal) decreases Equity
                    else if (account.AccountType == "Expense" || account.AccountNumber.StartsWith("5") || account.AccountNumber.StartsWith("6"))
                    {
                        retainedEarnings -= (group.Sum(x => x.DebitAmount) - group.Sum(x => x.CreditAmount));
                    }
                }
            }

            // Add Retained Earnings to Equity
            if (retainedEarnings != 0)
            {
                report.Equity.Add(new IncomeStatementItemDto
                {
                    AccountId = Guid.Empty, // Virtual
                    AccountNumber = "3999",
                    AccountName = "Net Income (Retained Earnings)",
                    Amount = retainedEarnings
                });
            }

            report.TotalAssets = report.Assets.Sum(x => x.Amount);
            report.TotalLiabilities = report.Liabilities.Sum(x => x.Amount);
            report.TotalEquity = report.Equity.Sum(x => x.Amount);

            return report;
        }

        public async Task<TrialBalanceDto> GetTrialBalanceAsync(Guid companyId, DateTime asOfDate)
        {
            var report = new TrialBalanceDto
            {
                AsOfDate = asOfDate
            };

            // Fetch balances
             var details = await _context.JournalEntryDetails
                .Include(d => d.GLAccount)
                .Include(d => d.JournalEntry)
                .Where(d => d.JournalEntry.CompanyId == companyId && 
                            d.JournalEntry.IsPosted &&
                            d.JournalEntry.EntryDate <= asOfDate)
                .ToListAsync();

             var grouped = details.GroupBy(d => d.GLAccount).OrderBy(g => g.Key?.AccountNumber).ToList();

             foreach (var group in grouped)
             {
                 var account = group.Key;
                 if (account == null) continue;

                 var totalDebit = group.Sum(x => x.DebitAmount);
                 var totalCredit = group.Sum(x => x.CreditAmount);
                 
                 // Netting? 
                 // Trial Balance usually shows net debit OR net credit per account, 
                 // OR it shows total debit activity and total credit activity.
                 // Standard format: Account | Debit | Credit (Net Balance)
                 
                 decimal netDebit = 0;
                 decimal netCredit = 0;

                 if (totalDebit >= totalCredit)
                 {
                     netDebit = totalDebit - totalCredit;
                 }
                 else
                 {
                     netCredit = totalCredit - totalDebit;
                 }

                 if (netDebit == 0 && netCredit == 0) continue;

                 report.Accounts.Add(new TrialBalanceItemDto
                 {
                     AccountNumber = account.AccountNumber,
                     AccountName = account.AccountName,
                     Debit = netDebit,
                     Credit = netCredit
                 });
             }

             report.TotalDebit = report.Accounts.Sum(x => x.Debit);
             report.TotalCredit = report.Accounts.Sum(x => x.Credit);

             return report;
        }
    }
}
