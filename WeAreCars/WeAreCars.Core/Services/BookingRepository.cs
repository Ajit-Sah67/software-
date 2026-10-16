using System.Text.Json;
using System.Text.Json.Serialization;
using WeAreCars.Core.Models;

namespace WeAreCars.Core.Services;

/// <summary>Reads and writes bookings in a local JSON file.</summary>
public sealed class BookingRepository(string? filePath = null)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };
    private readonly string filePath = filePath ?? Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "WeAreCars", "bookings.json");
    private readonly object syncLock = new();

    /// <summary>Loads all saved bookings, or an empty list when storage does not exist.</summary>
    public IReadOnlyList<Booking> GetAll()
    {
        lock (syncLock)
        {
            if (!File.Exists(filePath)) return [];
            List<Booking> bookings;
            try
            {
                bookings = JsonSerializer.Deserialize<List<Booking>>(File.ReadAllText(filePath), JsonOptions) ?? [];
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or JsonException)
            {
                throw new InvalidDataException("Bookings could not be read. The local storage file may be damaged or inaccessible.", exception);
            }

            if (EnsureUniqueBookingIds(bookings)) SaveAll(bookings);
            return bookings;
        }
    }

    /// <summary>Adds a booking without replacing existing entries or allowing duplicate IDs.</summary>
    public void Add(Booking booking)
    {
        ArgumentNullException.ThrowIfNull(booking);
        lock (syncLock)
        {
            var bookings = GetAll().ToList();
            if (bookings.Any(existing => string.Equals(existing.BookingId, booking.BookingId, StringComparison.Ordinal)))
            {
                throw new InvalidOperationException("This booking has already been stored.");
            }

            bookings.Add(booking);
            SaveAll(bookings);
        }
    }

    /// <summary>Replaces a saved booking with the same ID.</summary>
    public void Update(Booking booking)
    {
        ArgumentNullException.ThrowIfNull(booking);
        lock (syncLock)
        {
            var bookings = GetAll().ToList();
            var index = bookings.FindIndex(existing => string.Equals(existing.BookingId, booking.BookingId, StringComparison.Ordinal));
            if (index < 0) throw new KeyNotFoundException("The booking no longer exists in storage.");

            bookings[index] = booking;
            SaveAll(bookings);
        }
    }

    /// <summary>Removes a saved booking by ID; returns false if it was not found.</summary>
    public bool Delete(string bookingId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(bookingId);
        return DeleteMany([bookingId]) > 0;
    }

    /// <summary>Removes a group of bookings in one storage write and returns the number removed.</summary>
    public int DeleteMany(IEnumerable<string> bookingIds)
    {
        ArgumentNullException.ThrowIfNull(bookingIds);
        var ids = bookingIds.Where(id => !string.IsNullOrWhiteSpace(id)).ToHashSet(StringComparer.Ordinal);
        if (ids.Count == 0) return 0;

        lock (syncLock)
        {
            var bookings = GetAll().ToList();
            var remaining = bookings.Where(existing => !ids.Contains(existing.BookingId)).ToList();
            var removed = bookings.Count - remaining.Count;
            if (removed == 0) return 0;

            SaveAll(remaining);
            return removed;
        }
    }

    private void SaveAll(List<Booking> bookings)
    {
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
        var temporaryPath = filePath + ".tmp";
        try
        {
            File.WriteAllText(temporaryPath, JsonSerializer.Serialize(bookings, JsonOptions));
            File.Move(temporaryPath, filePath, overwrite: true);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            if (File.Exists(temporaryPath)) File.Delete(temporaryPath);
            throw new IOException("The booking could not be saved to local storage.", exception);
        }
    }

    private static bool EnsureUniqueBookingIds(List<Booking> bookings)
    {
        var reservedIds = bookings
            .Where(booking => !string.IsNullOrWhiteSpace(booking.BookingId))
            .Select(booking => booking.BookingId)
            .ToHashSet(StringComparer.Ordinal);
        var encounteredIds = new HashSet<string>(StringComparer.Ordinal);
        var changed = false;

        foreach (var booking in bookings)
        {
            if (!string.IsNullOrWhiteSpace(booking.BookingId) && encounteredIds.Add(booking.BookingId))
                continue;

            string bookingId;
            do
            {
                bookingId = $"WAC-{Guid.NewGuid():N}"[..16].ToUpperInvariant();
            } while (reservedIds.Contains(bookingId) || !encounteredIds.Add(bookingId));

            booking.BookingId = bookingId;
            reservedIds.Add(bookingId);
            changed = true;
        }

        return changed;
    }
}