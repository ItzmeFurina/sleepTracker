namespace WinFormsApp1;

internal sealed class EditSleepRecordForm : Form
{
    private readonly SleepRecord record;
    private readonly DateTimePicker datePicker = new() { Format = DateTimePickerFormat.Short };
    private readonly DateTimePicker bedPicker = new() { Format = DateTimePickerFormat.Time, ShowUpDown = true };
    private readonly DateTimePicker wakePicker = new() { Format = DateTimePickerFormat.Time, ShowUpDown = true };
    private readonly ComboBox qualityBox = new();
    private readonly TextBox notesBox = new();

    public EditSleepRecordForm(SleepRecord record)
    {
        this.record = record;
        Text = "Edit Sleep Record";
        ClientSize = new Size(520, 470);
        StartPosition = FormStartPosition.CenterParent;
        BackColor = UiTheme.Background;
        Icon = AppBrand.Icon;
        var title = UiTheme.Label("Edit Sleep Record", 20, FontStyle.Bold); title.Location = new Point(32, 22);
        AddRow("Sleep date", datePicker, 78); AddRow("Bedtime", bedPicker, 128); AddRow("Wake-up time", wakePicker, 178); AddRow("Sleep quality", qualityBox, 228); AddRow("Notes", notesBox, 278);
        notesBox.Multiline = true; notesBox.Height = 55;
        qualityBox.DropDownStyle = ComboBoxStyle.DropDownList; qualityBox.Items.AddRange(["Poor", "Fair", "Good", "Excellent"]);
        datePicker.Value = record.SleepDate; bedPicker.Value = DateTime.Today.Add(record.BedTime); wakePicker.Value = DateTime.Today.Add(record.WakeTime); qualityBox.SelectedItem = record.Quality; notesBox.Text = record.Notes;
        var durationLabel = UiTheme.Label(string.Empty, 10, FontStyle.Bold); durationLabel.Location = new Point(145, 345);
        var save = UiTheme.Button("Save Changes", 145); save.Location = new Point(145, 395);
        save.Click += SaveChanges; bedPicker.ValueChanged += (_, _) => UpdateDuration(durationLabel); wakePicker.ValueChanged += (_, _) => UpdateDuration(durationLabel); Controls.AddRange([title, datePicker, bedPicker, wakePicker, qualityBox, notesBox, durationLabel, save]); UpdateDuration(durationLabel);
    }

    private void AddRow(string caption, Control control, int y)
    {
        Controls.Add(new Label { Text = caption, AutoSize = true, Location = new Point(32, y + 8), Font = new Font("Segoe UI", 10) });
        control.SetBounds(145, y, 280, control is TextBox ? 55 : 32); control.Font = new Font("Segoe UI", 10);
    }

    private void SaveChanges(object? sender, EventArgs e)
    {
        if (DataStore.HasRecordOnDate(datePicker.Value.Date, record.Id))
        {
            MessageBox.Show("There is already another sleep record for this date.", "Duplicate date", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        DataStore.UpdateSleepRecord(record, datePicker.Value.Date, bedPicker.Value.TimeOfDay, wakePicker.Value.TimeOfDay, qualityBox.SelectedItem?.ToString() ?? "Good", notesBox.Text.Trim());
        DialogResult = DialogResult.OK; Close();
    }

    private void UpdateDuration(Label label)
    {
        var minutes = (int)(wakePicker.Value.TimeOfDay - bedPicker.Value.TimeOfDay).TotalMinutes;
        if (minutes <= 0) minutes += 1440;
        label.Text = $"Duration: {DashboardForm.FormatMinutes(minutes)}";
    }
}
