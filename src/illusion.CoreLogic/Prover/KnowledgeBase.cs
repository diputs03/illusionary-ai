using illusion.Common.Types;

namespace illusion.CoreLogic.Prover;

/// <summary>
/// Deterministic in-memory knowledge base for exact and Horn-rule reasoning.
/// It performs bounded forward chaining and records every derived expression.
/// </summary>
public sealed class KnowledgeBase
{
    private readonly HashSet<Expression> _facts = new();
    private readonly List<FormalRule> _rules = new();

    public IReadOnlyCollection<Expression> Facts => _facts.ToList().AsReadOnly();
    public IReadOnlyCollection<FormalRule> Rules => _rules.AsReadOnly();

    public KnowledgeBase(IEnumerable<Expression>? axioms = null, IEnumerable<FormalRule>? rules = null)
    {
        if (axioms is not null)
        {
            foreach (var axiom in axioms)
                AddFact(axiom);
        }

        if (rules is not null)
        {
            foreach (var rule in rules)
                AddRule(rule);
        }
    }

    public void AddFact(Expression fact)
    {
        if (fact is null)
            throw new IAException<ArgumentNullException>(nameof(fact));

        _facts.Add(fact);
    }

    public void AddRule(FormalRule rule)
    {
        if (rule is null)
            throw new IAException<ArgumentNullException>(nameof(rule));

        _rules.Add(rule);
    }

    public ProofTrace Prove(Expression target, int maxIterations = 512)
    {
        if (target is null)
            throw new IAException<ArgumentNullException>(nameof(target));
        if (maxIterations <= 0)
            throw new IAException<ArgumentOutOfRangeException>("max iterations must be positive");

        var started = System.Diagnostics.Stopwatch.StartNew();
        var known = new HashSet<Expression>(_facts);
        var stepByExpression = new Dictionary<Expression, int>();
        var steps = new List<ProofStep>();

        foreach (var fact in known.OrderBy(f => f.ToCanonicalString(), StringComparer.Ordinal))
        {
            AddStep("axiom", fact, "Accepted ground-truth axiom.", Array.Empty<int>());
            if (fact.Equals(target))
                return Success();
        }

        for (var iteration = 0; iteration < maxIterations; iteration++)
        {
            var derivedThisRound = false;

            foreach (var rule in _rules.OrderBy(r => r.Name, StringComparer.Ordinal))
            {
                foreach (var result in ApplyRule(rule, known).OrderBy(r => r.Conclusion.ToCanonicalString(), StringComparer.Ordinal))
                {
                    if (!known.Add(result.Conclusion))
                        continue;

                    derivedThisRound = true;
                    AddStep(rule.Name, result.Conclusion, rule.Description, result.PremiseStepNumbers);
                    if (result.Conclusion.Equals(target))
                        return Success();
                }
            }

            if (!derivedThisRound)
                break;
        }

        started.Stop();
        return new ProofTrace(
            CreateTraceId(),
            target,
            isSuccess: false,
            steps,
            started.ElapsedMilliseconds,
            $"Target proposition was not derivable from {_facts.Count} axioms and {_rules.Count} rules.");

        void AddStep(string ruleName, Expression expression, string? description, IReadOnlyList<int> premiseStepNumbers)
        {
            var step = new ProofStep(steps.Count + 1, ruleName, expression, description, premiseStepNumbers);
            steps.Add(step);
            stepByExpression[expression] = step.StepNumber;
        }

        ProofTrace Success()
        {
            started.Stop();
            return new ProofTrace(CreateTraceId(), target, isSuccess: true, steps, started.ElapsedMilliseconds);
        }

        static string CreateTraceId() => $"proof-{Guid.NewGuid():N}";

        IEnumerable<RuleApplication> ApplyRule(FormalRule rule, IReadOnlyCollection<Expression> availableFacts)
        {
            var bindings = new List<Dictionary<string, string>> { new(StringComparer.Ordinal) };
            var premiseSteps = new List<List<int>> { new() };

            foreach (var premise in rule.Premises)
            {
                var nextBindings = new List<Dictionary<string, string>>();
                var nextPremiseSteps = new List<List<int>>();

                for (var i = 0; i < bindings.Count; i++)
                {
                    foreach (var fact in availableFacts)
                    {
                        var candidate = new Dictionary<string, string>(bindings[i], StringComparer.Ordinal);
                        if (!TryUnify(premise, fact, candidate))
                            continue;

                        nextBindings.Add(candidate);
                        var stepNumbers = new List<int>(premiseSteps[i]);
                        if (stepByExpression.TryGetValue(fact, out var stepNumber))
                            stepNumbers.Add(stepNumber);
                        nextPremiseSteps.Add(stepNumbers);
                    }
                }

                bindings = nextBindings;
                premiseSteps = nextPremiseSteps;
                if (bindings.Count == 0)
                    yield break;
            }

            for (var i = 0; i < bindings.Count; i++)
            {
                yield return new RuleApplication(Instantiate(rule.Conclusion, bindings[i]), premiseSteps[i]);
            }
        }
    }

    private static bool TryUnify(Expression pattern, Expression fact, IDictionary<string, string> bindings) =>
        TryUnifyTerm(pattern.PredicateName, fact.PredicateName, bindings)
        && TryUnifyTerm(pattern.TargetObject.Id, fact.TargetObject.Id, bindings)
        && TryUnifyTerm(pattern.TargetObject.Name, fact.TargetObject.Name, bindings);

    private static bool TryUnifyTerm(string pattern, string value, IDictionary<string, string> bindings)
    {
        if (!IsVariable(pattern))
            return StringComparer.Ordinal.Equals(pattern, value);

        if (bindings.TryGetValue(pattern, out var existing))
            return StringComparer.Ordinal.Equals(existing, value);

        bindings[pattern] = value;
        return true;
    }

    private static Expression Instantiate(Expression template, IReadOnlyDictionary<string, string> bindings) =>
        new(
            Resolve(template.PredicateName, bindings),
            new illusion.Common.Types.Object(
                Resolve(template.TargetObject.Id, bindings),
                Resolve(template.TargetObject.Name, bindings)));

    private static string Resolve(string value, IReadOnlyDictionary<string, string> bindings) =>
        IsVariable(value) && bindings.TryGetValue(value, out var bound) ? bound : value;

    private static bool IsVariable(string value) => value.StartsWith("?", StringComparison.Ordinal);

    private sealed record RuleApplication(Expression Conclusion, IReadOnlyList<int> PremiseStepNumbers);
}
