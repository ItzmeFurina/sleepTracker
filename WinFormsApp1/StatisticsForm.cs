namespace WinFormsApp1;

internal sealed class StatisticsForm : Form
{
    private readonly ComboBox rangeBox = new();
    private readonly Label emptyState = UiTheme.Label("", 11);
    private readonly SleepChartPanel trendChart = new(ChartKind.Line);
    private readonly SleepChartPanel dailyChart = new(ChartKind.Bar);
    private readonly SleepChartPanel qualityChart = new(ChartKind.Pie);
    private readonly Label[] cards = Enumerable.Range(0, 6).Select(_ => UiTheme.Label("", 11, FontStyle.Bold)).ToArray();

    public StatisticsForm()
    {
        Text = "Sleep Tracker - Statistics"; ClientSize = new Size(1180, 820); MinimumSize = new Size(1000, 700); StartPosition = FormStartPosition.CenterParent; BackColor = UiTheme.Background; Icon = AppBrand.Icon;
        var title = UiTheme.Label("Sleep statistics", 20, FontStyle.Bold); title.Location = new Point(25, 20); Controls.Add(title);
        var subtitle = UiTheme.Label("Understand your patterns and celebrate steady progress", 9); subtitle.Location = new Point(28, 57); subtitle.ForeColor = UiTheme.Muted; Controls.Add(subtitle);
        var rangeLabel = UiTheme.Label("Date range", 9, FontStyle.Bold); rangeLabel.Location = new Point(800, 30); Controls.Add(rangeLabel);
        rangeBox.Items.AddRange(["This Week", "This Month", "Last 30 Days", "All Time"]); rangeBox.SelectedIndex = 0; rangeBox.DropDownStyle = ComboBoxStyle.DropDownList; rangeBox.SetBounds(900, 24, 215, 32); rangeBox.SelectedIndexChanged += (_, _) => LoadStats(); Controls.Add(rangeBox);
        for (var i = 0; i < cards.Length; i++)
        {
            var card = UiTheme.CardPanel(25 + (i % 3) * 245, 95 + (i / 3) * 95, 225, 82);
            cards[i].Location = new Point(16, 20); cards[i].MaximumSize = new Size(195, 54); cards[i].AutoSize = true; card.Controls.Add(cards[i]); Controls.Add(card);
        }
        emptyState.Location = new Point(30, 305); emptyState.ForeColor = UiTheme.Muted; Controls.Add(emptyState);
        trendChart.SetBounds(25, 320, 545, 235); dailyChart.SetBounds(590, 320, 545, 235); qualityChart.SetBounds(25, 575, 545, 220); Controls.AddRange([trendChart, dailyChart, qualityChart]);
        LoadStats();
    }

    private void LoadStats()
    {
        var (from, to) = SelectedRange();
        var records = DataStore.GetRecords(from, to).OrderBy(r => r.SleepDate).ToList();
        var goal = DataStore.GetSleepGoalMinutes();
        var hasData = records.Count > 0;
        emptyState.Text = hasData ? string.Empty : "No records in this range yet. Add a sleep record to unlock your trends and quality breakdown.";
        var average = hasData ? (int)records.Average(r => r.DurationMinutes) : 0;
        var longest = hasData ? records.Max(r => r.DurationMinutes) : 0;
        var shortest = hasData ? records.Min(r => r.DurationMinutes) : 0;
        var goalDays = records.Count(r => r.DurationMinutes >= goal);
        cards[0].Text = $"Average sleep\n{(hasData ? DashboardForm.FormatMinutes(average) : "—")}";
        cards[1].Text = $"Longest sleep\n{(hasData ? DashboardForm.FormatMinutes(longest) : "—")}";
        cards[2].Text = $"Shortest sleep\n{(hasData ? DashboardForm.FormatMinutes(shortest) : "—")}";
        cards[3].Text = $"Total records\n{records.Count}";
        cards[4].Text = $"Goal days\n{goalDays}";
        cards[5].Text = $"Goal achievement\n{(records.Count == 0 ? 0 : goalDays * 100 / records.Count)}%";
        trendChart.Records = records; trendChart.GoalMinutes = goal; trendChart.Invalidate();
        dailyChart.Records = records; dailyChart.GoalMinutes = goal; dailyChart.Invalidate();
        qualityChart.Records = records; qualityChart.GoalMinutes = goal; qualityChart.Invalidate();
    }

    private (DateTime? From, DateTime? To) SelectedRange()
    {
        var today = DateTime.Today;
        return rangeBox.SelectedIndex switch
        {
            0 => (today.AddDays(-(int)today.DayOfWeek + 1), today),
            1 => (new DateTime(today.Year, today.Month, 1), today),
            2 => (today.AddDays(-29), today),
            _ => (null, null)
        };
    }
}

internal enum ChartKind { Line, Bar, Pie }

internal sealed class SleepChartPanel : Panel
{
    internal List<SleepRecord> Records = [];
    internal int GoalMinutes = 480;
    private readonly ChartKind kind;

    public SleepChartPanel(ChartKind kind) { this.kind = kind; BackColor = Color.White; Padding = new Padding(12); DoubleBuffered = true; }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        using var titleFont = new Font("Segoe UI", 10, FontStyle.Bold);
        using var bodyFont = new Font("Segoe UI", 8);
        using var ink = new SolidBrush(UiTheme.Ink);
        var title = kind switch { ChartKind.Line => "Sleep duration trend", ChartKind.Bar => "Daily sleep duration", _ => "Sleep quality distribution" };
        e.Graphics.DrawString(title, titleFont, ink, 14, 12);
        if (Records.Count == 0) { e.Graphics.DrawString("No data for this range", bodyFont, Brushes.Gray, 14, 55); return; }
        var area = new Rectangle(42, 45, Width - 58, Height - 78);
        if (kind == ChartKind.Pie) DrawPie(e.Graphics, area, bodyFont); else if (kind == ChartKind.Line) DrawLine(e.Graphics, area, bodyFont); else DrawBars(e.Graphics, area, bodyFont);
    }

    private void DrawLine(Graphics g, Rectangle area, Font font)
    {
        var max = Math.Max(600, Records.Max(r => r.DurationMinutes)); DrawAxes(g, area, max, font);
        var points = Records.Select((r, i) => new PointF(area.Left + (Records.Count == 1 ? area.Width / 2 : i * area.Width / (float)(Records.Count - 1)), area.Bottom - r.DurationMinutes * area.Height / (float)max)).ToArray();
        using var goalPen = new Pen(UiTheme.Goal, 1) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash }; var goalY = area.Bottom - GoalMinutes * area.Height / (float)max; g.DrawLine(goalPen, area.Left, goalY, area.Right, goalY);
        using var pen = new Pen(UiTheme.Primary, 2); if (points.Length > 1) g.DrawLines(pen, points); foreach (var point in points) g.FillEllipse(Brushes.White, point.X - 4, point.Y - 4, 8, 8); foreach (var point in points) g.FillEllipse(new SolidBrush(UiTheme.Primary), point.X - 3, point.Y - 3, 6, 6);
        DrawLabels(g, font);
    }

    private void DrawBars(Graphics g, Rectangle area, Font font)
    {
        var max = Math.Max(600, Records.Max(r => r.DurationMinutes)); DrawAxes(g, area, max, font); var width = Math.Max(8, area.Width / Records.Count - 5);
        for (var i = 0; i < Records.Count; i++) { var record = Records[i]; var height = record.DurationMinutes * area.Height / max; var x = area.Left + i * (area.Width / Records.Count); using var brush = new SolidBrush(record.DurationMinutes >= GoalMinutes ? UiTheme.Accent : Color.FromArgb(185, 194, 215)); g.FillRectangle(brush, x, area.Bottom - height, width, height); if (Records.Count <= 12) g.DrawString(record.SleepDate.ToString("MM/dd"), font, Brushes.Gray, x, area.Bottom + 5); }
    }

    private void DrawPie(Graphics g, Rectangle area, Font font)
    {
        var groups = new[] { "Poor", "Fair", "Good", "Excellent" }.Select(q => (Name: q, Count: Records.Count(r => r.Quality == q))).ToArray(); var total = Math.Max(1, Records.Count); var colors = new[] { Color.FromArgb(190, 103, 112), Color.FromArgb(224, 170, 86), Color.FromArgb(101, 154, 163), Color.FromArgb(74, 92, 150) }; var bounds = new Rectangle(area.Left, area.Top, Math.Min(area.Height, 145), Math.Min(area.Height, 145)); float start = 0;
        for (var i = 0; i < groups.Length; i++) { var sweep = groups[i].Count * 360f / total; using var brush = new SolidBrush(colors[i]); g.FillPie(brush, bounds, start, sweep); g.FillRectangle(brush, area.Left + 175, area.Top + i * 30, 14, 14); g.DrawString($"{groups[i].Name}: {groups[i].Count} ({groups[i].Count * 100 / total}%)", font, Brushes.DimGray, area.Left + 196, area.Top - 1 + i * 30); start += sweep; }
    }

    private void DrawAxes(Graphics g, Rectangle area, int max, Font font) { using var pen = new Pen(Color.FromArgb(220, 225, 234)); g.DrawLine(pen, area.Left, area.Bottom, area.Right, area.Bottom); g.DrawLine(pen, area.Left, area.Top, area.Left, area.Bottom); g.DrawString($"{max / 60}h", font, Brushes.Gray, 3, area.Top - 5); g.DrawString("0h", font, Brushes.Gray, 12, area.Bottom - 8); }
    private void DrawLabels(Graphics g, Font font) { if (Records.Count <= 12) for (var i = 0; i < Records.Count; i++) g.DrawString(Records[i].SleepDate.ToString("MM/dd"), font, Brushes.Gray, 42 + (Records.Count == 1 ? 0 : i * (Width - 58) / (float)(Records.Count - 1)), Height - 27); }
}