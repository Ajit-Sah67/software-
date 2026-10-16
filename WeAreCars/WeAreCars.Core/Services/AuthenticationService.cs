namespace WeAreCars.Core.Services;

/// <summary>Checks the staff credentials specified for the assignment.</summary>
public sealed class AuthenticationService
{
    /// <summary>Returns whether the supplied values match the configured staff login.</summary>
    public bool IsValid(string username, string password) =>
        string.Equals(username, "sta001", StringComparison.Ordinal) &&
        string.Equals(password, "givemethekeys123", StringComparison.Ordinal);
}