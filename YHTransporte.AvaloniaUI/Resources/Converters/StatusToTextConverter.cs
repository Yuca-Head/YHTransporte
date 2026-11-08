using System;
using System.Globalization;
using Avalonia.Data.Converters;
using YHTransporte.Core.Enums;

namespace YHTransporte.AvaloniaUI.Resources.Converters;

/// <summary>
/// Texto en español de los estados de órdenes y envíos.
/// También sirve para el "En uso" (bool) de vehículos y conductores.
/// </summary>
public class StatusToTextConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    => value switch
    {
        OrderStatuses.Pending => "En espera",
        OrderStatuses.InProgress => "En tránsito",
        OrderStatuses.Completed => "Terminada",
        OrderStatuses.Canceled => "Cancelada",

        ShipmentStatuses.Pending => "Pendiente",
        ShipmentStatuses.OnWay => "En camino",
        ShipmentStatuses.Delivered => "Entregado",
        ShipmentStatuses.Canceled => "Cancelado",

        true => "En uso",
        false => "Disponible",

        _ => ""
    };

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    => throw new NotSupportedException();
}
