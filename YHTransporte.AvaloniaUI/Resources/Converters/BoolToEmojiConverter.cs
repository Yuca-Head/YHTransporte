using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace YHTransporte.AvaloniaUI.Resources.Converters;
public class BoolToEmojiConverter : IValueConverter
{
    public object Convert(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture)
    {
        return value is true ? "✔️" : "✖️";
    }


    public object ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture)
    {
        throw new NotSupportedException();
    }

}