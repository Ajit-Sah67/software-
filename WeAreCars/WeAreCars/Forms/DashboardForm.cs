using WeAreCars.Core.Models;
using WeAreCars.Core.Services;

namespace WeAreCars;

internal sealed class DashboardForm : Form
{
    private readonly BookingRepository repository;
    private readonly DataGridView bookingsGrid = new();
    private readonly Button editSelectedButton = UiTheme.Button("Edit selected");
    private readonly Button deleteSelectedButton = UiTheme.Button("Delete selected");
    private readonly CheckBox selectAllCheckBox = new() { Text = "Select all", AutoSize = true, ForeColor = UiTheme.Text, BackColor = UiTheme.Background };
    private readonly Label emptyState = UiTheme.Label("No bookings stored yet. Start with a new booking entry.", 11, false, UiTheme.Muted);
    private readonly Label countLabel = UiTheme.Label("", 9, false, UiTheme.Muted);
    private bool synchronizingSelection;

    public DashboardForm(BookingRepository repository)
    {
        this.repository = repository;
        UiTheme.StyleForm(this, "WeAreCars | Rental Dashboard");
        Size = new Size(1240, 800);

        var shell = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, BackColor = UiTheme.Background };
        shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 64));
        shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
        shell.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        shell.Controls.Add(BuildTopBar(), 0, 0);
        shell.Controls.Add(BuildNavigation(), 0, 1);
        shell.Controls.Add(BuildContent(), 0, 2);
        Controls.Add(shell);
        Load += (_, _) => RefreshBookings();
    }

    private Control BuildTopBar()
    {
        var bar = new Panel { Dock = DockStyle.Fill, BackColor = UiTheme.Surface, Padding = new Padding(24, 0, 24, 0) };
        var brand = UiTheme.Label("◈  WeAreCars", 15, true, UiTheme.Cyan);
        brand.Location = new Point(24, 20);
        var logout = UiTheme.Button("Log out");
        logout.MinimumSize = new Size(96, 34);
        logout.Height = 34;
        logout.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        logout.Location = new Point(ClientSize.Width - 120, 14);
        logout.Click += (_, _) => DialogResult = DialogResult.Retry;
        bar.Controls.Add(brand);
        bar.Controls.Add(logout);
        bar.Resize += (_, _) => logout.Location = new Point(bar.ClientSize.Width - logout.Width - 24, 14);
        return bar;
    }

    private Control BuildNavigation()
    {
        var bar = new FlowLayoutPanel { Dock = DockStyle.Fill, BackColor = UiTheme.Background, Padding = new Padding(24, 5, 0, 0), WrapContents = false };
        var active = UiTheme.Label("ACTIVE RENTALS", 9, true, UiTheme.Cyan);
        active.Font = UiTheme.MonoFont;
        active.Margin = new Padding(0, 12, 30, 0);
        var newBooking = UiTheme.Label("NEW BOOKING", 9, true, UiTheme.Muted);
        newBooking.Font = UiTheme.MonoFont;
        newBooking.Cursor = Cursors.Hand;
        newBooking.Margin = new Padding(0, 12, 30, 0);
        newBooking.Click += (_, _) => OpenBooking();
        bar.Controls.Add(active);
        bar.Controls.Add(newBooking);
        return bar;
    }

    private Control BuildContent()
    {
        var content = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(24, 20, 24, 24), ColumnCount = 1, RowCount = 4 };
        content.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
        content.RowStyles.Add(new RowStyle(SizeType.Absolute, 43));
        content.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        content.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));

        var heading = UiTheme.Label("GRID_RENTAL_DASHBOARD", 17, true);
        heading.Font = new Font("Consolas", 17, FontStyle.Bold);
        content.Controls.Add(heading, 0, 0);

        var actions = new Panel { Dock = DockStyle.Fill };
        var subtitle = UiTheme.Label("Current branch activity and confirmed customer rentals.", 9, false, UiTheme.Muted);
        subtitle.Location = new Point(0, 5);
        var refresh = UiTheme.Button("Refresh");
        refresh.MinimumSize = new Size(95, 34);
        refresh.Height = 34;
        refresh.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        refresh.Location = new Point(ClientSize.Width - 240, 0);
        refresh.Click += (_, _) => RefreshBookings();
        var create = UiTheme.Button("+  NEW BOOKING ENTRY", true);
        create.MinimumSize = new Size(190, 34);
        create.Height = 34;
        create.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        create.Location = new Point(ClientSize.Width - 190, 0);
        create.Click += (_, _) => OpenBooking();
        ConfigureActionButton(editSelectedButton, 120);
        editSelectedButton.Click += (_, _) => EditSelectedBooking();
        ConfigureActionButton(deleteSelectedButton, 120);
        deleteSelectedButton.BackColor = Color.FromArgb(55, 25, 34);
        deleteSelectedButton.ForeColor = UiTheme.Error;
        deleteSelectedButton.FlatAppearance.BorderColor = UiTheme.Error;
        deleteSelectedButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(76, 30, 42);
        deleteSelectedButton.Click += (_, _) => DeleteSelectedBooking();
        selectAllCheckBox.CheckedChanged += (_, _) =>
        {
            if (synchronizingSelection) return;
            synchronizingSelection = true;
            try
            {
                foreach (DataGridViewRow row in bookingsGrid.Rows)
                    row.Cells["Select"].Value = selectAllCheckBox.Checked;
            }
            finally
            {
                synchronizingSelection = false;
            }
            UpdateSelectionActions();
        };
        editSelectedButton.Enabled = false;
        deleteSelectedButton.Enabled = false;
        actions.Controls.Add(subtitle);
        actions.Controls.Add(refresh);
        actions.Controls.Add(create);
        actions.Controls.Add(editSelectedButton);
        actions.Controls.Add(deleteSelectedButton);
        actions.Controls.Add(selectAllCheckBox);
        actions.Resize += (_, _) =>
        {
            subtitle.Visible = actions.ClientSize.Width >= 1050;
            create.Location = new Point(actions.ClientSize.Width - create.Width, 0);
            refresh.Location = new Point(create.Left - refresh.Width - 10, 0);
            editSelectedButton.Location = new Point(refresh.Left - editSelectedButton.Width - 10, 0);
            deleteSelectedButton.Location = new Point(editSelectedButton.Left - deleteSelectedButton.Width - 10, 0);
            selectAllCheckBox.Location = new Point(deleteSelectedButton.Left - selectAllCheckBox.Width - 12, 8);
        };
        create.Location = new Point(actions.ClientSize.Width - create.Width, 0);
        refresh.Location = new Point(create.Left - refresh.Width - 10, 0);
        editSelectedButton.Location = new Point(refresh.Left - editSelectedButton.Width - 10, 0);
        deleteSelectedButton.Location = new Point(editSelectedButton.Left - deleteSelectedButton.Width - 10, 0);
        selectAllCheckBox.Location = new Point(deleteSelectedButton.Left - selectAllCheckBox.Width - 12, 8);
        content.Controls.Add(actions, 0, 1);

        var gridPanel = UiTheme.SurfacePanel();
        gridPanel.Dock = DockStyle.Fill;
        gridPanel.Padding = new Padding(1);
        gridPanel.Margin = new Padding(0);
        ConfigureGrid();
        gridPanel.Controls.Add(bookingsGrid);
        emptyState.Dock = DockStyle.Fill;
        emptyState.TextAlign = ContentAlignment.MiddleCenter;
        emptyState.BackColor = UiTheme.Surface;
        content.Controls.Add(gridPanel, 0, 2);
        countLabel.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
        countLabel.Font = UiTheme.MonoFont;
        content.Controls.Add(countLabel, 0, 3);
        return content;
    }

    private void ConfigureGrid()
    {
        bookingsGrid.Dock = DockStyle.Fill;
        bookingsGrid.BackgroundColor = UiTheme.Surface;
        bookingsGrid.BorderStyle = BorderStyle.None;
        bookingsGrid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        bookingsGrid.GridColor = UiTheme.Border;
        bookingsGrid.EnableHeadersVisualStyles = false;
        bookingsGrid.ColumnHeadersDefaultCellStyle.BackColor = UiTheme.SurfaceRaised;
        bookingsGrid.ColumnHeadersDefaultCellStyle.ForeColor = UiTheme.Muted;
        bookingsGrid.ColumnHeadersDefaultCellStyle.Font = new Font("Consolas", 9F, FontStyle.Bold);
        bookingsGrid.ColumnHeadersHeight = 44;
        bookingsGrid.DefaultCellStyle.BackColor = UiTheme.Surface;
        bookingsGrid.DefaultCellStyle.ForeColor = UiTheme.Text;
        bookingsGrid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(28, 57, 65);
        bookingsGrid.DefaultCellStyle.SelectionForeColor = UiTheme.Text;
        bookingsGrid.DefaultCellStyle.Padding = new Padding(6);
        bookingsGrid.RowTemplate.Height = 42;
        bookingsGrid.RowHeadersVisible = false;
        bookingsGrid.AllowUserToAddRows = false;
        bookingsGrid.AllowUserToDeleteRows = false;
        bookingsGrid.ReadOnly = true;
        bookingsGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        bookingsGrid.MultiSelect = true;
        bookingsGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        bookingsGrid.SelectionChanged += (_, _) => UpdateSelectionActions();
        bookingsGrid.CurrentCellDirtyStateChanged += (_, _) =>
        {
            if (bookingsGrid.IsCurrentCellDirty)
                bookingsGrid.CommitEdit(DataGridViewDataErrorContexts.Commit);
        };
        bookingsGrid.CellValueChanged += (_, eventArgs) =>
        {
            if (eventArgs.RowIndex >= 0 && bookingsGrid.Columns[eventArgs.ColumnIndex].Name == "Select")
                UpdateSelectionActions();
        };
        var selectColumn = new DataGridViewCheckBoxColumn
        {
            Name = "Select",
            HeaderText = "SELECT",
            Width = 56,
            MinimumWidth = 52,
            AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
            ReadOnly = false
        };
        bookingsGrid.ReadOnly = false;
        bookingsGrid.Columns.Add(selectColumn);
        bookingsGrid.Columns.Add("BookingId", "BOOKING ID");
        bookingsGrid.Columns.Add("Customer", "CUSTOMER NAME");
        bookingsGrid.Columns.Add("Vehicle", "VEHICLE TYPE");
        bookingsGrid.Columns.Add("Fuel", "FUEL TYPE");
        bookingsGrid.Columns.Add("Days", "DAYS");
        bookingsGrid.Columns.Add("Total", "TOTAL");
        bookingsGrid.Columns.Add("Status", "STATUS");
        foreach (DataGridViewColumn column in bookingsGrid.Columns)
            column.ReadOnly = column.Name != "Select";
    }

    private void RefreshBookings()
    {
        try
        {
            var bookings = repository.GetAll().OrderByDescending(booking => booking.CreatedAt).ToList();
            bookingsGrid.Rows.Clear();
            bookingsGrid.Visible = bookings.Count > 0;
            selectAllCheckBox.Enabled = bookings.Count > 0;
            if (bookings.Count == 0)
            {
                if (!bookingsGrid.Parent!.Controls.Contains(emptyState)) bookingsGrid.Parent.Controls.Add(emptyState);
                emptyState.BringToFront();
            }
            else
            {
                emptyState.Visible = false;
                foreach (var booking in bookings) AddBookingRow(booking);
            }
            countLabel.Text = $"{bookings.Count} CONFIRMED RENTAL{(bookings.Count == 1 ? "" : "S")}";
            UpdateSelectionActions();
        }
        catch (InvalidDataException exception)
        {
            MessageBox.Show(this, exception.Message, "Booking storage", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void AddBookingRow(Booking booking)
    {
        var status = booking.ReturnDate.Date < DateTime.Today ? BookingStatus.Completed : BookingStatus.Active;
        var customerName = string.Join(" ", new[] { booking.CustomerFirstName, booking.CustomerSurname }
            .Where(name => !string.IsNullOrWhiteSpace(name)));
        if (string.IsNullOrWhiteSpace(customerName)) customerName = "(customer name missing)";
        var rowIndex = bookingsGrid.Rows.Add(
            false,
            booking.BookingId,
            customerName,
            booking.CarType.ToString(),
            booking.FuelType.ToString(),
            booking.RentalDays,
            booking.TotalPrice.ToString("C2", System.Globalization.CultureInfo.GetCultureInfo("en-GB")),
            status.ToString().ToUpperInvariant());
        bookingsGrid.Rows[rowIndex].Tag = booking;
        var cell = bookingsGrid.Rows[rowIndex].Cells[7];
        cell.Style.ForeColor = status == BookingStatus.Active ? UiTheme.Success : UiTheme.Muted;
    }

    private void OpenBooking()
    {
        using var bookingForm = new BookingForm(repository);
        if (bookingForm.ShowDialog(this) == DialogResult.OK) RefreshBookings();
    }

    private static void ConfigureActionButton(Button button, int width)
    {
        button.AutoSize = false;
        button.MinimumSize = new Size(width, 34);
        button.Size = new Size(width, 34);
        button.Anchor = AnchorStyles.Top | AnchorStyles.Right;
    }

    private void UpdateSelectionActions()
    {
        if (synchronizingSelection) return;

        var checkedCount = bookingsGrid.Rows
            .Cast<DataGridViewRow>()
            .Count(row => row.Cells["Select"].Value is true);
        var allChecked = bookingsGrid.Rows.Count > 0 && checkedCount == bookingsGrid.Rows.Count;
        synchronizingSelection = true;
        selectAllCheckBox.Checked = allChecked;
        synchronizingSelection = false;

        var actionCount = GetActionBookings().Count;
        editSelectedButton.Enabled = actionCount == 1;
        deleteSelectedButton.Enabled = actionCount > 0;
    }

    private List<Booking> GetActionBookings()
    {
        var checkedBookings = bookingsGrid.Rows
            .Cast<DataGridViewRow>()
            .Where(row => row.Cells["Select"].Value is true)
            .Select(row => row.Tag as Booking)
            .OfType<Booking>()
            .ToList();
        if (checkedBookings.Count > 0) return checkedBookings;

        return bookingsGrid.SelectedRows
            .Cast<DataGridViewRow>()
            .Select(row => row.Tag as Booking)
            .OfType<Booking>()
            .DistinctBy(booking => booking.BookingId)
            .ToList();
    }

    private void EditSelectedBooking()
    {
        try
        {
            var bookings = GetActionBookings();
            if (bookings.Count != 1)
            {
                MessageBox.Show(this, "Select exactly one booking to edit.", "Select one booking", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var bookingForm = new BookingForm(repository, bookings[0]);
            if (bookingForm.ShowDialog(this) == DialogResult.OK) RefreshBookings();
        }
        catch (Exception exception) when (exception is IOException or InvalidDataException or UnauthorizedAccessException)
        {
            MessageBox.Show(this, exception.Message, "Booking storage", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void DeleteSelectedBooking()
    {
        try
        {
            var bookings = GetActionBookings();
            if (bookings.Count == 0)
            {
                MessageBox.Show(this, "Select one or more bookings to delete.", "No bookings selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var confirmationText = bookings.Count == 1
                ? $"Delete the booking for {DisplayCustomerName(bookings[0])} ({bookings[0].BookingId})? This cannot be undone."
                : $"Delete these {bookings.Count} bookings? This cannot be undone.";
            var answer = MessageBox.Show(this,
                confirmationText,
                "Confirm booking deletion", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
            if (answer != DialogResult.Yes) return;

            repository.DeleteMany(bookings.Select(booking => booking.BookingId));
            RefreshBookings();
        }
        catch (Exception exception) when (exception is IOException or InvalidDataException or UnauthorizedAccessException or InvalidOperationException)
        {
            MessageBox.Show(this, exception.Message, "Booking storage", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private static string DisplayCustomerName(Booking booking)
    {
        var customerName = string.Join(" ", new[] { booking.CustomerFirstName, booking.CustomerSurname }
            .Where(name => !string.IsNullOrWhiteSpace(name)));
        return string.IsNullOrWhiteSpace(customerName) ? "(customer name missing)" : customerName;
    }
}