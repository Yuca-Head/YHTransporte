using OneOf;
using OneOf.Types;
using YHTransporte.Application.Shared.Results;

namespace YHTransporte.Application.Shared;


/// <summary>
/// General Validator for some things.
/// </summary>
public static class MinimalValidator
{
    /// <summary>
    /// Validates keys looking for not repeated values.
    /// </summary>
    /// <returns>
    /// if any key exists returns the repeated values and how many times it appears
    /// else success.
    /// </returns>
    public static OneOf
    <Success, RepeatedValue<IEnumerable<RepeatedValue<T>.RepeatedKeyInformation>>> 
    ValidateForRepeatedKeys<T>(IEnumerable<T> values)
    {
        var repeatedValues = values.GroupBy(x => x)
        .Select(g => new RepeatedValue<T>.RepeatedKeyInformation(g.Key, g.Count()))
        .Where(x => x.Times > 1).ToArray();


        return repeatedValues.Length > 0 ?
        new RepeatedValue<IEnumerable<RepeatedValue<T>.RepeatedKeyInformation>>(repeatedValues) :
        new Success();
    }   
}