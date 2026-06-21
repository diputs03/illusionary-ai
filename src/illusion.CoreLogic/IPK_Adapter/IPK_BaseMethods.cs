using illusion.Common.Types;
using System.Runtime.InteropServices;
using System.Security;

namespace illusion.IPK_Adapter
{
    [SuppressUnmanagedCodeSecurity]
    internal static class IPK_BaseMethods
    {
        private const string DllName = "illusion.CoreLogic.Native";

        #region IPK_Common
        [DllImport(DllName, EntryPoint = "IPK_GetErrorMessage", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern nint IPK_GetErrorMessageNative(IPK_RESULT errorCode);

        [DllImport(DllName, EntryPoint = "IPK_FreeString", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void IPK_FreeString(nint str);

        internal static string GetErrorMessage(IPK_RESULT errorCode)
        {
            nint ptr = IPK_GetErrorMessageNative(errorCode);
            return Marshal.PtrToStringAnsi(ptr) ?? $"Unknown native error: {errorCode}";
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
        internal static extern IPK_RESULT IPK_CreateSymbolAST(string name, out nint out_ast);

        [DllImport(DllName, EntryPoint = "IPK_AST_CreateList", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IPK_RESULT IPK_CreateListAST(Size_t length, nint[] elements, out nint out_ast);

        [DllImport(DllName, EntryPoint = "IPK_AST_GetType", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IPK_RESULT IPK_GetTypeAST(nint ast, out AST_NodeType out_type);

        [DllImport(DllName, EntryPoint = "IPK_AST_GetSymbol", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IPK_RESULT IPK_GetSymbolAST(nint ast, out nint out_name);

        [DllImport(DllName, EntryPoint = "IPK_AST_GetList", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IPK_RESULT IPK_GetListAST(nint ast, Size_t index, out nint out_elements);

        [DllImport(DllName, EntryPoint = "IPK_AST_Clone", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IPK_RESULT IPK_CloneAST(nint ast, out nint out_ast);

        [DllImport(DllName, EntryPoint = "IPK_AST_Equal", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        internal static extern bool IPK_EqualAST(nint a, nint b);

        [DllImport(DllName, EntryPoint = "IPK_AST_Destroy", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void IPK_FreeAST(nint ast);

        [DllImport(DllName, EntryPoint = "IPK_AST_ToString", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IPK_RESULT IPK_ToStringAST(nint ast, out nint str);
        #endregion

        #region IPK_Parser
        [DllImport(DllName, EntryPoint = "IPK_ParseStatement", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern IPK_RESULT IPK_ParseStatement(string line, out nint out_ast);
        #endregion

        #region IPK_Context
        [DllImport(DllName, EntryPoint = "IPK_Context_Create", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IPK_RESULT IPK_CreateContext(out nint out_context);

        [DllImport(DllName, EntryPoint = "IPK_Context_Destroy", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void IPK_DestroyContext(nint context);
        #endregion
    }
}
