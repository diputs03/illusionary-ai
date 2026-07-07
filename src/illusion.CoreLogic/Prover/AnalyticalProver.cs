using illusion.Common.Types;
using illusion.CoreLogic.Kernel;

namespace illusion.CoreLogic.Prover;

public sealed class AnalyticalProver<T> : IAnalyticalProver where T : IStatement<T>
{
    private readonly IKernel<T>? _kernelAdapter;
    private readonly KnowledgeBase _knowledgeBase;
    private readonly int _maxIterations;

    public AnalyticalProver(IKernel<T> kernelAdapter)
        : this(kernelAdapter, Enumerable.Empty<Predicate>(), Enumerable.Empty<FormalRule>())
    {
    }

    public AnalyticalProver(IEnumerable<Predicate> axioms, IEnumerable<FormalRule>? rules = null, int maxIterations = 512)
        : this(null, axioms, rules, maxIterations)
    {
    }

    private AnalyticalProver(IKernel<T>? kernelAdapter, IEnumerable<Predicate> axioms,
        IEnumerable<FormalRule>? rules = null, int maxIterations = 512)
    {
        if (maxIterations <= 0)
            throw new IAException<ArgumentOutOfRangeException>("max iterations must be positive");

        _kernelAdapter = kernelAdapter;
        _knowledgeBase = new KnowledgeBase(axioms, rules);
        _maxIterations = maxIterations;
    }

    public void AddAxiom(Predicate axiom) => _knowledgeBase.AddFact(axiom);

    public void AddRule(FormalRule rule) => _knowledgeBase.AddRule(rule);

    public ProofTrace Prove(Predicate proposition)
    {
        if (_kernelAdapter is not null && !_kernelAdapter.IsInit)
            _kernelAdapter.Init();

        return _knowledgeBase.Prove(proposition, _maxIterations);
    }

    public bool Verify(Predicate proposition) => Prove(proposition).IsSuccess;
}
