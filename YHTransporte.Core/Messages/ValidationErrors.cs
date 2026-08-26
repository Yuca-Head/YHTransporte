

namespace YHTransporte.Core.Messages;

public static class ValidationErrors
{
    public static string ImplementRequiredField (string className, string fieldName)
    => $"Para crear {className} debe ingresar {fieldName}.";

    public static string ChangeRequiredField (string className, string fieldName)
    => $"Para modificar {className} debe ingresar {fieldName}.";

    public static string LowerDateThanAllowed(InvalidDateInfo info)
    => $"No se permite una fecha inferior a {info.ExpectedLimit:dd/mm/yyyy} para {info.FieldName} en {info.ClassName} "+
    $"(fecha ingresada: {info.InvalidDate:dd/mm/yyyy})";    

    public static string DateIsAlreadyCreated(InvalidDateInfo info)
    => $"{info.ClassName} ya tiene una fecha para {info.FieldName} ({info.ExpectedLimit:dd/mm/yyyy})";

    /// <summary>
    /// Contains Info about an invalid date.
    /// </summary>
    /// <param name="ClassName">Class where error happens.</param>
    /// <param name="FieldName">Field affected</param>
    /// <param name="InvalidDate">RecievedDate</param>
    /// <param name="ExpectedLimit"Date suppoused to be more/less than the Invalid One</param>
    public record InvalidDateInfo(string? ClassName, string? FieldName, DateTimeOffset InvalidDate, DateTimeOffset ExpectedLimit);
}