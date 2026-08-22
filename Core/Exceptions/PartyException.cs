namespace Core.Exceptions;

public class PartyException : DomainException
{
    public PartyException() { }

    public PartyException(string message) : base(message) { } 

    public PartyException(string message, string fieldName) : base(message, fieldName) { }
}