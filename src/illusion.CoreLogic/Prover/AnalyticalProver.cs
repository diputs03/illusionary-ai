using illusion.Common.Types;
using illusion.CoreLogic.Kernel;

namespace illusion.CoreLogic.Prover;

public class AnalyticalProver : IAnalyticalProver
{
    private readonly IKernelAdapter _kernelAdapter;

    public AnalyticalProver(IKernelAdapter kernelAdapter)
    {
        _kernelAdapter = kernelAdapter;
    }

    //public ProofTrace Prove(Common.Types.Expression proposition)
    //{
    //    return _kernelAdapter.Prove(proposition);
    //}

    //public bool Verify(Common.Types.Expression proposition)
    //{
    //    return _kernelAdapter.IsProvable(proposition);
    //}
}