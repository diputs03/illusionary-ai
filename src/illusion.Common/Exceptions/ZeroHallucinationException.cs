namespace illusion.Common.Exceptions;

/// <summary>
/// Zero Hallucination Exception 
/// when detecting groundless declorations, fictional content
/// </summary>
public class ZeroHallucinationException : IAException
{
    public const string DefaultErrorCode = "ZERO_HALLUCINATION_VIOLATION";
    public ZeroHallucinationException(string message)
        : base(message, DefaultErrorCode)
    {
    }
    public ZeroHallucinationException(string message, Exception innerException)
        : base(message, innerException, DefaultErrorCode)
    {
    }
}