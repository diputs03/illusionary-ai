using illusion.Common.Types;

namespace illusion.CoreLogic.Prover;

/// <summary>
/// Deterministic analytical prover interface.
/// </summary>
public interface IAnalyticalProver
{
    ProofTrace Prove(Predicate proposition);
    bool Verify(Predicate proposition);
}
