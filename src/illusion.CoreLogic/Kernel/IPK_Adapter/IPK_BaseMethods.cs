using illusion.Common.Types;
using System;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Security;

namespace illusion.Kernel.IPK_Adapter
{
    [SuppressUnmanagedCodeSecurity]
    internal static class IPK_BaseMethods
    {
        private const string DllName = "illusion.CoreLogic.Native.dll";

        #region IPK_Common
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern string IPK_GetErrorMessage(IPK_RESULT errorCode);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern IPK_RESULT IPK_MakeString(string str, out IntPtr out_str);

        internal static void CheckResult(IPK_RESULT result)
        {
            if (result != IPK_RESULT.IPK_SUCCESS)
            {
                string errorMessage = IPK_GetErrorMessage(result);
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
        internal static extern IPK_RESULT IPK_ContextAddAxiom(IntPtr context, Index_t axiomId, IntPtr statement);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern IPK_RESULT IPK_ContextAddRule(IntPtr context, Index_t ruleId, IntPtr rule);
        #endregion

        #region IPK_Prover
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IPK_RESULT IPK_VerifyProof(IntPtr context, IntPtr proofDag,
                                                            out bool outIsValid);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void IPK_FreeProofDAG(IntPtr proofDag);
        #endregion
    }
}
