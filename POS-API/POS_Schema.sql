IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251217224836_InitialCreate', N'9.0.0');

CREATE TABLE [Features] (
    [FeatureId] int NOT NULL IDENTITY,
    [FeatureName] nvarchar(max) NOT NULL,
    [Description] nvarchar(max) NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_Features] PRIMARY KEY ([FeatureId])
);

CREATE TABLE [Tenants] (
    [TenantId] uniqueidentifier NOT NULL DEFAULT (NEWSEQUENTIALID()),
    [TenantName] nvarchar(max) NOT NULL,
    [FeatureId] int NOT NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_Tenants] PRIMARY KEY ([TenantId])
);

CREATE TABLE [Companies] (
    [CompanyId] uniqueidentifier NOT NULL,
    [TenantId] uniqueidentifier NOT NULL,
    [FeatureId] int NULL,
    [CompanyName] nvarchar(max) NOT NULL,
    [CompanyAddress] nvarchar(max) NULL,
    [CompanyPrimaryPhoneNumber] nvarchar(max) NULL,
    [CompanyOtherPhoneNumbers] nvarchar(max) NULL,
    [CompanyEmail] nvarchar(max) NULL,
    [TaxId] nvarchar(max) NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_Companies] PRIMARY KEY ([CompanyId]),
    CONSTRAINT [FK_Companies_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [Tenants] ([TenantId]) ON DELETE CASCADE
);

CREATE TABLE [FeatureDetails] (
    [FeatureDetailId] int NOT NULL IDENTITY,
    [FeatureId] int NOT NULL,
    [TenantId] uniqueidentifier NOT NULL,
    [SpecificFeature] nvarchar(max) NOT NULL,
    [SpecificFeatureValue] nvarchar(max) NOT NULL,
    [IsEnabled] bit NOT NULL,
    CONSTRAINT [PK_FeatureDetails] PRIMARY KEY ([FeatureDetailId]),
    CONSTRAINT [FK_FeatureDetails_Features_FeatureId] FOREIGN KEY ([FeatureId]) REFERENCES [Features] ([FeatureId]) ON DELETE CASCADE,
    CONSTRAINT [FK_FeatureDetails_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [Tenants] ([TenantId]) ON DELETE CASCADE
);

CREATE TABLE [Users] (
    [UserId] uniqueidentifier NOT NULL,
    [Email] nvarchar(450) NOT NULL,
    [PasswordHash] nvarchar(max) NOT NULL,
    [FirstName] nvarchar(max) NULL,
    [LastName] nvarchar(max) NULL,
    [TenantId] uniqueidentifier NOT NULL,
    [IsActive] bit NOT NULL,
    [CreatedBy] uniqueidentifier NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_Users] PRIMARY KEY ([UserId]),
    CONSTRAINT [FK_Users_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [Tenants] ([TenantId]) ON DELETE CASCADE
);

CREATE TABLE [Locations] (
    [LocationId] uniqueidentifier NOT NULL,
    [TenantId] uniqueidentifier NOT NULL,
    [CompanyId] uniqueidentifier NULL,
    [Name] nvarchar(max) NOT NULL,
    [Address] nvarchar(max) NULL,
    [PhoneNumber] nvarchar(max) NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_Locations] PRIMARY KEY ([LocationId]),
    CONSTRAINT [FK_Locations_Companies_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [Companies] ([CompanyId]),
    CONSTRAINT [FK_Locations_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [Tenants] ([TenantId]) ON DELETE CASCADE
);

CREATE TABLE [Roles] (
    [RoleId] int NOT NULL IDENTITY,
    [RoleName] nvarchar(450) NOT NULL,
    [TenantId] uniqueidentifier NULL,
    [CompanyId] uniqueidentifier NULL,
    [Description] nvarchar(max) NULL,
    CONSTRAINT [PK_Roles] PRIMARY KEY ([RoleId]),
    CONSTRAINT [FK_Roles_Companies_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [Companies] ([CompanyId]),
    CONSTRAINT [FK_Roles_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [Tenants] ([TenantId])
);

CREATE TABLE [TenantFeatureAssignments] (
    [AssignmentId] int NOT NULL IDENTITY,
    [TenantId] uniqueidentifier NOT NULL,
    [CompanyId] uniqueidentifier NULL,
    [LocationId] uniqueidentifier NULL,
    [FeatureId] int NOT NULL,
    [IsEnabled] bit NOT NULL,
    CONSTRAINT [PK_TenantFeatureAssignments] PRIMARY KEY ([AssignmentId]),
    CONSTRAINT [FK_TenantFeatureAssignments_Companies_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [Companies] ([CompanyId]),
    CONSTRAINT [FK_TenantFeatureAssignments_Features_FeatureId] FOREIGN KEY ([FeatureId]) REFERENCES [Features] ([FeatureId]) ON DELETE CASCADE,
    CONSTRAINT [FK_TenantFeatureAssignments_Locations_LocationId] FOREIGN KEY ([LocationId]) REFERENCES [Locations] ([LocationId]),
    CONSTRAINT [FK_TenantFeatureAssignments_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [Tenants] ([TenantId]) ON DELETE CASCADE
);

CREATE TABLE [UserCompanyAssignments] (
    [Id] uniqueidentifier NOT NULL,
    [UserId] uniqueidentifier NOT NULL,
    [CompanyId] uniqueidentifier NOT NULL,
    [RoleId] int NOT NULL,
    [LocationId] uniqueidentifier NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_UserCompanyAssignments] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_UserCompanyAssignments_Companies_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [Companies] ([CompanyId]) ON DELETE CASCADE,
    CONSTRAINT [FK_UserCompanyAssignments_Locations_LocationId] FOREIGN KEY ([LocationId]) REFERENCES [Locations] ([LocationId]),
    CONSTRAINT [FK_UserCompanyAssignments_Roles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [Roles] ([RoleId]) ON DELETE CASCADE,
    CONSTRAINT [FK_UserCompanyAssignments_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId]) ON DELETE NO ACTION
);

CREATE INDEX [IX_Companies_TenantId] ON [Companies] ([TenantId]);

CREATE INDEX [IX_FeatureDetails_FeatureId] ON [FeatureDetails] ([FeatureId]);

CREATE INDEX [IX_FeatureDetails_TenantId] ON [FeatureDetails] ([TenantId]);

CREATE INDEX [IX_Locations_CompanyId] ON [Locations] ([CompanyId]);

CREATE INDEX [IX_Locations_TenantId] ON [Locations] ([TenantId]);

CREATE INDEX [IX_Roles_CompanyId] ON [Roles] ([CompanyId]);

CREATE UNIQUE INDEX [IX_Roles_RoleName] ON [Roles] ([RoleName]);

CREATE INDEX [IX_Roles_TenantId] ON [Roles] ([TenantId]);

CREATE INDEX [IX_TenantFeatureAssignments_CompanyId] ON [TenantFeatureAssignments] ([CompanyId]);

CREATE INDEX [IX_TenantFeatureAssignments_FeatureId] ON [TenantFeatureAssignments] ([FeatureId]);

CREATE INDEX [IX_TenantFeatureAssignments_LocationId] ON [TenantFeatureAssignments] ([LocationId]);

CREATE INDEX [IX_TenantFeatureAssignments_TenantId] ON [TenantFeatureAssignments] ([TenantId]);

CREATE INDEX [IX_UserCompanyAssignments_CompanyId] ON [UserCompanyAssignments] ([CompanyId]);

CREATE INDEX [IX_UserCompanyAssignments_LocationId] ON [UserCompanyAssignments] ([LocationId]);

CREATE INDEX [IX_UserCompanyAssignments_RoleId] ON [UserCompanyAssignments] ([RoleId]);

CREATE UNIQUE INDEX [IX_UserCompanyAssignments_UserId_CompanyId_RoleId] ON [UserCompanyAssignments] ([UserId], [CompanyId], [RoleId]);

CREATE UNIQUE INDEX [IX_Users_Email] ON [Users] ([Email]);

CREATE INDEX [IX_Users_TenantId] ON [Users] ([TenantId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251217230721_AddMultiTenantSchema', N'9.0.0');

ALTER TABLE [Locations] DROP CONSTRAINT [FK_Locations_Companies_CompanyId];

DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Locations]') AND [c].[name] = N'Address');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Locations] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [Locations] DROP COLUMN [Address];

DECLARE @var1 sysname;
SELECT @var1 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Locations]') AND [c].[name] = N'IsActive');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Locations] DROP CONSTRAINT [' + @var1 + '];');
ALTER TABLE [Locations] DROP COLUMN [IsActive];

EXEC sp_rename N'[Locations].[PhoneNumber]', N'AddressLine1', 'COLUMN';

EXEC sp_rename N'[Locations].[Name]', N'LocationType', 'COLUMN';

DROP INDEX [IX_Locations_CompanyId] ON [Locations];
DECLARE @var2 sysname;
SELECT @var2 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Locations]') AND [c].[name] = N'CompanyId');
IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [Locations] DROP CONSTRAINT [' + @var2 + '];');
UPDATE [Locations] SET [CompanyId] = '00000000-0000-0000-0000-000000000000' WHERE [CompanyId] IS NULL;
ALTER TABLE [Locations] ALTER COLUMN [CompanyId] uniqueidentifier NOT NULL;
ALTER TABLE [Locations] ADD DEFAULT '00000000-0000-0000-0000-000000000000' FOR [CompanyId];
CREATE INDEX [IX_Locations_CompanyId] ON [Locations] ([CompanyId]);

ALTER TABLE [Locations] ADD [CreatedBy] uniqueidentifier NULL;

ALTER TABLE [Locations] ADD [FeatureId] int NULL;

ALTER TABLE [Locations] ADD [LocationName] nvarchar(max) NOT NULL DEFAULT N'';

CREATE TABLE [Categories] (
    [CategoryId] uniqueidentifier NOT NULL,
    [CompanyId] uniqueidentifier NOT NULL,
    [TenantId] uniqueidentifier NOT NULL,
    [CategoryName] nvarchar(max) NOT NULL,
    [ParentCategoryId] uniqueidentifier NULL,
    [LocationId] uniqueidentifier NULL,
    [IsActive] bit NOT NULL,
    [CreatedBy] uniqueidentifier NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_Categories] PRIMARY KEY ([CategoryId]),
    CONSTRAINT [FK_Categories_Categories_ParentCategoryId] FOREIGN KEY ([ParentCategoryId]) REFERENCES [Categories] ([CategoryId]),
    CONSTRAINT [FK_Categories_Companies_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [Companies] ([CompanyId]) ON DELETE CASCADE,
    CONSTRAINT [FK_Categories_Locations_LocationId] FOREIGN KEY ([LocationId]) REFERENCES [Locations] ([LocationId])
);

CREATE TABLE [Customers] (
    [CustomerId] uniqueidentifier NOT NULL,
    [CompanyId] uniqueidentifier NOT NULL,
    [TenantId] uniqueidentifier NOT NULL,
    [CustomerCode] nvarchar(450) NOT NULL,
    [FirstName] nvarchar(max) NOT NULL,
    [LastName] nvarchar(max) NULL,
    [CompanyName] nvarchar(max) NULL,
    [Email] nvarchar(max) NULL,
    [Phone] nvarchar(max) NULL,
    [AddressLine1] nvarchar(max) NULL,
    [City] nvarchar(max) NULL,
    [IsActive] bit NOT NULL,
    [LoyaltyPoints] decimal(18,4) NOT NULL,
    [CreatedBy] uniqueidentifier NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_Customers] PRIMARY KEY ([CustomerId]),
    CONSTRAINT [FK_Customers_Companies_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [Companies] ([CompanyId]) ON DELETE CASCADE
);

CREATE TABLE [GLAccounts] (
    [GLAccountId] uniqueidentifier NOT NULL,
    [CompanyId] uniqueidentifier NOT NULL,
    [AccountNumber] nvarchar(450) NOT NULL,
    [AccountName] nvarchar(max) NOT NULL,
    [AccountType] nvarchar(max) NOT NULL,
    [DebitIncreases] bit NOT NULL,
    [IsActive] bit NOT NULL,
    [CreatedBy] uniqueidentifier NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_GLAccounts] PRIMARY KEY ([GLAccountId]),
    CONSTRAINT [FK_GLAccounts_Companies_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [Companies] ([CompanyId]) ON DELETE CASCADE
);

CREATE TABLE [GoodsReturns] (
    [ReturnId] uniqueidentifier NOT NULL,
    [CompanyId] uniqueidentifier NOT NULL,
    [LocationId] uniqueidentifier NOT NULL,
    [UserId] uniqueidentifier NOT NULL,
    [SalesOrPurchaseId] uniqueidentifier NULL,
    [ReturnType] nvarchar(max) NOT NULL,
    [ReturnDate] datetime2 NOT NULL,
    [TotalRefundAmount] decimal(18,4) NOT NULL,
    [ReturnReason] nvarchar(max) NULL,
    [CreatedBy] uniqueidentifier NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_GoodsReturns] PRIMARY KEY ([ReturnId]),
    CONSTRAINT [FK_GoodsReturns_Companies_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [Companies] ([CompanyId]) ON DELETE CASCADE,
    CONSTRAINT [FK_GoodsReturns_Locations_LocationId] FOREIGN KEY ([LocationId]) REFERENCES [Locations] ([LocationId]) ON DELETE CASCADE,
    CONSTRAINT [FK_GoodsReturns_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId]) ON DELETE CASCADE
);

CREATE TABLE [JournalEntries] (
    [JournalHeaderId] uniqueidentifier NOT NULL,
    [TenantId] uniqueidentifier NOT NULL,
    [CompanyId] uniqueidentifier NOT NULL,
    [LocationId] uniqueidentifier NOT NULL,
    [EntryDate] datetime2 NOT NULL,
    [SourceTable] nvarchar(max) NOT NULL,
    [SourceId] uniqueidentifier NOT NULL,
    [Description] nvarchar(max) NULL,
    [IsPosted] bit NOT NULL,
    [PostedDate] datetime2 NULL,
    [CreatedBy] uniqueidentifier NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_JournalEntries] PRIMARY KEY ([JournalHeaderId]),
    CONSTRAINT [FK_JournalEntries_Companies_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [Companies] ([CompanyId]) ON DELETE CASCADE,
    CONSTRAINT [FK_JournalEntries_Locations_LocationId] FOREIGN KEY ([LocationId]) REFERENCES [Locations] ([LocationId]) ON DELETE CASCADE
);

CREATE TABLE [PaymentMethods] (
    [PaymentMethodId] int NOT NULL IDENTITY,
    [CompanyId] uniqueidentifier NOT NULL,
    [MethodName] nvarchar(max) NOT NULL,
    [PaymentType] nvarchar(max) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [CreatedBy] uniqueidentifier NOT NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_PaymentMethods] PRIMARY KEY ([PaymentMethodId]),
    CONSTRAINT [FK_PaymentMethods_Companies_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [Companies] ([CompanyId]) ON DELETE CASCADE
);

CREATE TABLE [Suppliers] (
    [SupplierId] uniqueidentifier NOT NULL,
    [CompanyId] uniqueidentifier NOT NULL,
    [TenantId] uniqueidentifier NOT NULL,
    [SupplierName] nvarchar(450) NOT NULL,
    [ContactName] nvarchar(max) NULL,
    [ContactEmail] nvarchar(max) NULL,
    [Phone] nvarchar(max) NULL,
    [Terms] nvarchar(max) NULL,
    [IsActive] bit NOT NULL,
    [CreatedBy] uniqueidentifier NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_Suppliers] PRIMARY KEY ([SupplierId]),
    CONSTRAINT [FK_Suppliers_Companies_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [Companies] ([CompanyId]) ON DELETE CASCADE
);

CREATE TABLE [Products] (
    [ProductId] uniqueidentifier NOT NULL,
    [CompanyId] uniqueidentifier NOT NULL,
    [TenantId] uniqueidentifier NOT NULL,
    [ProductName] nvarchar(max) NOT NULL,
    [ProductSkuBase] nvarchar(450) NOT NULL,
    [CategoryId] uniqueidentifier NULL,
    [LocationId] uniqueidentifier NULL,
    [Description] nvarchar(max) NULL,
    [IsActive] bit NOT NULL,
    [CreatedBy] uniqueidentifier NULL,
    [LastUpdated] datetime2 NOT NULL,
    [UpdatedBy] uniqueidentifier NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_Products] PRIMARY KEY ([ProductId]),
    CONSTRAINT [FK_Products_Categories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [Categories] ([CategoryId]),
    CONSTRAINT [FK_Products_Companies_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [Companies] ([CompanyId]) ON DELETE CASCADE,
    CONSTRAINT [FK_Products_Locations_LocationId] FOREIGN KEY ([LocationId]) REFERENCES [Locations] ([LocationId])
);

CREATE TABLE [Sales] (
    [SaleHeaderId] uniqueidentifier NOT NULL,
    [CompanyId] uniqueidentifier NOT NULL,
    [LocationId] uniqueidentifier NOT NULL,
    [UserId] uniqueidentifier NOT NULL,
    [CustomerId] uniqueidentifier NULL,
    [SaleDate] datetime2 NOT NULL,
    [TotalAmount] decimal(18,4) NOT NULL,
    [TotalTax] decimal(18,4) NOT NULL,
    [TotalDiscount] decimal(18,4) NOT NULL,
    [CreatedBy] uniqueidentifier NOT NULL,
    [Status] nvarchar(max) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_Sales] PRIMARY KEY ([SaleHeaderId]),
    CONSTRAINT [FK_Sales_Companies_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [Companies] ([CompanyId]) ON DELETE CASCADE,
    CONSTRAINT [FK_Sales_Customers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Customers] ([CustomerId]),
    CONSTRAINT [FK_Sales_Locations_LocationId] FOREIGN KEY ([LocationId]) REFERENCES [Locations] ([LocationId]) ON DELETE CASCADE,
    CONSTRAINT [FK_Sales_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId]) ON DELETE CASCADE
);

CREATE TABLE [JournalEntryDetails] (
    [JournalLineId] uniqueidentifier NOT NULL,
    [JournalHeaderId] uniqueidentifier NOT NULL,
    [GLAccountId] uniqueidentifier NOT NULL,
    [DebitAmount] decimal(18,4) NOT NULL,
    [CreditAmount] decimal(18,4) NOT NULL,
    [JournalEntryId] uniqueidentifier NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_JournalEntryDetails] PRIMARY KEY ([JournalLineId]),
    CONSTRAINT [FK_JournalEntryDetails_GLAccounts_GLAccountId] FOREIGN KEY ([GLAccountId]) REFERENCES [GLAccounts] ([GLAccountId]) ON DELETE CASCADE,
    CONSTRAINT [FK_JournalEntryDetails_JournalEntries_JournalEntryId] FOREIGN KEY ([JournalEntryId]) REFERENCES [JournalEntries] ([JournalHeaderId])
);

CREATE TABLE [PurchaseOrders] (
    [PurchaseOrderId] uniqueidentifier NOT NULL,
    [CompanyId] uniqueidentifier NOT NULL,
    [TenantId] uniqueidentifier NOT NULL,
    [LocationId] uniqueidentifier NOT NULL,
    [SupplierId] uniqueidentifier NOT NULL,
    [OrderDate] datetime2 NOT NULL,
    [ExpectedDeliveryDate] datetime2 NULL,
    [TotalAmount] decimal(18,4) NOT NULL,
    [CreatedBy] uniqueidentifier NULL,
    [LastUpdated] datetime2 NULL,
    [UpdatedBy] uniqueidentifier NULL,
    [Status] nvarchar(max) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_PurchaseOrders] PRIMARY KEY ([PurchaseOrderId]),
    CONSTRAINT [FK_PurchaseOrders_Companies_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [Companies] ([CompanyId]) ON DELETE CASCADE,
    CONSTRAINT [FK_PurchaseOrders_Locations_LocationId] FOREIGN KEY ([LocationId]) REFERENCES [Locations] ([LocationId]) ON DELETE CASCADE,
    CONSTRAINT [FK_PurchaseOrders_Suppliers_SupplierId] FOREIGN KEY ([SupplierId]) REFERENCES [Suppliers] ([SupplierId]) ON DELETE CASCADE
);

CREATE TABLE [ProductVariants] (
    [ProductVariantId] uniqueidentifier NOT NULL,
    [ProductId] uniqueidentifier NOT NULL,
    [VariantName] nvarchar(max) NOT NULL,
    [VariantSku] nvarchar(450) NOT NULL,
    [Barcode] nvarchar(max) NULL,
    [CreatedBy] uniqueidentifier NULL,
    [LastUpdated] datetime2 NOT NULL,
    [UpdatedBy] uniqueidentifier NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_ProductVariants] PRIMARY KEY ([ProductVariantId]),
    CONSTRAINT [FK_ProductVariants_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([ProductId]) ON DELETE CASCADE
);

CREATE TABLE [SalePayments] (
    [SalePaymentId] uniqueidentifier NOT NULL,
    [SalesId] uniqueidentifier NOT NULL,
    [PaymentMethodId] int NOT NULL,
    [Amount] decimal(18,4) NOT NULL,
    [ReferenceNumber] nvarchar(max) NULL,
    [PaymentDate] datetime2 NOT NULL,
    [CreatedBy] uniqueidentifier NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_SalePayments] PRIMARY KEY ([SalePaymentId]),
    CONSTRAINT [FK_SalePayments_PaymentMethods_PaymentMethodId] FOREIGN KEY ([PaymentMethodId]) REFERENCES [PaymentMethods] ([PaymentMethodId]) ON DELETE CASCADE,
    CONSTRAINT [FK_SalePayments_Sales_SalesId] FOREIGN KEY ([SalesId]) REFERENCES [Sales] ([SaleHeaderId]) ON DELETE CASCADE
);

CREATE TABLE [GoodsReceipts] (
    [GoodsReceiptId] uniqueidentifier NOT NULL,
    [PurchaseOrderId] uniqueidentifier NULL,
    [CompanyId] uniqueidentifier NOT NULL,
    [LocationId] uniqueidentifier NOT NULL,
    [TenantId] uniqueidentifier NOT NULL,
    [SupplierId] uniqueidentifier NULL,
    [ReceiptDate] datetime2 NOT NULL,
    [TotalReceivedAmount] decimal(18,4) NOT NULL,
    [CreatedBy] uniqueidentifier NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_GoodsReceipts] PRIMARY KEY ([GoodsReceiptId]),
    CONSTRAINT [FK_GoodsReceipts_Companies_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [Companies] ([CompanyId]) ON DELETE CASCADE,
    CONSTRAINT [FK_GoodsReceipts_Locations_LocationId] FOREIGN KEY ([LocationId]) REFERENCES [Locations] ([LocationId]) ON DELETE CASCADE,
    CONSTRAINT [FK_GoodsReceipts_PurchaseOrders_PurchaseOrderId] FOREIGN KEY ([PurchaseOrderId]) REFERENCES [PurchaseOrders] ([PurchaseOrderId]),
    CONSTRAINT [FK_GoodsReceipts_Suppliers_SupplierId] FOREIGN KEY ([SupplierId]) REFERENCES [Suppliers] ([SupplierId])
);

CREATE TABLE [Costs] (
    [CostId] uniqueidentifier NOT NULL,
    [ProductId] uniqueidentifier NOT NULL,
    [ProductVariantId] uniqueidentifier NOT NULL,
    [LocationId] uniqueidentifier NULL,
    [CompanyId] uniqueidentifier NOT NULL,
    [TenantId] uniqueidentifier NOT NULL,
    [SupplierId] uniqueidentifier NOT NULL,
    [CostValue] decimal(18,4) NOT NULL,
    [EffectiveDate] datetime2 NOT NULL,
    [CreatedBy] uniqueidentifier NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_Costs] PRIMARY KEY ([CostId]),
    CONSTRAINT [FK_Costs_Locations_LocationId] FOREIGN KEY ([LocationId]) REFERENCES [Locations] ([LocationId]),
    CONSTRAINT [FK_Costs_ProductVariants_ProductVariantId] FOREIGN KEY ([ProductVariantId]) REFERENCES [ProductVariants] ([ProductVariantId]) ON DELETE CASCADE,
    CONSTRAINT [FK_Costs_Suppliers_SupplierId] FOREIGN KEY ([SupplierId]) REFERENCES [Suppliers] ([SupplierId]) ON DELETE CASCADE
);

CREATE TABLE [Inventory] (
    [InventoryId] uniqueidentifier NOT NULL,
    [ProductId] uniqueidentifier NOT NULL,
    [ProductVariantId] uniqueidentifier NOT NULL,
    [LocationId] uniqueidentifier NOT NULL,
    [InitialQuantity] decimal(18,4) NOT NULL,
    [Quantity] decimal(18,4) NOT NULL,
    [TotalQuantitySold] decimal(18,4) NOT NULL,
    [TotalQuantityReturned] decimal(18,4) NOT NULL,
    [CreatedBy] uniqueidentifier NOT NULL,
    [LastUpdated] datetime2 NULL,
    [UpdatedBy] uniqueidentifier NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_Inventory] PRIMARY KEY ([InventoryId]),
    CONSTRAINT [FK_Inventory_Locations_LocationId] FOREIGN KEY ([LocationId]) REFERENCES [Locations] ([LocationId]) ON DELETE CASCADE,
    CONSTRAINT [FK_Inventory_ProductVariants_ProductVariantId] FOREIGN KEY ([ProductVariantId]) REFERENCES [ProductVariants] ([ProductVariantId]) ON DELETE CASCADE,
    CONSTRAINT [FK_Inventory_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([ProductId]) ON DELETE CASCADE
);

CREATE TABLE [Pricing] (
    [PricingId] uniqueidentifier NOT NULL,
    [ProductVariantId] uniqueidentifier NULL,
    [LocationId] uniqueidentifier NOT NULL,
    [ProductId] uniqueidentifier NOT NULL,
    [CompanyId] uniqueidentifier NOT NULL,
    [TenantId] uniqueidentifier NOT NULL,
    [Price] decimal(18,4) NOT NULL,
    [PackType] nvarchar(max) NOT NULL,
    [QuantityReduction] decimal(18,4) NOT NULL,
    [ReductionAmount] decimal(18,4) NOT NULL,
    [ReductionPercentage] decimal(18,4) NOT NULL,
    [EffectiveDate] datetime2 NOT NULL,
    [IsActive] bit NOT NULL,
    [CreatedBy] uniqueidentifier NOT NULL,
    [LastUpdated] datetime2 NOT NULL,
    [UpdatedBy] uniqueidentifier NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_Pricing] PRIMARY KEY ([PricingId]),
    CONSTRAINT [FK_Pricing_Locations_LocationId] FOREIGN KEY ([LocationId]) REFERENCES [Locations] ([LocationId]) ON DELETE CASCADE,
    CONSTRAINT [FK_Pricing_ProductVariants_ProductVariantId] FOREIGN KEY ([ProductVariantId]) REFERENCES [ProductVariants] ([ProductVariantId])
);

CREATE TABLE [PurchaseOrderDetails] (
    [PurchaseOrderItemId] uniqueidentifier NOT NULL,
    [PurchaseOrderId] uniqueidentifier NOT NULL,
    [ProductId] uniqueidentifier NOT NULL,
    [ProductVariantId] uniqueidentifier NOT NULL,
    [Quantity] decimal(18,4) NOT NULL,
    [UnitCost] decimal(18,4) NOT NULL,
    [LineTotal] decimal(18,4) NOT NULL,
    [QuantityReceived] decimal(18,4) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_PurchaseOrderDetails] PRIMARY KEY ([PurchaseOrderItemId]),
    CONSTRAINT [FK_PurchaseOrderDetails_ProductVariants_ProductVariantId] FOREIGN KEY ([ProductVariantId]) REFERENCES [ProductVariants] ([ProductVariantId]) ON DELETE CASCADE,
    CONSTRAINT [FK_PurchaseOrderDetails_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([ProductId]) ON DELETE CASCADE,
    CONSTRAINT [FK_PurchaseOrderDetails_PurchaseOrders_PurchaseOrderId] FOREIGN KEY ([PurchaseOrderId]) REFERENCES [PurchaseOrders] ([PurchaseOrderId]) ON DELETE CASCADE
);

CREATE TABLE [SalesDetails] (
    [SaleItemId] uniqueidentifier NOT NULL,
    [SaleHeaderId] uniqueidentifier NOT NULL,
    [ProductVariantId] uniqueidentifier NOT NULL,
    [Quantity] decimal(18,4) NOT NULL,
    [UnitCost] decimal(18,4) NOT NULL,
    [UnitPrice] decimal(18,4) NOT NULL,
    [TotalProfit] decimal(18,4) NOT NULL,
    [DiscountAmount] decimal(18,4) NOT NULL,
    [LineTotal] decimal(18,4) NOT NULL,
    [SalesId] uniqueidentifier NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_SalesDetails] PRIMARY KEY ([SaleItemId]),
    CONSTRAINT [FK_SalesDetails_ProductVariants_ProductVariantId] FOREIGN KEY ([ProductVariantId]) REFERENCES [ProductVariants] ([ProductVariantId]) ON DELETE CASCADE,
    CONSTRAINT [FK_SalesDetails_Sales_SalesId] FOREIGN KEY ([SalesId]) REFERENCES [Sales] ([SaleHeaderId])
);

CREATE TABLE [GoodsReceiptDetails] (
    [GoodsReceiptItemId] uniqueidentifier NOT NULL,
    [GoodsReceiptId] uniqueidentifier NOT NULL,
    [ProductId] uniqueidentifier NOT NULL,
    [ProductVariantId] uniqueidentifier NOT NULL,
    [QuantityReceived] decimal(18,4) NOT NULL,
    [UnitCost] decimal(18,4) NOT NULL,
    [LineTotal] decimal(18,4) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_GoodsReceiptDetails] PRIMARY KEY ([GoodsReceiptItemId]),
    CONSTRAINT [FK_GoodsReceiptDetails_GoodsReceipts_GoodsReceiptId] FOREIGN KEY ([GoodsReceiptId]) REFERENCES [GoodsReceipts] ([GoodsReceiptId]) ON DELETE CASCADE,
    CONSTRAINT [FK_GoodsReceiptDetails_ProductVariants_ProductVariantId] FOREIGN KEY ([ProductVariantId]) REFERENCES [ProductVariants] ([ProductVariantId]) ON DELETE CASCADE,
    CONSTRAINT [FK_GoodsReceiptDetails_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([ProductId]) ON DELETE CASCADE
);

CREATE TABLE [SupplierInvoices] (
    [SupplierInvoiceId] uniqueidentifier NOT NULL,
    [GoodsReceiptId] uniqueidentifier NULL,
    [SupplierId] uniqueidentifier NOT NULL,
    [CompanyId] uniqueidentifier NOT NULL,
    [TenantId] uniqueidentifier NOT NULL,
    [InvoiceNumber] nvarchar(max) NOT NULL,
    [InvoiceDate] datetime2 NOT NULL,
    [DueDate] datetime2 NOT NULL,
    [TotalAmount] decimal(18,4) NOT NULL,
    [TotalPaid] decimal(18,4) NOT NULL,
    [CreatedBy] uniqueidentifier NOT NULL,
    [Status] nvarchar(max) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_SupplierInvoices] PRIMARY KEY ([SupplierInvoiceId]),
    CONSTRAINT [FK_SupplierInvoices_Companies_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [Companies] ([CompanyId]) ON DELETE CASCADE,
    CONSTRAINT [FK_SupplierInvoices_GoodsReceipts_GoodsReceiptId] FOREIGN KEY ([GoodsReceiptId]) REFERENCES [GoodsReceipts] ([GoodsReceiptId]),
    CONSTRAINT [FK_SupplierInvoices_Suppliers_SupplierId] FOREIGN KEY ([SupplierId]) REFERENCES [Suppliers] ([SupplierId]) ON DELETE CASCADE
);

CREATE TABLE [GoodsReturnedDetails] (
    [ReturnItemId] uniqueidentifier NOT NULL,
    [ReturnHeaderId] uniqueidentifier NOT NULL,
    [ProductVariantId] uniqueidentifier NOT NULL,
    [ProductId] uniqueidentifier NULL,
    [SaleItemId] uniqueidentifier NULL,
    [QuantityReturned] decimal(18,4) NOT NULL,
    [RefundAmount] decimal(18,4) NOT NULL,
    [ReturnOutcome] nvarchar(max) NOT NULL,
    [CreatedBy] uniqueidentifier NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_GoodsReturnedDetails] PRIMARY KEY ([ReturnItemId]),
    CONSTRAINT [FK_GoodsReturnedDetails_GoodsReturns_ReturnHeaderId] FOREIGN KEY ([ReturnHeaderId]) REFERENCES [GoodsReturns] ([ReturnId]) ON DELETE CASCADE,
    CONSTRAINT [FK_GoodsReturnedDetails_ProductVariants_ProductVariantId] FOREIGN KEY ([ProductVariantId]) REFERENCES [ProductVariants] ([ProductVariantId]) ON DELETE CASCADE,
    CONSTRAINT [FK_GoodsReturnedDetails_SalesDetails_SaleItemId] FOREIGN KEY ([SaleItemId]) REFERENCES [SalesDetails] ([SaleItemId])
);

CREATE TABLE [InvoicePayments] (
    [PaymentId] uniqueidentifier NOT NULL,
    [SupplierInvoiceId] uniqueidentifier NOT NULL,
    [SupplierOrCustomer] nvarchar(max) NOT NULL,
    [PaymentDate] datetime2 NOT NULL,
    [AmountPaid] decimal(18,4) NOT NULL,
    [PaymentMethod] nvarchar(max) NOT NULL,
    [CreatedBy] uniqueidentifier NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_InvoicePayments] PRIMARY KEY ([PaymentId]),
    CONSTRAINT [FK_InvoicePayments_SupplierInvoices_SupplierInvoiceId] FOREIGN KEY ([SupplierInvoiceId]) REFERENCES [SupplierInvoices] ([SupplierInvoiceId]) ON DELETE CASCADE
);

CREATE INDEX [IX_Categories_CompanyId] ON [Categories] ([CompanyId]);

CREATE INDEX [IX_Categories_LocationId] ON [Categories] ([LocationId]);

CREATE INDEX [IX_Categories_ParentCategoryId] ON [Categories] ([ParentCategoryId]);

CREATE INDEX [IX_Costs_LocationId] ON [Costs] ([LocationId]);

CREATE INDEX [IX_Costs_ProductVariantId] ON [Costs] ([ProductVariantId]);

CREATE INDEX [IX_Costs_SupplierId] ON [Costs] ([SupplierId]);

CREATE UNIQUE INDEX [IX_Customers_CompanyId_CustomerCode] ON [Customers] ([CompanyId], [CustomerCode]);

CREATE UNIQUE INDEX [IX_GLAccounts_CompanyId_AccountNumber] ON [GLAccounts] ([CompanyId], [AccountNumber]);

CREATE INDEX [IX_GoodsReceiptDetails_GoodsReceiptId] ON [GoodsReceiptDetails] ([GoodsReceiptId]);

CREATE INDEX [IX_GoodsReceiptDetails_ProductId] ON [GoodsReceiptDetails] ([ProductId]);

CREATE INDEX [IX_GoodsReceiptDetails_ProductVariantId] ON [GoodsReceiptDetails] ([ProductVariantId]);

CREATE INDEX [IX_GoodsReceipts_CompanyId] ON [GoodsReceipts] ([CompanyId]);

CREATE INDEX [IX_GoodsReceipts_LocationId] ON [GoodsReceipts] ([LocationId]);

CREATE INDEX [IX_GoodsReceipts_PurchaseOrderId] ON [GoodsReceipts] ([PurchaseOrderId]);

CREATE INDEX [IX_GoodsReceipts_SupplierId] ON [GoodsReceipts] ([SupplierId]);

CREATE INDEX [IX_GoodsReturnedDetails_ProductVariantId] ON [GoodsReturnedDetails] ([ProductVariantId]);

CREATE INDEX [IX_GoodsReturnedDetails_ReturnHeaderId] ON [GoodsReturnedDetails] ([ReturnHeaderId]);

CREATE INDEX [IX_GoodsReturnedDetails_SaleItemId] ON [GoodsReturnedDetails] ([SaleItemId]);

CREATE INDEX [IX_GoodsReturns_CompanyId] ON [GoodsReturns] ([CompanyId]);

CREATE INDEX [IX_GoodsReturns_LocationId] ON [GoodsReturns] ([LocationId]);

CREATE INDEX [IX_GoodsReturns_UserId] ON [GoodsReturns] ([UserId]);

CREATE INDEX [IX_Inventory_LocationId] ON [Inventory] ([LocationId]);

CREATE INDEX [IX_Inventory_ProductId] ON [Inventory] ([ProductId]);

CREATE UNIQUE INDEX [IX_Inventory_ProductVariantId_LocationId] ON [Inventory] ([ProductVariantId], [LocationId]);

CREATE INDEX [IX_InvoicePayments_SupplierInvoiceId] ON [InvoicePayments] ([SupplierInvoiceId]);

CREATE INDEX [IX_JournalEntries_CompanyId] ON [JournalEntries] ([CompanyId]);

CREATE INDEX [IX_JournalEntries_LocationId] ON [JournalEntries] ([LocationId]);

CREATE INDEX [IX_JournalEntryDetails_GLAccountId] ON [JournalEntryDetails] ([GLAccountId]);

CREATE INDEX [IX_JournalEntryDetails_JournalEntryId] ON [JournalEntryDetails] ([JournalEntryId]);

CREATE INDEX [IX_PaymentMethods_CompanyId] ON [PaymentMethods] ([CompanyId]);

CREATE INDEX [IX_Pricing_LocationId] ON [Pricing] ([LocationId]);

CREATE INDEX [IX_Pricing_ProductVariantId] ON [Pricing] ([ProductVariantId]);

CREATE INDEX [IX_Products_CategoryId] ON [Products] ([CategoryId]);

CREATE UNIQUE INDEX [IX_Products_CompanyId_ProductSkuBase] ON [Products] ([CompanyId], [ProductSkuBase]);

CREATE INDEX [IX_Products_LocationId] ON [Products] ([LocationId]);

CREATE UNIQUE INDEX [IX_ProductVariants_ProductId_VariantSku] ON [ProductVariants] ([ProductId], [VariantSku]);

CREATE INDEX [IX_PurchaseOrderDetails_ProductId] ON [PurchaseOrderDetails] ([ProductId]);

CREATE INDEX [IX_PurchaseOrderDetails_ProductVariantId] ON [PurchaseOrderDetails] ([ProductVariantId]);

CREATE INDEX [IX_PurchaseOrderDetails_PurchaseOrderId] ON [PurchaseOrderDetails] ([PurchaseOrderId]);

CREATE INDEX [IX_PurchaseOrders_CompanyId] ON [PurchaseOrders] ([CompanyId]);

CREATE INDEX [IX_PurchaseOrders_LocationId] ON [PurchaseOrders] ([LocationId]);

CREATE INDEX [IX_PurchaseOrders_SupplierId] ON [PurchaseOrders] ([SupplierId]);

CREATE INDEX [IX_SalePayments_PaymentMethodId] ON [SalePayments] ([PaymentMethodId]);

CREATE INDEX [IX_SalePayments_SalesId] ON [SalePayments] ([SalesId]);

CREATE INDEX [IX_Sales_CompanyId] ON [Sales] ([CompanyId]);

CREATE INDEX [IX_Sales_CustomerId] ON [Sales] ([CustomerId]);

CREATE INDEX [IX_Sales_LocationId] ON [Sales] ([LocationId]);

CREATE INDEX [IX_Sales_UserId] ON [Sales] ([UserId]);

CREATE INDEX [IX_SalesDetails_ProductVariantId] ON [SalesDetails] ([ProductVariantId]);

CREATE INDEX [IX_SalesDetails_SalesId] ON [SalesDetails] ([SalesId]);

CREATE INDEX [IX_SupplierInvoices_CompanyId] ON [SupplierInvoices] ([CompanyId]);

CREATE INDEX [IX_SupplierInvoices_GoodsReceiptId] ON [SupplierInvoices] ([GoodsReceiptId]);

CREATE INDEX [IX_SupplierInvoices_SupplierId] ON [SupplierInvoices] ([SupplierId]);

CREATE UNIQUE INDEX [IX_Suppliers_CompanyId_SupplierName] ON [Suppliers] ([CompanyId], [SupplierName]);

ALTER TABLE [Locations] ADD CONSTRAINT [FK_Locations_Companies_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [Companies] ([CompanyId]) ON DELETE CASCADE;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251217231112_AddFullPOSSchema', N'9.0.0');

COMMIT;
GO

