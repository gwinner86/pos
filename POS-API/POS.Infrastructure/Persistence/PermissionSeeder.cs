using Microsoft.EntityFrameworkCore;
using POS.Domain.Entities;

namespace POS.Infrastructure.Persistence
{
    public static class PermissionSeeder
    {
        public static async Task SeedPermissionsAsync(ApplicationDbContext context)
        {
            var existingPermissions = await context.Permissions.Select(p => p.Name).ToListAsync();

            var permissions = new List<Permission>
            {
                // User Management
                new Permission { Name = "Users.View", Description = "Can view users", Group = "Users" },
                new Permission { Name = "Users.Create", Description = "Can create users", Group = "Users" },
                new Permission { Name = "Users.Edit", Description = "Can edit users", Group = "Users" },
                new Permission { Name = "Users.Delete", Description = "Can delete users", Group = "Users" },

                // Role Management
                new Permission { Name = "Roles.View", Description = "Can view roles", Group = "Roles" },
                new Permission { Name = "Roles.Manage", Description = "Can create, edit, delete roles", Group = "Roles" },
                new Permission { Name = "Permissions.Manage", Description = "Can assign permissions to roles", Group = "Roles" },

                // Inventory
                new Permission { Name = "Inventory.View", Description = "Can view inventory", Group = "Inventory" },
                new Permission { Name = "Inventory.Adjust", Description = "Can adjust inventory", Group = "Inventory" },

                // Sales
                new Permission { Name = "Sales.View", Description = "Can view sales", Group = "Sales" },
                new Permission { Name = "POS.Access", Description = "Can access Point of Sale", Group = "Sales" },
                
                // Reports
                new Permission { Name = "Reports.View", Description = "Can view various reports", Group = "Reports" },

                // ============================
                // Menu Restrictions
                // ============================
                new Permission { Name = "Menu.Dashboard", Group = "Dashboard", Description = "Access to the main dashboard" },
                new Permission { Name = "Menu.POS", Group = "Point Of Sale", Description = "Access to the POS screen" },
                new Permission { Name = "Menu.SalesHistory", Group = "Sales", Description = "Access to Sales History" },

                new Permission { Name = "Menu.Inventory.View", Group = "Inventory Menu", Description = "View the Inventory menu section" },
                new Permission { Name = "Menu.Inventory.Products", Group = "Inventory Menu", Description = "Access Products" },
                new Permission { Name = "Menu.Inventory.Categories", Group = "Inventory Menu", Description = "Access Categories" },
                new Permission { Name = "Menu.Inventory.Adjustments", Group = "Inventory Menu", Description = "Access Inventory Adjustments" },

                new Permission { Name = "Menu.Procurement.View", Group = "Procurement Menu", Description = "View the Procurement menu section" },
                new Permission { Name = "Menu.Procurement.Suppliers", Group = "Procurement Menu", Description = "Access Suppliers" },
                new Permission { Name = "Menu.Procurement.PurchaseOrders", Group = "Procurement Menu", Description = "Access Purchase Orders" },
                new Permission { Name = "Menu.Procurement.InstantPurchases", Group = "Procurement Menu", Description = "Access Instant Purchases" },
                new Permission { Name = "Menu.Procurement.GoodsReceipts", Group = "Procurement Menu", Description = "Access Goods Receipts" },
                new Permission { Name = "Menu.Procurement.VendorInvoices", Group = "Procurement Menu", Description = "Access Vendor Invoices" },
                new Permission { Name = "Menu.Procurement.VendorPayments", Group = "Procurement Menu", Description = "Access Vendor Payments" },

                new Permission { Name = "Menu.Customers", Group = "Customers Menu", Description = "Access Customers" },

                new Permission { Name = "Menu.Accounting.View", Group = "Accounting Menu", Description = "View the Accounting menu section" },
                new Permission { Name = "Menu.Accounting.Financials", Group = "Accounting Menu", Description = "Access Financials" },
                new Permission { Name = "Menu.Accounting.Transactions", Group = "Accounting Menu", Description = "Access Journal Transactions" },
                new Permission { Name = "Menu.Accounting.Expenses", Group = "Accounting Menu", Description = "Access Expenses" },
                new Permission { Name = "Menu.Accounting.IncomeStatement", Group = "Accounting Menu", Description = "Access Income Statement" },
                new Permission { Name = "Menu.Accounting.BalanceSheet", Group = "Accounting Menu", Description = "Access Balance Sheet" },
                new Permission { Name = "Menu.Accounting.TrialBalance", Group = "Accounting Menu", Description = "Access Trial Balance" },

                new Permission { Name = "Menu.Reports.View", Group = "Reports Menu", Description = "View the Reports menu section" },
                new Permission { Name = "Menu.Reports.UserSales", Group = "Reports Menu", Description = "Access User Transactions Report" },

                new Permission { Name = "Menu.Settings", Group = "Settings Menu", Description = "Access Settings menu" }
            };

            var newPermissions = permissions.Where(p => !existingPermissions.Contains(p.Name)).ToList();

            if (newPermissions.Any())
            {
                await context.Permissions.AddRangeAsync(newPermissions);
                await context.SaveChangesAsync();
            }

            // Assign newly added menu permissions to existing Admin roles
            await AssignPermissionsToAdminsAsync(context);
        }

        private static async Task AssignPermissionsToAdminsAsync(ApplicationDbContext context)
        {
            var adminRoles = await context.Roles
                .Where(r => r.RoleName == "Admin" || r.RoleName == "SuperAdmin")
                .Include(r => r.RolePermissions)
                .ToListAsync();

            var allPermissions = await context.Permissions.ToListAsync();

            foreach (var role in adminRoles)
            {
                var currentPermissionIds = role.RolePermissions.Select(rp => rp.PermissionId).ToList();
                var missingPermissions = allPermissions
                    .Where(p => !currentPermissionIds.Contains(p.PermissionId))
                    .Select(p => new RolePermission 
                    { 
                        RoleId = role.RoleId, 
                        PermissionId = p.PermissionId 
                    })
                    .ToList();

                if (missingPermissions.Any())
                {
                    await context.RolePermissions.AddRangeAsync(missingPermissions);
                }
            }
            
            await context.SaveChangesAsync();
        }
    }
}
