using WeAreCars.Core.Models;
using WeAreCars.Core.Services;

namespace WeAreCars.Tests;

public sealed class PricingServiceTests
{
    private readonly PricingService pricingService = new();

    [Theory]
    [InlineData(CarType.CityCar, FuelType.Petrol, 2, false, false, 50)]
    [InlineData(CarType.FamilyCar, FuelType.Diesel, 7, false, false, 225)]
    [InlineData(CarType.SportsCar, FuelType.FullElectric, 3, false, false, 200)]
    [InlineData(CarType.Suv, FuelType.Hybrid, 1, true, true, 132)]
    [InlineData(CarType.CityCar, FuelType.Petrol, 28, true, true, 1036)]
    public void Calculate_ComposesAllCharges(CarType carType, FuelType fuelType, int days,
        bool mileage, bool breakdown, decimal expectedTotal)
    {
        var result = pricingService.Calculate(new BookingDraft
        {
            RentalDays = days,
            CarType = carType,
            FuelType = fuelType,
            UnlimitedMileage = mileage,
            BreakdownCover = breakdown
        });

        Assert.Equal(expectedTotal, result.Total);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(29)]
    public void Calculate_RejectsDaysOutsideAllowedRange(int days) =>
        Assert.Throws<ArgumentOutOfRangeException>(() => pricingService.Calculate(new BookingDraft
        {
            RentalDays = days,
            CarType = CarType.CityCar,
            FuelType = FuelType.Petrol
        }));
}