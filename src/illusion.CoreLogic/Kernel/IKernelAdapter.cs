namespace illusion.CoreLogic.Kernel;

/// <summary>
/// Unified kernel boundary for native AST handles and managed statement handles.
/// The statement operations are the preferred high-level API; AST operations are
/// retained only for native IPK interoperability and tests.
/// </summary>
public interface IKernelAdapter : IDisposable
{
    bool IsInitialized();
    bool Initialize();
    void Shutdown();

    Statement CreateStatement(string canonicalText);
    void DestroyStatement(Statement statement);
    Statement ParseRegularExpression(string expression);
    string ToCanonicalString(Statement statement);

    IntPtr CreateSymbolAST(string name);
    IntPtr CreateListAST(IntPtr[] elements);
    IntPtr CloneAST(IntPtr ast);
    IntPtr ParseStatement(string statement);
    bool EqualAST(IntPtr a, IntPtr b);
    void PrintAST(IntPtr ast);
    void FreeAST(IntPtr ast);
}
