namespace illusion.Common.Types;

/// <summary>
/// base exceptions class
/// </summary>
/// 
public class IAException<T> : Exception
{
    public string ErrorCode { get; }
    public DateTime OccuredAt { get; }
    public string? Description { get; }
    public IAException(string message, Exception? e = null, string? description = null)
        : base(message, e)
    {
        ErrorCode = nameof(T)+"Error";
        OccuredAt = DateTime.UtcNow;
        Description = description;
    }
}