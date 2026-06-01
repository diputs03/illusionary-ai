global using Index_t = System.Int64;
global using Size_t = System.UInt64;

using System;
using System.Runtime.InteropServices;
namespace illusion.CoreLogic.Kernel
{
    #region IPK_Common
    internal enum IPK_RESULT
    {
        IPK_SUCCESS                      = 0x000,
        IPK_ERROR                        = 0x001,
        IPK_ERROR_RUNTIME_ERROR          = 0x011,
        IPK_ERROR_INVALID_ARGUMENT       = 0x111,
        IPK_ERROR_SYNTAX_ERROR           = 0x211,
        IPK_ERROR_UNSUPPORTED_OPERATION  = 0x311,
        IPK_ERROR_AXIOM_NOT_FOUND        = 0x411,
        IPK_ERROR_UNIFICATION_FAILED     = 0x511,
        IPK_ERROR_NOT_FOUND              = 0x611,
        IPK_ERROR_INTERNET_ERROR         = 0x021,
        IPK_ERROR_MEMORY_ERROR           = 0x031,
        IPK_ERROR_NULL_POINTER           = 0x131,
        IPK_ERROR_OUT_OF_MEMORY          = 0x231,
    }
    #endregion

    #region IPK_Kernel
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    internal struct IPK_ProofNode
    {
        public Index_t Id;
        public string RuleName;
        public IntPtr Statement;
        public Size_t PremiseCount;
        public IntPtr PremiseIds;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    internal struct IPK_ProofDAG
    {
        public Size_t NodeCount;
        public IntPtr Nodes;
    }
    #endregion
}
