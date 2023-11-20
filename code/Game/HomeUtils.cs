using Sandbox;

namespace Home;

public static class HomeUtils
{
    public static string FormatMoney( long amount )
    {
        return $"${amount:N0}";
    }
}