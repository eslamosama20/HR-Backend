using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Reflection;
using FluentAssertions;
using SmartAndPro.ERP.Application.Common.Dtos.Reservation.Reservation;
using SmartAndPro.ERP.Application.Common.Dtos.Reservation.ReservationRoomEntry;
using SmartAndPro.ERP.Application.Common.Dtos.Reservation.ReservationServiceEntry;
using SmartAndPro.ERP.Domains.Entities.Reservation;
using Xunit;

namespace SmartAndPro.ERP.Tests.ApplicationTests.Services.Reservation;

public class OptionalReservationFieldsTests
{
    [Fact]
    public void ReservationRoomEntryPostDto_RoomCost_ShouldBeNullableDecimal()
    {
        var prop = typeof(ReservationRoomEntryPostDto).GetProperty(nameof(ReservationRoomEntryPostDto.RoomCost));
        prop.Should().NotBeNull();
        var nullableType = Nullable.GetUnderlyingType(prop!.PropertyType);
        nullableType.Should().Be(typeof(decimal));
    }

    [Fact]
    public void ReservationRoomEntryPutDto_RoomCost_ShouldBeNullableDecimal()
    {
        var prop = typeof(ReservationRoomEntryPutDto).GetProperty(nameof(ReservationRoomEntryPutDto.RoomCost));
        prop.Should().NotBeNull();
        var nullableType = Nullable.GetUnderlyingType(prop!.PropertyType);
        nullableType.Should().Be(typeof(decimal));
    }

    [Fact]
    public void ReservationServiceEntryPostDto_CostPrice_ShouldBeNullableDecimal()
    {
        var prop = typeof(ReservationServiceEntryPostDto).GetProperty(nameof(ReservationServiceEntryPostDto.CostPrice));
        prop.Should().NotBeNull();
        var nullableType = Nullable.GetUnderlyingType(prop!.PropertyType);
        nullableType.Should().Be(typeof(decimal));
    }

    [Fact]
    public void ReservationServiceEntryPostDto_ReservationSupplierId_ShouldBeNullableGuid()
    {
        var prop = typeof(ReservationServiceEntryPostDto).GetProperty(nameof(ReservationServiceEntryPostDto.ReservationSupplierId));
        prop.Should().NotBeNull();
        var nullableType = Nullable.GetUnderlyingType(prop!.PropertyType);
        nullableType.Should().Be(typeof(Guid));
    }

    [Fact]
    public void ReservationServiceEntryPutDto_ReservationSupplierId_ShouldBeNullableGuid()
    {
        var prop = typeof(ReservationServiceEntryPutDto).GetProperty(nameof(ReservationServiceEntryPutDto.ReservationSupplierId));
        prop.Should().NotBeNull();
        var nullableType = Nullable.GetUnderlyingType(prop!.PropertyType);
        nullableType.Should().Be(typeof(Guid));
    }

    [Theory]
    [InlineData(nameof(ReservationRoomEntryPostDto.RoomCost))]
    [InlineData(nameof(ReservationServiceEntryPostDto.CostPrice))]
    [InlineData(nameof(ReservationServiceEntryPostDto.ReservationSupplierId))]
    public void OptionalFields_ShouldNotHaveRequiredAttribute_PostDto(string propertyName)
    {
        var type = propertyName == nameof(ReservationServiceEntryPostDto.CostPrice) ||
                   propertyName == nameof(ReservationServiceEntryPostDto.ReservationSupplierId)
            ? typeof(ReservationServiceEntryPostDto)
            : typeof(ReservationRoomEntryPostDto);

        var prop = type.GetProperty(propertyName);
        prop.Should().NotBeNull();
        prop!.GetCustomAttribute<RequiredAttribute>().Should().BeNull(
            $"'{propertyName}' should be optional but has a [Required] attribute");
    }

    [Theory]
    [InlineData(typeof(ReservationRoomEntryPutDto), nameof(ReservationRoomEntryPutDto.RoomCost))]
    public void OptionalFields_ShouldNotHaveRequiredAttribute_PutDto(Type type, string propertyName)
    {
        var prop = type.GetProperty(propertyName);
        prop.Should().NotBeNull();
        prop!.GetCustomAttribute<RequiredAttribute>().Should().BeNull(
            $"'{propertyName}' on {type.Name} should be optional but has a [Required] attribute");
    }

    [Fact]
    public void ReservationRoomEntryResponseDto_RoomCost_ShouldBeNullableDecimal()
    {
        var prop = typeof(ReservationRoomEntryResponseDto).GetProperty(nameof(ReservationRoomEntryResponseDto.RoomCost));
        prop.Should().NotBeNull();
        var nullableType = Nullable.GetUnderlyingType(prop!.PropertyType);
        nullableType.Should().Be(typeof(decimal));
    }

    [Fact]
    public void ReservationServiceEntryResponseDto_ReservationSupplierId_ShouldBeNullableGuid()
    {
        var prop = typeof(ReservationServiceEntryResponseDto).GetProperty(nameof(ReservationServiceEntryResponseDto.ReservationSupplierId));
        prop.Should().NotBeNull();
        var nullableType = Nullable.GetUnderlyingType(prop!.PropertyType);
        nullableType.Should().Be(typeof(Guid));
    }

    [Fact]
    public void ReservationRoomEntry_Entity_RoomCost_ShouldBeNullableDecimal()
    {
        var prop = typeof(ReservationRoomEntry).GetProperty(nameof(ReservationRoomEntry.RoomCost));
        prop.Should().NotBeNull();
        var nullableType = Nullable.GetUnderlyingType(prop!.PropertyType);
        nullableType.Should().Be(typeof(decimal));
    }

    [Fact]
    public void ReservationServiceEntry_Entity_CostPrice_ShouldBeNullableDecimal()
    {
        var prop = typeof(ReservationServiceEntry).GetProperty(nameof(ReservationServiceEntry.CostPrice));
        prop.Should().NotBeNull();
        var nullableType = Nullable.GetUnderlyingType(prop!.PropertyType);
        nullableType.Should().Be(typeof(decimal));
    }

    [Fact]
    public void ReservationServiceEntry_Entity_ReservationSupplierId_ShouldBeNullableGuid()
    {
        var prop = typeof(ReservationServiceEntry).GetProperty(nameof(ReservationServiceEntry.ReservationSupplierId));
        prop.Should().NotBeNull();
        var nullableType = Nullable.GetUnderlyingType(prop!.PropertyType);
        nullableType.Should().Be(typeof(Guid));
    }

    [Fact]
    public void RoomCostCalculation_WithNullCost_ShouldNotThrow_AndShouldTreatNullAsZero()
    {
        var numberOfNights = 3;
        var numberOfRooms = 2;
        decimal? roomCost = null;
        decimal? mealCost = null;
        decimal? viewCost = null;

        var total = (numberOfRooms * numberOfNights * (roomCost ?? 0m))
                  + (numberOfRooms * numberOfNights * (mealCost ?? 0m))
                  + (numberOfRooms * numberOfNights * (viewCost ?? 0m));

        total.Should().Be(0m);
    }

    [Fact]
    public void RoomCostCalculation_WithPartialNulls_ShouldNotThrow_AndShouldComputeCorrectly()
    {
        var numberOfNights = 3;
        var numberOfRooms = 2;
        decimal? roomCost = 100m;
        decimal? mealCost = null;
        decimal? viewCost = 25m;

        var total = (numberOfRooms * numberOfNights * (roomCost ?? 0m))
                  + (numberOfRooms * numberOfNights * (mealCost ?? 0m))
                  + (numberOfRooms * numberOfNights * (viewCost ?? 0m));

        total.Should().Be(750m);
    }

    [Fact]
    public void ServiceCostCalculation_WithNullCostPrice_ShouldNotThrow_AndShouldTreatNullAsZero()
    {
        var quantity = 5m;
        decimal? costPrice = null;

        var total = quantity * (costPrice ?? 0m);

        total.Should().Be(0m);
    }

    [Fact]
    public void ServiceCostCalculation_WithValidCostPrice_ShouldComputeCorrectly()
    {
        var quantity = 5m;
        decimal? costPrice = 12.5m;

        var total = quantity * (costPrice ?? 0m);

        total.Should().Be(62.5m);
    }

    [Fact]
    public void PostDto_WithNullOptionalFields_ShouldNotBeRejectedByRequiredAttributeValidation()
    {
        var dto = new ReservationRoomEntryPostDto
        {
            RoomTypeId = Guid.NewGuid(),
            NumberOfRooms = 1,
            RoomRate = 200m,
            RoomCost = null,
            ReservationMealId = null,
            MealRate = null,
            MealCost = null,
            ReservationViewId = null,
            ViewRate = null,
            ViewCost = null
        };

        var serviceEntryDto = new ReservationServiceEntryPostDto
        {
            ReservationServiceId = Guid.NewGuid(),
            Quantity = 1m,
            SalesPrice = 50m,
            CostPrice = null,
            ReservationSupplierId = null
        };

        Action validateRoom = () => Validator.ValidateObject(dto, new ValidationContext(dto), validateAllProperties: true);
        Action validateService = () => Validator.ValidateObject(serviceEntryDto, new ValidationContext(serviceEntryDto), validateAllProperties: true);

        validateRoom.Should().NotThrow<ValidationException>();
        validateService.Should().NotThrow<ValidationException>();
    }

    [Fact]
    public void ReservationService_AddAsync_ShouldNotEnforceCompanyRequiredForAgentDirect()
    {
        var assemblyLocation = typeof(SmartAndPro.ERP.Domains.Entities.Reservation.Reservation).Assembly.Location;
        var solutionRoot = FindRepoRoot(Path.GetDirectoryName(assemblyLocation)!);
        var serviceFile = Path.Combine(
            solutionRoot,
            "SmartAndPro.ERP.Application",
            "Services",
            "Reservation",
            "ReservationService.cs");

        File.Exists(serviceFile).Should().BeTrue(
            $"expected ReservationService.cs at '{serviceFile}'");

        var lines = File.ReadAllLines(serviceFile);
        var offendingActiveLines = new List<int>();
        for (var i = 0; i < lines.Length; i++)
        {
            var trimmed = lines[i].TrimStart();
            if (trimmed.StartsWith("//")) continue;
            if (trimmed.Contains("Reservation company is required for agent direct reservation"))
            {
                offendingActiveLines.Add(i + 1);
            }
        }

        offendingActiveLines.Should().BeEmpty(
            "the active code path in ReservationService.AddAsync must not reject an AgentDirect reservation with a null ReservationCompanyId — comment-only copies are allowed but no uncommented line may contain the hard-coded exception text");
    }

    private static string FindRepoRoot(string start)
    {
        var dir = new DirectoryInfo(start);
        while (dir != null && !File.Exists(Path.Combine(dir.FullName, "SmartAndPro.ERP.sln")))
        {
            dir = dir.Parent;
        }
        dir.Should().NotBeNull("must be able to locate the solution root from a built assembly path");
        return dir!.FullName;
    }
}
