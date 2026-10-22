namespace WinFormsApp1;

internal static class AppBrand
{
    private static readonly Bitmap? LogoBitmap = LoadLogo();

    public static Image? Logo => LogoBitmap;
    public static Icon? Icon { get; } = CreateIcon();

    private static Bitmap? LoadLogo()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Properties", "sleep.png");
        if (!File.Exists(path)) return null;
        using var source = Image.FromFile(path);
        return new Bitmap(source);
    }

    private static Icon? CreateIcon()
    {
        return LogoBitmap is null ? null : Icon.FromHandle(LogoBitmap.GetHicon());
    }
}
