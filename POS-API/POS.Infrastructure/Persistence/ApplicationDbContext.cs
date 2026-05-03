using Microsoft.EntityFrameworkCore;
using POS.Domain.Common;
using POS.Domain.Entities;

namespace POS.Infrastructure.Persistence
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Feature> Features { get; set; }
        public DbSet<FeatureDetail> FeatureDetails { get; set; }
        public DbSet<TenantFeatureAssignment> TenantFeatureAssignments { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserCompanyAssignment> UserCompanyAssignments { get; set; }
        public DbSet<Expense> Expenses { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        
        // Inventory & Products
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductVariant> ProductVariants { get; set; }
        public DbSet<ProductAudit> ProductAudits { get; set; }
        public DbSet<Inventory> Inventory { get; set; }
        public DbSet<InventoryTransaction> InventoryTransactions { get; set; }
        public DbSet<Pricing> Pricing { get; set; }
        public DbSet<Cost> Costs { get; set; }

        // Sales
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Sale> Sales { get; set; }
        public DbSet<SaleDetail> SaleDetails { get; set; }
        public DbSet<SalePayment> SalePayments { get; set; }
        public DbSet<PaymentMethod> PaymentMethods { get; set; }

        // Procurement
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<PurchaseOrder> PurchaseOrders { get; set; }
        public DbSet<PurchaseOrderDetail> PurchaseOrderDetails { get; set; }
        public DbSet<GoodsReceipt> GoodsReceipts { get; set; }
        public DbSet<GoodsReceiptDetail> GoodsReceiptDetails { get; set; }
        public DbSet<SupplierInvoice> SupplierInvoices { get; set; }
        public DbSet<InvoicePayment> InvoicePayments { get; set; }

        // Returns
        public DbSet<GoodsReturn> GoodsReturns { get; set; }
        public DbSet<GoodsReturnedDetail> GoodsReturnedDetails { get; set; }

        // Accounting
        public DbSet<GLAccount> GLAccounts { get; set; }
        public DbSet<JournalEntry> JournalEntries { get; set; }
        public DbSet<JournalEntryDetail> JournalEntryDetails { get; set; }
        public DbSet<Currency> Currencies { get; set; }

        // Settings
        public DbSet<SetupVAT> SetupVATs { get; set; }
        public DbSet<SetupCountry> SetupCountries { get; set; }
        public DbSet<SetupExpenseType> SetupExpenseTypes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Unique Constraints & Indexes
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<Role>()
                .HasIndex(r => new { r.TenantId, r.RoleName }) 
                .IsUnique();
            
            modelBuilder.Entity<Company>()
                .HasIndex(c => c.TenantId);

            modelBuilder.Entity<UserCompanyAssignment>()
                .HasIndex(uca => new { uca.UserId, uca.CompanyId, uca.RoleId })
                .IsUnique();

            // Base Entity Configuration
            modelBuilder.Entity<Tenant>().Property(t => t.Id).HasColumnName("TenantId").HasDefaultValueSql("NEWSEQUENTIALID()");
            modelBuilder.Entity<Company>().Property(c => c.Id).HasColumnName("CompanyId");
            modelBuilder.Entity<User>().Property(u => u.Id).HasColumnName("UserId");
            modelBuilder.Entity<Location>().Property(l => l.Id).HasColumnName("LocationId");
            
            // Inventory Config
            modelBuilder.Entity<Category>().Property(c => c.Id).HasColumnName("CategoryId");
            modelBuilder.Entity<Product>().Property(p => p.Id).HasColumnName("ProductId");
            modelBuilder.Entity<ProductVariant>().Property(pv => pv.Id).HasColumnName("ProductVariantId");
            modelBuilder.Entity<Inventory>().Property(i => i.Id).HasColumnName("InventoryId");
            modelBuilder.Entity<Supplier>().Property(s => s.Id).HasColumnName("SupplierId");
            
            // Sales/Procurement Config
            modelBuilder.Entity<PurchaseOrder>().Property(p => p.Id).HasColumnName("PurchaseOrderId");
            modelBuilder.Entity<PurchaseOrderDetail>().Property(p => p.Id).HasColumnName("PurchaseOrderItemId");
            modelBuilder.Entity<GoodsReceipt>().Property(g => g.Id).HasColumnName("GoodsReceiptId");
            modelBuilder.Entity<GoodsReceiptDetail>().Property(g => g.Id).HasColumnName("GoodsReceiptItemId");
            modelBuilder.Entity<SupplierInvoice>().Property(s => s.Id).HasColumnName("SupplierInvoiceId");
            modelBuilder.Entity<InvoicePayment>().Property(i => i.Id).HasColumnName("PaymentId");
            
            modelBuilder.Entity<Customer>().Property(c => c.Id).HasColumnName("CustomerId");
            modelBuilder.Entity<Sale>().Property(s => s.Id).HasColumnName("SaleHeaderId");
            modelBuilder.Entity<SaleDetail>().Property(s => s.Id).HasColumnName("SaleItemId");

            modelBuilder.Entity<Sale>().HasOne(s => s.Location).WithMany().HasForeignKey(s => s.LocationId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Sale>().HasOne(s => s.Customer).WithMany().HasForeignKey(s => s.CustomerId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SaleDetail>().HasOne(sd => sd.ProductVariant).WithMany().HasForeignKey(sd => sd.ProductVariantId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<SaleDetail>().HasOne(sd => sd.Product).WithMany().HasForeignKey(sd => sd.ProductId).OnDelete(DeleteBehavior.Restrict);
            
            // Purchase Order
            modelBuilder.Entity<PurchaseOrder>().HasOne(p => p.Location).WithMany().HasForeignKey(p => p.LocationId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<PurchaseOrder>().HasOne(p => p.Supplier).WithMany().HasForeignKey(p => p.SupplierId).OnDelete(DeleteBehavior.Restrict);

            // Goods Receipt
            modelBuilder.Entity<GoodsReceipt>().HasOne(g => g.Location).WithMany().HasForeignKey(g => g.LocationId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<GoodsReceipt>().HasOne(g => g.Supplier).WithMany().HasForeignKey(g => g.SupplierId).OnDelete(DeleteBehavior.Restrict);

            // Journal Entry
            modelBuilder.Entity<JournalEntry>().HasOne(j => j.Location).WithMany().HasForeignKey(j => j.LocationId).OnDelete(DeleteBehavior.Restrict);

            // Prevent Cycles in Details and Payments
            modelBuilder.Entity<SalePayment>().HasOne(sp => sp.PaymentMethod).WithMany().HasForeignKey(sp => sp.PaymentMethodId).OnDelete(DeleteBehavior.Restrict);

            // Expense setup
            modelBuilder.Entity<Expense>().HasOne(e => e.ExpenseType).WithMany().HasForeignKey(e => e.ExpenseTypeId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<SetupExpenseType>().HasOne(e => e.ExpenseAccount).WithMany().HasForeignKey(e => e.ExpenseAccountId).OnDelete(DeleteBehavior.Restrict);
            
            modelBuilder.Entity<PurchaseOrderDetail>().HasOne(pod => pod.Product).WithMany().HasForeignKey(pod => pod.ProductId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<PurchaseOrderDetail>().HasOne(pod => pod.ProductVariant).WithMany().HasForeignKey(pod => pod.ProductVariantId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<GoodsReceiptDetail>().HasOne(grd => grd.Product).WithMany().HasForeignKey(grd => grd.ProductId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<GoodsReceiptDetail>().HasOne(grd => grd.ProductVariant).WithMany().HasForeignKey(grd => grd.ProductVariantId).OnDelete(DeleteBehavior.Restrict);
            
            modelBuilder.Entity<SalePayment>().Property(s => s.Id).HasColumnName("SalePaymentId");
            modelBuilder.Entity<GoodsReturn>().Property(g => g.Id).HasColumnName("ReturnId");
            modelBuilder.Entity<GoodsReturnedDetail>().Property(g => g.Id).HasColumnName("ReturnItemId");
            
            // Accounting Config
            modelBuilder.Entity<GLAccount>().Property(g => g.Id).HasColumnName("GLAccountId");
            modelBuilder.Entity<JournalEntry>().Property(j => j.Id).HasColumnName("JournalHeaderId");
            modelBuilder.Entity<JournalEntryDetail>().Property(j => j.Id).HasColumnName("JournalLineId");
            modelBuilder.Entity<Cost>().Property(c => c.Id).HasColumnName("CostId");
            modelBuilder.Entity<Pricing>().Property(p => p.Id).HasColumnName("PricingId");

            // Unique Constraints
            modelBuilder.Entity<Product>().HasIndex(p => new { p.CompanyId, p.ProductSkuBase }).IsUnique();
            modelBuilder.Entity<ProductVariant>().HasIndex(pv => new { pv.ProductId, pv.VariantSku }).IsUnique();
            modelBuilder.Entity<Inventory>().HasIndex(i => new { i.ProductVariantId, i.LocationId }).IsUnique();
            modelBuilder.Entity<Supplier>().HasIndex(s => new { s.CompanyId, s.SupplierName }).IsUnique();
            modelBuilder.Entity<Customer>().HasIndex(c => new { c.CompanyId, c.CustomerCode }).IsUnique();
            modelBuilder.Entity<Customer>().HasIndex(c => new { c.CompanyId, c.CustomerCode }).IsUnique();
            modelBuilder.Entity<GLAccount>().HasIndex(g => new { g.CompanyId, g.AccountNumber }).IsUnique();
            modelBuilder.Entity<Currency>().HasIndex(c => new { c.CompanyId, c.CurrencyCode }).IsUnique();

            // Prevent Multiple Cascade Paths (SQL Server Cycles)
            // GoodsReturn
            modelBuilder.Entity<GoodsReturn>().HasOne(g => g.Location).WithMany().HasForeignKey(g => g.LocationId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<GoodsReturn>().HasOne(g => g.User).WithMany().HasForeignKey(g => g.UserId).OnDelete(DeleteBehavior.Restrict);
            
            // Sales — restrict cascade on Creator to prevent cycle
            modelBuilder.Entity<Sale>().HasOne(s => s.Creator).WithMany().HasForeignKey(s => s.CreatedBy).OnDelete(DeleteBehavior.Restrict);

            // Prevent Cycles in Details and Payments
            
            modelBuilder.Entity<GoodsReturnedDetail>().HasOne(grd => grd.ProductVariant).WithMany().HasForeignKey(grd => grd.ProductVariantId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<GoodsReturnedDetail>().HasOne(grd => grd.SaleItem).WithMany().HasForeignKey(grd => grd.SaleItemId).OnDelete(DeleteBehavior.Restrict);

            // Inventory Cycles (Company -> Location -> Inventory AND Company -> Product -> Inventory)
            modelBuilder.Entity<Inventory>().HasOne(i => i.Location).WithMany().HasForeignKey(i => i.LocationId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Inventory>().HasOne(i => i.Product).WithMany().HasForeignKey(i => i.ProductId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Inventory>().HasOne(i => i.ProductVariant).WithMany(pv => pv.InventoryItems).HasForeignKey(i => i.ProductVariantId).OnDelete(DeleteBehavior.Restrict);
            
            // Cost & Pricing
            modelBuilder.Entity<Cost>().HasOne(c => c.Location).WithMany().HasForeignKey(c => c.LocationId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Cost>().HasOne(c => c.Supplier).WithMany().HasForeignKey(c => c.SupplierId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Cost>().HasOne(c => c.ProductVariant).WithMany(pv => pv.Costs).HasForeignKey(c => c.ProductVariantId).OnDelete(DeleteBehavior.Restrict);
            
            modelBuilder.Entity<Pricing>().HasOne(p => p.Location).WithMany().HasForeignKey(p => p.LocationId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Pricing>().HasOne(p => p.ProductVariant).WithMany(pv => pv.Pricings).HasForeignKey(p => p.ProductVariantId).OnDelete(DeleteBehavior.Restrict);

            // Supplier Invoice & Payments
            modelBuilder.Entity<SupplierInvoice>().HasOne(si => si.Supplier).WithMany().HasForeignKey(si => si.SupplierId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<InvoicePayment>().HasOne(ip => ip.SupplierInvoice).WithMany().HasForeignKey(ip => ip.SupplierInvoiceId).OnDelete(DeleteBehavior.Restrict);

            // ===================================
            // CYCLE FIX: TENANT VS COMPANY PATHS
            // ===================================
            // Since Tenant Deletes Company (Cascade), any entity with BOTH TenantId and CompanyId 
            // will have two delete paths (Tenant -> Entity AND Tenant -> Company -> Entity).
            // We must set the direct Tenant -> Entity path to RESTRICT for all such entities.

            modelBuilder.Entity<Location>().HasOne(l => l.Tenant).WithMany().HasForeignKey(l => l.TenantId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<SetupVAT>().HasOne(s => s.Tenant).WithMany().HasForeignKey(s => s.TenantId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<SetupCountry>().HasOne(s => s.Tenant).WithMany().HasForeignKey(s => s.TenantId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<SetupCountry>().HasOne(s => s.Company).WithMany().HasForeignKey(s => s.CompanyId).OnDelete(DeleteBehavior.Restrict);
            
            // UserCompanyAssignment Cycle (Tenant -> User -> Assignment AND Tenant -> Company -> Assignment)
            modelBuilder.Entity<UserCompanyAssignment>().HasOne(uca => uca.Company).WithMany(c => c.UserAssignments).HasForeignKey(uca => uca.CompanyId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<UserCompanyAssignment>().HasOne(uca => uca.User).WithMany(u => u.Assignments).HasForeignKey(uca => uca.UserId).OnDelete(DeleteBehavior.Restrict);
            
            // TenantFeatureAssignment (Tenant -> Assignment AND Tenant -> Company -> Assignment)
            modelBuilder.Entity<TenantFeatureAssignment>().HasOne(tfa => tfa.Company).WithMany().HasForeignKey(tfa => tfa.CompanyId).OnDelete(DeleteBehavior.Restrict);

            // Inventory / Catalog
            modelBuilder.Entity<Category>().HasOne<Tenant>().WithMany().HasForeignKey(c => c.TenantId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Product>().HasOne<Tenant>().WithMany().HasForeignKey(p => p.TenantId).OnDelete(DeleteBehavior.Restrict);
            
            // Procurement
            modelBuilder.Entity<Supplier>().HasOne<Tenant>().WithMany().HasForeignKey(s => s.TenantId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<PurchaseOrder>().HasOne<Tenant>().WithMany().HasForeignKey(p => p.TenantId).OnDelete(DeleteBehavior.Restrict);
            
            // Journal
            modelBuilder.Entity<JournalEntry>().HasOne<Tenant>().WithMany().HasForeignKey(j => j.TenantId).OnDelete(DeleteBehavior.Restrict);

            // Precision Configuration for Decimals
            var decimalProps = modelBuilder.Model.GetEntityTypes()
                .SelectMany(t => t.GetProperties())
                .Where(p => (System.Nullable.GetUnderlyingType(p.ClrType) ?? p.ClrType) == typeof(decimal));

            foreach (var property in decimalProps)
            {
                property.SetPrecision(18);
                property.SetScale(4); // Defaulting to 4 for high precision needed in Inventory/Qty
            }

            // Relationship Configurations as per SQL
            modelBuilder.Entity<UserCompanyAssignment>()
                .HasOne(uca => uca.User)
                .WithMany(u => u.Assignments)
                .HasForeignKey(uca => uca.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TenantFeatureAssignment>()
                .HasOne(tfa => tfa.Tenant)
                .WithMany(t => t.FeatureAssignments)
                .HasForeignKey(tfa => tfa.TenantId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<FeatureDetail>()
                .HasOne(fd => fd.Feature)
                .WithMany(f => f.FeatureDetails)
                .HasForeignKey(fd => fd.FeatureId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // Permission Configuration
            modelBuilder.Entity<Permission>()
                .HasIndex(p => p.Name)
                .IsUnique();

            modelBuilder.Entity<RolePermission>()
                .HasKey(rp => new { rp.RoleId, rp.PermissionId });

            modelBuilder.Entity<RolePermission>()
                .HasOne(rp => rp.Role)
                .WithMany(r => r.RolePermissions)
                .HasForeignKey(rp => rp.RoleId);

            modelBuilder.Entity<RolePermission>()
                .HasOne(rp => rp.Permission)
                .WithMany()
                .HasForeignKey(rp => rp.PermissionId);

            // Product Audit Configuration (Optional specifics)
            modelBuilder.Entity<ProductAudit>()
                .HasIndex(pa => pa.ProductId);

            modelBuilder.Entity<InventoryTransaction>()
                .HasIndex(it => it.InventoryId);
            
            // Expense Configuration
            modelBuilder.Entity<Expense>(entity =>
            {
                entity.HasOne(e => e.ExpenseType)
                      .WithMany()
                      .HasForeignKey(e => e.ExpenseTypeId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.PaymentAccount)
                      .WithMany()
                      .HasForeignKey(e => e.PaymentAccountId)
                      .OnDelete(DeleteBehavior.Restrict);
                      
                entity.HasOne<Tenant>()
                      .WithMany()
                      .HasForeignKey(e => e.TenantId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Apply configurations from assembly
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        }
    }
}
