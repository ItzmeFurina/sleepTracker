namespace WinFormsApp1;

internal sealed class HistoryForm : Form
{
    private readonly DataGridView grid = new();
    private readonly TextBox searchBox = new();
    private readonly DateTimePicker fromPicker = new() { Format = DateTimePickerFormat.Short, ShowCheckBox = true };
    private readonly DateTimePicker toPicker = new() { Format = DateTimePickerFormat.Short, ShowCheckBox = true };

    public HistoryForm()
    {
        Text = "Sleep Tracker - History";
        ClientSize = new Size(1050, 650);
        MinimumSize = new Size(850, 550);
        StartPosition = FormStartPosition.CenterParent;
        BackColor = UiTheme.Background;
        Icon = AppBrand.Icon;
        var title = UiTheme.Label("Sleep history", 20, FontStyle.Bold); title.Location = new Point(25, 20); Controls.Add(title);
        var subtitle = UiTheme.Label("Review, search, and update your saved nights", 9); subtitle.Location = new Point(28, 57); subtitle.ForeColor = UiTheme.Muted; Controls.Add(subtitle);
        Controls.Add(UiTheme.Label("Search", 9, FontStyle.Bold)); Controls[^1].Location = new Point(25, 92);
        searchBox.SetBounds(25, 112, 220, 32); searchBox.PlaceholderText = "Quality or notes"; searchBox.TextChanged += (_, _) => LoadRecords(); Controls.Add(searchBox);
        var fromLabel = UiTheme.Label("From", 9, FontStyle.Bold); fromLabel.Location = new Point(275, 92); Controls.Add(fromLabel);
        fromPicker.SetBounds(275, 112, 125, 32); Controls.Add(fromPicker);
        var toLabel = UiTheme.Label("To", 9, FontStyle.Bold); toLabel.Location = new Point(425, 92); Controls.Add(toLabel);
        toPicker.SetBounds(425, 112, 125, 32); Controls.Add(toPicker);
        var filter = UiTheme.Button("Apply filters", 125); filter.Location = new Point(580, 110); filter.Click += (_, _) => LoadRecords(); Controls.Add(filter);
        var details = UiTheme.Button("View details", 125); details.Location = new Point(720, 110); details.Click += ShowDetails; Controls.Add(details);
        var edit = UiTheme.Button("Edit", 100); edit.Location = new Point(25, 585); edit.Click += EditSelected; Controls.Add(edit);
        var delete = UiTheme.Button("Delete", 100); delete.Location = new Point(140, 585); delete.BackColor = Color.FromArgb(170, 83, 91); delete.Click += DeleteSelected; Controls.Add(delete);
        grid.SetBounds(25, 165, 985, 395); grid.ReadOnly = true; grid.AllowUserToAddRows = false; grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect; grid.MultiSelect = false; grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill; grid.BackgroundColor = Color.White; grid.BorderStyle = BorderStyle.None; grid.RowHeadersVisible = false; grid.DoubleBuffered(true);
        Controls.Add(grid); LoadRecords();
    }

    private void LoadRecords()
    {
        var records = DataStore.GetRecords(fromPicker.Checked ? fromPicker.Value.Date : null, toPicker.Checked ? toPicker.Value.Date : null);
        var search = searchBox.Text.Trim();
        if (search.Length > 0)
        {
            records = records.Where(r => r.Quality.Contains(search, StringComparison.OrdinalIgnoreCase) || r.Notes.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();
        }
        grid.DataSource = records.Select(r => new { r.Id, Date = r.SleepDate.ToString("MMM dd, yyyy"), Bedtime = r.BedTime.ToString(@"hh\:mm"), WakeUp = r.WakeTime.ToString(@"hh\:mm"), Duration = r.DurationText, r.Quality, r.Notes }).ToList();
        var idColumn = grid.Columns["Id"]; if (idColumn is not null) idColumn.Visible = false;
    }

    private SleepRecord? SelectedRecord()
    {
        if (grid.CurrentRow?.Cells["Id"].Value is not int id) return null;
        return DataStore.GetRecords().FirstOrDefault(r => r.Id == id);
    }

    private void ShowDetails(object? sender, EventArgs e)
    {
        var record = SelectedRecord();
        if (record is null) return;
        MessageBox.Show($"{record.SleepDate:dddd, MMMM d, yyyy}\n\nBedtime: {record.BedTime.ToString(@"hh\:mm")}\nWake-up: {record.WakeTime.ToString(@"hh\:mm")}\nDuration: {record.DurationText}\nQuality: {record.Quality}\n\n{(string.IsNullOrWhiteSpace(record.Notes) ? "No notes" : record.Notes)}", "Sleep record details", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void EditSelected(object? sender, EventArgs e)
    {
        var record = SelectedRecord(); if (record is null) return;
        using var form = new EditSleepRecordForm(record); if (form.ShowDialog(this) == DialogResult.OK) LoadRecords();
    }

    private void DeleteSelected(object? sender, EventArgs e)
    {
        var record = SelectedRecord(); if (record is null) return;
        if (MessageBox.Show("Delete this sleep record? This cannot be undone.", "Confirm deletion", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes) { DataStore.DeleteRecord(record.Id); LoadRecords(); }
    }
}

internal static class DataGridViewExtensions
{
    public static void DoubleBuffered(this DataGridView grid, bool enabled)
    {
        typeof(DataGridView).GetProperty("DoubleBuffered", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)?.SetValue(grid, enabled);
    }
}