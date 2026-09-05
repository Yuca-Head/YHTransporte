namespace YHTransporte.Application.Shared.Results;

public record AlreadyExists(object? Argument);
public record AlreadyExists<T>(T Argument);