using illusion.Common.Types;
using illusion.CoreLogic.Kernel;

namespace illusion.CoreLogic.Prover;

public sealed class AnalyticalProver : IAnalyticalProver
{
    private readonly IKernelAdapter? _kernelAdapter;
    private readonly KnowledgeBase _knowledgeBase;
    private readonly int _maxIterations;

    public AnalyticalProver(IKernelAdapter kernelAdapter)
        : this(kernelAdapter, Enumerable.Empty<Expression>(), Enumerable.Empty<FormalRule>())
    {
    }

    public AnalyticalProver(IEnumerable<Expression> axioms, IEnumerable<FormalRule>? rules = null, int maxIterations = 512)
        : this(null, axioms, rules, maxIterations)
    {
    }

    private AnalyticalProver(IKernelAdapter? kernelAdapter, IEnumerable<Expression> axioms,
        IEnumerable<FormalRule>? rules = null, int maxIterations = 512)
    {
        if (maxIterations <= 0)
            throw new IAException<ArgumentOutOfRangeException>("max iterations must be positive");

        _kernelAdapter = kernelAdapter;
        _knowledgeBase = new KnowledgeBase(axioms, rules);
        _maxIterations = maxIterations;
    }

    public void AddAxiom(Expression axiom) => _knowledgeBase.AddFact(axiom);

    public void AddRule(FormalRule rule) => _knowledgeBase.AddRule(rule);

    public ProofTrace Prove(Expression proposition)
    {
        if (_kernelAdapter is not null && !_kernelAdapter.IsInitialized())
            _kernelAdapter.Initialize();

        return _knowledgeBase.Prove(proposition, _maxIterations);
    }

    public bool Verify(Expression proposition) => Prove(proposition).IsSuccess;
}
