using System.Globalization;

namespace server.Utils;

public class DateTimeUtils
{
    public static long ToUnixTimestamp(string datetime)
    {
        if (string.IsNullOrEmpty(datetime))
            return 0;

        if (long.TryParse(datetime, out long unixTimestamp))
            return unixTimestamp;


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
        return GetMonthDateRange(randomMonth);
    }

    public static (string StartDate, string EndDate) GetConsistentRandomMonth(string symbol)
    {
        // Generate a "random" month that's consistent for the same symbol
        // This ensures all users requesting the same symbol get the same month
        var start = new DateTime(2020, 2, 11, 0, 0, 0, DateTimeKind.Utc);
        var now = DateTime.UtcNow;
        var endMonthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var totalMonths = (endMonthStart.Year - start.Year) * 12 + (endMonthStart.Month - start.Month);

        // Use hash of symbol to get consistent "random" offset
        var hashCode = symbol.GetHashCode();
        var offset = Math.Abs(hashCode) % (totalMonths + 1);

        var randomMonth = start.AddMonths(offset);
        return GetMonthDateRange(randomMonth);
    }

    public static (string StartDate, string EndDate) GetDefaultMonth()
    {
        // Return a consistent default month for shared cache
        // Using January 2024 as a stable default that all users will hit
        var defaultMonth = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        return GetMonthDateRange(defaultMonth);
    }

    public static (string StartDate, string EndDate) GetMonthDateRange(DateTime month)
    {
        var monthStart = new DateTime(month.Year, month.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var monthEnd = monthStart.AddMonths(1).AddDays(-1);

        return (
            monthStart.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            monthEnd.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
        );
    }
}