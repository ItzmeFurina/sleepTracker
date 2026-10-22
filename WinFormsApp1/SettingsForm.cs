namespace WinFormsApp1;

internal sealed class SettingsForm : Form
{
    private readonly TextBox nameBox = new();
    private readonly NumericUpDown goalBox = new() { Minimum = 1, Maximum = 24, Increment = 0.25M, DecimalPlaces = 2 };
    public SettingsForm()
    {
        Text = "Profile / Settings"; ClientSize = new Size(500, 310); StartPosition = FormStartPosition.CenterParent; BackColor = UiTheme.Background; Icon = AppBrand.Icon;
        var title = UiTheme.Label("Profile & settings", 20, FontStyle.Bold); title.Location = new Point(30, 25); Controls.Add(title);
        var nameLabel = UiTheme.Label("Your name", 10, FontStyle.Bold); nameLabel.Location = new Point(30, 90); Controls.Add(nameLabel); nameBox.SetBounds(210, 84, 220, 32); nameBox.Text = DataStore.GetProfileName() ?? string.Empty; nameBox.Font = new Font("Segoe UI", 10); Controls.Add(nameBox);
        var goalLabel = UiTheme.Label("Sleep goal (hours)", 10, FontStyle.Bold); goalLabel.Location = new Point(30, 145); Controls.Add(goalLabel); goalBox.SetBounds(210, 139, 220, 32); goalBox.Value = Math.Clamp(DataStore.GetSleepGoalMinutes() / 60M, goalBox.Minimum, goalBox.Maximum); goalBox.Font = new Font("Segoe UI", 10); Controls.Add(goalBox);
        var hint = UiTheme.Label("Most adults benefit from around 8 hours. Choose what feels right for you.", 9); hint.Location = new Point(30, 185); hint.MaximumSize = new Size(430, 40); hint.ForeColor = UiTheme.Muted; Controls.Add(hint);
        var save = UiTheme.Button("Save changes", 125); save.Location = new Point(210, 230); save.Click += (_, _) => Save(); Controls.Add(save);
    }
    private void Save()
    {
        if (string.IsNullOrWhiteSpace(nameBox.Text)) { MessageBox.Show("Name is required."); return; }
        DataStore.UpdateProfile(nameBox.Text.Trim(), (int)Math.Round(goalBox.Value * 60)); DialogResult = DialogResult.OK; Close();
    }
}
