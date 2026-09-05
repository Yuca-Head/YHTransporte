namespace YHTransporte.Application.Shared.Results;

public record ValidationError(string Field, IEnumerable<string> Errors);
public record ValidationError<T>(T Field, IEnumerable<string> Errors);