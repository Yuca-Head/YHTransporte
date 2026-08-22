namespace Core.Exceptions;

public class DomainException : Exception
{

    public DomainException() : base() { }

    public DomainException(string message) : base(message) { }

    public DomainException(string message, System.Exception inner) : base(message, inner) { }

    public DomainException(string message, string fieldName) : this(message) => FieldName = fieldName;

    public string? FieldName {get; init;}

}