namespace illusion.Common.Exceptions;

/// <summary>
/// natural language resolve error
/// grammatic error, ambiguity unable to resolve, definition conflict 
/// </summary>
public class ParserException : IAException
{
    public const string DefaultErrorCode = "PARSER_ERROR";
    public int? LineNumber { get; }
    public int? ColumnNumber { get; }
    public ParserException(string message, int? lineNumber = null, int? columnNumber = null)
        : base(message, DefaultErrorCode)
    {
        LineNumber = lineNumber;
        ColumnNumber = columnNumber;
    }
    public ParserException(string message, Exception innerException, int? lineNumber = null, int? columnNumber = null)
        : base(message, innerException, DefaultErrorCode)
    {
        LineNumber = lineNumber;
        ColumnNumber = columnNumber;
    }
}