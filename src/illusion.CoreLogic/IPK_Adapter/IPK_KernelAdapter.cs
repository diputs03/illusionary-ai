using System.Runtime.InteropServices;
using System.Diagnostics;
using illusion.Common.Types;
using illusion.Common.Utils;
using illusion.CoreLogic.Prover;
using System.Text.Json;
using illusion.CoreLogic.Kernel;
using System.Text.RegularExpressions;

namespace illusion.IPK_Adapter;

/// <summary>
/// LEAN kernel adapter
/// </summary>
public class IPK_KernelAdapter : IKernelAdapter
{
    private nint _context;

    public bool Initialize()
    {
        _context = nint.Zero;
        IPK_BaseMethods.CheckResult(IPK_BaseMethods.IPK_CreateContext(out _context));
        return true;
    }
    public bool IsInitialized() => _context != nint.Zero;
    public void Shutdown()
    {
        Dispose();
    }
    public void Dispose()
    {
        if (_context != nint.Zero)
        {
            IPK_BaseMethods.IPK_DestroyContext(_context);
            _context = nint.Zero;
        }
        return;
    }
    public nint CreateSymbolAST(string name)
    {
        nint ast;
        IPK_BaseMethods.CheckResult(IPK_BaseMethods.IPK_CreateSymbolAST(name, out ast));
        return ast;
    }
    public nint CreateListAST(nint[] elements)
    {
        nint ast;
        IPK_BaseMethods.CheckResult(IPK_BaseMethods.IPK_CreateListAST((Size_t)elements.Length, elements, out ast));
        return ast;
    }
    public nint CloneAST(nint ast)
    {
        nint clonedAst;
        IPK_BaseMethods.CheckResult(IPK_BaseMethods.IPK_CloneAST(ast, out clonedAst));
        return clonedAst;
    }
    public nint ParseStatement(string statement)
    {
        nint ast;
        IPK_BaseMethods.CheckResult(IPK_BaseMethods.IPK_ParseStatement(statement, out ast));
        return ast;
    }
    public Statement CreateStatement(string canonicalText)
    {
        var ast = ParseStatement(canonicalText);
        return new Statement(canonicalText, ast, DestroyStatement);
    }
    public void DestroyStatement(Statement statement)
    {
        ArgumentNullException.ThrowIfNull(statement);
        if (statement.NativeHandle != nint.Zero)
            FreeAST(statement.NativeHandle);
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
        return statement.ToString();
    }
    public bool EqualAST(nint a, nint b)
    {
        return IPK_BaseMethods.IPK_EqualAST(a, b);
    }
    public void PrintAST(nint ast)
    {
        nint str;
        IPK_BaseMethods.CheckResult(IPK_BaseMethods.IPK_ToStringAST(ast, out str));
        try
        {
            Console.WriteLine(Marshal.PtrToStringAnsi(str) ?? string.Empty);
        }
        finally
        {
            IPK_BaseMethods.IPK_FreeString(str);
        }
    }
    public void FreeAST(nint ast)
    {
        IPK_BaseMethods.IPK_FreeAST(ast);
    }
}
