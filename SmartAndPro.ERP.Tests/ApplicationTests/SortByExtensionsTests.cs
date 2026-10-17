using FluentAssertions;
using SmartAndPro.ERP.Application.Common.Extensions;
using SmartAndPro.ERP.Domains.Common.Models;
using SmartAndPro.ERP.Domains.Entities.Reservation;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace SmartAndPro.ERP.Tests.ApplicationTests;

public class SortByExtensionsTests
{
    private IQueryable<Hotel> CreateTestQueryable()
    {
        return new List<Hotel>
        {
            new Hotel { Id = Guid.NewGuid(), ArabicName = "فندق ب", EnglishName = "Hotel B", CreatedAt = new DateTime(2024, 1, 2) },
            new Hotel { Id = Guid.NewGuid(), ArabicName = "فندق أ", EnglishName = "Hotel A", CreatedAt = new DateTime(2024, 1, 1) },
            new Hotel { Id = Guid.NewGuid(), ArabicName = "فندق ج", EnglishName = "Hotel C", CreatedAt = new DateTime(2024, 1, 3) }
        }.AsQueryable();
    }

    [Fact]
    public void ApplySort_ShouldSortByFieldName_WhenFieldNameIsValid()
    {
        // Arrange
        var query = CreateTestQueryable();

        // Act - Sort by EnglishName ascending
        var result = query.ApplySort("EnglishName", 1).ToList();

        // Assert
        result.Should().HaveCount(3);
        result[0].EnglishName.Should().Be("Hotel A");
        result[1].EnglishName.Should().Be("Hotel B");
        result[2].EnglishName.Should().Be("Hotel C");
    }

    [Fact]
    public void ApplySort_ShouldSortByFieldNameCaseInsensitive_WhenFieldNameIsCamelCase()
    {
        // Arrange
        var query = CreateTestQueryable();

        // Act - Sort by "englishName" (camelCase) instead of "EnglishName" (PascalCase)
        var result = query.ApplySort("englishName", 1).ToList();

        // Assert
        result.Should().HaveCount(3);
        result[0].EnglishName.Should().Be("Hotel A");
        result[1].EnglishName.Should().Be("Hotel B");
        result[2].EnglishName.Should().Be("Hotel C");
    }

    [Fact]
    public void ApplySort_ShouldSortDescending_WhenSortOrderIsNegative()
    {
        // Arrange
        var query = CreateTestQueryable();

        // Act - Sort by EnglishName descending
        var result = query.ApplySort("EnglishName", -1).ToList();

        // Assert
        result.Should().HaveCount(3);
        result[0].EnglishName.Should().Be("Hotel C");
        result[1].EnglishName.Should().Be("Hotel B");
        result[2].EnglishName.Should().Be("Hotel A");
    }

    [Fact]
    public void ApplySort_ShouldSortByCreatedAtDescending_WhenSortFieldIsNull()
    {
        // Arrange
        var query = CreateTestQueryable();

        // Act - No sort field, should default to CreatedAt descending
        var result = query.ApplySort(null, null).ToList();

        // Assert
        result.Should().HaveCount(3);
        result[0].CreatedAt.Should().Be(new DateTime(2024, 1, 3));
        result[1].CreatedAt.Should().Be(new DateTime(2024, 1, 2));
        result[2].CreatedAt.Should().Be(new DateTime(2024, 1, 1));
    }

    [Fact]
    public void ApplySort_ShouldThrowArgumentException_WhenSortFieldDoesNotExist()
    {
        // Arrange
        var query = CreateTestQueryable();

        // Act & Assert
        var act = () => query.ApplySort("NonExistentField", 1).ToList();
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Sort field 'NonExistentField' does not exist*");
    }

    [Fact]
    public void ApplySort_ShouldThrowArgumentException_WhenSortFieldIsNavigationProperty()
    {
        // Arrange
        var query = CreateTestQueryable();

        // Act & Assert - "HeaderAccounts" is a navigation property, not sortable
        var act = () => query.ApplySort("HeaderAccounts", 1).ToList();
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Sort field 'HeaderAccounts' does not exist*");
    }

    [Fact]
    public void ApplySort_ShouldSortByArabicName_WhenSortFieldIsArabicName()
    {
        // Arrange
        var query = CreateTestQueryable();

        // Act - Sort by ArabicName ascending
        var result = query.ApplySort("ArabicName", 1).ToList();

        // Assert
        result.Should().HaveCount(3);
        result[0].ArabicName.Should().Be("فندق أ");
        result[1].ArabicName.Should().Be("فندق ب");
        result[2].ArabicName.Should().Be("فندق ج");
    }
}
