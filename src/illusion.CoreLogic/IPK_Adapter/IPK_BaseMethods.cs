using illusion.Common.Types;
using System.Runtime.InteropServices;
using System.Security;

// TODO: use Unreal Header Tool for better and more automated reflection
namespace illusion.CoreLogic.IPK_Adapter;

using CPP_String = nint;
using AST_Handle = nint;
using Context_Handle = nint;

[SuppressUnmanagedCodeSecurity]
internal static class IPK_BaseMethods
{
    private const string DllName = "illusion.CoreLogic.Native";

    #region IPK_Common
    [DllImport(DllName, EntryPoint = "IPK_GetErrorMessage", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    private static extern string IPK_GetErrorMessage(IPK_RESULT errorCode);

    [DllImport(DllName, EntryPoint = "IPK_MakeString", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    internal static extern IPK_RESULT IPK_MakeString(string i, out CPP_String o);

    [DllImport(DllName, EntryPoint = "IPK_GetString", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    internal static extern IPK_RESULT IPK_GetString(CPP_String i, out string o);

    [DllImport(DllName, EntryPoint = "IPK_FreeString", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void IPK_FreeString(CPP_String str);

    internal static string GetErrorMessage(IPK_RESULT errorCode)
    {
        return IPK_GetErrorMessage(errorCode);
    }

    internal static void CheckResult(IPK_RESULT result)
    {
        if (result != IPK_RESULT.IPK_SUCCESS)
        {
            throw new IAException<IPK_RESULT>(GetErrorMessage(result));
        }
    }
    #endregion



    #region IPK_AST
    [DllImport(DllName, EntryPoint = "IPK_AST_CreateSymbol", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    internal static extern IPK_RESULT IPK_AST_CreateSymbol(CPP_String name, out CPP_String out_ast);

    [DllImport(DllName, EntryPoint = "IPK_AST_CreateList", CallingConvention = CallingConvention.Cdecl)]
    internal static extern IPK_RESULT IPK_AST_CreateList(Size_t length, nint[] elements, out AST_Handle out_ast);

    [DllImport(DllName, EntryPoint = "IPK_AST_GetType", CallingConvention = CallingConvention.Cdecl)]
    internal static extern IPK_RESULT IPK_AST_GetType(AST_Handle ast, out AST.NodeType out_type);

    [DllImport(DllName, EntryPoint = "IPK_AST_GetSymbol", CallingConvention = CallingConvention.Cdecl)]
    internal static extern IPK_RESULT IPK_AST_GetSymbol(AST_Handle ast, out CPP_String out_name);

    [DllImport(DllName, EntryPoint = "IPK_AST_GetList", CallingConvention = CallingConvention.Cdecl)]
    internal static extern IPK_RESULT IPK_AST_GetList(AST_Handle ast, Size_t index, out nint out_elements);

    [DllImport(DllName, EntryPoint = "IPK_AST_Clone", CallingConvention = CallingConvention.Cdecl)]
    internal static extern IPK_RESULT IPK_AST_Clone(AST_Handle ast, out AST_Handle out_ast);

    [DllImport(DllName, EntryPoint = "IPK_AST_Equal", CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool IPK_AST_Equal(AST_Handle a, AST_Handle b);

    [DllImport(DllName, EntryPoint = "IPK_AST_Destroy", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void IPK_AST_Destroy(AST_Handle ast);

    [DllImport(DllName, EntryPoint = "IPK_AST_ToString", CallingConvention = CallingConvention.Cdecl)]
    internal static extern IPK_RESULT IPK_AST_ToString(AST_Handle ast, out CPP_String str);
    #endregion



    #region IPK_Parser
    [DllImport(DllName, EntryPoint = "IPK_ParseStatement", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    internal static extern IPK_RESULT IPK_ParseStatement(string line, out AST_Handle out_ast);
    #endregion

    #region IPK_Context
    [DllImport(DllName, EntryPoint = "IPK_Context_Create", CallingConvention = CallingConvention.Cdecl)]
    internal static extern IPK_RESULT IPK_CreateContext(out Context_Handle out_context);

    [DllImport(DllName, EntryPoint = "IPK_Context_Destroy", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void IPK_DestroyContext(Context_Handle context);
    #endregion

    #region IPK_Substitution
    [DllImport(DllName, EntryPoint = "IPK_Sub_Create", CallingConvention = CallingConvention.Cdecl)]
    internal static extern IPK_RESULT IPK_Sub_Create(out nint subst);

    internal static IPK_RESULT IPK_Sub_Add(nint nativeHandle1, nint v, nint nativeHandle2)
    {
        throw new NotImplementedException();
    }

    internal static IPK_RESULT IPK_Sub_Lookup(nint nativeHandle, nint v, out nint result)
    {
        throw new NotImplementedException();
    }
    #endregion
}