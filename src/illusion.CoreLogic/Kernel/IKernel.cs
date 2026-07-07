namespace illusion.CoreLogic.Kernel;


/// <summary>
/// formal proof assistant, adapted for use in the Illusion framework.
/// </summary>
public interface IKernel<T> : IDisposable
    where T : IStatement<T>
{
    bool IsInit { get; }
    bool Init();
    void Shutdown();
    void LoadContext();
    void ClearContext();
    bool Verify();
    Index_t AddAxiom(string name, string statement);
    string GetAxiom(Index_t axiomId);
    Index_t AddRule(string name, string statement, IntPtr[] premiseIds);
    string GetRule(string ruleName);
}