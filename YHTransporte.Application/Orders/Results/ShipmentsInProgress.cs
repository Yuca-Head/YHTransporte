namespace YHTransporte.Application.Orders.Results;

public readonly record struct ShipmentsUncompleted(params IEnumerable<string> ShipmentsCode);