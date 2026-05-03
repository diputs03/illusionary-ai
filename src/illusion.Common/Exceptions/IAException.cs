namespace illusion.Common.Exceptions;

/// <summary>
/// base exceptions class
/// </summary>
/// 
public class IAException : Exception
{
    public string ErrorCode { get; }
    public DateTime OccuredAt { get; }
    public IAException(string message, string errorCode = "UNKNOW_ERROR")
        : base(message)
    {
        ErrorCode = errorCode;
        OccuredAt = DateTime.UtcNow;
    }
    public IAException(string message, Exception innerException, string errorCode = "UNKNOW_ERROR")
        : base(message, innerException)
    {
        ErrorCode = errorCode;
        OccuredAt = DateTime.UtcNow;
    }
}