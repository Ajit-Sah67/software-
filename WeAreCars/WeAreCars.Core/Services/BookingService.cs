using System.Globalization;
using WeAreCars.Core.Models;

namespace WeAreCars.Core.Services;

/// <summary>Turns valid form values into a priced booking.</summary>
public sealed class BookingService(ValidationService validationService, PricingService pricingService)
{
    /// <summary>Validates, prices and assigns an identifier to a confirmed booking.</summary>
    public Booking Create(BookingDraft draft, DateTime? createdAt = null)
    {
        var errors = validationService.Validate(draft);
        if (errors.Count > 0)
        {
            throw new ArgumentException(string.Join(Environment.NewLine, errors), nameof(draft));
        }

        var created = createdAt ?? DateTime.Now;
        var price = pricingService.Calculate(draft);
        return new Booking
        {
            BookingId = $"WAC-{Guid.NewGuid():N}"[..16].ToUpperInvariant(),
            CustomerFirstName = draft.FirstName.Trim(),
            CustomerSurname = draft.Surname.Trim(),
            CustomerAddress = draft.Address.Trim(),
            CustomerAge = int.Parse(draft.AgeText, CultureInfo.InvariantCulture),
            HasValidDrivingLicense = true,
            CarType = draft.CarType!.Value,
            FuelType = draft.FuelType!.Value,
            RentalDays = draft.RentalDays!.Value,
            UnlimitedMileage = draft.UnlimitedMileage,
            BreakdownCover = draft.BreakdownCover,
            BaseRentalCost = price.BaseRentalCost,
            CarSurcharge = price.CarSurcharge,
            FuelSurcharge = price.FuelSurcharge,
            ExtraCharges = price.ExtraCharges,
            TotalPrice = price.Total,
            Status = created.Date.AddDays(draft.RentalDays.Value) >= DateTime.Today
                ? BookingStatus.Active
                : BookingStatus.Completed,
            CreatedAt = created,
            ReturnDate = created.AddDays(draft.RentalDays.Value)
        };
    }
}