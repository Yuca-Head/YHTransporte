namespace YHTransporte.Core.Enums
{
    // Los números de los enums son IGUALES a los Id de las tablas OrderStatuses / ShipmentStatuses
    // de SQL Server (empiezan en 1). Así, el repositorio puede hacer (OrderStatuses)idStatus sin tabla de conversión.

    public enum OrderStatuses
    {
        Pending = 1,
        InProgress = 2,
        Completed = 3,
        Canceled = 4
    }

    public enum ShipmentStatuses
    {
        Pending = 1,
        OnWay = 2,
        Delivered = 3,
        Canceled = 4
    }
}
