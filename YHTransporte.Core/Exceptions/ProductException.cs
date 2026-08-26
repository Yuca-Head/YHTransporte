namespace YHTransporte.Core.Exceptions;

public class ProductException : DomainException
{

    public ProductException() { }

    public ProductException(string message) : base(message) { }

    public ProductException(string message, string fieldName) : base(message, fieldName) { }
}