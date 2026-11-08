

namespace YHTransporte.Core.Messages;

public static class ValidationErrors
{
    public static string ImplementRequiredField (string className, string fieldName)
    => $"Para crear {className} debe ingresar {fieldName}.";

    public static string ChangeRequiredField (string className, string fieldName)
    => $"Para modificar {className} debe ingresar {fieldName}.";

    public static string LowerDateThanAllowed(InvalidDateInfo info)
    => $"No se permite una fecha inferior a {info.ExpectedLimit:dd/MM/yyyy} para {info.FieldName} en {info.ClassName} "+
    $"(fecha ingresada: {info.InvalidDate:dd/MM/yyyy})";    

    public static string MustBePositive(string className, string fieldName)
    => $"{fieldName} de {className} debe ser mayor que cero.";

    public static string GreaterThanAllowed(string className, string fieldName, decimal limit)
    => $"{fieldName} de {className} no puede ser mayor que {limit:N2}.";

    public static string ExceedsMaxLength(string className, string fieldName, int maxLength)
    => $"{fieldName} de {className} no puede tener más de {maxLength} caracteres.";

    public static string DateIsAlreadyCreated(InvalidDateInfo info)
    => $"{info.ClassName} ya tiene una fecha para {info.FieldName} ({info.ExpectedLimit:dd/MM/yyyy})";

    /// <summary>
    /// Contains Info about an invalid date.
    /// </summary>
    /// <param name="ClassName">Class where error happens.</param>
    /// <param name="FieldName">Field affected</param>
    /// <param name="InvalidDate">RecievedDate</param>
    /// <param name="ExpectedLimit"Date suppoused to be more/less than the Invalid One</param>
    public record InvalidDateInfo(string? ClassName, string? FieldName, DateTimeOffset InvalidDate, DateTimeOffset ExpectedLimit);
}