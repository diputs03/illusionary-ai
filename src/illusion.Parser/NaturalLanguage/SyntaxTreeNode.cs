namespace illusion.Parser.NaturalLanguage;

/// <summary>
/// Represents a node in the English syntax parse tree.
/// Deterministic, rule-based parse tree with no probabilistic ambiguity.
/// </summary>
public sealed class SyntaxTreeNode
{
    public string Word { get; }
    public PartOfSpeech Pos { get; }
    public SyntaxRole Role { get; set; }
    public List<SyntaxTreeNode> Children { get; } = [];
    public SyntaxTreeNode? Parent { get; set; }

    public SyntaxTreeNode(string word, PartOfSpeech pos)
    {
        Word = word.ToLowerInvariant();
        Pos = pos;
        Role = SyntaxRole.Unknown;
    }

    public void AddChild(SyntaxTreeNode child)
    {
        child.Parent = this;
        Children.Add(child);
    }

    public IEnumerable<SyntaxTreeNode> GetAllNodes()
    {
        yield return this;
        foreach (var child in Children)
        {
            foreach (var node in child.GetAllNodes())
            {
                yield return node;
            }
        }
    }

    public override string ToString() => $"{Word}/{Pos} [{Role}]";

    public string PrettyPrint(int indent = 0)
    {
        var prefix = new string(' ', indent * 2);
        var result = $"{prefix}{ToString()}";
        foreach (var child in Children)
        {
            result += Environment.NewLine + child.PrettyPrint(indent + 1);
        }
        return result;
    }
}

/// <summary>
/// Deterministic part-of-speech tags.
/// No probabilistic tagging - exact dictionary lookup.
/// </summary>
public enum PartOfSpeech
{
    Unknown,
    Noun,
    Verb,
    Adjective,
    Adverb,
    Preposition,
    Determiner,
    Pronoun,
    Conjunction,
    Auxiliary,
    Copula, // is, are, was, were
    Particle,
    Modal, // will, can, may, might, must
    Interjection // well, oh, ah
}

/// <summary>
/// Syntactic role in the sentence structure.
/// </summary>
public enum SyntaxRole
{
    Unknown,
    Root,
    Subject,
    Predicate,
    Object,
    Attribute,
    Complement,
    PrepositionalPhrase
}
