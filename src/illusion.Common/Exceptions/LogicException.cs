namespace illusion.Common.Exceptions;

/// <summary>
/// Logic Exceptions
/// contradictions, failure in deduction, axiom conflics
/// </summary>
public class LogicException : IAException
{
    public const string DefaultErrorCode = "LOGIC_ERROR";
    public LogicException(string message)
        : base(message, DefaultErrorCode)
    {
    }
    public LogicException(string message, Exception innerException)
        : base(message, innerException, DefaultErrorCode)
    {
    }
}