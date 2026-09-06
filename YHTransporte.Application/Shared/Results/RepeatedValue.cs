namespace YHTransporte.Application.Shared.Results;

public readonly record struct RepeatedValue<T>(T Argument)
{
    /// <summary>
    /// Contains general information for repeated values in a collection of commands.
    /// </summary>
    /// <param name="Value">Repeated Value</param>
    /// <param name="Times">How many times is repeated</param>
    public readonly record struct RepeatedKeyInformation(T Value, int Times);
}