namespace WinFormsApp1;

internal sealed class ProfileSetupForm : Form
{
    private readonly TextBox nameBox = new();

    public ProfileSetupForm()
    {
        Text = "Sleep Tracker - Welcome";
        ClientSize = new Size(520, 330);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = UiTheme.Background;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        Icon = AppBrand.Icon;

        var logo = new PictureBox { Image = AppBrand.Logo, SizeMode = PictureBoxSizeMode.Zoom, Bounds = new Rectangle(25, 45, 60, 60), BackColor = Color.Transparent };
        var title = UiTheme.Label("Welcome to Sleep Tracker", 24, FontStyle.Bold); title.Location = new Point(95, 55);
        var prompt = UiTheme.Label("What should we call you?", 12); prompt.Location = new Point(155, 125);
        nameBox.SetBounds(95, 165, 330, 38);
        nameBox.Font = new Font("Segoe UI", 12);
        nameBox.PlaceholderText = "Enter your name";

        var continueButton = UiTheme.Button("Continue", 180); continueButton.Location = new Point(170, 230); continueButton.Height = 42;
        continueButton.Click += SaveName;
        AcceptButton = continueButton;
        Controls.AddRange([logo, title, prompt, nameBox, continueButton]);
    }

    private void SaveName(object? sender, EventArgs e)
    {
        var name = nameBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            MessageBox.Show("Please enter your name.", "Name required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        DataStore.SaveProfile(name);
        Hide();
        using var dashboard = new DashboardForm(name);
        dashboard.ShowDialog(this);
        Close();
    }
}
