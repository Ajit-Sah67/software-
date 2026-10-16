using System.Drawing.Drawing2D;

namespace WeAreCars;

internal static class UiTheme
{
    public static readonly Color Background = Color.FromArgb(8, 10, 17);
    public static readonly Color Surface = Color.FromArgb(19, 23, 35);
    public static readonly Color SurfaceRaised = Color.FromArgb(26, 31, 46);
    public static readonly Color Input = Color.FromArgb(13, 17, 27);
    public static readonly Color Cyan = Color.FromArgb(0, 218, 231);
    public static readonly Color Text = Color.FromArgb(241, 245, 249);
    public static readonly Color Muted = Color.FromArgb(148, 163, 184);
    public static readonly Color Border = Color.FromArgb(43, 52, 69);
    public static readonly Color Error = Color.FromArgb(255, 92, 112);
    public static readonly Color Success = Color.FromArgb(0, 214, 143);
    public static readonly Font BodyFont = new("Segoe UI", 10F);
    public static readonly Font MonoFont = new("Consolas", 9.5F);

    public static Label Label(string text, float size = 10F, bool bold = false, Color? color = null)
    {
        return new Label
        {
            Text = text,
            AutoSize = true,
            ForeColor = color ?? Text,
            Font = new Font("Segoe UI", size, bold ? FontStyle.Bold : FontStyle.Regular),
            Margin = new Padding(0, 0, 0, 7)
        };
    }

    public static Button Button(string text, bool primary = false)
    {
        var button = new Button
        {
            Text = text,
            AutoSize = true,
            MinimumSize = new Size(136, 42),
            Padding = new Padding(12, 4, 12, 4),
            FlatStyle = FlatStyle.Flat,
            BackColor = primary ? Cyan : SurfaceRaised,
            ForeColor = primary ? Background : Text,
            Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
            Cursor = Cursors.Hand,
            UseVisualStyleBackColor = false,
            Margin = new Padding(0, 0, 10, 0)
        };
        button.FlatAppearance.BorderColor = primary ? Cyan : Border;
        button.FlatAppearance.BorderSize = 1;
        button.FlatAppearance.MouseOverBackColor = primary
            ? Color.FromArgb(72, 235, 242)
            : Color.FromArgb(37, 45, 63);
        return button;
    }

    public static TextBox TextBox(string placeholder = "", bool multiline = false)
    {
        return new TextBox
        {
            PlaceholderText = placeholder,
            Multiline = multiline,
            AcceptsReturn = multiline,
            ScrollBars = multiline ? ScrollBars.Vertical : ScrollBars.None,
            BackColor = Input,
            ForeColor = Text,
            BorderStyle = BorderStyle.FixedSingle,
            Font = BodyFont,
            Dock = DockStyle.Fill,
            Height = multiline ? 82 : 38,
            Margin = new Padding(0, 0, 0, 12)
        };
    }

    public static ComboBox ComboBox()
    {
        return new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            BackColor = Input,
            ForeColor = Text,
            FlatStyle = FlatStyle.Flat,
            Font = BodyFont,
            Dock = DockStyle.Fill,
            Height = 38,
            Margin = new Padding(0, 0, 0, 12)
        };
    }

    public static Panel SurfacePanel() => new()
    {
        BackColor = Surface,
        Padding = new Padding(18),
        Margin = new Padding(0, 0, 0, 16)
    };

    public static void StyleForm(Form form, string title)
    {
        form.Text = title;
        form.BackColor = Background;
        form.ForeColor = Text;
        form.Font = BodyFont;
        form.StartPosition = FormStartPosition.CenterScreen;
        form.MinimumSize = new Size(800, 620);
        form.Size = new Size(1180, 820);
    }

    public static GraphicsPath RoundedPath(Rectangle bounds, int radius)
    {
        var diameter = radius * 2;
        var path = new GraphicsPath();
        path.AddArc(bounds.X, bounds.Y, diameter, diameter, 180, 90);
        path.AddArc(bounds.Right - diameter, bounds.Y, diameter, diameter, 270, 90);
        path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
        path.AddArc(bounds.X, bounds.Bottom - diameter, diameter, diameter, 90, 90);
        path.CloseFigure();
        return path;
    }
}