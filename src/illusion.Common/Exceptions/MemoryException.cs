namespace illusion.Common.Exceptions;

/// <summary>
/// Memory Exceptions
/// nonexisting node, namespace violation, module loading error
/// </summary>
public class MemoryException : IAException
{
    public const string DefaultErrorCode = "MEMORY_ERROR";
    public MemoryException(string message)
        : base(message, DefaultErrorCode)
    {
    }
    public MemoryException(string message, Exception innerException)
        : base(message, innerException, DefaultErrorCode)
    {
    }
}