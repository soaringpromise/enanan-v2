using System.Globalization;

namespace EnananV2.Tools;

public static class IsvTools
{
    private static int CalculateBoost(double lead, double team)
    {
        if (lead > 10) lead /= 100;
        if (team > 10) team /= 100;

        return (int)Math.Round((lead + (team - lead) / 5) * 100);
    }

    public static int CalculateBoost(string isv)
    {
        isv = StringTools.NormalizeWhitespace(isv);
        var parts = isv.Split('/');

        var lead = double.Parse(parts[0], NumberStyles.Number, CultureInfo.InvariantCulture);
        var team = double.Parse(parts[1],  NumberStyles.Number, CultureInfo.InvariantCulture);
        
        return CalculateBoost(lead, team);
    }
}