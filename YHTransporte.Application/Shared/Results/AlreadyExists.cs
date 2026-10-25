namespace YHTransporte.Application.Shared.Results;

public abstract record AlreadyExists(object? Argument);
public record AlreadyExists<T>(T Argument);