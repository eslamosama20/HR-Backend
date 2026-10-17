using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.ResponseCompression;
using Hangfire;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using QuestPDF.Drawing;
using QuestPDF.Infrastructure;
using SmartAndPro.ERP.Application.Common.AutoMapper;
using SmartAndPro.ERP.Application.Common.Dtos.HotelAllotmentInvoices.AllotmentUsages;
using SmartAndPro.ERP.Application.IServices;
using SmartAndPro.ERP.Application.IServices.Projects;
using SmartAndPro.ERP.Application.Services;
using SmartAndPro.ERP.Application.Services.Dashboard;
using SmartAndPro.ERP.Application.Services.Email;
using SmartAndPro.ERP.Application.Services.HotelAllotmentInvoices;
using SmartAndPro.ERP.Application.Services.Inventory.PermitManagement;
using SmartAndPro.ERP.Application.Services.Inventory.SupplierManagement;
using SmartAndPro.ERP.Application.Services.Invoice;
using SmartAndPro.ERP.Application.Services.Invoice.Sale;
using SmartAndPro.ERP.Application.Services.Notifications;
using SmartAndPro.ERP.Application.Services.Notifications.UserNotifications;
using SmartAndPro.ERP.Application.Services.PaymentScheduleServices;
using SmartAndPro.ERP.Application.Services.Projects;
using SmartAndPro.ERP.Application.Services.PublicBooking;
using SmartAndPro.ERP.Application.Services.Purchase;
using SmartAndPro.ERP.Application.Services.PurchaseQuotations;
using SmartAndPro.ERP.Application.Services.Receipts.PaycheckReceipts;
using SmartAndPro.ERP.Application.Services.Reports;
using SmartAndPro.ERP.Application.Services.SMS;
using SmartAndPro.ERP.Infrastructure.Contexts;
using SmartAndPro.ERP.Infrastructure.Data.Persistence;
using SmartAndPro.ERP.Infrastructure.Hubs;
using SmartAndPro.ERP.Infrastructure.Repositories.PurchaseInvoice;
using SmartAndPro.ERP.Infrastructure.Repositories.Report.Supplier;
using SmartAndPro.ERP.Infrastructure.Repositories.SalesInvoice;
using SmartAndPro.ERP.Infrastructure.WebSockets;
using SmartAndPro.ERP.SharedKernel.Entities.Configurations;
using SmartAndPro.ERP.SharedKernel.Utilities.ShareHandlers;
using SmartAndPro.ERP.WebAPI.BackgroundServices;
using SmartAndPro.ERP.WebAPI.Installers.@base;
using SmartAndPro.ERP.WebAPI.Middlewares;
using SmartAndPro.ERP.WebAPI.Services.Cashier;
using SmartAndPro.ERP.WebAPI.Configurations;
using SmartAndPro.ERP.WebAPI.Services.Cloudinary;
using System.IO.Compression;
using System.Text;
using SmartAndPro.ERP.Application.Services.PurchaseQuotations;
using SmartAndPro.ERP.WebAPI.Services.Cashier;
using SmartAndPro.ERP.Application.Services.Pharmacy;
QuestPDF.Settings.License = LicenseType.Community;
QuestPDF.Settings.EnableDebugging = true;
FontManager.RegisterFont(File.OpenRead("Assets/Fonts/Almarai-Regular.ttf"));
var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://0.0.0.0:5010");

// التأكد من إنشاء مجلد الرفع uploads تلقائياً عند بدء التطبيق
var contentRootUploads = System.IO.Path.Combine(builder.Environment.ContentRootPath, "wwwroot", "uploads");
if (!System.IO.Directory.Exists(contentRootUploads))
{
    System.IO.Directory.CreateDirectory(contentRootUploads);
}
var currentDirUploads = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "wwwroot", "uploads");
if (!System.IO.Directory.Exists(currentDirUploads))
{
    System.IO.Directory.CreateDirectory(currentDirUploads);
}

// تعطيل طباعة SQL queries في Production
if (!builder.Environment.IsDevelopment())
{
    builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning);
    builder.Logging.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Warning);
}

// Add services to the container.
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
    // Allow case-insensitive property matching for JSON deserialization
    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    // Use camelCase for JSON serialization to match frontend expectations
    options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    // Prevent circular reference errors from EF navigation properties
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    // Add DateOnly converters for proper JSON serialization
    options.JsonSerializerOptions.Converters.Add(new SmartAndPro.ERP.WebAPI.Converters.DateOnlyJsonConverter());
    options.JsonSerializerOptions.Converters.Add(new SmartAndPro.ERP.WebAPI.Converters.DateOnlyNonNullableJsonConverter());
    // Add DateTime UTC converters for standard ISO 8601 UTC serialization with 'Z' suffix
    options.JsonSerializerOptions.Converters.Add(new SmartAndPro.ERP.WebAPI.Converters.DateTimeUtcJsonConverter());
    options.JsonSerializerOptions.Converters.Add(new SmartAndPro.ERP.WebAPI.Converters.NullableDateTimeUtcJsonConverter());
});
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<GzipCompressionProvider>();
    options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[] { "application/json" });
});
builder.Services.Configure<GzipCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Fastest;
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();
builder.InstallServices();
builder.Services.InstallAutoMapper();
builder.Services.AddOutputCache();

var hangfireConnStr = builder.Configuration.GetValue<string>("ConnectionStrings:InventoryConnectionString");
if (!string.IsNullOrEmpty(hangfireConnStr))
{
    builder.Services.AddHangfire(config =>
        config.UseSqlServerStorage(hangfireConnStr));
    builder.Services.AddHangfireServer();
}

builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("MailSettings"));
builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("Cloudinary"));
builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();

var jwt = builder.Configuration.GetSection("AppSettings:Token");
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false, // Disabled for development
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"])),
        ValidIssuer = jwt["Issuer"],
        ValidateAudience = false,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

#region Swagger Auth

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Next Driven API", Version = "v1" });
    c.CustomSchemaIds(type => type.FullName); // Fix duplicate schema names with same class name in different namespaces
    c.ResolveConflictingActions(x => x.First());
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Auth header using the Bearer scheme. Example : \"Auth : Bearer {token}\""
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Name = "Bearer",
                In = ParameterLocation.Header
            },
            new string[] { }
        }
    });
});

#endregion

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowMyOrigin", policy =>
    {
        policy.AllowAnyHeader().AllowAnyMethod()
            .SetIsOriginAllowed(_ => true)
            .AllowCredentials();
    });
});

builder.Services.AddScoped<IPurchaseInvoiceRepository, PurchaseInvoiceRepository>();
builder.Services.AddScoped<IPurchaseInvoiceService, PurchaseInvoiceService>();

builder.Services.AddScoped<ISalesInvoicesRepository, SalesInvoicesRepository>();
builder.Services.AddScoped<ISalesInvoicesService, SaleInvoiceService>();
builder.Services.AddScoped<ISalesInvoiceImportService, SalesInvoiceImportService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IUserNotificationService, UserNotificationService>();

builder.Services.AddScoped<SmartAndPro.ERP.Application.Services.Auth.Interfaces.IRoleQuotaService, SmartAndPro.ERP.Application.Services.Auth.RoleQuotaService>();

// خدمة التذكير بانتهاء الصلاحية
builder.Services.AddScoped<IExpiryReminderService, ExpiryReminderService>();
// Background Service للتذكيرات التلقائية
builder.Services.AddHostedService<ExpiryReminderBackgroundService>();
builder.Services.AddHostedService<TempImageCleanupBackgroundService>();
builder.Services.AddHostedService<ProjectDelayCheckBackgroundService>();

builder.Services.AddScoped<ISupplierDetailsReportService, SupplierDetailsReportService>();
builder.Services.AddScoped<ISupplierReportRepository, SupplierReportDetailsRepository>();
builder.Services.AddScoped<PaymentScheduleService>();
builder.Services.AddScoped<PaycheckReceiptService>();

// Project Management Services
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<IProjectReportService, ProjectReportService>();

builder.Services.AddScoped<IPurchaseQuotationService, PurchaseQuotationService>();
builder.Services.AddSingleton<ICashierRuntimeStore, CashierRuntimeStore>();
builder.Services.AddSingleton<IRestaurantRuntimeStore, RestaurantRuntimeStore>();
builder.Services.AddScoped<ICashierReservationService, CashierReservationService>();
builder.Services.AddScoped<ICashierAuditService, CashierAuditService>();
builder.Services.AddScoped<ICashierInvoiceControlService, CashierInvoiceControlService>();
builder.Services.AddSingleton<ICashierStockLockService, CashierStockLockService>();

// Top Products Report Services
builder.Services.AddScoped<TopSellingProductsService>();
builder.Services.AddScoped<TopOrderedProductsService>();

builder.Services.AddTransient<ShareHandler, EmailShareHandler>();
builder.Services.AddTransient<ShareHandler, WhatsAppShareHandler<string>>();

builder.Services.AddTransient<IDocumentShareService, DocumentShareService>();

// HttpClient for SMS service
builder.Services.AddHttpClient();

// Public Booking Services
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ISmsService, SmsService>();
builder.Services.AddScoped<IHotelReviewService, HotelReviewService>();
builder.Services.AddScoped<AllotmentUsageService>();
builder.Services.AddScoped<PaymentScheduleService>();


// Paymob Payment Gateway
builder.Services.AddHttpClient<PaymobService>();
builder.Services.AddScoped<PaymobService>();

// Pharmacy Module
builder.Services.AddScoped<SmartAndPro.ERP.WebAPI.Services.Pharmacy.IPharmacyPrescriptionService,
    SmartAndPro.ERP.WebAPI.Services.Pharmacy.PharmacyPrescriptionService>();
builder.Services.AddScoped<SmartAndPro.ERP.WebAPI.Services.Pharmacy.IPharmacyInsuranceClaimsService,
    SmartAndPro.ERP.WebAPI.Services.Pharmacy.PharmacyInsuranceClaimsService>();
builder.Services.AddScoped<SmartAndPro.ERP.WebAPI.Services.Pharmacy.IPharmacyControlledDrugsService,
    SmartAndPro.ERP.WebAPI.Services.Pharmacy.PharmacyControlledDrugsService>();
builder.Services.AddScoped<SmartAndPro.ERP.WebAPI.Services.Pharmacy.IPharmacyAlertsService,
    SmartAndPro.ERP.WebAPI.Services.Pharmacy.PharmacyAlertsService>();
builder.Services.AddScoped<SmartAndPro.ERP.WebAPI.Services.Pharmacy.IPharmacyReturnsService,
    SmartAndPro.ERP.WebAPI.Services.Pharmacy.PharmacyReturnsService>();
builder.Services.AddScoped<SmartAndPro.ERP.WebAPI.Services.Pharmacy.IRestrictedDrugsService,
    SmartAndPro.ERP.WebAPI.Services.Pharmacy.RestrictedDrugsService>();
// CCHI / NPHIES Insurance Claims Integration
builder.Services.AddHttpClient<SmartAndPro.ERP.WebAPI.Services.Pharmacy.IPharmacyCchiClaimService,
    SmartAndPro.ERP.WebAPI.Services.Pharmacy.PharmacyCchiClaimService>();

// Pharmacy Inventory, Dispense & Support (PDF spec §9-§20)
builder.Services.AddScoped<SmartAndPro.ERP.WebAPI.Services.Pharmacy.IPharmacyInventoryService,
    SmartAndPro.ERP.WebAPI.Services.Pharmacy.PharmacyInventoryService>();
builder.Services.AddScoped<SmartAndPro.ERP.WebAPI.Services.Pharmacy.IPharmacyDispenseService,
    SmartAndPro.ERP.WebAPI.Services.Pharmacy.PharmacyDispenseService>();
builder.Services.AddScoped<SmartAndPro.ERP.WebAPI.Services.Pharmacy.IPharmacySupportService,
    SmartAndPro.ERP.WebAPI.Services.Pharmacy.PharmacySupportService>();
builder.Services.AddScoped<SmartAndPro.ERP.Application.IServices.IPharmacyItemService,
    SmartAndPro.ERP.Application.Services.Pharmacy.PharmacyItemService>();
builder.Services.AddScoped<SmartAndPro.ERP.Application.IServices.IPharmacyInsuranceService,
    SmartAndPro.ERP.Application.Services.Pharmacy.PharmacyInsuranceService>();

// Salla E-Commerce Integration
builder.Services.AddHttpClient<SmartAndPro.ERP.WebAPI.Services.Salla.ISallaIntegrationService,
    SmartAndPro.ERP.WebAPI.Services.Salla.SallaIntegrationService>();

// Zid E-Commerce Integration
builder.Services.AddHttpClient<SmartAndPro.ERP.WebAPI.Services.Zid.IZidIntegrationService,
    SmartAndPro.ERP.WebAPI.Services.Zid.ZidIntegrationService>();

// Wasafaty (وصفتي) E-Prescription Integration
builder.Services.AddHttpClient<SmartAndPro.ERP.WebAPI.Services.Wasafaty.IWasafatyIntegrationService,
    SmartAndPro.ERP.WebAPI.Services.Wasafaty.WasafatyIntegrationService>();

// Channel Manager Integration
builder.Services.AddHttpClient<SmartAndPro.ERP.WebAPI.Services.ChannelManager.IChannelManagerService,
    SmartAndPro.ERP.WebAPI.Services.ChannelManager.ChannelManagerService>();
builder.Services.AddHostedService<SmartAndPro.ERP.WebAPI.BackgroundServices.ChannelManagerBackgroundService>();

// SignalR for real-time notifications
builder.Services.AddSignalR();

builder.Services.AddSingleton<WebSocketConnectionManager>();

// ── Payment Orchestration System ────────────────────────────────────
builder.Services.Configure<SmartAndPro.ERP.WebAPI.Services.Payment.PaymentProvidersOptions>(
    builder.Configuration.GetSection("PaymentProviders"));
builder.Services.AddHttpClient<SmartAndPro.ERP.WebAPI.Services.Payment.Providers.PaymobProvider>();
builder.Services.AddScoped<SmartAndPro.ERP.WebAPI.Services.Payment.Providers.PaymobProvider>();
builder.Services.AddScoped<SmartAndPro.ERP.WebAPI.Services.Payment.Providers.TabbyProvider>();
builder.Services.AddScoped<SmartAndPro.ERP.WebAPI.Services.Payment.Providers.TamaraProvider>();
builder.Services.AddScoped<SmartAndPro.ERP.WebAPI.Services.Payment.Providers.HyperPayProvider>();
builder.Services.AddScoped<SmartAndPro.ERP.WebAPI.Services.Payment.IPaymentService,
    SmartAndPro.ERP.WebAPI.Services.Payment.PaymentService>();


var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ERPContext>();
        
        try
        {
            app.Logger.LogInformation("Checking and applying database migrations...");
            context.Database.Migrate();
            app.Logger.LogInformation("Database migrations applied successfully.");
        }
        catch (Exception exMig)
        {
            app.Logger.LogWarning(exMig, "Database auto-migration warning/notice");
        }

        try
        {
            DataSeeder.Seed(context);
            app.Logger.LogInformation("Database seeded successfully.");
        }
        catch (Exception exSeed)
        {
            app.Logger.LogWarning(exSeed, "Database seeding warning/notice: " + exSeed.Message);
        }

        try
        {
            context.Database.ExecuteSqlRaw(@"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[CustomerProfile]') AND name = 'EnglishCity')
                BEGIN
                    ALTER TABLE [CustomerProfile] ADD [EnglishCity] nvarchar(200) NULL;
                END
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[CustomerProfile]') AND name = 'EnglishCountry')
                BEGIN
                    ALTER TABLE [CustomerProfile] ADD [EnglishCountry] nvarchar(200) NULL;
                END
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[CustomerProfile]') AND name = 'EnglishDistrict')
                BEGIN
                    ALTER TABLE [CustomerProfile] ADD [EnglishDistrict] nvarchar(200) NULL;
                END
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[CustomerProfile]') AND name = 'EnglishStreet')
                BEGIN
                    ALTER TABLE [CustomerProfile] ADD [EnglishStreet] nvarchar(200) NULL;
                END
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[CompanyDetails]') AND name = 'EnglishDistrict')
                BEGIN
                    ALTER TABLE [CompanyDetails] ADD [EnglishDistrict] nvarchar(200) NULL;
                END
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Attendance]') AND name = 'Reason')
                BEGIN
                    ALTER TABLE [Attendance] ADD [Reason] nvarchar(1000) NULL;
                END
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Penalties]') AND name = 'Reason')
                BEGIN
                    ALTER TABLE [Penalties] ADD [Reason] nvarchar(1000) NULL;
                END
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Bonuses]') AND name = 'Reason')
                BEGIN
                    ALTER TABLE [Bonuses] ADD [Reason] nvarchar(1000) NULL;
                END
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[OvertimeHours]') AND name = 'Reason')
                BEGIN
                    ALTER TABLE [OvertimeHours] ADD [Reason] nvarchar(1000) NULL;
                END
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[MonthlyLoans]') AND name = 'Reason')
                BEGIN
                    ALTER TABLE [MonthlyLoans] ADD [Reason] nvarchar(1000) NULL;
                END
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[FingerprintDeviceSettings]') AND name = 'Factory3DeviceIP')
                BEGIN
                    ALTER TABLE [FingerprintDeviceSettings] ADD [Factory3DeviceIP] nvarchar(max) NULL;
                END
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[FingerprintDeviceSettings]') AND name = 'Factory3DevicePort')
                BEGIN
                    ALTER TABLE [FingerprintDeviceSettings] ADD [Factory3DevicePort] nvarchar(max) NULL;
                END
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[FingerprintDeviceSettings]') AND name = 'Factory3DeviceEnabled')
                BEGIN
                    ALTER TABLE [FingerprintDeviceSettings] ADD [Factory3DeviceEnabled] bit NOT NULL DEFAULT 0;
                END
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[FingerprintDeviceSettings]') AND name = 'Factory3LastSyncTime')
                BEGIN
                    ALTER TABLE [FingerprintDeviceSettings] ADD [Factory3LastSyncTime] datetime2 NULL;
                END
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[FingerprintDeviceSettings]') AND name = 'Factory3LastSyncStatus')
                BEGIN
                    ALTER TABLE [FingerprintDeviceSettings] ADD [Factory3LastSyncStatus] nvarchar(max) NULL;
                END
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[FingerprintDeviceSettings]') AND name = 'Factory3LastSyncMessage')
                BEGIN
                    ALTER TABLE [FingerprintDeviceSettings] ADD [Factory3LastSyncMessage] nvarchar(max) NULL;
                END
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[FingerprintDeviceSettings]') AND name = 'Factory3LastSyncFetchedCount')
                BEGIN
                    ALTER TABLE [FingerprintDeviceSettings] ADD [Factory3LastSyncFetchedCount] int NOT NULL DEFAULT 0;
                END
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[FingerprintDeviceSettings]') AND name = 'Factory3LastSyncInsertedCount')
                BEGIN
                    ALTER TABLE [FingerprintDeviceSettings] ADD [Factory3LastSyncInsertedCount] int NOT NULL DEFAULT 0;
                END
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[FingerprintDeviceSettings]') AND name = 'Factory3LastSyncSkippedCount')
                BEGIN
                    ALTER TABLE [FingerprintDeviceSettings] ADD [Factory3LastSyncSkippedCount] int NOT NULL DEFAULT 0;
                END
            ");
        }
        catch (Exception exSql)
        {
            Console.WriteLine("Auto-migration SQL exception: " + exSql.Message);
        }
        
        // Seed test projects for testing
        // SeedProjects.SeedTestProjectsAsync(context).Wait();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

//app.UseMiddleware<ErrorHandlingMiddleware>();
// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

//Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseMiddleware<SwaggerBasicAuthMiddleware>();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ErrorHandlingMiddleware>();

//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}


// app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseResponseCompression();
app.UseRouting();

app.UseCors("AllowMyOrigin");
app.UseOutputCache();
app.UseAuthentication();
app.UseAuthorization();

// Cashier session guard - ensures all POS operations have active session with store
app.UseCashierSessionGuard();

app.MapControllers();
app.MapHub<NotificationHub>("/notificationHub");
app.UseWebSockets();

app.UseMiddleware<WebSocketMiddleware>();

if (!string.IsNullOrEmpty(hangfireConnStr))
{
    app.UseHangfireDashboard("/hangfire");
    using (var scope = app.Services.CreateScope())
    {
        try
        {
            var scheduler = scope.ServiceProvider.GetRequiredService<SmartAndPro.ERP.WebAPI.Services.IHangfireSchedulerService>();
            await scheduler.ScheduleAllJobsAsync();
        }
        catch (Exception ex)
        {
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "An error occurred while scheduling Hangfire jobs on startup.");
        }
    }
}

app.Run();
