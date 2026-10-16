using WeAreCars.Core.Services;

namespace WeAreCars;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        var repository = new BookingRepository();
        var authenticationService = new AuthenticationService();

        using (var splash = new SplashForm())
        {
            if (splash.ShowDialog() != DialogResult.OK) return;
        }

        while (true)
        {
            using (var login = new LoginForm(authenticationService))
            {
                if (login.ShowDialog() != DialogResult.OK) return;
            }

            using var dashboard = new DashboardForm(repository);
            if (dashboard.ShowDialog() != DialogResult.Retry) return;
        }
    }
}