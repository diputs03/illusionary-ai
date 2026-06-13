using illusion.Common.Types;
using illusion.CoreLogic.Prover;

namespace illusion.Parser.NaturalLanguage;

/// <summary>
/// Converts natural English sentences to formal predicate expressions.
/// Uses syntax tree + word embedding for deterministic, zero-hallucination mapping.
/// </summary>
public sealed class NaturalLanguageFormalizer
{
    private readonly EnglishSyntaxParser _syntaxParser = new();
    private readonly WordEmbeddingIndex _embeddingIndex = new();

    /// <summary>
    /// Convert a natural English sentence to a formal Expression.
    /// Example: "Socrates is a man" → Human(socrates:socrates)
    /// </summary>
    public Expression Formalize(string englishSentence)
    {
        if (!TryFormalize(englishSentence, out var expression, out var error))
        {
            throw new FormatException($"Failed to formalize: {error}");
        }
        return expression;
    }

    /// <summary>
    /// Try to convert natural English to formal Expression.
    /// </summary>
    public bool TryFormalize(string englishSentence, out Expression expression, out string error)
    {
        expression = null!;
        error = string.Empty;

        if (string.IsNullOrWhiteSpace(englishSentence))
        {
            error = "sentence cannot be empty";
            return false;
        }

        try
        {
            // Step 1: Parse syntax tree
            var parseTree = _syntaxParser.Parse(englishSentence);

            // Step 2: Extract predicate structure from syntax tree
            var (predicate, subjectId, subjectName) = _syntaxParser.ExtractPredicate(parseTree);

            // Step 3: Use word embedding to map to canonical predicate names
            var canonicalPredicate = CanonicalizePredicate(predicate);

            // Step 4: Create formal expression
            expression = new Expression(
                canonicalPredicate,
                new illusion.Common.Types.Object(subjectId.ToLowerInvariant(), subjectName));

            return true;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return false;
        }
    }

    /// <summary>
    /// Convert multiple English sentences to axioms for the knowledge base.
    /// </summary>
    public List<Expression> FormalizeAxioms(IEnumerable<string> sentences)
    {
        return sentences.Select(Formalize).ToList();
    }

    /// <summary>
    /// Create a formal inference rule from natural language pattern.
    /// </summary>
    public FormalRule FormalizeRule(string name, string naturalRule, string description = "")
    {
        // Parse: "If ?x is Human then ?x is Mortal"
        var parts = naturalRule.Split(new[] { "then", "->" }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2)
        {
            throw new FormatException("Rule must be in form: 'If PREMISE then CONCLUSION'");
        }

        var premisePart = parts[0].Replace("If", "", StringComparison.OrdinalIgnoreCase).Trim();
        var conclusionPart = parts[1].Trim();

        var premises = ExtractRulePremises(premisePart);
        var conclusion = FormalizeRuleExpression(conclusionPart);

        return new FormalRule(name, premises, conclusion, description);
    }

    /// <summary>
    /// Use word embedding to find the canonical predicate name.
    /// Ensures consistent predicate naming across different phrasings.
    /// </summary>
    private string CanonicalizePredicate(string predicate)
    {
        var canonicalPredicates = new[] { "Human", "Mortal", "Animal", "Wise", "Living", "Person", "Philosopher" };
        return _embeddingIndex.FindClosestPredicate(predicate, canonicalPredicates);
    }

    private List<Expression> ExtractRulePremises(string premiseText)
    {
        // Split on "and" for multiple premises
        var parts = premiseText.Split(new[] { "and" }, StringSplitOptions.RemoveEmptyEntries);
        return parts.Select(p => FormalizeRuleExpression(p.Trim())).ToList();
    }

    private Expression FormalizeRuleExpression(string rulePart)
    {
        // Handle rule variables: "?x is Human" → Human(?x:?x)
        var tokens = rulePart.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        string subject = "?x";
        string predicate = "Unknown";

        for (var i = 0; i < tokens.Length; i++)
        {
            if (tokens[i].StartsWith("?"))
            {
                subject = tokens[i];
            }
            if (tokens[i].Equals("is", StringComparison.OrdinalIgnoreCase) && i + 1 < tokens.Length)
            {
                predicate = CanonicalizePredicate(tokens[i + 1]);
            }
        }

        return new Expression(predicate, new illusion.Common.Types.Object(subject, subject));
    }

    /// <summary>
    /// Get semantic similarity between two words for disambiguation.
    /// </summary>
    public float GetSemanticSimilarity(string word1, string word2)
    {
        return _embeddingIndex.Similarity(word1, word2);
    }

    /// <summary>
    /// Convert Syntax Tree directly to canonical E(O) form.
    /// Automatically creates new Predicates (E) and Objects (O) as needed.
    /// 
    /// Canonical form: PredicateName(objectId:objectName)
    /// </summary>
    public Expression SyntaxTreeToEO(SyntaxTreeNode parseTree)
    {
        ArgumentNullException.ThrowIfNull(parseTree);

        var (subjectNode, predicateNode, complementNode) = ExtractSemanticTriple(parseTree);
        var obj = CreateDeterministicObject(subjectNode);
        var predicate = CreateCanonicalPredicate(predicateNode ?? complementNode);

        return new Expression(predicate, obj);
    }

    /// <summary>
    /// Batch convert multiple sentences to E(O) axioms.
    /// </summary>
    public List<Expression> SentencesToEOAxioms(IEnumerable<string> sentences)
    {
        var axioms = new List<Expression>();
        foreach (var sentence in sentences)
        {
            var tree = _syntaxParser.Parse(sentence);
            axioms.Add(SyntaxTreeToEO(tree));
        }
        return axioms;
    }

    private (SyntaxTreeNode Subject, SyntaxTreeNode? Predicate, SyntaxTreeNode Complement) 
        ExtractSemanticTriple(SyntaxTreeNode root)
    {
        var subject = root.Children.FirstOrDefault(c => c.Role == SyntaxRole.Subject)
            ?? throw new FormatException("No subject node in parse tree");

        var complement = root.Children.FirstOrDefault(c => c.Role == SyntaxRole.Complement)
            ?? throw new FormatException("No complement node in parse tree");

        return (subject, null, complement);
    }

    private illusion.Common.Types.Object CreateDeterministicObject(SyntaxTreeNode subjectNode)
    {
        var headNoun = subjectNode.Children
            .OrderByDescending(c => c.Pos == PartOfSpeech.Noun ? 1 : 0)
            .FirstOrDefault(c => !string.IsNullOrEmpty(c.Word))
            ?? subjectNode;

        var objectId = NormalizeId(headNoun.Word);
        var objectName = headNoun.Word;

        return new illusion.Common.Types.Object(objectId, objectName);
    }

    private string CreateCanonicalPredicate(SyntaxTreeNode complementNode)
    {
        var headWord = GetHeadWordFromComplement(complementNode);
        var rawPredicate = Capitalize(headWord);

        var canonicalPredicates = new[] 
        { 
            "Human", "Mortal", "Animal", "Wise", "Living", 
            "Person", "Philosopher", "Happy", "Sad", "Good", 
            "Bad", "True", "False", "Smart", "Foolish",
            "Doctor", "Teacher", "Student", "Engineer"
        };

        var matched = _embeddingIndex.FindClosestPredicate(rawPredicate, canonicalPredicates);
        var similarity = _embeddingIndex.Similarity(rawPredicate, matched);

        if (similarity < 0.5f)
        {
            return Capitalize(rawPredicate);
        }

        return matched;
    }

    /// <summary>
    /// Generate inference rule in E(O) form from natural language.
    /// </summary>
    public FormalRule CreateEORule(string ruleName, string naturalLanguageRule, string description = "")
    {
        var match = System.Text.RegularExpressions.Regex.Match(
            naturalLanguageRule, 
            @"(All|Every|Any)\s+(?<premise>\w+)\s+(are|is)\s+(?<conclusion>\w+)",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        if (match.Success)
        {
            var premisePred = SingularizeAndCapitalize(match.Groups["premise"].Value);
            var conclusionPred = SingularizeAndCapitalize(match.Groups["conclusion"].Value);

            var premise = new Expression(
                premisePred, 
                new illusion.Common.Types.Object("?x", "?x"));

            var conclusion = new Expression(
                conclusionPred, 
                new illusion.Common.Types.Object("?x", "?x"));

            return new FormalRule(
                ruleName,
                new List<Expression> { premise },
                conclusion,
                description);
        }

        return FormalizeRule(ruleName, naturalLanguageRule, description);
    }

    private static string GetHeadWordFromComplement(SyntaxTreeNode complement)
    {
        var noun = complement.Children.FirstOrDefault(c => c.Pos == PartOfSpeech.Noun);
        if (noun != null) return noun.Word;

        var adj = complement.Children.FirstOrDefault(c => c.Pos == PartOfSpeech.Adjective);
        if (adj != null) return adj.Word;

        return complement.Children.FirstOrDefault()?.Word ?? "Unknown";
    }

    private static string NormalizeId(string word)
    {
        return new string(word
            .ToLowerInvariant()
            .Where(c => char.IsLetterOrDigit(c) || c == '_')
            .ToArray());
    }

    private static string Capitalize(string s)
    {
        if (string.IsNullOrEmpty(s)) return s;
        return char.ToUpperInvariant(s[0]) + s.Substring(1).ToLowerInvariant();
    }

    private static string SingularizeAndCapitalize(string s)
    {
        if (string.IsNullOrEmpty(s)) return s;
        
        var lower = s.ToLowerInvariant();
        
        // Simple deterministic singularization for common cases
        if (lower.EndsWith("s") && lower.Length > 2)
        {
            // humans → human, animals → animal
            lower = lower.Substring(0, lower.Length - 1);
        }
        
        return char.ToUpperInvariant(lower[0]) + lower.Substring(1);
    }

    /// <summary>
    /// Convert entire knowledge base from natural language to formal E(O) system.
    /// </summary>
    public (List<Expression> Axioms, List<FormalRule> Rules) BuildFormalKnowledgeBase(
        IEnumerable<string> factSentences,
        IEnumerable<(string Name, string RuleText, string Desc)> ruleDefinitions)
    {
        var axioms = SentencesToEOAxioms(factSentences).ToList();
        var rules = ruleDefinitions
            .Select(r => CreateEORule(r.Name, r.RuleText, r.Desc))
            .ToList();

        return (axioms, rules);
    }
}
