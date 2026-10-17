using System;
using System.Collections.Generic;
using FluentAssertions;
using SmartAndPro.ERP.Application.Common.Dtos.Dashboard;
using SmartAndPro.ERP.Application.Services.Dashboard;
using SmartAndPro.ERP.SharedKernel.Constants;
using Xunit;

namespace SmartAndPro.ERP.Tests.ApplicationTests.Services.Dashboard;

public class DashboardKpiAccessTests
{
    private static DashboardKpiDto CreateRawDto() => new()
    {
        SalesTotal = 100m,
        PurchaseTotal = 200m,
        SalesCount = 10,
        PurchaseCount = 20,
        ReservationsCount = 30,
        ConfirmedCount = 5,
        InventoryItems = 15,
        CashIn30 = 500m,
        CashOut30 = 600m,
        Treasuries = 3,
        StoreCount = 4,
        SupplierCount = 7,
        BankAccountCount = 2
    };

    [Fact]
    public void Apply_AllPermissionsGranted_AllFieldsPopulated()
    {
        var raw = CreateRawDto();
        var allowed = new HashSet<string>
        {
            Permissions.Dashboard.SalesTotal,
            Permissions.Dashboard.PurchaseTotal,
            Permissions.Dashboard.SalesCount,
            Permissions.Dashboard.PurchaseCount,
            Permissions.Dashboard.ReservationsCount,
            Permissions.Dashboard.ReservationsConfirmed,
            Permissions.Dashboard.InventoryItems,
            Permissions.Dashboard.CashIn30,
            Permissions.Dashboard.CashOut30,
            Permissions.Dashboard.Treasuries,
            Permissions.Dashboard.StoreCount,
            Permissions.Dashboard.SupplierCount,
            Permissions.Dashboard.BankAccounts
        };

        var result = DashboardKpiAccess.Apply(raw, allowed);

        result.SalesTotal.Should().Be(100m);
        result.PurchaseTotal.Should().Be(200m);
        result.SalesCount.Should().Be(10);
        result.PurchaseCount.Should().Be(20);
        result.ReservationsCount.Should().Be(30);
        result.ConfirmedCount.Should().Be(5);
        result.InventoryItems.Should().Be(15);
        result.CashIn30.Should().Be(500m);
        result.CashOut30.Should().Be(600m);
        result.Treasuries.Should().Be(3);
        result.StoreCount.Should().Be(4);
        result.SupplierCount.Should().Be(7);
        result.BankAccountCount.Should().Be(2);
    }

    [Fact]
    public void Apply_EmptySet_AllFieldsNull()
    {
        var raw = CreateRawDto();
        var allowed = new HashSet<string>();

        var result = DashboardKpiAccess.Apply(raw, allowed);

        result.SalesTotal.Should().BeNull();
        result.PurchaseTotal.Should().BeNull();
        result.SalesCount.Should().BeNull();
        result.PurchaseCount.Should().BeNull();
        result.ReservationsCount.Should().BeNull();
        result.ConfirmedCount.Should().BeNull();
        result.InventoryItems.Should().BeNull();
        result.CashIn30.Should().BeNull();
        result.CashOut30.Should().BeNull();
        result.Treasuries.Should().BeNull();
        result.StoreCount.Should().BeNull();
        result.SupplierCount.Should().BeNull();
        result.BankAccountCount.Should().BeNull();
    }

    [Theory]
    [InlineData(Permissions.Dashboard.SalesTotal, nameof(DashboardKpiDto.SalesTotal), 100)]
    [InlineData(Permissions.Dashboard.PurchaseTotal, nameof(DashboardKpiDto.PurchaseTotal), 200)]
    [InlineData(Permissions.Dashboard.SalesCount, nameof(DashboardKpiDto.SalesCount), 10)]
    [InlineData(Permissions.Dashboard.PurchaseCount, nameof(DashboardKpiDto.PurchaseCount), 20)]
    [InlineData(Permissions.Dashboard.ReservationsCount, nameof(DashboardKpiDto.ReservationsCount), 30)]
    [InlineData(Permissions.Dashboard.ReservationsConfirmed, nameof(DashboardKpiDto.ConfirmedCount), 5)]
    [InlineData(Permissions.Dashboard.InventoryItems, nameof(DashboardKpiDto.InventoryItems), 15)]
    [InlineData(Permissions.Dashboard.CashIn30, nameof(DashboardKpiDto.CashIn30), 500)]
    [InlineData(Permissions.Dashboard.CashOut30, nameof(DashboardKpiDto.CashOut30), 600)]
    [InlineData(Permissions.Dashboard.Treasuries, nameof(DashboardKpiDto.Treasuries), 3)]
    [InlineData(Permissions.Dashboard.StoreCount, nameof(DashboardKpiDto.StoreCount), 4)]
    [InlineData(Permissions.Dashboard.SupplierCount, nameof(DashboardKpiDto.SupplierCount), 7)]
    [InlineData(Permissions.Dashboard.BankAccounts, nameof(DashboardKpiDto.BankAccountCount), 2)]
    public void Apply_SinglePermissionGranted_OnlyItsFieldPopulated(
        string permission, string fieldName, int expectedValue)
    {
        var raw = CreateRawDto();
        var allowed = new HashSet<string> { permission };

        var result = DashboardKpiAccess.Apply(raw, allowed);

        // Verify the granted field is populated
        var grantedValue = GetPropertyValue(result, fieldName);
        grantedValue.Should().NotBeNull();
        Convert.ToDecimal(grantedValue).Should().Be(expectedValue);

        // Verify all other fields are null
        var allFieldNames = new[]
        {
            nameof(DashboardKpiDto.SalesTotal),
            nameof(DashboardKpiDto.PurchaseTotal),
            nameof(DashboardKpiDto.SalesCount),
            nameof(DashboardKpiDto.PurchaseCount),
            nameof(DashboardKpiDto.ReservationsCount),
            nameof(DashboardKpiDto.ConfirmedCount),
            nameof(DashboardKpiDto.InventoryItems),
            nameof(DashboardKpiDto.CashIn30),
            nameof(DashboardKpiDto.CashOut30),
            nameof(DashboardKpiDto.Treasuries),
            nameof(DashboardKpiDto.StoreCount),
            nameof(DashboardKpiDto.SupplierCount),
            nameof(DashboardKpiDto.BankAccountCount)
        };

        foreach (var name in allFieldNames)
        {
            if (name != fieldName)
            {
                GetPropertyValue(result, name).Should().BeNull(
                    $"because {name} permission was not granted");
            }
        }
    }

    [Fact]
    public void Apply_MixedPartialSet_OnlyGrantedFieldsPopulated()
    {
        var raw = CreateRawDto();
        var allowed = new HashSet<string>
        {
            Permissions.Dashboard.SalesTotal,
            Permissions.Dashboard.CashIn30,
            Permissions.Dashboard.SupplierCount
        };

        var result = DashboardKpiAccess.Apply(raw, allowed);

        result.SalesTotal.Should().Be(100m);
        result.CashIn30.Should().Be(500m);
        result.SupplierCount.Should().Be(7);

        result.PurchaseTotal.Should().BeNull();
        result.SalesCount.Should().BeNull();
        result.PurchaseCount.Should().BeNull();
        result.ReservationsCount.Should().BeNull();
        result.ConfirmedCount.Should().BeNull();
        result.InventoryItems.Should().BeNull();
        result.CashOut30.Should().BeNull();
        result.Treasuries.Should().BeNull();
        result.StoreCount.Should().BeNull();
        result.BankAccountCount.Should().BeNull();
    }

    private static object? GetPropertyValue(DashboardKpiDto dto, string propertyName)
    {
        return typeof(DashboardKpiDto).GetProperty(propertyName)?.GetValue(dto);
    }
}
