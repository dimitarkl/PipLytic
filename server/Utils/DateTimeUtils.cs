using System.Globalization;

namespace server.Utils;

public class DateTimeUtils
{
    public static long ToUnixTimestamp(string datetime)
    {
        if (string.IsNullOrEmpty(datetime))
            return 0;

        var dt = System.DateTime.ParseExact(
            datetime,
            "yyyy-MM-dd HH:mm:ss",
            CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal
        );

        return new DateTimeOffset(dt).ToUnixTimeSeconds();
    }
    public static (string StartDate, string EndDate) GenerateRandomMonth()
    {
        var start = new DateTime(2020, 2, 11, 0, 0, 0, DateTimeKind.Utc);
        var now = DateTime.UtcNow;
        var endMonthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var totalMonths = (endMonthStart.Year - start.Year) * 12 + (endMonthStart.Month - start.Month);

        var offset = Random.Shared.Next(0, totalMonths + 1);

        var randomMonth = start.AddMonths(offset);

        var monthStart = new DateTime(randomMonth.Year, randomMonth.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var monthEnd = monthStart.AddMonths(1).AddDays(-1);

        return (
            monthStart.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            monthEnd.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
        );
    }
}