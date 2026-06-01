using System;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Security;

namespace illusion.CoreLogic.Kernel
{
    [SuppressUnmanagedCodeSecurity]
    internal static class IPK_NativeMethods
    {
        private const string DllName = "illusion.CoreLogic.Native.dll";

        #region IPK_Common
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern string IPK_GetErrorMessage([In] IPK_RESULT errorCode);
        #endregion

        #region IPK_AST
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IPK_RESULT IPK_CreateSymbolAST(string name, out IntPtr out_ast);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IPK_RESULT IPK_CreateListAST(Size_t length, IntPtr[] elements,
                                                         out IntPtr out_ast);
        
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IPK_RESULT IPK_CloneAST(IntPtr ast, out IntPtr out_ast);
        
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern bool IPK_EqualAST(IntPtr a, IntPtr b);
        
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void IPK_FreeAST(IntPtr ast);
        
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void IPK_PrintAST(IntPtr ast, Size_t indent);
        #endregion

        #region IPK_Context
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IPK_RESULT IPK_CreateContext(out IntPtr context);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void IPK_DestroyContext(IntPtr context);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern IPK_RESULT IPK_ContextAddAxiom(IntPtr context,
                                                         string name,
                                                         string statement,
                                                         out Index_t outAxiomId);
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern IPK_RESULT IPK_ContextGetAxiom(IntPtr context, Index_t axiomId,
                                                         out IntPtr outStatement);
        
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern IPK_RESULT IPK_ContextAddRule(IntPtr context, string name,
                                                         string statement, Size_t premiseCount,
                                                         IntPtr premiseIds, out Index_t outRuleId);
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern IPK_RESULT IPK_ContextGetRule(IntPtr context, string ruleName,
                                                         out IntPtr outRule);
        #endregion

        #region IPK_Kernel
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IPK_RESULT IPK_VerifyProof(IntPtr context, IntPtr proofDag,
                                                            out bool outIsValid);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void IPK_FreeProofDAG(IntPtr proofDag);
        #endregion
    }
}
