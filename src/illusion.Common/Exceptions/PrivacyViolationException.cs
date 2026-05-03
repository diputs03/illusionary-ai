namespace illusion.Common.Exceptions;

/// <summary>
/// Privacy Exceptions
/// unauthorized access of private data
/// </summary>
public class PrivacyViolationException : IAException
{
    public const string DefaultErrorCode = "PRIVACY_VIOLATION";
    public PrivacyViolationException(string message)
        : base(message, DefaultErrorCode)
    {
    }
    public PrivacyViolationException(string message, Exception innerException)
        : base(message, innerException, DefaultErrorCode)
    {
    }
}