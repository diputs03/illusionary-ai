using System.Text.RegularExpressions;

namespace illusion.CoreLogic.Kernel;

/// <summary>
/// Deterministic managed kernel fallback for statement-level operations. Native
/// AST methods intentionally throw because this adapter has no native backend.
/// </summary>
public sealed class ManagedKernelAdapter : IKernelAdapter
{
    private bool _initialized;

    public bool IsInitialized() => _initialized;
    public bool Initialize() => _initialized = true;
    public void Shutdown() => _initialized = false;
    public void Dispose() => Shutdown();

    public Statement CreateStatement(string canonicalText) => new(Normalize(canonicalText), destroy: DestroyStatement);

    public void DestroyStatement(Statement statement)
    {
        ArgumentNullException.ThrowIfNull(statement);
        statement.NativeHandle = 0;
    }

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

    public IntPtr CreateSymbolAST(string name) => throw new NotSupportedException("ManagedKernelAdapter does not expose native AST handles.");
    public IntPtr CreateListAST(IntPtr[] elements) => throw new NotSupportedException("ManagedKernelAdapter does not expose native AST handles.");
    public IntPtr CloneAST(IntPtr ast) => throw new NotSupportedException("ManagedKernelAdapter does not expose native AST handles.");
    public IntPtr ParseStatement(string statement) => throw new NotSupportedException("ManagedKernelAdapter does not expose native AST handles.");
    public bool EqualAST(IntPtr a, IntPtr b) => throw new NotSupportedException("ManagedKernelAdapter does not expose native AST handles.");
    public void PrintAST(IntPtr ast) => throw new NotSupportedException("ManagedKernelAdapter does not expose native AST handles.");
    public void FreeAST(IntPtr ast) => throw new NotSupportedException("ManagedKernelAdapter does not expose native AST handles.");

    private static string Normalize(string text) => string.Join(' ', text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
}
