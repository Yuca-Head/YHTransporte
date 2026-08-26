namespace YHTransporte.Core.Exceptions;

public class OrderException : DomainException
{

    public OrderException() { }

    public OrderException(string message) : base(message) { }

    public OrderException(string message, string fieldName) : base(message, fieldName) { }
}