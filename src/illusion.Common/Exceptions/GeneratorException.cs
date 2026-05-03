namespace illusion.Common.Exceptions;

/// <summary>
/// Generator Exceptions
/// template render failure, content generate failure, mapping failure
/// </summary>
public class GeneratorException : IAException
{
    public const string DefaultErrorCode = "GENERATOR_ERROR";
    public string? TemplateName { get; }
    public GeneratorException(string message, string? templateName = null)
        : base(message, DefaultErrorCode)
    {
        TemplateName = templateName;
    }
    public GeneratorException(string message, Exception innerException, string? templateName = null)
        : base(message, innerException, DefaultErrorCode)
    {
        TemplateName = templateName;
    }
}