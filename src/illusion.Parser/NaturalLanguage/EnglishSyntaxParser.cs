namespace illusion.Parser.NaturalLanguage;

/// <summary>
/// Deterministic English syntax parser.
/// Builds constituency parse trees using explicit grammar rules.
/// No probabilistic parsing - 100% deterministic, zero hallucination.
/// </summary>
public sealed class EnglishSyntaxParser
{
    /// <summary>
    /// Parse an English sentence into a syntax tree.
    /// Supports: S → NP VP (copula sentences: "X is Y")
    /// </summary>
    public SyntaxTreeNode Parse(string sentence)
    {
        var tokens = EnglishTokenizer.TokenizeAndTag(sentence);
        return BuildParseTree(tokens);
    }

    /// <summary>
    /// Build deterministic parse tree using explicit grammar rules.
    /// Grammar:
    ///   S  → NP Copula NP    (Socrates is a man)
    ///   S  → NP Copula Adj   (Socrates is wise)
    ///   NP → Det? (Adj*) Noun
    /// </summary>
    private SyntaxTreeNode BuildParseTree(List<SyntaxTreeNode> tokens)
    {
        // Root node (sentence)
        var root = new SyntaxTreeNode("S", PartOfSpeech.Unknown) { Role = SyntaxRole.Root };

        // Find copula position
        var copulaIndex = tokens.FindIndex(t => t.Pos == PartOfSpeech.Copula);

        if (copulaIndex >= 0)
        {
            // Copular sentence structure: [Subject] [Copula] [Complement]
            ParseCopularSentence(tokens, copulaIndex, root);
        }
        else
        {
            // Simple noun phrase or other structure
            ParseSimpleStructure(tokens, root);
        }

        return root;
    }

    private void ParseCopularSentence(List<SyntaxTreeNode> tokens, int copulaIndex, SyntaxTreeNode root)
    {
        // Subject: everything before copula
        var subjectTokens = tokens.Take(copulaIndex).ToList();
        var subjectPhrase = BuildNounPhrase(subjectTokens);
        subjectPhrase.Role = SyntaxRole.Subject;
        root.AddChild(subjectPhrase);

        // Copula (predicate head)
        var copula = tokens[copulaIndex];
        copula.Role = SyntaxRole.Predicate;
        root.AddChild(copula);

        // Complement: everything after copula
        var complementTokens = tokens.Skip(copulaIndex + 1).ToList();
        var complementPhrase = BuildComplementPhrase(complementTokens);
        complementPhrase.Role = SyntaxRole.Complement;
        root.AddChild(complementPhrase);
    }

    private SyntaxTreeNode BuildNounPhrase(List<SyntaxTreeNode> tokens)
    {
        var np = new SyntaxTreeNode("NP", PartOfSpeech.Noun);

        // Filter out determiners, build noun phrase structure
        var contentTokens = tokens.Where(t => t.Pos != PartOfSpeech.Determiner).ToList();

        foreach (var token in contentTokens)
        {
            if (token.Pos == PartOfSpeech.Adjective)
            {
                token.Role = SyntaxRole.Attribute;
            }
            np.AddChild(token);
        }

        return np;
    }

    private SyntaxTreeNode BuildComplementPhrase(List<SyntaxTreeNode> tokens)
    {
        var cp = new SyntaxTreeNode("COMP", PartOfSpeech.Unknown);

        // Filter out determiners
        var contentTokens = tokens.Where(t => t.Pos != PartOfSpeech.Determiner).ToList();

        foreach (var token in contentTokens)
        {
            if (token.Pos == PartOfSpeech.Adjective)
            {
                token.Role = SyntaxRole.Attribute;
            }
            cp.AddChild(token);
        }

        return cp;
    }

    private void ParseSimpleStructure(List<SyntaxTreeNode> tokens, SyntaxTreeNode root)
    {
        foreach (var token in tokens)
        {
            root.AddChild(token);
        }
    }

    /// <summary>
    /// Extract semantic predicate from parse tree.
    /// Returns (PredicateName, SubjectId, SubjectName)
    /// </summary>
    public (string Predicate, string SubjectId, string SubjectName) ExtractPredicate(SyntaxTreeNode parseTree)
    {
        var subjectNode = parseTree.Children.FirstOrDefault(c => c.Role == SyntaxRole.Subject);
        var complementNode = parseTree.Children.FirstOrDefault(c => c.Role == SyntaxRole.Complement);

        string subjectId = "entity";
        string subjectName = "unknown";
        string predicate = "is";

        // Extract subject - find the main noun
        if (subjectNode != null)
        {
            var mainNoun = subjectNode.Children.FirstOrDefault(c => c.Pos == PartOfSpeech.Noun);
            if (mainNoun != null)
            {
                subjectId = mainNoun.Word;
                subjectName = mainNoun.Word;
            }
        }

        // Extract predicate from complement
        if (complementNode != null)
        {
            // Adjective complement: "is wise" → predicate = "Wise"
            var adj = complementNode.Children.FirstOrDefault(c => c.Pos == PartOfSpeech.Adjective);
            if (adj != null)
            {
                predicate = Capitalize(adj.Word);
            }

            // Noun complement: "is a human" → predicate = "Human"
            var noun = complementNode.Children.FirstOrDefault(c => c.Pos == PartOfSpeech.Noun);
            if (noun != null)
            {
                predicate = Capitalize(noun.Word);
            }
        }

        return (predicate, subjectId, subjectName);
    }

    private static string Capitalize(string s)
    {
        if (string.IsNullOrEmpty(s)) return s;
        return char.ToUpperInvariant(s[0]) + s.Substring(1).ToLowerInvariant();
    }
}
