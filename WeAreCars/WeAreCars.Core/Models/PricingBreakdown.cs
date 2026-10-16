namespace WeAreCars.Core.Models;

/// <summary>Itemized GBP costs for a rental.</summary>
public sealed record PricingBreakdown(
    decimal BaseRentalCost,
    decimal CarSurcharge,
    decimal FuelSurcharge,
    decimal UnlimitedMileageCost,
    decimal BreakdownCoverCost)
{
    public decimal ExtraCharges => UnlimitedMileageCost + BreakdownCoverCost;
    public decimal Total => BaseRentalCost + CarSurcharge + FuelSurcharge + ExtraCharges;
}