namespace WinFormsApp1;

internal sealed class DashboardForm : Form
{
    private readonly string profileName;
    private readonly Label welcomeLabel = UiTheme.Label(string.Empty, 22, FontStyle.Bold);
    private readonly Label todayValue = UiTheme.Label(string.Empty, 18, FontStyle.Bold);
    private readonly Label todayDetail = UiTheme.Label(string.Empty, 9);
    private readonly Label averageValue = UiTheme.Label(string.Empty, 18, FontStyle.Bold);
    private readonly Label weekValue = UiTheme.Label(string.Empty, 18, FontStyle.Bold);
    private readonly Label goalValue = UiTheme.Label(string.Empty, 15, FontStyle.Bold);
    private readonly Label goalDetail = UiTheme.Label(string.Empty, 9);
    private readonly Label latestValue = UiTheme.Label(string.Empty, 11);
    private readonly Label achievementValue = UiTheme.Label(string.Empty, 11);
    private readonly System.Windows.Forms.Timer factTimer = new() { Interval = 10_000 };
    private int factIndex;
    private static readonly string[] SleepFacts =
    [
        "A consistent wake-up time can help your body clock stay balanced.",
        "A cool, dark, and quiet room can make it easier to sleep well.",
        "Morning sunlight helps your body know when it is time to feel awake.",
        "Keeping a relaxing bedtime routine helps signal that it is time to rest.",
        "Avoiding caffeine later in the day may help improve your sleep quality.",
        "Good sleep supports memory, mood, focus, and your body's recovery."
    ];

    public DashboardForm(string profileName)
    {
        this.profileName = profileName;
        Text = "Sleep Tracker - Dashboard";
        ClientSize = new Size(1080, 730);
        MinimumSize = new Size(900, 650);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = UiTheme.Background;
        Icon = AppBrand.Icon;

        var logo = new PictureBox { Image = AppBrand.Logo, SizeMode = PictureBoxSizeMode.Zoom, Bounds = new Rectangle(32, 24, 58, 58), BackColor = Color.Transparent };
        Controls.Add(logo);
        welcomeLabel.Location = new Point(105, 24);
        Controls.Add(welcomeLabel);
        var subtitle = UiTheme.Label("A calmer way to understand your rest", 10);
        subtitle.Location = new Point(108, 72);
        subtitle.ForeColor = UiTheme.Muted;
        Controls.Add(subtitle);

        AddMetricCard("TODAY'S SLEEP", todayValue, todayDetail, 32, 120, 235);
        AddMetricCard("AVERAGE SLEEP", averageValue, null, 285, 120, 235);
        AddMetricCard("THIS WEEK", weekValue, null, 538, 120, 235);
        AddMetricCard("SLEEP GOAL", goalValue, goalDetail, 791, 120, 250);

        var latestCard = UiTheme.CardPanel(32, 275, 490, 170);
        AddCardTitle(latestCard, "LATEST RECORD");
        latestValue.Location = new Point(18, 55);
        latestValue.MaximumSize = new Size(450, 90);
        latestCard.Controls.Add(latestValue);
        Controls.Add(latestCard);

        var achievementCard = UiTheme.CardPanel(545, 275, 496, 170);
        AddCardTitle(achievementCard, "SLEEP FACT");
        achievementValue.Location = new Point(18, 55);
        achievementValue.MaximumSize = new Size(450, 90);
        achievementCard.Controls.Add(achievementValue);
        Controls.Add(achievementCard);

        var nav = new FlowLayoutPanel { Location = new Point(32, 490), Size = new Size(1000, 110), WrapContents = true, AutoScroll = false, BackColor = Color.Transparent };
        AddNavButton(nav, "＋  Add Sleep Record", () => OpenForm<AddSleepRecordForm>());
        AddNavButton(nav, "▣  History", () => OpenForm<HistoryForm>());
        AddNavButton(nav, "◒  Statistics", () => OpenForm<StatisticsForm>());
        AddNavButton(nav, "⚙  Settings", () => OpenForm<SettingsForm>());
        Controls.Add(nav);
        Activated += (_, _) => RefreshDashboard();
        factTimer.Tick += (_, _) => ShowNextSleepFact();
        factTimer.Start();
        FormClosed += (_, _) => factTimer.Dispose();
        RefreshDashboard();
    }

    private void AddMetricCard(string title, Label value, Label? detail, int x, int y, int width)
    {
        var card = UiTheme.CardPanel(x, y, width, 125);
        AddCardTitle(card, title);
        value.Location = new Point(18, 47);
        card.Controls.Add(value);
        if (detail is not null)
        {
            detail.Location = new Point(18, 84);
            detail.ForeColor = UiTheme.Muted;
            card.Controls.Add(detail);
        }
        Controls.Add(card);
    }

    private static void AddCardTitle(Control card, string text)
    {
        var title = UiTheme.Label(text, 8, FontStyle.Bold);
        title.ForeColor = UiTheme.Muted;
        title.Location = new Point(18, 16);
        card.Controls.Add(title);
    }

    private static void AddNavButton(Control parent, string text, Action action)
    {
        var button = UiTheme.Button(text, 220);
        button.Margin = new Padding(0, 0, 14, 14);
        button.Click += (_, _) => action();
        parent.Controls.Add(button);
    }

    private void OpenForm<T>() where T : Form, new()
    {
        using var form = new T();
        form.ShowDialog(this);
        RefreshDashboard();
    }

    private void RefreshDashboard()
    {
        var name = DataStore.GetProfileName() ?? profileName;
        welcomeLabel.Text = $"Good evening, {name}.";
        var records = DataStore.GetRecords();
        var today = records.FirstOrDefault(r => r.SleepDate.Date == DateTime.Today);
        var goal = DataStore.GetSleepGoalMinutes();
        todayValue.Text = today is null ? "No record" : today.DurationText;
        todayDetail.Text = today is null ? "Add tonight's rest when you wake" : $"{today.Quality} quality · {today.SleepDate:MMM d}";
        averageValue.Text = records.Count == 0 ? "No data" : FormatMinutes((int)records.Average(r => r.DurationMinutes));
        var weekStart = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + 1);
        var weekRecords = records.Where(r => r.SleepDate.Date >= weekStart.Date).ToList();
        weekValue.Text = $"{weekRecords.Count} record{(weekRecords.Count == 1 ? "" : "s")}";
        goalValue.Text = today is null ? $"{FormatMinutes(goal)} goal" : $"{today.DurationText} / {FormatMinutes(goal)}";
        goalDetail.Text = today is null ? "Your target for a restful night" : today.DurationMinutes >= goal ? "Goal achieved · well done!" : "Keep going · every hour helps";
        latestValue.Text = records.Count == 0 ? "No sleep records yet. Add your first record to see your trends." : $"{records[0].SleepDate:dddd, MMMM d}\n{records[0].DurationText} · {records[0].Quality}\n{records[0].BedTime.ToString(@"hh\:mm")} – {records[0].WakeTime.ToString(@"hh\:mm")}";
        achievementValue.Text = SleepFacts[factIndex];
    }

    private void ShowNextSleepFact()
    {
        factIndex = (factIndex + 1) % SleepFacts.Length;
        achievementValue.Text = SleepFacts[factIndex];
    }

    internal static string FormatMinutes(int minutes) => UiTheme.FormatMinutes(minutes);
}