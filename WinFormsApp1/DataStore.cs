using Microsoft.Data.Sqlite;

namespace WinFormsApp1;

internal static class DataStore
{
    private static readonly string FolderPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "WinFormsApp1");

    public static string DatabasePath => Path.Combine(FolderPath, "sleeptracker.db");
    private static string ConnectionString => $"Data Source={DatabasePath}";

    public static SqliteConnection OpenConnection()
    {
        Directory.CreateDirectory(FolderPath);
        var connection = new SqliteConnection(ConnectionString);
        connection.Open();
        return connection;
    }

    public static void Initialize()
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
            CREATE TABLE IF NOT EXISTS Profile (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                SleepGoalMinutes INTEGER NOT NULL DEFAULT 480
            );

            CREATE TABLE IF NOT EXISTS SleepRecords (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                SleepDate TEXT NOT NULL,
                BedTime TEXT NOT NULL,
                WakeTime TEXT NOT NULL,
                DurationMinutes INTEGER NOT NULL,
                Quality TEXT NOT NULL,
                Notes TEXT
            );

            CREATE TABLE IF NOT EXISTS GoalAchievements (
                RecordId INTEGER PRIMARY KEY,
                AchievedAt TEXT NOT NULL
            );
            """;
        command.ExecuteNonQuery();
    }

    public static string? GetProfileName()
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT Name FROM Profile ORDER BY Id LIMIT 1";
        return command.ExecuteScalar() as string;
    }

    public static void SaveProfile(string name)
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO Profile (Name, SleepGoalMinutes)
            SELECT $name, 480
            WHERE NOT EXISTS (SELECT 1 FROM Profile);
            """;
        command.Parameters.AddWithValue("$name", name);
        command.ExecuteNonQuery();
    }

    public static void UpdateProfile(string name, int goalMinutes)
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "UPDATE Profile SET Name = $name, SleepGoalMinutes = $goal";
        command.Parameters.AddWithValue("$name", name);
        command.Parameters.AddWithValue("$goal", goalMinutes);
        command.ExecuteNonQuery();
    }

    public static int GetSleepGoalMinutes()
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT SleepGoalMinutes FROM Profile ORDER BY Id LIMIT 1";
        return Convert.ToInt32(command.ExecuteScalar() ?? 480);
    }

    public static bool HasRecordOnDate(DateTime date, int? exceptId = null)
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT 1 FROM SleepRecords WHERE SleepDate = $date AND ($exceptId IS NULL OR Id <> $exceptId) LIMIT 1";
        command.Parameters.AddWithValue("$date", date.ToString("yyyy-MM-dd"));
        command.Parameters.AddWithValue("$exceptId", exceptId ?? (object)DBNull.Value);
        return command.ExecuteScalar() is not null;
    }

    public static SleepRecord AddSleepRecord(DateTime date, TimeSpan bedtime, TimeSpan wakeTime, string quality, string? notes)
    {
        var duration = (wakeTime - bedtime).TotalMinutes;
        if (duration <= 0)
        {
            duration += 24 * 60;
        }

        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO SleepRecords
                (SleepDate, BedTime, WakeTime, DurationMinutes, Quality, Notes)
            VALUES
                ($date, $bedtime, $waketime, $duration, $quality, $notes)
            """;
        command.Parameters.AddWithValue("$date", date.ToString("yyyy-MM-dd"));
        command.Parameters.AddWithValue("$bedtime", bedtime.ToString(@"hh\:mm"));
        command.Parameters.AddWithValue("$waketime", wakeTime.ToString(@"hh\:mm"));
        command.Parameters.AddWithValue("$duration", (int)Math.Round(duration));
        command.Parameters.AddWithValue("$quality", quality);
        command.Parameters.AddWithValue("$notes", (object?)notes ?? DBNull.Value);
        command.ExecuteNonQuery();
        command.CommandText = "SELECT last_insert_rowid()";
        var id = Convert.ToInt32(command.ExecuteScalar());
        return new SleepRecord(id, date, bedtime, wakeTime, (int)Math.Round(duration), quality, notes ?? string.Empty);
    }

    public static bool MarkGoalAchievement(int recordId)
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "INSERT OR IGNORE INTO GoalAchievements (RecordId, AchievedAt) VALUES ($id, $at)";
        command.Parameters.AddWithValue("$id", recordId);
        command.Parameters.AddWithValue("$at", DateTime.UtcNow.ToString("O"));
        return command.ExecuteNonQuery() == 1;
    }

    public static int GetGoalAchievementDays(int goalMinutes)
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(DISTINCT SleepDate) FROM SleepRecords WHERE DurationMinutes >= $goal";
        command.Parameters.AddWithValue("$goal", goalMinutes);
        return Convert.ToInt32(command.ExecuteScalar());
    }

    public static List<SleepRecord> GetRecords(DateTime? from = null, DateTime? to = null)
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT Id, SleepDate, BedTime, WakeTime, DurationMinutes, Quality, Notes
            FROM SleepRecords
            WHERE ($from IS NULL OR SleepDate >= $from)
              AND ($to IS NULL OR SleepDate <= $to)
            ORDER BY SleepDate DESC, Id DESC
            """;
        command.Parameters.AddWithValue("$from", from?.ToString("yyyy-MM-dd") ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("$to", to?.ToString("yyyy-MM-dd") ?? (object)DBNull.Value);

        using var reader = command.ExecuteReader();
        var records = new List<SleepRecord>();
        while (reader.Read())
        {
            records.Add(new SleepRecord(
                reader.GetInt32(0),
                DateTime.Parse(reader.GetString(1)),
                TimeSpan.Parse(reader.GetString(2)),
                TimeSpan.Parse(reader.GetString(3)),
                reader.GetInt32(4),
                reader.GetString(5),
                reader.IsDBNull(6) ? string.Empty : reader.GetString(6)));
        }
        return records;
    }

    public static void DeleteRecord(int id)
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM SleepRecords WHERE Id = $id";
        command.Parameters.AddWithValue("$id", id);
        command.ExecuteNonQuery();
    }

    public static bool UpdateSleepRecord(SleepRecord record, DateTime date, TimeSpan bedtime, TimeSpan wakeTime, string quality, string? notes)
    {
        var duration = (wakeTime - bedtime).TotalMinutes;
        if (duration <= 0) duration += 24 * 60;
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
            UPDATE SleepRecords
            SET SleepDate = $date, BedTime = $bedtime, WakeTime = $waketime,
                DurationMinutes = $duration, Quality = $quality, Notes = $notes
            WHERE Id = $id
            """;
        command.Parameters.AddWithValue("$date", date.ToString("yyyy-MM-dd"));
        command.Parameters.AddWithValue("$bedtime", bedtime.ToString(@"hh\:mm"));
        command.Parameters.AddWithValue("$waketime", wakeTime.ToString(@"hh\:mm"));
        command.Parameters.AddWithValue("$duration", (int)Math.Round(duration));
        command.Parameters.AddWithValue("$quality", quality);
        command.Parameters.AddWithValue("$notes", (object?)notes ?? DBNull.Value);
        command.Parameters.AddWithValue("$id", record.Id);
        return command.ExecuteNonQuery() == 1;
    }
}

internal sealed record SleepRecord(
    int Id,
    DateTime SleepDate,
    TimeSpan BedTime,
    TimeSpan WakeTime,
    int DurationMinutes,
    string Quality,
    string Notes)
{
    public string DurationText => $"{DurationMinutes / 60}h {DurationMinutes % 60}m";
}
