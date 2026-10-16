namespace WeAreCars.Core.Models;

/// <summary>A validated rental booking that can be persisted.</summary>
public sealed class Booking
{
    public string BookingId { get; set; } = string.Empty;
    public string CustomerFirstName { get; set; } = string.Empty;
    public string CustomerSurname { get; set; } = string.Empty;
    public string CustomerAddress { get; set; } = string.Empty;
    public int CustomerAge { get; set; }
    public bool HasValidDrivingLicense { get; set; }
    public CarType CarType { get; set; }
    public FuelType FuelType { get; set; }
    public int RentalDays { get; set; }
    public bool UnlimitedMileage { get; set; }
    public bool BreakdownCover { get; set; }
    public decimal BaseRentalCost { get; set; }
    public decimal CarSurcharge { get; set; }
    public decimal FuelSurcharge { get; set; }
    public decimal ExtraCharges { get; set; }
    public decimal TotalPrice { get; set; }
    public BookingStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ReturnDate { get; set; }
}