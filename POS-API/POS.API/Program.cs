using Microsoft.EntityFrameworkCore;
using POS.Infrastructure.Persistence;
using FluentValidation;
using POS.Application.DTOs.Auth;
using POS.Application.DTOs.Auth.Validators;
using POS.Application.DTOs.Features;
using POS.Application.DTOs.Tenant;
using POS.Application.DTOs.Company;
using POS.Application.DTOs.Role;
using POS.Application.DTOs.Location;
using POS.Application.Interfaces;
using POS.Application.Interfaces;
using POS.Infrastructure.Services;
using POS.Application.DTOs.Category;
using POS.Application.DTOs.Category.Validators;
using POS.Application.DTOs.Product;
using POS.Application.DTOs.Product.Validators;
using POS.Application.DTOs.Inventory;
using POS.Application.DTOs.Inventory.Validators;
using POS.Application.DTOs.Supplier;
using POS.Application.DTOs.Supplier.Validators;
using POS.Application.DTOs.SupplierInvoice;
using POS.Application.DTOs.SupplierInvoice.Validators;
using POS.Application.DTOs.InvoicePayment;
using POS.Application.DTOs.InvoicePayment.Validators;
using POS.Application.DTOs.PurchaseOrder;
using POS.Application.DTOs.PurchaseOrder.Validators;
using POS.Application.DTOs.GoodsReceipt;
using POS.Application.DTOs.GoodsReceipt.Validators;
using POS.Application.DTOs.Cost;
using POS.Application.DTOs.Cost.Validators;
using POS.Application.DTOs.Pricing;
using POS.Application.DTOs.Pricing.Validators;
using POS.Application.DTOs.Customer;
using POS.Application.DTOs.Customer.Validators;
using POS.Application.DTOs.Sale;
using POS.Application.DTOs.Sale.Validators;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new POS.API.Converters.NullableGuidJsonConverter());
    });

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => {
    c.CustomSchemaIds(type => type.FullName);
    // Configure Swagger to accept Bearer Token
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter 'Bearer' [space] and then your valid token in the text input below.\r\n\r\nExample: \"Bearer eyJhbGciOiJIUzI1Ni...\""
    });
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// Configure JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
})

.AddJwtBearer(options =>
{
    var jwtSettings = builder.Configuration.GetSection("JwtSettings");
    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]))
    };
});

builder.Services.AddAuthorization(); // Required for middleware

// Validator Registrations
// Validator Registrations
builder.Services.AddScoped<IValidator<RegisterTenantRequest>, RegisterTenantRequestValidator>();
builder.Services.AddScoped<IValidator<RegisterUserRequest>, POS.Application.DTOs.Auth.Validators.RegisterUserRequestValidator>();
builder.Services.AddScoped<IValidator<CreateFeatureDto>, POS.Application.DTOs.Features.Validators.CreateFeatureDtoValidator>();
builder.Services.AddScoped<IValidator<CreateTenantDto>, POS.Application.DTOs.Tenant.Validators.CreateTenantDtoValidator>();
builder.Services.AddScoped<IValidator<CreateRoleDto>, POS.Application.DTOs.Role.Validators.CreateRoleDtoValidator>();
builder.Services.AddScoped<IValidator<CreateCompanyDto>, POS.Application.DTOs.Company.Validators.CreateCompanyDtoValidator>();
builder.Services.AddScoped<IValidator<CreateLocationDto>, POS.Application.DTOs.Location.Validators.CreateLocationDtoValidator>();
builder.Services.AddScoped<IValidator<CreateCategoryDto>, POS.Application.DTOs.Category.Validators.CreateCategoryDtoValidator>();
builder.Services.AddScoped<IValidator<CreateProductDto>, CreateProductDtoValidator>();
builder.Services.AddScoped<IValidator<UpdateProductDto>, UpdateProductDtoValidator>();
builder.Services.AddScoped<IValidator<AdjustInventoryDto>, AdjustInventoryDtoValidator>();
builder.Services.AddScoped<IValidator<CreateInventoryDto>, CreateInventoryDtoValidator>();

// Register Domain Services
    builder.Services.AddScoped<ITenantService, TenantService>();
    builder.Services.AddScoped<IAuthService, AuthService>();
    builder.Services.AddScoped<ICurrencyService, CurrencyService>();
    builder.Services.AddScoped<ICustomerService, CustomerService>();
    builder.Services.AddScoped<ICostService, CostService>();
    builder.Services.AddScoped<IPricingService, PricingService>();
    builder.Services.AddScoped<ISupplierService, SupplierService>();
    builder.Services.AddScoped<IPurchaseOrderService, PurchaseOrderService>();
    builder.Services.AddScoped<IGoodsReceiptService, GoodsReceiptService>();
    builder.Services.AddScoped<ISupplierInvoiceService, SupplierInvoiceService>();
    builder.Services.AddScoped<IInvoicePaymentService, InvoicePaymentService>();
    builder.Services.AddScoped<ISaleService, SaleService>();
    builder.Services.AddScoped<IAccountingService, AccountingService>();
    builder.Services.AddScoped<IExpenseService, ExpenseService>(); // NEW
    builder.Services.AddScoped<IPaymentMethodService, PaymentMethodService>();
    builder.Services.AddScoped<ISalePaymentService, SalePaymentService>();
    builder.Services.AddScoped<IGoodsReturnService, GoodsReturnService>();
    builder.Services.AddScoped<IProductService, ProductService>();
    builder.Services.AddScoped<IInventoryService, InventoryService>();
    builder.Services.AddScoped<ICategoryService, CategoryService>();
    builder.Services.AddScoped<ILocationService, LocationService>();
    builder.Services.AddScoped<IRoleService, RoleService>();
    builder.Services.AddScoped<ICompanyService, CompanyService>();
    builder.Services.AddScoped<IPermissionService, PermissionService>();
    builder.Services.AddScoped<IFeatureService, FeatureService>();
    builder.Services.AddScoped<IDashboardService, DashboardService>();
    builder.Services.AddScoped<ISetupVATService, SetupVATService>();
    builder.Services.AddScoped<ISetupCountryService, SetupCountryService>();
    builder.Services.AddScoped<ISetupExpenseTypeService, SetupExpenseTypeService>();
    
    builder.Services.AddScoped<IValidator<CreateCustomerDto>, POS.Application.DTOs.Customer.Validators.CreateCustomerDtoValidator>();
    builder.Services.AddScoped<IValidator<UpdateCustomerDto>, POS.Application.DTOs.Customer.Validators.UpdateCustomerDtoValidator>();

    builder.Services.AddScoped<IValidator<CreateCostDto>, POS.Application.DTOs.Cost.Validators.CreateCostDtoValidator>();
    builder.Services.AddScoped<IValidator<UpdateCostDto>, POS.Application.DTOs.Cost.Validators.UpdateCostDtoValidator>();

    builder.Services.AddScoped<IValidator<CreatePricingDto>, POS.Application.DTOs.Pricing.Validators.CreatePricingDtoValidator>();
    builder.Services.AddScoped<IValidator<UpdatePricingDto>, POS.Application.DTOs.Pricing.Validators.UpdatePricingDtoValidator>();

    builder.Services.AddScoped<IValidator<CreateSupplierInvoiceDto>, POS.Application.DTOs.SupplierInvoice.Validators.CreateSupplierInvoiceDtoValidator>();
    builder.Services.AddScoped<IValidator<UpdateSupplierInvoiceDto>, POS.Application.DTOs.SupplierInvoice.Validators.UpdateSupplierInvoiceDtoValidator>();

    builder.Services.AddScoped<IValidator<CreateSupplierDto>, POS.Application.DTOs.Supplier.Validators.CreateSupplierDtoValidator>();
    builder.Services.AddScoped<IValidator<UpdateSupplierDto>, POS.Application.DTOs.Supplier.Validators.UpdateSupplierDtoValidator>();

    builder.Services.AddScoped<IValidator<CreateGoodsReceiptDto>, POS.Application.DTOs.GoodsReceipt.Validators.CreateGoodsReceiptDtoValidator>();
    builder.Services.AddScoped<IValidator<UpdateGoodsReceiptDto>, POS.Application.DTOs.GoodsReceipt.Validators.UpdateGoodsReceiptDtoValidator>();

    builder.Services.AddScoped<IValidator<CreatePurchaseOrderDto>, POS.Application.DTOs.PurchaseOrder.Validators.CreatePurchaseOrderDtoValidator>();
    builder.Services.AddScoped<IValidator<UpdatePurchaseOrderDto>, POS.Application.DTOs.PurchaseOrder.Validators.UpdatePurchaseOrderDtoValidator>();

    builder.Services.AddScoped<IValidator<CreateInvoicePaymentDto>, POS.Application.DTOs.InvoicePayment.Validators.CreateInvoicePaymentDtoValidator>();

    builder.Services.AddScoped<IValidator<CreateSaleDto>, CreateSaleDtoValidator>();

// Database Context
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly("POS.Infrastructure").EnableRetryOnFailure()));

var app = builder.Build();

// Seed Database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try 
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        await PermissionSeeder.SeedPermissionsAsync(context);
        await FeatureSeeder.SeedFeaturesAsync(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database. The database may be unavailable.");
    }
}

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "POS API V1");
    // By default, this will be at /swagger
});

var provider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
provider.Mappings[".avif"] = "image/avif";
provider.Mappings[".webp"] = "image/webp";

app.UseStaticFiles(new StaticFileOptions
{
    ContentTypeProvider = provider
});

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors("AllowFrontend");

app.UseAuthentication(); // Must be before Authorization
app.UseAuthorization();

app.MapControllers();

app.Run();
