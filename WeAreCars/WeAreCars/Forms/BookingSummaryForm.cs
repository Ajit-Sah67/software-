using System.Globalization;
using WeAreCars.Core.Models;
using WeAreCars.Core.Services;

namespace WeAreCars;

internal sealed class BookingSummaryForm : Form
{
    private readonly BookingDraft draft;
    private readonly BookingRepository repository;
    private readonly Booking? existingBooking;
    private readonly PricingService pricingService = new();
    private readonly Button storeButton = UiTheme.Button("CONFIRM BOOKING", true);
    private bool stored;

    public BookingSummaryForm(BookingDraft draft, BookingRepository repository, Booking? existingBooking = null)
    {
        this.draft = draft;
        this.repository = repository;
        this.existingBooking = existingBooking;
        UiTheme.StyleForm(this, existingBooking is null ? "WeAreCars | Booking Review" : "WeAreCars | Edit Review");
        Size = new Size(1060, 760);

        var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, Padding = new Padding(26) };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 72));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        var title = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2 };
        title.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
        title.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
        var heading = UiTheme.Label(existingBooking is null ? "BOOKING REVIEW" : "EDIT BOOKING REVIEW", 18, true);
        heading.Font = new Font("Consolas", 18, FontStyle.Bold);
        title.Controls.Add(heading, 0, 0);
        title.Controls.Add(UiTheme.Label("Review customer, vehicle and billing details before confirming.", 9, false, UiTheme.Muted), 0, 1);
        root.Controls.Add(title, 0, 0);

        var content = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1 };
        content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 56));
        content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 44));
        content.Controls.Add(BuildDetails(), 0, 0);
        content.Controls.Add(BuildBilling(), 1, 0);
        root.Controls.Add(content, 0, 1);
        Controls.Add(root);
    }

    private Control BuildDetails()
    {
        var flow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true, BackColor = UiTheme.Background, Padding = new Padding(0, 0, 16, 0) };
        flow.Controls.Add(DetailCard("CUSTOMER IDENTITY", [
            ("Full name", $"{draft.FirstName.Trim()} {draft.Surname.Trim()}"),
            ("Address", draft.Address.Trim()),
            ("Age", draft.AgeText),
            ("Driving license", "Verified")
        ]));

        var extras = new List<(string, string)>();
        if (draft.UnlimitedMileage) extras.Add(("Unlimited mileage", "Selected · £10 per day"));
        if (draft.BreakdownCover) extras.Add(("Breakdown cover", "Selected · £2 per day"));
        if (extras.Count == 0) extras.Add(("Optional extras", "None selected"));
        flow.Controls.Add(DetailCard("VEHICLE + RENTAL", new List<(string Label, string Value)>
        {
            ("Vehicle type", FormatCar(draft.CarType!.Value)),
            ("Fuel type", FormatFuel(draft.FuelType!.Value)),
            ("Rental duration", $"{draft.RentalDays} days")
        }.Concat(extras).ToList()));
        return flow;
    }

    private Control BuildBilling()
    {
        var card = UiTheme.SurfacePanel();
        card.Dock = DockStyle.Fill;
        card.Margin = new Padding(0);
        var price = pricingService.Calculate(draft);
        var layout = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 2, RowCount = 8 };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        var heading = UiTheme.Label("BILLING BREAKDOWN", 9, true, UiTheme.Cyan);
        heading.Font = UiTheme.MonoFont;
        layout.Controls.Add(heading, 0, 0);
        layout.SetColumnSpan(heading, 2);

        AddBillingLine(layout, 1, "Base rental", price.BaseRentalCost);
        AddBillingLine(layout, 2, "Car surcharge", price.CarSurcharge);
        AddBillingLine(layout, 3, "Fuel surcharge", price.FuelSurcharge);
        AddBillingLine(layout, 4, "Unlimited mileage", price.UnlimitedMileageCost);
        AddBillingLine(layout, 5, "Breakdown cover", price.BreakdownCoverCost);

        var totalPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(14, 45, 53), Padding = new Padding(12), Margin = new Padding(0, 12, 0, 12), Height = 72 };
        var totalTitle = UiTheme.Label("TOTAL PRICE", 8, true, UiTheme.Cyan);
        totalTitle.Font = UiTheme.MonoFont;
        totalTitle.Location = new Point(12, 10);
        var total = UiTheme.Label(price.Total.ToString("C2", CultureInfo.GetCultureInfo("en-GB")), 20, true, UiTheme.Text);
        total.Location = new Point(12, 30);
        totalPanel.Controls.Add(totalTitle);
        totalPanel.Controls.Add(total);
        layout.Controls.Add(totalPanel, 0, 6);
        layout.SetColumnSpan(totalPanel, 2);

        var actions = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoSize = true, Margin = new Padding(0, 10, 0, 0) };
        var edit = UiTheme.Button("←  EDIT BOOKING");
        edit.Width = 240;
        edit.Click += (_, _) => DialogResult = DialogResult.Cancel;
        storeButton.Width = 240;
        if (existingBooking is not null) storeButton.Text = "SAVE CHANGES";
        storeButton.Click += (_, _) => StoreBooking();
        actions.Controls.Add(edit);
        actions.Controls.Add(storeButton);
        layout.Controls.Add(actions, 0, 7);
        layout.SetColumnSpan(actions, 2);
        card.Controls.Add(layout);
        return card;
    }

    private static Control DetailCard(string title, IReadOnlyList<(string Label, string Value)> rows)
    {
        var panel = UiTheme.SurfacePanel();
        panel.Width = 560;
        panel.AutoSize = true;
        panel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        panel.Padding = new Padding(18);
        var layout = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 2, RowCount = rows.Count + 1 };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 62));
        var heading = UiTheme.Label(title, 9, true, UiTheme.Cyan);
        heading.Font = UiTheme.MonoFont;
        layout.Controls.Add(heading, 0, 0);
        layout.SetColumnSpan(heading, 2);
        for (var index = 0; index < rows.Count; index++)
        {
            var (label, value) = rows[index];
            var labelControl = UiTheme.Label(label, 9, false, UiTheme.Muted);
            labelControl.Margin = new Padding(0, 10, 8, 8);
            var valueControl = UiTheme.Label(value, 9, true);
            valueControl.MaximumSize = new Size(300, 0);
            valueControl.Margin = new Padding(0, 10, 0, 8);
            layout.Controls.Add(labelControl, 0, index + 1);
            layout.Controls.Add(valueControl, 1, index + 1);
        }
        panel.Controls.Add(layout);
        return panel;
    }

    private static void AddBillingLine(TableLayoutPanel layout, int row, string label, decimal amount)
    {
        var name = UiTheme.Label(label, 9, false, UiTheme.Muted);
        var value = UiTheme.Label(amount.ToString("C2", CultureInfo.GetCultureInfo("en-GB")), 9, true);
        name.Margin = new Padding(0, 12, 12, 6);
        value.Margin = new Padding(0, 12, 0, 6);
        value.Anchor = AnchorStyles.Right;
        layout.Controls.Add(name, 0, row);
        layout.Controls.Add(value, 1, row);
    }

    private static string FormatCar(CarType carType) => carType switch
    {
        CarType.CityCar => "City Car",
        CarType.FamilyCar => "Family Car",
        CarType.SportsCar => "Sports Car",
        CarType.Suv => "SUV",
        _ => carType.ToString()
    };

    private static string FormatFuel(FuelType fuelType) => fuelType switch
    {
        FuelType.FullElectric => "Full Electric",
        _ => fuelType.ToString()
    };

    private void StoreBooking()
    {
        if (stored) return;
        storeButton.Enabled = false;
        try
        {
            var bookingService = new BookingService(new ValidationService(), pricingService);
            var booking = bookingService.Create(draft, existingBooking?.CreatedAt);
            if (existingBooking is null)
            {
                repository.Add(booking);
            }
            else
            {
                booking.BookingId = existingBooking.BookingId;
                repository.Update(booking);
            }
            stored = true;
            var message = existingBooking is null
                ? "The booking has been saved to local storage."
                : "The booking changes have been saved.";
            MessageBox.Show(this, message, existingBooking is null ? "Booking stored" : "Booking updated", MessageBoxButtons.OK, MessageBoxIcon.Information);
            DialogResult = DialogResult.OK;
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or InvalidOperationException or KeyNotFoundException)
        {
            storeButton.Enabled = true;
            MessageBox.Show(this, exception.Message, "Unable to store booking", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}