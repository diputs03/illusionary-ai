namespace illusion.Parser.NaturalLanguage;

/// <summary>
/// Deterministic English tokenizer and POS tagger.
/// Uses exact dictionary lookup - no probabilistic ML, zero hallucination.
/// </summary>
public static class EnglishTokenizer
{
    #region Deterministic POS Dictionaries

    private static readonly HashSet<string> Copulas = new(StringComparer.OrdinalIgnoreCase)
    {
        "is", "are", "was", "were", "be", "been", "being", "am"
    };

    private static readonly HashSet<string> Auxiliaries = new(StringComparer.OrdinalIgnoreCase)
    {
        "has", "have", "had", "do", "does", "did", "will", "would",
        "shall", "should", "may", "might", "can", "could", "must"
    };

    private static readonly HashSet<string> Determiners = new(StringComparer.OrdinalIgnoreCase)
    {
        "a", "an", "the", "this", "that", "these", "those",
        "my", "your", "his", "her", "its", "our", "their",
        "some", "any", "each", "every", "all", "both", "few", "many"
    };

    private static readonly HashSet<string> Prepositions = new(StringComparer.OrdinalIgnoreCase)
    {
        "in", "on", "at", "by", "for", "with", "about", "against",
        "between", "into", "through", "during", "before", "after",
        "above", "below", "to", "from", "up", "down", "of", "off"
    };

    private static readonly HashSet<string> Pronouns = new(StringComparer.OrdinalIgnoreCase)
    {
        "i", "you", "he", "she", "it", "we", "they",
        "me", "him", "her", "us", "them",
        "who", "whom", "whose", "which", "what"
    };

    private static readonly HashSet<string> Conjunctions = new(StringComparer.OrdinalIgnoreCase)
    {
        "and", "but", "or", "nor", "so", "for", "yet",
        "because", "since", "while", "although", "if", "when"
    };

    private static readonly HashSet<string> CommonNouns = new(StringComparer.OrdinalIgnoreCase)
    {
        "human", "person", "man", "woman", "child", "animal", "dog", "cat",
        "mortal", "entity", "object", "thing", "concept", "idea", "fact",
        "socrates", "plato", "aristotle", "alice", "bob", "charlie",
        "philosopher", "student", "teacher", "doctor", "engineer"
    };

    private static readonly HashSet<string> CommonVerbs = new(StringComparer.OrdinalIgnoreCase)
    {
        "eat", "drink", "sleep", "walk", "run", "talk", "think", "know",
        "love", "hate", "see", "hear", "feel", "become", "remain", "live", "die"
    };

    private static readonly HashSet<string> CommonAdjectives = new(StringComparer.OrdinalIgnoreCase)
    {
        "good", "bad", "big", "small", "happy", "sad", "old", "new",
        "young", "ancient", "modern", "smart", "wise", "foolish",
        "beautiful", "ugly", "true", "false", "valid", "invalid"
    };

    #endregion

    /// <summary>
    /// Deterministically tokenize and tag a sentence.
    /// No guesswork - every token gets an exact tag based on dictionary lookup.
    /// </summary>
    public static List<SyntaxTreeNode> TokenizeAndTag(string sentence)
    {
        var tokens = Tokenize(sentence);
        var result = new List<SyntaxTreeNode>();

        foreach (var token in tokens)
        {
            var pos = TagWord(token);
            result.Add(new SyntaxTreeNode(token, pos));
        }

        return result;
    }

    /// <summary>
    /// Simple deterministic tokenization - split on word boundaries.
    /// </summary>
    private static List<string> Tokenize(string sentence)
    {
        // Remove punctuation and split
        var cleaned = new string(sentence.Where(c => char.IsLetterOrDigit(c) || char.IsWhiteSpace(c) || c == '\'').ToArray());
        return cleaned.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(t => t.Trim())
            .Where(t => !string.IsNullOrEmpty(t))
            .ToList();
    }

    /// <summary>
    /// Deterministic POS tagging with priority ordering.
    /// Priority: Copula > Auxiliary > Determiner > Preposition > Pronoun > Conjunction > Noun > Verb > Adjective
    /// </summary>
    private static PartOfSpeech TagWord(string word)
    {
        var lower = word.ToLowerInvariant();

        if (Copulas.Contains(lower)) return PartOfSpeech.Copula;
        if (Auxiliaries.Contains(lower)) return PartOfSpeech.Auxiliary;
        if (Determiners.Contains(lower)) return PartOfSpeech.Determiner;
        if (Prepositions.Contains(lower)) return PartOfSpeech.Preposition;
        if (Pronouns.Contains(lower)) return PartOfSpeech.Pronoun;
        if (Conjunctions.Contains(lower)) return PartOfSpeech.Conjunction;
        if (CommonNouns.Contains(lower)) return PartOfSpeech.Noun;
        if (CommonVerbs.Contains(lower)) return PartOfSpeech.Verb;
        if (CommonAdjectives.Contains(lower)) return PartOfSpeech.Adjective;

        // Default: proper nouns (names) are treated as Noun
        if (char.IsUpper(word[0])) return PartOfSpeech.Noun;

        return PartOfSpeech.Unknown;
    }

    /// <summary>
    /// Extend the vocabulary with domain-specific terms.
    /// </summary>
    public static void AddDomainVocabulary(IEnumerable<string> nouns, IEnumerable<string> verbs, IEnumerable<string> adjectives)
    {
        foreach (var n in nouns) CommonNouns.Add(n.ToLowerInvariant());
        foreach (var v in verbs) CommonVerbs.Add(v.ToLowerInvariant());
        foreach (var a in adjectives) CommonAdjectives.Add(a.ToLowerInvariant());
    }
}
