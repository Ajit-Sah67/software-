namespace WeAreCars;

internal sealed class SplashForm : Form
{
    public SplashForm()
    {
        UiTheme.StyleForm(this, "WeAreCars | Welcome");
        MinimumSize = new Size(760, 620);
        Size = new Size(1040, 720);

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Padding = new Padding(42),
            BackColor = UiTheme.Background
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 54));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 46));

        var copy = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 6,
            Padding = new Padding(0, 28, 28, 0)
        };
        copy.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
        copy.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
        copy.RowStyles.Add(new RowStyle(SizeType.Absolute, 160));
        copy.RowStyles.Add(new RowStyle(SizeType.Absolute, 64));
        copy.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        copy.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));

        var logo = UiTheme.Label("◈  WeAreCars", 19, true, UiTheme.Cyan);
        logo.Anchor = AnchorStyles.Left;
        copy.Controls.Add(logo, 0, 0);
        var eyebrow = UiTheme.Label("STAFF RENTAL MANAGEMENT", 9, true, UiTheme.Muted);
        eyebrow.Font = UiTheme.MonoFont;
        copy.Controls.Add(eyebrow, 0, 1);
        var heading = UiTheme.Label("WELCOME TO\nWEARECARS\nRENTAL", 34, true);
        heading.AutoSize = false;
        heading.Dock = DockStyle.Fill;
        heading.Margin = new Padding(0, 8, 0, 0);
        heading.TextAlign = ContentAlignment.MiddleLeft;
        copy.Controls.Add(heading, 0, 2);
        var continueButton = UiTheme.Button("Continue to staff login  →", true);
        continueButton.Width = 280;
        continueButton.Click += (_, _) => DialogResult = DialogResult.OK;
        copy.Controls.Add(continueButton, 0, 3);
        var footer = UiTheme.Label("SECURE STAFF ACCESS", 8, false, UiTheme.Muted);
        footer.Font = UiTheme.MonoFont;
        footer.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
        copy.Controls.Add(footer, 0, 5);

        var art = new CarArtwork { Dock = DockStyle.Fill, Margin = new Padding(8, 24, 0, 24) };
        layout.Controls.Add(copy, 0, 0);
        layout.Controls.Add(art, 1, 0);
        Controls.Add(layout);
    }
}

internal sealed class CarArtwork : Panel
{
    public CarArtwork()
    {
        DoubleBuffered = true;
        BackColor = Color.FromArgb(12, 17, 28);
        Padding = new Padding(12);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        var bounds = ClientRectangle;
        if (bounds.Width < 20 || bounds.Height < 20) return;
        using var border = new Pen(UiTheme.Border, 1);
        e.Graphics.DrawRectangle(border, 0, 0, bounds.Width - 1, bounds.Height - 1);

        var scale = Math.Min(bounds.Width / 440f, bounds.Height / 370f);
        var x = (bounds.Width - 440 * scale) / 2;
        var y = (bounds.Height - 370 * scale) / 2;
        e.Graphics.TranslateTransform(x, y);
        e.Graphics.ScaleTransform(scale, scale);

        using (var glow = new SolidBrush(Color.FromArgb(26, 0, 218, 231)))
            e.Graphics.FillEllipse(glow, 40, 270, 360, 48);
        using (var body = new SolidBrush(Color.FromArgb(37, 49, 67)))
        using (var outline = new Pen(UiTheme.Cyan, 2.4f))
        {
            var car = new Point[]
            {
                new(56, 253), new(82, 220), new(127, 201), new(170, 151),
                new(270, 151), new(326, 208), new(373, 220), new(393, 253),
                new(387, 278), new(62, 278)
            };
            e.Graphics.FillPolygon(body, car);
            e.Graphics.DrawPolygon(outline, car);
        }
        using (var glass = new SolidBrush(Color.FromArgb(14, 28, 43)))
        using (var trim = new Pen(Color.FromArgb(78, 111, 134), 2))
        {
            e.Graphics.FillPolygon(glass, new Point[] { new(147, 204), new(180, 162), new(224, 162), new(224, 204) });
            e.Graphics.FillPolygon(glass, new Point[] { new(233, 162), new(266, 162), new(306, 204), new(233, 204) });
            e.Graphics.DrawLine(trim, 229, 159, 229, 207);
        }
        using (var wheel = new SolidBrush(UiTheme.Background))
        using (var hub = new Pen(UiTheme.Cyan, 2))
        {
            foreach (var center in new[] { new Point(119, 275), new Point(332, 275) })
            {
                e.Graphics.FillEllipse(wheel, center.X - 28, center.Y - 28, 56, 56);
                e.Graphics.DrawEllipse(hub, center.X - 17, center.Y - 17, 34, 34);
                e.Graphics.DrawEllipse(hub, center.X - 4, center.Y - 4, 8, 8);
            }
        }
        using var caption = new Font("Consolas", 10F, FontStyle.Regular);
        using var muted = new SolidBrush(UiTheme.Muted);
        e.Graphics.DrawString("FLEET READY  /  RENTAL SYSTEMS", caption, muted, 42, 324);
        e.Graphics.ResetTransform();
    }
}