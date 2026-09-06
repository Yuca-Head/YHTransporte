namespace YHTransporte.Application.Shared.Results;

public record ValidationError(string Field, params IEnumerable<string> Errors);
public record ValidationError<T>(T Field, params IEnumerable<string> Errors);