using System;
using FluentAssertions;
using SmartAndPro.ERP.Application.Services.Dashboard;
using Xunit;

namespace SmartAndPro.ERP.Tests.ApplicationTests.Services.Dashboard;

public class DashboardCashWindowTests
{
    [Fact]
    public void GetWindow_WindowSpansExactly31DaysInclusive()
    {
        // Arrange
        var today = new DateOnly(2026, 8, 4);

        // Act
        var (from, to) = DashboardCashWindow.GetWindow(today);

        // Assert
        var daysInclusive = (to.DayNumber - from.DayNumber) + 1;
        daysInclusive.Should().Be(31, "because the window is 30 days plus today (inclusive)");
    }

    [Fact]
    public void GetWindow_ToEqualsReferenceDate()
    {
        // Arrange
        var today = new DateOnly(2026, 8, 4);

        // Act
        var (from, to) = DashboardCashWindow.GetWindow(today);

        // Assert
        to.Should().Be(today);
    }

    [Fact]
    public void GetWindow_FromIsExactly30DaysBeforeTo()
    {
        // Arrange
        var today = new DateOnly(2026, 8, 4);

        // Act
        var (from, to) = DashboardCashWindow.GetWindow(today);

        // Assert
        var daysDifference = to.DayNumber - from.DayNumber;
        daysDifference.Should().Be(30);
    }

    [Fact]
    public void GetWindow_WorksAcrossMonthBoundary()
    {
        // Arrange: March 1 => From should be January 30
        var today = new DateOnly(2026, 3, 1);

        // Act
        var (from, to) = DashboardCashWindow.GetWindow(today);

        // Assert
        from.Should().Be(new DateOnly(2026, 1, 30));
        to.Should().Be(new DateOnly(2026, 3, 1));
    }

    [Fact]
    public void GetWindow_WorksAcrossYearBoundary()
    {
        // Arrange: Jan 5 => From should be Dec 6 of previous year
        var today = new DateOnly(2026, 1, 5);

        // Act
        var (from, to) = DashboardCashWindow.GetWindow(today);

        // Assert
        from.Should().Be(new DateOnly(2025, 12, 6));
        to.Should().Be(new DateOnly(2026, 1, 5));
    }

    [Fact]
    public void GetWindow_FromIsAlwaysBeforeTo()
    {
        // Arrange & Act & Assert
        var testDates = new[]
        {
            new DateOnly(2026, 8, 4),
            new DateOnly(2026, 1, 1),
            new DateOnly(2026, 12, 31),
            new DateOnly(2025, 2, 28),
            new DateOnly(2024, 2, 29) // Leap year
        };

        foreach (var today in testDates)
        {
            var (from, to) = DashboardCashWindow.GetWindow(today);
            from.Should().BeBefore(to, "From must always be before To for {0}", today);
        }
    }

    [Fact]
    public void GetWindow_WindowDaysConstantEquals30()
    {
        // Assert
        DashboardCashWindow.WindowDays.Should().Be(30);
    }

    [Fact]
    public void GetWindow_WorksForLeapYearDate()
    {
        // Arrange: Feb 29 in leap year
        var today = new DateOnly(2024, 2, 29);

        // Act
        var (from, to) = DashboardCashWindow.GetWindow(today);

        // Assert
        from.Should().Be(new DateOnly(2024, 1, 30));
        to.Should().Be(new DateOnly(2024, 2, 29));
    }
}
