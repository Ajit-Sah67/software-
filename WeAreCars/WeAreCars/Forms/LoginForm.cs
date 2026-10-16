using WeAreCars.Core.Services;

namespace WeAreCars;

internal sealed class LoginForm : Form
{
    private readonly AuthenticationService authenticationService;
    private readonly TextBox username = UiTheme.TextBox("Staff username");
    private readonly TextBox password = UiTheme.TextBox("Password");
    private readonly Label error = UiTheme.Label("", 9, false, UiTheme.Error);

    public LoginForm(AuthenticationService authenticationService)
    {
        this.authenticationService = authenticationService;
        UiTheme.StyleForm(this, "WeAreCars | Secure Login");
        Size = new Size(920, 680);
        MinimumSize = new Size(760, 600);

        var card = UiTheme.SurfacePanel();
        card.Size = new Size(430, 500);
        card.Padding = new Padding(32);
        card.Anchor = AnchorStyles.None;
        card.Location = new Point((ClientSize.Width - card.Width) / 2, (ClientSize.Height - card.Height) / 2);
        Resize += (_, _) => card.Location = new Point((ClientSize.Width - card.Width) / 2, (ClientSize.Height - card.Height) / 2);

        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 10 };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var brand = UiTheme.Label("◈  WeAreCars", 20, true, UiTheme.Cyan);
        brand.Dock = DockStyle.Fill;
        brand.TextAlign = ContentAlignment.MiddleLeft;
        layout.Controls.Add(brand, 0, 0);
        layout.Controls.Add(UiTheme.Label("SECURE LOGIN", 14, true), 0, 1);
        layout.Controls.Add(UiTheme.Label("STAFF USERNAME", 8, true, UiTheme.Muted), 0, 2);
        username.Margin = new Padding(0, 0, 0, 7);
        layout.Controls.Add(username, 0, 3);
        layout.Controls.Add(UiTheme.Label("PASSWORD", 8, true, UiTheme.Muted), 0, 4);
        password.UseSystemPasswordChar = true;
        password.Margin = new Padding(0, 0, 0, 7);
        layout.Controls.Add(password, 0, 5);

        var reveal = new CheckBox { Text = "Show password", AutoSize = true, ForeColor = UiTheme.Muted, BackColor = UiTheme.Surface, Margin = new Padding(0, 0, 0, 4) };
        reveal.CheckedChanged += (_, _) => password.UseSystemPasswordChar = !reveal.Checked;
        layout.Controls.Add(reveal, 0, 6);
        error.Dock = DockStyle.Fill;
        error.AutoSize = false;
        layout.Controls.Add(error, 0, 7);

        var signIn = UiTheme.Button("LOGIN", true);
        signIn.Dock = DockStyle.Fill;
        signIn.Click += (_, _) => SignIn();
        layout.Controls.Add(signIn, 0, 8);
        layout.Controls.Add(UiTheme.Label("AUTHORIZED STAFF ACCESS ONLY", 8, false, UiTheme.Muted), 0, 9);
        card.Controls.Add(layout);
        Controls.Add(card);
        AcceptButton = signIn;
    }

    private void SignIn()
    {
        if (authenticationService.IsValid(username.Text, password.Text))
        {
            DialogResult = DialogResult.OK;
            return;
        }

        error.Text = "Credentials not recognized. Check both fields and try again.";
        password.Clear();
        password.Focus();
    }
}