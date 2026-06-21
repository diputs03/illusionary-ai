using illusion.Common.Types;

namespace illusion.CoreLogic.Prover;

/// <summary>
/// Deterministic analytical prover interface.
/// </summary>
public interface IAnalyticalProver
{
    ProofTrace Prove(Expression proposition);
    bool Verify(Expression proposition);
}
