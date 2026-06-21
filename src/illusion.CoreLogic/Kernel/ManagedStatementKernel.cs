using System.Text.RegularExpressions;

namespace illusion.CoreLogic.Kernel;

/// <summary>
/// Deterministic managed kernel fallback. It validates that regular-expression
/// statements compile in .NET and canonicalizes whitespace, giving higher-level
/// graph/meta code a kernel even when native IPK binaries are unavailable.
/// </summary>
public sealed class ManagedStatementKernel : IStatementKernel
{
    public Statement CreateStatement(string canonicalText) => new(Normalize(canonicalText), destroy: DestroyStatement);

    public Statement ParseRegularExpression(string expression)
    {
        ArgumentNullException.ThrowIfNull(expression);
        _ = new Regex(expression, RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1));
        return CreateStatement(expression);
    }

    public string ToCanonicalString(Statement statement)
    {
        ArgumentNullException.ThrowIfNull(statement);
        if (statement.IsDestroyed)
            throw new ObjectDisposedException(nameof(Statement));
        return statement.Text;
    }

    public void DestroyStatement(Statement statement)
    {
        ArgumentNullException.ThrowIfNull(statement);
        statement.NativeHandle = 0;
    }

    public void Dispose() { }

    private static string Normalize(string text) => string.Join(' ', text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
}
