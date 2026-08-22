namespace Core.Exceptions;

public class AddressException : DomainException
{

    public AddressException() { }

    public AddressException(string message) : base(message) { }

    public AddressException(string message, string fieldName) : base(message, fieldName) { }
}