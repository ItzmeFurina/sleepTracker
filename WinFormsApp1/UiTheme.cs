namespace WinFormsApp1;

internal static class UiTheme
{
    public static readonly Color Background = Color.FromArgb(246, 248, 252);
    public static readonly Color Ink = Color.FromArgb(35, 48, 65);
    public static readonly Color Muted = Color.FromArgb(103, 116, 136);
    public static readonly Color Primary = Color.FromArgb(74, 92, 150);
    public static readonly Color Accent = Color.FromArgb(101, 154, 163);
    public static readonly Color Card = Color.White;
    public static readonly Color Goal = Color.FromArgb(224, 170, 86);

    public static Label Label(string text, float size = 10, FontStyle style = FontStyle.Regular)
    {
        return new Label { Text = text, AutoSize = true, Font = new Font("Segoe UI", size, style), ForeColor = Ink };
    }

    public static Panel CardPanel(int x, int y, int width, int height)
    {
        return new Panel { Bounds = new Rectangle(x, y, width, height), BackColor = Card, Padding = new Padding(18) };
    }

    public static Button Button(string text, int width = 150)
    {
        return new Button
        {
            Text = text,
            Width = width,
            Height = 38,
            BackColor = Primary,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            FlatAppearance = { BorderSize = 0 },
            Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
    }

    public static string FormatMinutes(int minutes) => $"{minutes / 60}h {minutes % 60:00}m";
}
