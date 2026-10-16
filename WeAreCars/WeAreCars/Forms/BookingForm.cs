using System.Globalization;
using WeAreCars.Core.Models;
using WeAreCars.Core.Services;

namespace WeAreCars;

internal sealed class BookingForm : Form
{
    private readonly BookingRepository repository;
    private readonly Booking? existingBooking;
    private readonly TextBox firstName = UiTheme.TextBox("e.g. John");
    private readonly TextBox surname = UiTheme.TextBox("e.g. Smith");
    private readonly TextBox address = UiTheme.TextBox("Street, town, postcode", true);
    private readonly TextBox age = UiTheme.TextBox("e.g. 18");
    private readonly NumericUpDown days = new() { Minimum = 1, Maximum = 28, Value = 7, BackColor = UiTheme.Input, ForeColor = UiTheme.Text, BorderStyle = BorderStyle.FixedSingle, Font = UiTheme.BodyFont, Dock = DockStyle.Fill, Height = 38 };
    private readonly ComboBox car = UiTheme.ComboBox();
    private readonly ComboBox fuel = UiTheme.ComboBox();
    private readonly RadioButton licenseYes = new() { Text = "Yes", AutoSize = true, ForeColor = UiTheme.Text, BackColor = UiTheme.Surface };
    private readonly RadioButton licenseNo = new() { Text = "No", AutoSize = true, ForeColor = UiTheme.Text, BackColor = UiTheme.Surface };
    private readonly CheckBox unlimitedMileage = new() { Text = "Unlimited mileage  ·  £10 / day", AutoSize = true, ForeColor = UiTheme.Text, BackColor = UiTheme.Surface };
    private readonly CheckBox breakdownCover = new() { Text = "Breakdown cover  ·  £2 / day", AutoSize = true, ForeColor = UiTheme.Text, BackColor = UiTheme.Surface };
    private readonly Label licenseWarning = UiTheme.Label("A valid driving license is required to proceed.", 9, false, UiTheme.Error);
    private readonly Label estimate = UiTheme.Label("Select a car and fuel type to calculate the estimate.", 11, true);
    private readonly Button reviewButton = UiTheme.Button("INITIATE BOOKING REVIEW", true);
    private readonly PricingService pricingService = new();
    private readonly ValidationService validationService = new();

    public BookingForm(BookingRepository repository, Booking? existingBooking = null)
    {
        this.repository = repository;
        this.existingBooking = existingBooking;
        UiTheme.StyleForm(this, existingBooking is null ? "WeAreCars | New Booking" : "WeAreCars | Edit Booking");
        Size = new Size(1160, 850);

        var shell = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, Padding = new Padding(24, 18, 24, 20) };
        shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 78));
        shell.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        shell.Controls.Add(BuildHeading(existingBooking is null ? "NEW BOOKING ENTRY" : "EDIT BOOKING"), 0, 0);

        var scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = UiTheme.Background };
        var content = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 1, RowCount = 4, Padding = new Padding(0, 0, 8, 0) };
        content.MinimumSize = new Size(0, 830);
        content.RowStyles.Add(new RowStyle(SizeType.Absolute, 330));
        content.RowStyles.Add(new RowStyle(SizeType.Absolute, 170));
        content.RowStyles.Add(new RowStyle(SizeType.Absolute, 220));
        content.RowStyles.Add(new RowStyle(SizeType.Absolute, 86));
        content.Controls.Add(BuildCustomerCard(), 0, 0);
        content.Controls.Add(BuildLicenseCard(), 0, 1);
        content.Controls.Add(BuildRentalCard(), 0, 2);
        content.Controls.Add(BuildFooter(), 0, 3);
        scroll.Controls.Add(content);
        shell.Controls.Add(scroll, 0, 1);
        Controls.Add(shell);

        foreach (var control in new Control[] { days, car, fuel, unlimitedMileage, breakdownCover })
        {
            control.Validated += (_, _) => RecalculateEstimate();
            if (control is ComboBox combo) combo.SelectedIndexChanged += (_, _) => RecalculateEstimate();
            if (control is NumericUpDown numeric) numeric.ValueChanged += (_, _) => RecalculateEstimate();
            if (control is CheckBox checkBox) checkBox.CheckedChanged += (_, _) => RecalculateEstimate();
        }
        licenseYes.CheckedChanged += (_, _) =>
        {
            licenseWarning.Visible = !licenseYes.Checked;
            reviewButton.Enabled = licenseYes.Checked;
        };
        licenseNo.CheckedChanged += (_, _) =>
        {
            licenseWarning.Visible = licenseNo.Checked;
            reviewButton.Enabled = licenseYes.Checked;
        };
        licenseWarning.Visible = true;
        reviewButton.Enabled = false;
        if (existingBooking is not null) LoadExistingBooking(existingBooking);
        RecalculateEstimate();
    }

    private Control BuildHeading(string headingText)
    {
        var panel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 2 };
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 37));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));
        var heading = UiTheme.Label(headingText, 18, true);
        heading.Font = new Font("Consolas", 18, FontStyle.Bold);
        panel.Controls.Add(heading, 0, 0);
        panel.Controls.Add(UiTheme.Label("Enter customer and rental details. Required fields are marked *.", 9, false, UiTheme.Muted), 0, 1);
        var back = UiTheme.Button("←  Dashboard");
        back.MinimumSize = new Size(128, 36);
        back.Height = 36;
        back.Click += (_, _) => DialogResult = DialogResult.Cancel;
        panel.Controls.Add(back, 1, 0);
        panel.SetRowSpan(back, 2);
        return panel;
    }

    private Control BuildCustomerCard()
    {
        var panel = CreateSection("01  /  CUSTOMER DETAILS");
        var fields = TwoColumnGrid(3);
        AddField(fields, "Customer First Name*", firstName, 0, 0);
        AddField(fields, "Customer Surname*", surname, 1, 0);
        AddField(fields, "Customer Address*", address, 0, 1, span: 2);
        AddField(fields, "Customer Age*", age, 0, 2);
        var hint = UiTheme.Label("customers must be at least 18.", 8, false, UiTheme.Muted);
        fields.Controls.Add(hint, 1, 2);
        ((TableLayoutPanel)panel.Tag!).Controls.Add(fields, 0, 1);
        return panel;
    }

    private Control BuildLicenseCard()
    {
        var panel = CreateSection("02  /  DRIVING LICENSE VERIFICATION");
        var content = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, Padding = new Padding(0, 6, 0, 0) };
        content.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
        content.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
        content.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));
        content.Controls.Add(UiTheme.Label("VALID_DRIVING_LICENSE_VERIFIED*", 9, true, UiTheme.Muted), 0, 0);
        var choices = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false, BackColor = UiTheme.Surface };
        choices.Controls.Add(licenseYes);
        choices.Controls.Add(licenseNo);
        content.Controls.Add(choices, 0, 1);
        content.Controls.Add(licenseWarning, 0, 2);
        ((TableLayoutPanel)panel.Tag!).Controls.Add(content, 0, 1);
        return panel;
    }

    private Control BuildRentalCard()
    {
        var panel = CreateSection("03  /  RENTAL DETAILS  +  OPTIONAL EXTRAS");
        var grid = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 3, Padding = new Padding(0, 4, 0, 0) };
        for (var index = 0; index < 3; index++) grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.333F));
        grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 22));
        grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        grid.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        car.Items.AddRange(["City Car  ·  £0", "Family Car  ·  £50", "Sports Car  ·  £75", "SUV  ·  £65"]);
        fuel.Items.AddRange(["Petrol  ·  £0", "Diesel  ·  £0", "Hybrid  ·  £30", "Full Electric  ·  £50"]);
        car.SelectedIndexChanged += (_, _) => RecalculateEstimate();
        fuel.SelectedIndexChanged += (_, _) => RecalculateEstimate();

        grid.Controls.Add(UiTheme.Label("Rental days*  ·  £25 per day", 9, true, UiTheme.Muted), 0, 0);
        grid.Controls.Add(UiTheme.Label("Type of car*", 9, true, UiTheme.Muted), 1, 0);
        grid.Controls.Add(UiTheme.Label("Fuel type*", 9, true, UiTheme.Muted), 2, 0);
        grid.Controls.Add(days, 0, 1);
        grid.Controls.Add(car, 1, 1);
        grid.Controls.Add(fuel, 2, 1);

        var extras = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, Padding = new Padding(0, 4, 0, 0), BackColor = UiTheme.Surface };
        extras.Controls.Add(unlimitedMileage);
        extras.Controls.Add(breakdownCover);
        grid.Controls.Add(extras, 0, 2);
        grid.SetColumnSpan(extras, 3);
        ((TableLayoutPanel)panel.Tag!).Controls.Add(grid, 0, 1);
        return panel;
    }

    private Control BuildFooter()
    {
        var footer = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 2, Padding = new Padding(0, 8, 0, 0) };
        footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        footer.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        footer.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
        footer.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        estimate.Font = new Font("Consolas", 11, FontStyle.Bold);
        footer.Controls.Add(estimate, 0, 0);
        var note = UiTheme.Label("ESTIMATE UPDATES WHEN RENTAL OPTIONS CHANGE", 8, false, UiTheme.Muted);
        note.Font = UiTheme.MonoFont;
        footer.Controls.Add(note, 0, 1);

        var actions = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, WrapContents = false, BackColor = UiTheme.Background };
        reviewButton.Click += (_, _) => ReviewBooking();
        reviewButton.Text = "REVIEW BOOKING";
        reviewButton.MinimumSize = new Size(190, 42);
        reviewButton.Width = 190;
        var clear = UiTheme.Button("Clear Form");
        clear.Click += (_, _) => ClearForm();
        actions.Controls.Add(reviewButton);
        actions.Controls.Add(clear);
        footer.Controls.Add(actions, 1, 0);
        footer.SetRowSpan(actions, 2);
        return footer;
    }

    private static Panel CreateSection(string title)
    {
        var panel = UiTheme.SurfacePanel();
        panel.Dock = DockStyle.Fill;
        panel.Margin = new Padding(0, 0, 0, 14);
        panel.Padding = new Padding(18, 14, 18, 14);
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2 };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 31));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        var heading = UiTheme.Label(title, 9, true, UiTheme.Cyan);
        heading.Font = UiTheme.MonoFont;
        layout.Controls.Add(heading, 0, 0);
        panel.Controls.Add(layout);
        panel.Tag = layout;
        return panel;
    }

    private static TableLayoutPanel TwoColumnGrid(int rows)
    {
        var grid = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = rows, BackColor = UiTheme.Surface };
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 68));
        grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 112));
        grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 68));
        return grid;
    }

    private static void AddField(TableLayoutPanel grid, string label, Control input, int column, int row, int span = 1)
    {
        var wrapper = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, BackColor = UiTheme.Surface, Padding = new Padding(0, 0, 16, 0) };
        wrapper.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
        wrapper.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        wrapper.Controls.Add(UiTheme.Label(label, 9, true), 0, 0);
        wrapper.Controls.Add(input, 0, 1);
        grid.Controls.Add(wrapper, column, row);
        if (span > 1) grid.SetColumnSpan(wrapper, span);
    }

    private BookingDraft ReadDraft() => new()
    {
        FirstName = firstName.Text,
        Surname = surname.Text,
        Address = address.Text,
        AgeText = age.Text,
        HasValidDrivingLicense = licenseYes.Checked ? true : licenseNo.Checked ? false : null,
        RentalDays = (int)days.Value,
        CarType = car.SelectedIndex switch
        {
            0 => CarType.CityCar,
            1 => CarType.FamilyCar,
            2 => CarType.SportsCar,
            3 => CarType.Suv,
            _ => null
        },
        FuelType = fuel.SelectedIndex switch
        {
            0 => FuelType.Petrol,
            1 => FuelType.Diesel,
            2 => FuelType.Hybrid,
            3 => FuelType.FullElectric,
            _ => null
        },
        UnlimitedMileage = unlimitedMileage.Checked,
        BreakdownCover = breakdownCover.Checked
    };

    private void LoadExistingBooking(Booking booking)
    {
        firstName.Text = booking.CustomerFirstName ?? string.Empty;
        surname.Text = booking.CustomerSurname ?? string.Empty;
        address.Text = booking.CustomerAddress ?? string.Empty;
        age.Text = booking.CustomerAge.ToString(CultureInfo.InvariantCulture);
        licenseYes.Checked = booking.HasValidDrivingLicense;
        licenseNo.Checked = !booking.HasValidDrivingLicense;
        days.Value = Math.Clamp(booking.RentalDays, (int)days.Minimum, (int)days.Maximum);
        car.SelectedIndex = booking.CarType switch
        {
            CarType.CityCar => 0,
            CarType.FamilyCar => 1,
            CarType.SportsCar => 2,
            CarType.Suv => 3,
            _ => -1
        };
        fuel.SelectedIndex = booking.FuelType switch
        {
            FuelType.Petrol => 0,
            FuelType.Diesel => 1,
            FuelType.Hybrid => 2,
            FuelType.FullElectric => 3,
            _ => -1
        };
        unlimitedMileage.Checked = booking.UnlimitedMileage;
        breakdownCover.Checked = booking.BreakdownCover;
    }

    private void RecalculateEstimate()
    {
        var draft = ReadDraft();
        if (draft.CarType is null || draft.FuelType is null)
        {
            estimate.Text = "Select a car and fuel type to calculate the estimate.";
            return;
        }

        var price = pricingService.Calculate(draft);
        estimate.Text = $"ESTIMATED TOTAL   {price.Total.ToString("C2", CultureInfo.GetCultureInfo("en-GB"))}";
    }

    private void ReviewBooking()
    {
        var draft = ReadDraft();
        var errors = validationService.Validate(draft);
        if (errors.Count > 0)
        {
            MessageBox.Show(this, string.Join(Environment.NewLine, errors.Select(error => "• " + error)),
                "Check booking details", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        using var summary = new BookingSummaryForm(draft, repository, existingBooking);
        if (summary.ShowDialog(this) == DialogResult.OK) DialogResult = DialogResult.OK;
    }

    private void ClearForm()
    {
        firstName.Clear();
        surname.Clear();
        address.Clear();
        age.Clear();
        licenseYes.Checked = false;
        licenseNo.Checked = false;
        days.Value = 7;
        car.SelectedIndex = -1;
        fuel.SelectedIndex = -1;
        unlimitedMileage.Checked = false;
        breakdownCover.Checked = false;
        RecalculateEstimate();
    }
}