namespace YHTransporte.Application.Shared.Results;

public record  NotFound(object? Argument = null);
public record NotFound<T>(T Argument);