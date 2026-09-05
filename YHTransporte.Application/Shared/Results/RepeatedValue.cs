namespace YHTransporte.Application.Shared.Results;

public readonly record struct RepeatedValue<T>(T Argument)
{
    public readonly record struct RepeatedKeyInformation(T Value, int Times);
}