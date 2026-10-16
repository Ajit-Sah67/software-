using System.Globalization;
using WeAreCars.Core.Models;

namespace WeAreCars.Core.Services;

/// <summary>Validates booking input, including the minimum customer age.</summary>
public sealed class ValidationService
{
    /// <summary>Returns all input problems; an empty list means the draft is valid.</summary>
    public IReadOnlyList<string> Validate(BookingDraft draft)
    {
        ArgumentNullException.ThrowIfNull(draft);
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(draft.FirstName)) errors.Add("Enter the customer's first name.");
        if (string.IsNullOrWhiteSpace(draft.Surname)) errors.Add("Enter the customer's surname.");
        if (string.IsNullOrWhiteSpace(draft.Address)) errors.Add("Enter the customer's address.");
        if (!int.TryParse(draft.AgeText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var age))
            errors.Add("Enter the customer's age as a whole number.");
        else if (age < 18)
            errors.Add("The customer must be at least 18 years old to rent a vehicle.");
        if (draft.HasValidDrivingLicense != true)
            errors.Add("A valid driving license must be confirmed before booking.");
        if (draft.RentalDays is null or < 1 or > 28)
            errors.Add("Rental duration must be a whole number between 1 and 28 days.");
        if (draft.CarType is null) errors.Add("Select a car type.");
        if (draft.FuelType is null) errors.Add("Select a fuel type.");

        return errors;
    }
}