using Core.Exceptions;
namespace Core.Exceptions;

public class ShipmentException : DomainException
{

    public ShipmentException() { }

    public ShipmentException(string message) : base(message) { }

    public ShipmentException(string message, string fieldName) : base(message, fieldName) { }
}