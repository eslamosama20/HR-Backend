using System;
using System.Collections.Generic;
using FluentAssertions;
using SmartAndPro.ERP.Application.Common.Dtos.Dashboard;
using SmartAndPro.ERP.Application.Services.Dashboard;
using SmartAndPro.ERP.SharedKernel.Constants;
using Xunit;

namespace SmartAndPro.ERP.Tests.ApplicationTests.Services.Dashboard;

public class DashboardChartAccessTests
{
    private static DashboardChartDataDto CreateRawChart(string seriesName, int seriesCount = 2, bool withStatusCounts = false)
    {
        var series = new List<DashboardChartSeriesDto>();
        for (var i = 0; i < seriesCount; i++)
        {
            series.Add(new DashboardChartSeriesDto
            {
                Name = i == 0 ? seriesName : $"other_{i}",
                Data = [10m, 20m, 30m]
            });
        }

        var dto = new DashboardChartDataDto
        {
            Categories = ["Jan", "Feb", "Mar"],
            Series = series
        };

        if (withStatusCounts)
        {
            dto.StatusCounts = new Dictionary<string, int>
            {
                ["confirmed"] = 5,
                ["pending"] = 3,
                ["cancelled"] = 2
            };
        }

        return dto;
    }

    // ── Sales Purchase Chart ──────────────────────────────────────────

    [Fact]
    public void ApplySalesPurchase_AllGranted_BothSeriesPopulated()
    {
        var raw = CreateRawChart("sales", 2);
        raw.Series[1].Name = "purchases";
        var allowed = new HashSet<string>
        {
            Permissions.Dashboard.SalesTotal,
            Permissions.Dashboard.PurchaseTotal
        };

        var result = DashboardChartAccess.ApplySalesPurchase(raw, allowed);

        result.Series.Should().HaveCount(2);
        result.Series[0].Data.Should().BeEquivalentTo([10m, 20m, 30m]);
        result.Series[1].Data.Should().BeEquivalentTo([10m, 20m, 30m]);
    }

    [Fact]
    public void ApplySalesPurchase_EmptySet_BothSeriesEmpty()
    {
        var raw = CreateRawChart("sales", 2);
        raw.Series[1].Name = "purchases";
        var allowed = new HashSet<string>();

        var result = DashboardChartAccess.ApplySalesPurchase(raw, allowed);

        result.Series[0].Data.Should().BeEmpty();
        result.Series[1].Data.Should().BeEmpty();
    }

    [Fact]
    public void ApplySalesPurchase_OnlySalesGranted_SalesPopulatedPurchasesEmpty()
    {
        var raw = CreateRawChart("sales", 2);
        raw.Series[1].Name = "purchases";
        var allowed = new HashSet<string> { Permissions.Dashboard.SalesTotal };

        var result = DashboardChartAccess.ApplySalesPurchase(raw, allowed);

        result.Series[0].Name.Should().Be("sales");
        result.Series[0].Data.Should().BeEquivalentTo([10m, 20m, 30m]);
        result.Series[1].Name.Should().Be("purchases");
        result.Series[1].Data.Should().BeEmpty();
    }

    [Fact]
    public void ApplySalesPurchase_OnlyPurchasesGranted_PurchasesPopulatedSalesEmpty()
    {
        var raw = CreateRawChart("sales", 2);
        raw.Series[1].Name = "purchases";
        var allowed = new HashSet<string> { Permissions.Dashboard.PurchaseTotal };

        var result = DashboardChartAccess.ApplySalesPurchase(raw, allowed);

        result.Series[0].Data.Should().BeEmpty();
        result.Series[1].Data.Should().BeEquivalentTo([10m, 20m, 30m]);
    }

    // ── Reservations Chart ────────────────────────────────────────────

    [Fact]
    public void ApplyReservations_AllGranted_SeriesAndStatusCountsPopulated()
    {
        var raw = CreateRawChart("reservations", 1, withStatusCounts: true);
        var allowed = new HashSet<string>
        {
            Permissions.Dashboard.ReservationsCount,
            Permissions.Dashboard.ReservationsConfirmed
        };

        var result = DashboardChartAccess.ApplyReservations(raw, allowed);

        result.Series[0].Data.Should().BeEquivalentTo([10m, 20m, 30m]);
        result.StatusCounts.Should().HaveCount(3);
        result.StatusCounts!["confirmed"].Should().Be(5);
    }

    [Fact]
    public void ApplyReservations_EmptySet_SeriesEmptyStatusCountsEmpty()
    {
        var raw = CreateRawChart("reservations", 1, withStatusCounts: true);
        var allowed = new HashSet<string>();

        var result = DashboardChartAccess.ApplyReservations(raw, allowed);

        result.Series[0].Data.Should().BeEmpty();
        result.StatusCounts.Should().BeEmpty();
    }

    [Fact]
    public void ApplyReservations_OnlyReservationsGranted_SeriesPopulatedStatusCountsEmpty()
    {
        var raw = CreateRawChart("reservations", 1, withStatusCounts: true);
        var allowed = new HashSet<string> { Permissions.Dashboard.ReservationsCount };

        var result = DashboardChartAccess.ApplyReservations(raw, allowed);

        result.Series[0].Data.Should().BeEquivalentTo([10m, 20m, 30m]);
        result.StatusCounts.Should().BeEmpty();
    }

    [Fact]
    public void ApplyReservations_OnlyConfirmedGranted_SeriesEmptyStatusCountsPopulated()
    {
        var raw = CreateRawChart("reservations", 1, withStatusCounts: true);
        var allowed = new HashSet<string> { Permissions.Dashboard.ReservationsConfirmed };

        var result = DashboardChartAccess.ApplyReservations(raw, allowed);

        result.Series[0].Data.Should().BeEmpty();
        result.StatusCounts.Should().HaveCount(3);
        result.StatusCounts!["confirmed"].Should().Be(5);
    }

    // ── Treasury Chart ────────────────────────────────────────────────

    [Fact]
    public void ApplyTreasury_AllGranted_BothSeriesPopulated()
    {
        var raw = CreateRawChart("cashIn", 2);
        raw.Series[1].Name = "cashOut";
        var allowed = new HashSet<string>
        {
            Permissions.Dashboard.CashIn30,
            Permissions.Dashboard.CashOut30
        };

        var result = DashboardChartAccess.ApplyTreasury(raw, allowed);

        result.Series[0].Data.Should().BeEquivalentTo([10m, 20m, 30m]);
        result.Series[1].Data.Should().BeEquivalentTo([10m, 20m, 30m]);
    }

    [Fact]
    public void ApplyTreasury_EmptySet_BothSeriesEmpty()
    {
        var raw = CreateRawChart("cashIn", 2);
        raw.Series[1].Name = "cashOut";
        var allowed = new HashSet<string>();

        var result = DashboardChartAccess.ApplyTreasury(raw, allowed);

        result.Series[0].Data.Should().BeEmpty();
        result.Series[1].Data.Should().BeEmpty();
    }

    [Fact]
    public void ApplyTreasury_OnlyCashInGranted_CashInPopulatedCashOutEmpty()
    {
        var raw = CreateRawChart("cashIn", 2);
        raw.Series[1].Name = "cashOut";
        var allowed = new HashSet<string> { Permissions.Dashboard.CashIn30 };

        var result = DashboardChartAccess.ApplyTreasury(raw, allowed);

        result.Series[0].Data.Should().BeEquivalentTo([10m, 20m, 30m]);
        result.Series[1].Data.Should().BeEmpty();
    }

    // ── Inventory Chart ───────────────────────────────────────────────

    [Fact]
    public void ApplyInventory_AllGranted_SeriesPopulated()
    {
        var raw = CreateRawChart("inventory", 1);
        var allowed = new HashSet<string> { Permissions.Dashboard.InventoryItems };

        var result = DashboardChartAccess.ApplyInventory(raw, allowed);

        result.Series[0].Data.Should().BeEquivalentTo([10m, 20m, 30m]);
    }

    [Fact]
    public void ApplyInventory_EmptySet_SeriesEmpty()
    {
        var raw = CreateRawChart("inventory", 1);
        var allowed = new HashSet<string>();

        var result = DashboardChartAccess.ApplyInventory(raw, allowed);

        result.Series[0].Data.Should().BeEmpty();
    }

    // ── Categories preserved ──────────────────────────────────────────

    [Fact]
    public void ApplySalesPurchase_CategoriesPreservedWhenSeriesEmpty()
    {
        var raw = CreateRawChart("sales", 2);
        raw.Series[1].Name = "purchases";
        var allowed = new HashSet<string>();

        var result = DashboardChartAccess.ApplySalesPurchase(raw, allowed);

        result.Categories.Should().BeEquivalentTo(["Jan", "Feb", "Mar"]);
    }
}
