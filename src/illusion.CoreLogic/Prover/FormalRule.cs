using illusion.Common.Types;

namespace illusion.CoreLogic.Prover;

/// <summary>
/// Immutable Horn-style inference rule. Variables are represented by terms that
/// begin with '?' in predicate names, object ids, or object names.
/// </summary>
public sealed class FormalRule
{
    public string Name { get; }
    public IReadOnlyList<Predicate> Premises { get; }
    public Predicate Conclusion { get; }
    public string Description { get; }

    public FormalRule(string name, IEnumerable<Predicate> premises, Predicate conclusion, string description = "")
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new IAException<ArgumentException>("rule name cannot be empty");

        Name = name.Trim();
        Premises = (premises ?? throw new IAException<ArgumentNullException>(nameof(premises))).ToList().AsReadOnly();
        Conclusion = conclusion ?? throw new IAException<ArgumentNullException>(nameof(conclusion));
        Description = description ?? string.Empty;
    }
}
