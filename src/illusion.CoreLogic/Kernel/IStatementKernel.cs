namespace illusion.CoreLogic.Kernel;

/// <summary>
/// Kernel interface for statement lifecycle and string conversion.
/// Implementations may be native-backed or fully managed, but must expose a
/// stable statement class and total create/destroy/parse/to-string operations.
/// </summary>
public interface IStatementKernel : IDisposable
{
    Statement CreateStatement(string canonicalText);
    void DestroyStatement(Statement statement);
    Statement ParseRegularExpression(string expression);
    string ToCanonicalString(Statement statement);
}
