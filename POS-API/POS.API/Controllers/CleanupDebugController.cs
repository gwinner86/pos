using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POS.Infrastructure.Persistence;

namespace POS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CleanupDebugController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CleanupDebugController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpDelete("inventory")]
        public async Task<IActionResult> DeleteInventoryData()
        {
            try
            {
                // Delete in reverse order of dependencies
                
                // 1. Inventory Transactions and Inventory items (referencing Variants)
                await _context.Database.ExecuteSqlRawAsync("DELETE FROM InventoryTransactions");
                await _context.Database.ExecuteSqlRawAsync("DELETE FROM Inventory");

                // 2. Costs, Pricings, BOMs etc (omitted if not used yet, but checking typical dependencies)
                await _context.Database.ExecuteSqlRawAsync("DELETE FROM Costs");
                await _context.Database.ExecuteSqlRawAsync("DELETE FROM Pricing");
                await _context.Database.ExecuteSqlRawAsync("DELETE FROM PurchaseOrderDetails"); // References Variants
                 // Note: If POs exist, we might need to delete headers too, but let's stick to Product/Variant references first.
                 // Ideally user should empty POs too but they asked for Products/Variants.
                 // If FKs prevent it, it will fail. Let's try to clear mostly product specific tables.
                 
                // 3. Variant dependencies (SaleDetails, etc?)
                // Assuming no sales yet as it's dev.

                // 4. Product Variants
                await _context.Database.ExecuteSqlRawAsync("DELETE FROM ProductVariants");

                // 5. Product Audits
                await _context.Database.ExecuteSqlRawAsync("DELETE FROM ProductAudits");

                // 6. Products
                await _context.Database.ExecuteSqlRawAsync("DELETE FROM Products");

                // 7. Categories
                await _context.Database.ExecuteSqlRawAsync("DELETE FROM Categories");
                
                return Ok(new { message = "Inventory data (Products, Variants, Categories, Inventory) cleaned up." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message, details = ex.InnerException?.Message });
            }
        }
    }
}
