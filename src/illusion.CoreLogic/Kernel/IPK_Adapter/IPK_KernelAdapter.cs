using System.Runtime.InteropServices;
using System.Diagnostics;
using illusion.Common.Types;
using illusion.Common.Utils;
using illusion.CoreLogic.Prover;
using System.Text.Json;
using illusion.CoreLogic.Kernel;

namespace illusion.Kernel.IPK_Adapter;

/// <summary>
/// LEAN kernel adapter
/// </summary>
public class IPK_KernelAdapter : IKernelAdapter
{
    private IntPtr _context;

    public bool Initialize()
    {
        _context = IntPtr.Zero;
        IPK_BaseMethods.CheckResult(IPK_BaseMethods.IPK_CreateContext(out _context));
        return true;
    }
    public bool IsInitialized() => _context != IntPtr.Zero;
    public void Shutdown()
    {
        Dispose();
    }
    public void Dispose()
    {
        if (_context != IntPtr.Zero)
        {
            IPK_BaseMethods.IPK_DestroyContext(_context);
            _context = IntPtr.Zero;
        }
        return;
    }
    public IntPtr CreateSymbolAST(string name)
    {
        IntPtr ast;
        IPK_BaseMethods.CheckResult(IPK_BaseMethods.IPK_CreateSymbolAST(name, out ast));
        return ast;
    }
    public IntPtr CreateListAST(IntPtr[] elements)
    {
        IntPtr ast;
        IPK_BaseMethods.CheckResult(IPK_BaseMethods.IPK_CreateListAST((Size_t)elements.Length, elements, out ast));
        return ast;
    }
    public IntPtr CloneAST(IntPtr ast)
    {
        IntPtr clonedAst;
        IPK_BaseMethods.CheckResult(IPK_BaseMethods.IPK_CloneAST(ast, out clonedAst));
        return clonedAst;
    }
    public IntPtr ParseStatement(string statement)
    {
        IntPtr ast;
        IPK_BaseMethods.CheckResult(IPK_BaseMethods.IPK_ParseStatement(statement, out ast));
        return ast;
    }
    public bool EqualAST(IntPtr a, IntPtr b)
    {
        return IPK_BaseMethods.IPK_EqualAST(a, b);
    }
    public void PrintAST(IntPtr ast)
    {
        IntPtr str;
        IPK_BaseMethods.CheckResult(IPK_BaseMethods.IPK_ToStringAST(ast, out str));
        try
        {
            Console.WriteLine(System.Runtime.InteropServices.Marshal.PtrToStringAnsi(str) ?? string.Empty);
        }
        finally
        {
            IPK_BaseMethods.IPK_FreeString(str);
        }
    }
    public void FreeAST(IntPtr ast)
    {
        IPK_BaseMethods.IPK_FreeAST(ast);
    }
}