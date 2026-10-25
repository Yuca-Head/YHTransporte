namespace YHTransporte.Application.Shared.Results;

public abstract record NotFound(object? Argument = null);
public readonly record struct NotFound<T>(T Argument);