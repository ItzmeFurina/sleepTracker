namespace WinFormsApp1;

internal sealed class AddSleepRecordForm : Form
{
    private readonly DateTimePicker datePicker = new() { Format = DateTimePickerFormat.Short };
    private readonly DateTimePicker bedPicker = new() { Format = DateTimePickerFormat.Time, ShowUpDown = true };
    private readonly DateTimePicker wakePicker = new() { Format = DateTimePickerFormat.Time, ShowUpDown = true };
    private readonly ComboBox qualityBox = new();
    private readonly TextBox notesBox = new();
    private readonly Label durationLabel = new();

    public AddSleepRecordForm()
    {
        Text = "Add Sleep Record";
        ClientSize = new Size(520, 470);
        StartPosition = FormStartPosition.CenterParent;
        BackColor = UiTheme.Background;
        Icon = AppBrand.Icon;
        var title = UiTheme.Label("Add Sleep Record", 20, FontStyle.Bold); title.Location = new Point(32, 22);
        AddRow("Sleep date", datePicker, 78);
        AddRow("Bedtime", bedPicker, 128);
        AddRow("Wake-up time", wakePicker, 178);
        AddRow("Sleep quality", qualityBox, 228);
        AddRow("Notes", notesBox, 278);
        notesBox.Multiline = true; notesBox.Height = 55;
        qualityBox.DropDownStyle = ComboBoxStyle.DropDownList;
        qualityBox.Items.AddRange(["Poor", "Fair", "Good", "Excellent"]);
        qualityBox.SelectedIndex = 2;
        durationLabel.SetBounds(145, 345, 280, 25);
        durationLabel.Font = new Font("Segoe UI", 10, FontStyle.Bold);
        var save = UiTheme.Button("Save Record", 145); save.Location = new Point(145, 395);
        save.Click += SaveRecord;
        bedPicker.ValueChanged += (_, _) => UpdateDuration();
        wakePicker.ValueChanged += (_, _) => UpdateDuration();
        Controls.AddRange([title, datePicker, bedPicker, wakePicker, qualityBox, notesBox, durationLabel, save]);
        UpdateDuration();
    }

    private void AddRow(string caption, Control control, int y)
    {
        Controls.Add(new Label { Text = caption, AutoSize = true, Location = new Point(32, y + 8), Font = new Font("Segoe UI", 10) });
        control.SetBounds(145, y, 280, control is TextBox ? 55 : 32);
        control.Font = new Font("Segoe UI", 10);
    }

    private void UpdateDuration()
    {
        var minutes = (int)(wakePicker.Value.TimeOfDay - bedPicker.Value.TimeOfDay).TotalMinutes;
        if (minutes <= 0) minutes += 1440;
        durationLabel.Text = $"Duration: {DashboardForm.FormatMinutes(minutes)}";
    }

    private void SaveRecord(object? sender, EventArgs e)
    {
        if (DataStore.HasRecordOnDate(datePicker.Value.Date))
        {
            MessageBox.Show("There is already a sleep record for this date. Edit the existing record instead.", "Duplicate date", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        var quality = qualityBox.SelectedItem?.ToString() ?? "Good";
        var notes = notesBox.Text.Trim();
        var record = DataStore.AddSleepRecord(datePicker.Value.Date, bedPicker.Value.TimeOfDay, wakePicker.Value.TimeOfDay, quality, notes);
        var goal = DataStore.GetSleepGoalMinutes();
        var message = $"Saved {record.DurationText} of sleep.";
        if (record.DurationMinutes >= goal && DataStore.MarkGoalAchievement(record.Id))
        {
            message = $"🎉 Amazing! You reached your sleep goal!\n\n{record.DurationText} / {DashboardForm.FormatMinutes(goal)} — Goal achieved!";
        }
        else if (record.DurationMinutes < goal)
        {
            message += $"\n\nYour goal is {DashboardForm.FormatMinutes(goal)}. Every bit of rest helps—keep going!";
        }
        MessageBox.Show(message, "Sleep saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
        DialogResult = DialogResult.OK;
        Close();
    }
}
