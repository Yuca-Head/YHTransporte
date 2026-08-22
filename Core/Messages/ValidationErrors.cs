namespace Core.Messages;

public static class ValidationErrors
{
    public static string ImplementRequiredField (string className, string fieldName)
    => $"Para crear {className} debe ingresar {fieldName}";
}