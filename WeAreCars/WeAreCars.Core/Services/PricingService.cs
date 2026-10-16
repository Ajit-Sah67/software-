using WeAreCars.Core.Models;

namespace WeAreCars.Core.Services;

/// <summary>Calculates rental prices using the assignment's GBP tariff.</summary>
public sealed class PricingService
{
    public const decimal DailyBaseRate = 25m;
    public const decimal UnlimitedMileageDailyRate = 10m;
    public const decimal BreakdownCoverDailyRate = 2m;

    /// <summary>Calculates base, option and extra charges for one booking draft.</summary>
    public PricingBreakdown Calculate(BookingDraft draft)
    {
        ArgumentNullException.ThrowIfNull(draft);
        if (draft.RentalDays is not int days || days is < 1 or > 28)
        {
            throw new ArgumentOutOfRangeException(nameof(draft), "Rental days must be between 1 and 28.");
        }

        if (draft.CarType is null || draft.FuelType is null)
        {
            throw new ArgumentException("A car type and fuel type must be selected.", nameof(draft));
        }

        return new PricingBreakdown(
            days * DailyBaseRate,
            GetCarSurcharge(draft.CarType.Value),
            GetFuelSurcharge(draft.FuelType.Value),
            draft.UnlimitedMileage ? days * UnlimitedMileageDailyRate : 0m,
            draft.BreakdownCover ? days * BreakdownCoverDailyRate : 0m);
    }

    public static decimal GetCarSurcharge(CarType carType) => carType switch
    {
        CarType.CityCar => 0m,
        CarType.FamilyCar => 50m,
        CarType.SportsCar => 75m,
        CarType.Suv => 65m,
        _ => throw new ArgumentOutOfRangeException(nameof(carType))
    };

    public static decimal GetFuelSurcharge(FuelType fuelType) => fuelType switch
    {
        FuelType.Petrol or FuelType.Diesel => 0m,
        FuelType.Hybrid => 30m,
        FuelType.FullElectric => 50m,
        _ => throw new ArgumentOutOfRangeException(nameof(fuelType))
    };
}