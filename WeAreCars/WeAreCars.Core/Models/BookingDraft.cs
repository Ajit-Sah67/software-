namespace WeAreCars.Core.Models;

/// <summary>Unconfirmed values entered on the booking form.</summary>
public sealed class BookingDraft
{
    public string FirstName { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string AgeText { get; set; } = string.Empty;
    public bool? HasValidDrivingLicense { get; set; }
    public int? RentalDays { get; set; }
    public CarType? CarType { get; set; }
    public FuelType? FuelType { get; set; }
    public bool UnlimitedMileage { get; set; }
    public bool BreakdownCover { get; set; }
}