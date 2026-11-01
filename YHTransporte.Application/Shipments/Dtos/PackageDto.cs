namespace YHTransporte.Application.Shipments.Dtos;

public sealed record    PackageDto(int Key, ProductDto Product, decimal Weight, int ProductAmount, string ShipmentCode);