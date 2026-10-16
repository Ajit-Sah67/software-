using WeAreCars.Core.Models;
using WeAreCars.Core.Services;

namespace WeAreCars.Tests;

public sealed class ValidationServiceTests
{
    private readonly ValidationService validationService = new();

    [Fact]
    public void Validate_AcceptsCompleteDraftAtMinimumAge()
    {
        var errors = validationService.Validate(new BookingDraft
        {
            FirstName = "Avery",
            Surname = "Taylor",
            Address = "1 High Street",
            AgeText = "18",
            HasValidDrivingLicense = true,
            RentalDays = 28,
            CarType = CarType.CityCar,
            FuelType = FuelType.Petrol
        });

        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_RejectsCustomerYoungerThan18()
    {
        var errors = validationService.Validate(new BookingDraft
        {
            FirstName = "Avery",
            Surname = "Taylor",
            Address = "1 High Street",
            AgeText = "17",
            HasValidDrivingLicense = true,
            RentalDays = 1,
            CarType = CarType.CityCar,
            FuelType = FuelType.Petrol
        });

        Assert.Contains(errors, error => error.Contains("at least 18", StringComparison.Ordinal));
    }

    [Fact]
    public void Validate_RejectsMissingLicenseAndInvalidAge()
    {
        var errors = validationService.Validate(new BookingDraft
        {
            FirstName = "Avery",
            Surname = "Taylor",
            Address = "1 High Street",
            AgeText = "twenty",
            HasValidDrivingLicense = false,
            RentalDays = 1,
            CarType = CarType.CityCar,
            FuelType = FuelType.Petrol
        });

        Assert.Equal(2, errors.Count);
    }
}