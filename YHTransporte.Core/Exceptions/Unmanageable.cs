namespace YHTransporte.Core.Exceptions;

[System.Serializable]
public class UnmanageableException : System.Exception
{
    public UnmanageableException() { }
    public UnmanageableException(string message) : base(message) { }
    public UnmanageableException(string message, System.Exception inner) : base(message, inner) { }
}