using System.Collections.Generic;
using FluentAssertions;
using SmartAndPro.ERP.Application.Services.Dashboard;
using Xunit;

namespace SmartAndPro.ERP.Tests.ApplicationTests.Services.Dashboard;

public class DashboardInventoryAccessTests
{
    private static List<(string Name, decimal Total)> CreateItems(params (string Name, decimal Total)[] items)
        => new(items);

    [Fact]
    public void RankTopItems_SameNameInBothLists_CombinesTotal()
    {
        var purchase = CreateItems(("Widget", 100m));
        var sales = CreateItems(("Widget", 250m));

        var result = DashboardInventoryAccess.RankTopItems(purchase, sales);

        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Widget");
        result[0].Total.Should().Be(350m);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    public void RankTopItems_NullOrWhitespaceName_DroppedFromOutput(string? name)
    {
        var purchase = CreateItems((name!, 50m));
        var sales = CreateItems();

        var result = DashboardInventoryAccess.RankTopItems(purchase, sales);

        result.Should().BeEmpty();
    }

    [Fact]
    public void RankTopItems_ZeroAndNegativeTotals_DroppedFromOutput()
    {
        var purchase = CreateItems(("Zero", 0m), ("Negative", -10m));
        var sales = CreateItems(("AlsoNeg", -5m));

        var result = DashboardInventoryAccess.RankTopItems(purchase, sales);

        result.Should().BeEmpty();
    }

    [Fact]
    public void RankTopItems_MultipleItems_OrderedByTotalDescendingThenNameAscending()
    {
        var purchase = CreateItems(("Bravo", 300m), ("Alpha", 300m));
        var sales = CreateItems(("Charlie", 100m), ("Delta", 500m));

        var result = DashboardInventoryAccess.RankTopItems(purchase, sales);

        result.Should().HaveCount(4);
        result[0].Name.Should().Be("Delta");
        result[0].Total.Should().Be(500m);
        result[1].Name.Should().Be("Alpha");
        result[1].Total.Should().Be(300m);
        result[2].Name.Should().Be("Bravo");
        result[2].Total.Should().Be(300m);
        result[3].Name.Should().Be("Charlie");
        result[3].Total.Should().Be(100m);
    }

    [Fact]
    public void RankTopItems_MoreThanSixItems_OnlySixReturned()
    {
        var purchase = CreateItems(
            ("A", 10m),
            ("B", 20m),
            ("C", 30m),
            ("D", 40m),
            ("E", 50m),
            ("F", 60m),
            ("G", 70m));
        var sales = CreateItems();

        var result = DashboardInventoryAccess.RankTopItems(purchase, sales);

        result.Should().HaveCount(6);
        result[0].Name.Should().Be("G");
        result[5].Name.Should().Be("B");
        result.Should().NotContain(x => x.Name == "A");
    }

    [Fact]
    public void RankTopItems_BothListsEmpty_ReturnsEmptyOutput()
    {
        var purchase = new List<(string Name, decimal Total)>();
        var sales = new List<(string Name, decimal Total)>();

        var result = DashboardInventoryAccess.RankTopItems(purchase, sales);

        result.Should().BeEmpty();
    }

    [Fact]
    public void RankTopItems_SingleItemInPurchaseListOnly_ReturnsThatItem()
    {
        var purchase = CreateItems(("OnlyPurchase", 75m));
        var sales = new List<(string Name, decimal Total)>();

        var result = DashboardInventoryAccess.RankTopItems(purchase, sales);

        result.Should().HaveCount(1);
        result[0].Name.Should().Be("OnlyPurchase");
        result[0].Total.Should().Be(75m);
    }

    [Fact]
    public void RankTopItems_SingleItemInSalesListOnly_ReturnsThatItem()
    {
        var purchase = new List<(string Name, decimal Total)>();
        var sales = CreateItems(("OnlySales", 120m));

        var result = DashboardInventoryAccess.RankTopItems(purchase, sales);

        result.Should().HaveCount(1);
        result[0].Name.Should().Be("OnlySales");
        result[0].Total.Should().Be(120m);
    }
}
