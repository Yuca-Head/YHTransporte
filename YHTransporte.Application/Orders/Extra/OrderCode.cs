using System.Globalization;

namespace YHTransporte.Application.Orders.Extra;

public readonly record struct OrderCode(string Code)
{
    public int Id => int.Parse(Code[..4]);
    public DateOnly Date => DateOnly.ParseExact(Code.Substring(5, 6), "ddMMyy", CultureInfo.InvariantCulture);

    public static string Generate(string? lastCode)
    {
        int serie = 0;
        if(lastCode is not null && int.TryParse(lastCode[..4], out var id))
            serie = id;
        serie++;
        var now = DateTimeOffset.UtcNow;
        return $"{serie:D4}-{now:ddMMyy}%";
    }
}