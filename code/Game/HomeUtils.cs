using Sandbox;

namespace Home;

public static class HomeUtils
{
    public static string FormatMoney( long amount )
    {
        return $"${amount:N0}";
    }

    public static DateTime UnixTimeStampToDateTime( double unixTimeStamp )
    {
        // Unix timestamp is seconds past epoch
        DateTime dateTime = new DateTime( 1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc );
        dateTime = dateTime.AddSeconds( unixTimeStamp ).ToLocalTime();
        return dateTime;
    }
}