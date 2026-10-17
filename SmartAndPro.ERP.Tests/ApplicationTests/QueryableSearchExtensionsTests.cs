using System.Collections.Generic;
using System.Linq;
using SmartAndPro.ERP.Application.Common.Extensions;
using SmartAndPro.ERP.SharedKernel.Entities.Attributes;
using Xunit;

namespace SmartAndPro.ERP.Tests.ApplicationTests;

public class QueryableSearchExtensionsTests
{
    [Fact]
    public void ApplySearchKeyword_NullKeyword_ReturnsAllRecords()
    {
        // Arrange
        var entities = GetTestEntities();

        // Act
        var result = entities.AsQueryable().ApplySearchKeyword(null).ToList();

        // Assert
        Assert.Equal(3, result.Count);
    }

    [Fact]
    public void ApplySearchKeyword_EmptyKeyword_ReturnsAllRecords()
    {
        // Arrange
        var entities = GetTestEntities();

        // Act
        var result = entities.AsQueryable().ApplySearchKeyword("").ToList();

        // Assert
        Assert.Equal(3, result.Count);
    }

    [Fact]
    public void ApplySearchKeyword_WhitespaceKeyword_ReturnsAllRecords()
    {
        // Arrange
        var entities = GetTestEntities();

        // Act
        var result = entities.AsQueryable().ApplySearchKeyword("   ").ToList();

        // Assert
        Assert.Equal(3, result.Count);
    }

    [Fact]
    public void ApplySearchKeyword_SingleMatch_ReturnsMatchingRecord()
    {
        // Arrange
        var entities = GetTestEntities();

        // Act
        var result = entities.AsQueryable().ApplySearchKeyword("hilton").ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal("Hilton Hotel", result[0].ArabicName);
    }

    [Fact]
    public void ApplySearchKeyword_MatchesAcrossProperties_ReturnsAll()
    {
        // Arrange
        var entities = GetTestEntities();

        // Act — "marriott" only appears in entity 2's EnglishName
        var result = entities.AsQueryable().ApplySearchKeyword("marriott").ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal("فندق ماريوت", result[0].ArabicName);
    }

    [Fact]
    public void ApplySearchKeyword_CaseInsensitive_MatchesRegardless()
    {
        // Arrange
        var entities = GetTestEntities();

        // Act
        var upperResult = entities.AsQueryable().ApplySearchKeyword("HILTON").ToList();
        var mixedResult = entities.AsQueryable().ApplySearchKeyword("HiLtOn").ToList();

        // Assert
        Assert.Single(upperResult);
        Assert.Single(mixedResult);
        Assert.Equal(upperResult[0].Id, mixedResult[0].Id);
    }

    [Fact]
    public void ApplySearchKeyword_NoMatch_ReturnsEmpty()
    {
        // Arrange
        var entities = GetTestEntities();

        // Act
        var result = entities.AsQueryable().ApplySearchKeyword("xyz_nonexistent").ToList();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void ApplySearchKeyword_MatchesMultipleRecords_ReturnsAll()
    {
        // Arrange
        var entities = GetTestEntities();

        // Act — "hotel" appears in EnglishName of entities 1, 2, and 3
        var result = entities.AsQueryable().ApplySearchKeyword("hotel").ToList();

        // Assert
        Assert.Equal(3, result.Count);
    }

    [Fact]
    public void ApplySearchKeyword_ArabicContent_MatchesArabicName()
    {
        // Arrange
        var entities = GetTestEntities();

        // Act — search by Arabic substring
        var result = entities.AsQueryable().ApplySearchKeyword("ماريوت").ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal("فندق ماريوت", result[0].ArabicName);
    }

    [Fact]
    public void ApplySearchKeyword_OrPredicate_MatchesEitherProperty()
    {
        // Arrange — entity 1 has "Hilton Hotel" in both ArabicName and EnglishName
        //           entity 3 has "Small Hotel" only in EnglishName
        var entities = GetTestEntities();

        // Act — "hilton" matches entity 1 via ArabicName OR EnglishName
        var result = entities.AsQueryable().ApplySearchKeyword("hilton").ToList();

        // Assert — only entity 1 matches
        Assert.Single(result);
        Assert.Equal(1, result[0].Id);
    }

    [Fact]
    public void ApplySearchKeyword_NoSearchableProperties_ReturnsAll()
    {
        // Arrange — entity has no [Searchable] properties
        var entities = new List<NonSearchableEntity>
        {
            new() { Id = 1, Name = "Test" },
            new() { Id = 2, Name = "Another" }
        };

        // Act
        var result = entities.AsQueryable().ApplySearchKeyword("test").ToList();

        // Assert — no searchable properties, so return empty (footgun protection)
        Assert.Empty(result);
    }

    [Fact]
    public void ApplySearchKeyword_NotSearchableAttribute_ExcludesProperty()
    {
        // Arrange
        var entities = new List<PartiallySearchableEntity>
        {
            new() { Id = 1, SearchableField = "FindMe", IgnoredField = "IgnoreMe" },
            new() { Id = 2, SearchableField = "Other", IgnoredField = "FindMe" }
        };

        // Act — "FindMe" exists in IgnoredField of entity 2, but that field is excluded
        var result = entities.AsQueryable().ApplySearchKeyword("FindMe").ToList();

        // Assert — only entity 1 matches (via SearchableField)
        Assert.Single(result);
        Assert.Equal(1, result[0].Id);
    }

    [Fact]
    public void ApplySearchKeyword_SubstringMatch_Works()
    {
        // Arrange
        var entities = GetTestEntities();

        // Act — partial word match
        var result = entities.AsQueryable().ApplySearchKeyword("Hil").ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal("Hilton Hotel", result[0].ArabicName);
    }

    private static List<TestSearchEntity> GetTestEntities()
    {
        return new List<TestSearchEntity>
        {
            new() { Id = 1, ArabicName = "Hilton Hotel", EnglishName = "Hilton Hotel" },
            new() { Id = 2, ArabicName = "فندق ماريوت", EnglishName = "Marriott Hotel" },
            new() { Id = 3, ArabicName = "فندق صغير", EnglishName = "Small Hotel" }
        };
    }

    public class TestSearchEntity
    {
        public int Id { get; set; }

        [Searchable]
        public string ArabicName { get; set; } = string.Empty;

        [Searchable]
        public string EnglishName { get; set; } = string.Empty;

        public string NonSearchableField { get; set; } = string.Empty;
    }

    public class NonSearchableEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class PartiallySearchableEntity
    {
        public int Id { get; set; }

        [Searchable]
        public string SearchableField { get; set; } = string.Empty;

        [NotSearchable]
        public string IgnoredField { get; set; } = string.Empty;
    }
}
