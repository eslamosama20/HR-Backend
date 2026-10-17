using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using SmartAndPro.ERP.Application.Common.AutoMapper;
using SmartAndPro.ERP.Application.Common.Dtos.Inventory.SupplierDetails;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Application.Services.Inventory.SupplierManagement;
using SmartAndPro.ERP.Domains.Entities.Accounting;
using SmartAndPro.ERP.Domains.Entities.Inventory.SupplierManagement;
using SmartAndPro.ERP.Infrastructure.Contexts;
using SmartAndPro.ERP.Infrastructure.Contexts.Utility;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.SharedKernel.Entities.Configurations;
using SmartAndPro.ERP.SharedKernel.Enums;
using Xunit;

namespace SmartAndPro.ERP.Tests.ApplicationTests.Services.Inventory;

/// <summary>
/// Tests for SupplierService.UpdateAsync account handling (plan Phase 5 / I1 fix):
///  - a) a manually-linked MainAccount is NOT renamed when the supplier name changes
///  - b) an auto-created account (names matching the supplier's previous names) IS renamed
///  - c) changing MainAccountId on update persists (FK updated) and the old account is kept (I2)
/// </summary>
public class SupplierServiceUpdateAccountTests
{
    private static IMapper CreateMapper()
        => new MapperConfiguration(cfg => cfg.AddProfile<MappingProfiles>()).CreateMapper();

    private static Mock<IAutoNumberGenerator> CreateGenerator()
    {
        var generator = new Mock<IAutoNumberGenerator>();
        // Seeds always set ReferenceCode so the generator is normally never called;
        // keep a safe default for any unexpected invocation.
        generator
            .Setup(g => g.GenerateAsync<It.IsAnyType>(
                It.IsAny<DbContext>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()))
            .ReturnsAsync("X");
        return generator;
    }

    private static ERPContext CreateContext(string dbName, Mock<IAutoNumberGenerator> generator)
    {
        var options = new DbContextOptionsBuilder<ERPContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        return new ERPContext(options, generator.Object);
    }

    private static SupplierService CreateService(ERPContext context)
    {
        var serviceProvider = new ServiceCollection()
            .AddSingleton<IHttpContextAccessor>(new HttpContextAccessor { HttpContext = new DefaultHttpContext() })
            .BuildServiceProvider();

        var repositoriesFactory = new RepositoriesFactory(context, serviceProvider);
        var appSettings = new AppSettings();
        var mapper = CreateMapper();
        var serviceBaseFactory = new ServiceBaseFactory(
            repositoriesFactory, appSettings, mapper,
            serviceProvider.GetRequiredService<IHttpContextAccessor>());

        return new SupplierService(repositoriesFactory, appSettings, mapper, serviceBaseFactory);
    }

    private static DetailAccount SeedDetailAccount(ERPContext context, Guid id)
    {
        var detailAccount = new DetailAccount
        {
            Id = id,
            ArabicName = "دائنون",
            EnglishName = "Creditors",
            ReferenceCode = "1",
            HeaderAccountId = Guid.NewGuid()
        };
        context.Set<DetailAccount>().Add(detailAccount);
        return detailAccount;
    }

    private static MainAccount SeedMainAccount(
        ERPContext context, Guid id, Guid detailAccountId, string arabicName, string englishName)
    {
        var mainAccount = new MainAccount
        {
            Id = id,
            ArabicName = arabicName,
            EnglishName = englishName,
            ReferenceCode = "MA-" + id.ToString("N")[..6],
            DetailAccountId = detailAccountId
        };
        context.Set<MainAccount>().Add(mainAccount);
        return mainAccount;
    }

    private static SupplierDetails SeedSupplier(
        ERPContext context, Guid id, string arabicName, string englishName, Guid? mainAccountId)
    {
        var supplier = new SupplierDetails
        {
            Id = id,
            ArabicName = arabicName,
            EnglishName = englishName,
            MainAccountId = mainAccountId,
            IsActive = true
        };
        context.Set<SupplierDetails>().Add(supplier);
        return supplier;
    }

    [Fact]
    public async Task UpdateAsync_ManuallyLinkedAccount_IsNotRenamed()
    {
        var dbName = Guid.NewGuid().ToString();
        var generator = CreateGenerator();
        var supplierId = Guid.NewGuid();
        var detailAccountId = Guid.NewGuid();
        var accountId = Guid.NewGuid();

        using (var context = CreateContext(dbName, generator))
        {
            SeedDetailAccount(context, detailAccountId);
            // Manually-linked/shared account: its names differ from the supplier's name.
            SeedMainAccount(context, accountId, detailAccountId, "موردين عام", "General Suppliers");
            SeedSupplier(context, supplierId, "المورد القديم", "Old Supplier", accountId);
            await context.SaveChangesAsync();
        }

        using (var context = CreateContext(dbName, generator))
        {
            var service = CreateService(context);
            var response = await service.UpdateAsync(new SupplierDetailsPutDto
            {
                Id = supplierId,
                ArabicName = "المورد الجديد",
                EnglishName = "New Supplier",
                MainAccountId = accountId
            });

            response.ErrorCode.Should().Be(ErrorCodes.Success);
        }

        using (var context = CreateContext(dbName, generator))
        {
            var supplier = await context.Set<SupplierDetails>().IgnoreQueryFilters().FirstAsync(s => s.Id == supplierId);
            supplier.ArabicName.Should().Be("المورد الجديد");
            supplier.MainAccountId.Should().Be(accountId);

            var account = await context.Set<MainAccount>().IgnoreQueryFilters().FirstAsync(a => a.Id == accountId);
            account.ArabicName.Should().Be("موردين عام");      // NOT renamed
            account.EnglishName.Should().Be("General Suppliers"); // NOT renamed
        }
    }

    [Fact]
    public async Task UpdateAsync_AutoCreatedAccount_IsRenamedWithSupplier()
    {
        var dbName = Guid.NewGuid().ToString();
        var generator = CreateGenerator();
        var supplierId = Guid.NewGuid();
        var detailAccountId = Guid.NewGuid();
        var accountId = Guid.NewGuid();

        using (var context = CreateContext(dbName, generator))
        {
            SeedDetailAccount(context, detailAccountId);
            // Auto-created account: named exactly like the supplier.
            SeedMainAccount(context, accountId, detailAccountId, "مورد تلقائي", "Auto Supplier");
            SeedSupplier(context, supplierId, "مورد تلقائي", "Auto Supplier", accountId);
            await context.SaveChangesAsync();
        }

        using (var context = CreateContext(dbName, generator))
        {
            var service = CreateService(context);
            var response = await service.UpdateAsync(new SupplierDetailsPutDto
            {
                Id = supplierId,
                ArabicName = "مورد تلقائي جديد",
                EnglishName = "Auto Supplier New",
                MainAccountId = accountId
            });

            response.ErrorCode.Should().Be(ErrorCodes.Success);
        }

        using (var context = CreateContext(dbName, generator))
        {
            var account = await context.Set<MainAccount>().IgnoreQueryFilters().FirstAsync(a => a.Id == accountId);
            account.ArabicName.Should().Be("مورد تلقائي جديد");   // renamed with the supplier
            account.EnglishName.Should().Be("Auto Supplier New");
        }
    }

    [Fact]
    public async Task UpdateAsync_ChangingAccount_PersistsNewAccountAndKeepsOldOne()
    {
        var dbName = Guid.NewGuid().ToString();
        var generator = CreateGenerator();
        var supplierId = Guid.NewGuid();
        var detailAccountId = Guid.NewGuid();
        var oldAccountId = Guid.NewGuid();
        var newAccountId = Guid.NewGuid();

        using (var context = CreateContext(dbName, generator))
        {
            SeedDetailAccount(context, detailAccountId);
            SeedMainAccount(context, oldAccountId, detailAccountId, "حساب قديم", "Old Account");
            SeedMainAccount(context, newAccountId, detailAccountId, "حساب جديد", "New Account");
            SeedSupplier(context, supplierId, "المورد الحالي", "Current Supplier", oldAccountId);
            await context.SaveChangesAsync();
        }

        using (var context = CreateContext(dbName, generator))
        {
            var service = CreateService(context);
            // Change the linked account AND the supplier name in one update.
            var response = await service.UpdateAsync(new SupplierDetailsPutDto
            {
                Id = supplierId,
                ArabicName = "المورد الجديد",
                EnglishName = "New Supplier",
                MainAccountId = newAccountId
            });

            response.ErrorCode.Should().Be(ErrorCodes.Success);
        }

        using (var context = CreateContext(dbName, generator))
        {
            var supplier = await context.Set<SupplierDetails>().IgnoreQueryFilters().FirstAsync(s => s.Id == supplierId);
            supplier.ArabicName.Should().Be("المورد الجديد");
            supplier.MainAccountId.Should().Be(newAccountId); // FK updated → change persists

            // I2: the old auto-created account is kept (never deleted).
            var oldAccount = await context.Set<MainAccount>().IgnoreQueryFilters().FirstAsync(a => a.Id == oldAccountId);
            oldAccount.ArabicName.Should().Be("حساب قديم");

            // The newly linked account is NOT renamed (manual — names differ from supplier's previous name).
            var newAccount = await context.Set<MainAccount>().IgnoreQueryFilters().FirstAsync(a => a.Id == newAccountId);
            newAccount.ArabicName.Should().Be("حساب جديد");
            newAccount.EnglishName.Should().Be("New Account");
        }
    }
}
