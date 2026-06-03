using illusion.Common.Types;
using System;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Security;

namespace illusion.CoreLogic.Kernel
{
    [SuppressUnmanagedCodeSecurity]
    internal static class IPK_BaseMethods
    {
        private const string DllName = "illusion.CoreLogic.Native.dll";

        #region IPK_Common
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern string IPK_GetErrorMessage([In] IPK_RESULT errorCode);

        internal static void CheckResult(IPK_RESULT result)
        {
            if (result != IPK_RESULT.IPK_SUCCESS)
            {
                var errorMessage = IPK_GetErrorMessage(result);
                throw new IAException<IPK_RESULT>(errorMessage);
            }
        }
        #endregion

        #region IPK_AST
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IPK_RESULT IPK_CreateSymbolAST(string name, out IntPtr out_ast);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IPK_RESULT IPK_CreateListAST(Size_t length, IntPtr[] elements,
                                                         out IntPtr out_ast);
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IPK_RESULT IPK_GetTypeAST(IntPtr ast, out AST_NodeType out_type);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IPK_RESULT IPK_GetSymbolAST(IntPtr ast, out string out_name);
        
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IPK_RESULT IPK_GetListAST(IntPtr ast, Size_t index,
                                                         out IntPtr out_elements);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IPK_RESULT IPK_CloneAST(IntPtr ast, out IntPtr out_ast);
        
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern bool IPK_EqualAST(IntPtr a, IntPtr b);
        
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void IPK_FreeAST(IntPtr ast);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern void IPK_ToStringAST(IntPtr ast, out string str);
        #endregion

        #region IPK_Parser
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern IPK_RESULT IPK_ParseStatement(string statement, out IntPtr outAst);
        #endregion

        #region IPK_Substitution
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IPK_RESULT IPK_CreateSubstitution(out IntPtr out_subst);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern IPK_RESULT IPK_AddSubstitution(IntPtr sub, string varName, IntPtr replacement);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern IPK_RESULT IPK_LookupSubstitution(IntPtr sub, string varName, out IntPtr outReplacement);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern IPK_RESULT IPK_ApplySubstitution(IntPtr subst, IntPtr ast, out IntPtr outAst);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void IPK_DestroySubstitution(IntPtr subst);
        #endregion

        #region IPK_Rule
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern IPK_RESULT IPK_CreateRule(string name, Size_t premiseCount,
                                                IntPtr[] premises, IntPtr conclusion,
                                                out IntPtr outRule);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern IPK_RESULT IPK_ApplyRule(IntPtr rule, IntPtr[] premiseIns, out IntPtr outIns);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern void IPK_FreeRule(IntPtr rule);
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
