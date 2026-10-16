using WeAreCars.Core.Models;
using WeAreCars.Core.Services;

namespace WeAreCars.Tests;

public sealed class BookingRepositoryTests
{
    [Fact]
    public void Add_PreservesBookingsAndReloadsFromJson()
    {
        var directory = Path.Combine(Path.GetTempPath(), "WeAreCars.Tests", Guid.NewGuid().ToString("N"));
        var filePath = Path.Combine(directory, "bookings.json");
        try
        {
            var repository = new BookingRepository(filePath);
            Assert.Empty(repository.GetAll());

            var first = CreateBooking("WAC-ONE");
            var second = CreateBooking("WAC-TWO");
            repository.Add(first);
            repository.Add(second);

            var reloaded = new BookingRepository(filePath).GetAll();
            Assert.Equal(2, reloaded.Count);
            Assert.Contains(reloaded, booking => booking.BookingId == first.BookingId);
            Assert.Contains(reloaded, booking => booking.BookingId == second.BookingId);
        }
        finally
        {
            if (Directory.Exists(directory)) Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public void GetAll_ReportsMalformedJsonClearly()
    {
        var directory = Path.Combine(Path.GetTempPath(), "WeAreCars.Tests", Guid.NewGuid().ToString("N"));
        var filePath = Path.Combine(directory, "bookings.json");
        Directory.CreateDirectory(directory);
        try
        {
            File.WriteAllText(filePath, "{ not valid json }");
            Assert.Throws<InvalidDataException>(() => new BookingRepository(filePath).GetAll());
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public void Update_ReplacesBookingWithoutChangingItsIdOrOtherBookings()
    {
        var directory = Path.Combine(Path.GetTempPath(), "WeAreCars.Tests", Guid.NewGuid().ToString("N"));
        var filePath = Path.Combine(directory, "bookings.json");
        try
        {
            var repository = new BookingRepository(filePath);
            var target = CreateBooking("WAC-EDIT");
            var other = CreateBooking("WAC-OTHER");
            repository.Add(target);
            repository.Add(other);

            target.CustomerFirstName = "Updated";
            target.RentalDays = 5;
            repository.Update(target);

            var bookings = repository.GetAll();
            Assert.Equal(2, bookings.Count);
            Assert.Contains(bookings, booking => booking.BookingId == "WAC-EDIT" &&
                booking.CustomerFirstName == "Updated" && booking.RentalDays == 5);
            Assert.Contains(bookings, booking => booking.BookingId == "WAC-OTHER");
        }
        finally
        {
            if (Directory.Exists(directory)) Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public void Delete_RemovesOnlyRequestedBooking()
    {
        var directory = Path.Combine(Path.GetTempPath(), "WeAreCars.Tests", Guid.NewGuid().ToString("N"));
        var filePath = Path.Combine(directory, "bookings.json");
        try
        {
            var repository = new BookingRepository(filePath);
            repository.Add(CreateBooking("WAC-DELETE"));
            repository.Add(CreateBooking("WAC-KEEP"));

            Assert.True(repository.Delete("WAC-DELETE"));
            Assert.False(repository.Delete("WAC-MISSING"));
            Assert.Equal(["WAC-KEEP"], repository.GetAll().Select(booking => booking.BookingId));
        }
        finally
        {
            if (Directory.Exists(directory)) Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public void DeleteMany_RemovesSelectedBookingsInOneOperation()
    {
        var directory = Path.Combine(Path.GetTempPath(), "WeAreCars.Tests", Guid.NewGuid().ToString("N"));
        var filePath = Path.Combine(directory, "bookings.json");
        try
        {
            var repository = new BookingRepository(filePath);
            repository.Add(CreateBooking("WAC-ONE"));
            repository.Add(CreateBooking("WAC-TWO"));
            repository.Add(CreateBooking("WAC-KEEP"));

            Assert.Equal(2, repository.DeleteMany(["WAC-ONE", "WAC-TWO", "WAC-MISSING"]));
            Assert.Equal(["WAC-KEEP"], repository.GetAll().Select(booking => booking.BookingId));
        }
        finally
        {
            if (Directory.Exists(directory)) Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public void GetAll_AssignsAndPersistsUniqueIdsForLegacyBookings()
    {
        var directory = Path.Combine(Path.GetTempPath(), "WeAreCars.Tests", Guid.NewGuid().ToString("N"));
        var filePath = Path.Combine(directory, "bookings.json");
        Directory.CreateDirectory(directory);
        try
        {
            File.WriteAllText(filePath,
                "[{\"BookingId\":\"\"},{\"BookingId\":null},{\"BookingId\":\"WAC-DUPLICATE\"},{\"BookingId\":\"WAC-DUPLICATE\"}]");

            var bookings = new BookingRepository(filePath).GetAll();
            var bookingIds = bookings.Select(booking => booking.BookingId).ToList();

            Assert.Equal(4, bookingIds.Distinct(StringComparer.Ordinal).Count());
            Assert.All(bookingIds, bookingId => Assert.False(string.IsNullOrWhiteSpace(bookingId)));
            Assert.Equal(bookingIds, new BookingRepository(filePath).GetAll().Select(booking => booking.BookingId));

            bookings[0].CustomerFirstName = "Repaired";
            new BookingRepository(filePath).Update(bookings[0]);
            Assert.True(new BookingRepository(filePath).Delete(bookingIds[1]));
            Assert.Contains(new BookingRepository(filePath).GetAll(), booking =>
                booking.BookingId == bookingIds[0] && booking.CustomerFirstName == "Repaired");
            Assert.DoesNotContain(new BookingRepository(filePath).GetAll(), booking => booking.BookingId == bookingIds[1]);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    private static Booking CreateBooking(string bookingId) => new()
    {
        BookingId = bookingId,
        CustomerFirstName = "Avery",
        CustomerSurname = "Taylor",
        CustomerAddress = "1 High Street",
        CustomerAge = 25,
        HasValidDrivingLicense = true,
        CarType = CarType.CityCar,
        FuelType = FuelType.Petrol,
        RentalDays = 3,
        BaseRentalCost = 75m,
        TotalPrice = 75m,
        Status = BookingStatus.Active,
        CreatedAt = DateTime.Now,
        ReturnDate = DateTime.Now.AddDays(3)
    };
}