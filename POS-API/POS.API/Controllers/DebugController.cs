using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POS.Infrastructure.Persistence;

namespace POS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DebugController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DebugController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("reset-transactions")]
        public async Task<IActionResult> ResetTransactions()
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Sales
                await _context.Database.ExecuteSqlRawAsync("DELETE FROM SalePayments");
                await _context.Database.ExecuteSqlRawAsync("DELETE FROM SaleDetails");
                await _context.Database.ExecuteSqlRawAsync("DELETE FROM Sales");

                // Returns
                await _context.Database.ExecuteSqlRawAsync("DELETE FROM GoodsReturnedDetails");
                await _context.Database.ExecuteSqlRawAsync("DELETE FROM GoodsReturns");

                // Purchases & Receipts
                await _context.Database.ExecuteSqlRawAsync("DELETE FROM InvoicePayments");
                await _context.Database.ExecuteSqlRawAsync("DELETE FROM SupplierInvoices");
                await _context.Database.ExecuteSqlRawAsync("DELETE FROM GoodsReceiptDetails");
                await _context.Database.ExecuteSqlRawAsync("DELETE FROM GoodsReceipts");
                await _context.Database.ExecuteSqlRawAsync("DELETE FROM PurchaseOrderDetails");
                await _context.Database.ExecuteSqlRawAsync("DELETE FROM PurchaseOrders");

                // Expenses
                await _context.Database.ExecuteSqlRawAsync("DELETE FROM Expenses");

                // Inventory Transactions
                await _context.Database.ExecuteSqlRawAsync("DELETE FROM InventoryTransactions");

                // Journals
                await _context.Database.ExecuteSqlRawAsync("DELETE FROM JournalEntryDetails");
                await _context.Database.ExecuteSqlRawAsync("DELETE FROM JournalEntries");

                // Reset Inventory statements
                await _context.Database.ExecuteSqlRawAsync("UPDATE Inventory SET Quantity = 0, InitialQuantity = 0, TotalQuantitySold = 0, TotalQuantityReturned = 0");

                // Reset Customer Loyola Points (just to be safe)
                await _context.Database.ExecuteSqlRawAsync("UPDATE Customers SET LoyaltyPoints = 0");

                await transaction.CommitAsync();

                return Ok(new { message = "All transaction tables cleared and statements reset to 0." });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { message = ex.Message, details = ex.InnerException?.Message });
            }
        }
    }
}
