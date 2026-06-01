using System.Runtime.InteropServices;
using System.Diagnostics;
using illusion.Common.Types;
using illusion.Common.Utils;
using illusion.CoreLogic.Prover;
using System.Text.Json;
using illusion.Types;

namespace illusion.CoreLogic.Kernel;

/// <summary>
/// LEAN kernel adapter
/// </summary>
public class IPK_KernelAdapter : IKernelAdapter
{
    private IntPtr _context;

    public bool Initialize()
    {
        var result = IPK_NativeMethods.IPK_CreateContext(out _context);
        if (result != IPK_RESULT.IPK_SUCCESS)
        {
            var errorMessage = IPK_NativeMethods.IPK_GetErrorMessage(result);
            throw new IAException<IPK_RESULT>(errorMessage);
        }
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
            IPK_NativeMethods.IPK_DestroyContext(_context);
            _context = IntPtr.Zero;
        }
        return;
    }
    public IntPtr CreateSymbolAST(string name)
    {
        IntPtr ast = IntPtr.Zero;
        var result = IPK_NativeMethods.IPK_CreateSymbolAST(name, out ast);
        if (result != IPK_RESULT.IPK_SUCCESS)
        {
            var errorMessage = IPK_NativeMethods.IPK_GetErrorMessage(result);
            throw new IAException<IPK_RESULT>(errorMessage);
        }
        return ast;
    }
    public IntPtr CreateListAST(IntPtr[] elements)
    {
        IntPtr ast = IntPtr.Zero;
        var result = IPK_NativeMethods.IPK_CreateListAST((ulong)elements.Length, elements, out ast);
        if (result != IPK_RESULT.IPK_SUCCESS)
        {
            var errorMessage = IPK_NativeMethods.IPK_GetErrorMessage(result);
            throw new IAException<IPK_RESULT>(errorMessage);
        }
        return ast;
    }
    public IntPtr CloneAST(IntPtr ast)
    {
        IntPtr clonedAst = IntPtr.Zero;
        var result = IPK_NativeMethods.IPK_CloneAST(ast, out clonedAst);
        if (result != IPK_RESULT.IPK_SUCCESS)
        {
            var errorMessage = IPK_NativeMethods.IPK_GetErrorMessage(result);
            throw new IAException<IPK_RESULT>(errorMessage);
        }
        return clonedAst;
    }
    public bool EqualAST(IntPtr a, IntPtr b)
    {
        return IPK_NativeMethods.IPK_EqualAST(a, b);
    }
    public void PrintAST(IntPtr ast, Size_t indent = 0)
    {
        IPK_NativeMethods.IPK_PrintAST(ast, indent);
    }
    public void FreeAST(IntPtr ast)
    {
        IPK_NativeMethods.IPK_FreeAST(ast);
    }
}