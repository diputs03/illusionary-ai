using illusion.Common.Types;

using illusion.CoreLogic.Prover;

namespace illusion.CoreLogic.Kernel;

/// <summary>
/// LEAN formal proof assistant, adapted for use in the Illusion framework.
/// </summary>
public interface IKernelAdapter : IDisposable
{
    bool IsInitialized();
    bool Initialize();
    void Shutdown();
    IntPtr CreateSymbolAST(string name);
    IntPtr CreateListAST(IntPtr[] elements);
    IntPtr CloneAST(IntPtr ast);
    bool EqualAST(IntPtr a, IntPtr b);
    void PrintAST(IntPtr ast, ulong indent = 0);
    //Index_t AddAxiom(string name, string statement);
    //string GetAxiom(Index_t axiomId);
    //Index_t AddRule(string name, string statement, IntPtr[] premiseIds);
    //string GetRule(string ruleName);
}