namespace illusion.Parser.NaturalLanguage;

/// <summary>
/// Dynamic English tokenizer and POS tagger.
/// NO HARDCODED WORDS - uses Dictionary API and dynamic polysemy resolution.
/// Aligns with IPK design philosophy: all knowledge comes from rules, not baked-in data.
/// </summary>
public static class EnglishTokenizer
{
    private static readonly DynamicPolysemousDictionary _dictionary = new();
    private static bool _initialized = false;

    /// <summary>
    /// Initialize the tokenizer with dynamic dictionary.
    /// Preloads common function words from API.
    /// </summary>
    public static async Task InitializeAsync()
    {
        if (_initialized) return;
        await _dictionary.PreloadCommonWords();
        _initialized = true;
    }

    /// <summary>
    /// Deterministically tokenize and tag a sentence.
    /// Uses dynamic dictionary API - no hardcoded words.
    /// </summary>
    public static async Task<List<SyntaxTreeNode>> TokenizeAndTagAsync(string sentence)
    {
        if (!_initialized) await InitializeAsync();

        var tokens = Tokenize(sentence);
        var result = new List<SyntaxTreeNode>();

        for (int i = 0; i < tokens.Count; i++)
        {
            var token = tokens[i];
            var pos = await _dictionary.DisambiguatePOS(token, tokens, i);
            result.Add(new SyntaxTreeNode(token, pos));
        }

        return result;
    }

    /// <summary>
    /// Synchronous version for backward compatibility.
    /// </summary>
    public static List<SyntaxTreeNode> TokenizeAndTag(string sentence)
    {
        return TokenizeAndTagAsync(sentence).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Simple deterministic tokenization - split on word boundaries.
    /// </summary>
    private static List<string> Tokenize(string sentence)
    {
        var cleaned = new string(sentence.Where(c => 
            char.IsLetterOrDigit(c) || char.IsWhiteSpace(c) || c == '\'').ToArray());
        
        return cleaned.Split(new[] { ' ', '\t', '\n', '\r' }, 
            StringSplitOptions.RemoveEmptyEntries)
            .Select(t => t.Trim())
            .Where(t => !string.IsNullOrEmpty(t))
            .ToList();
    }
}
