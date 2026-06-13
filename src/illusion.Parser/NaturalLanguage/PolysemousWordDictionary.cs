namespace illusion.Parser.NaturalLanguage;

/// <summary>
/// Comprehensive dictionary entry with all possible parts of speech for a word.
/// Implements deterministic context-based part-of-speech disambiguation.
/// Zero hallucination, 100% rule-based.
/// </summary>
public sealed class DictionaryEntry
{
    public string Word { get; }
    public List<PartOfSpeech> PossiblePartsOfSpeech { get; }
    public Dictionary<string, string> Definitions { get; }
    public List<string> Examples { get; }

    public DictionaryEntry(string word)
    {
        Word = word;
        PossiblePartsOfSpeech = new List<PartOfSpeech>();
        Definitions = new Dictionary<string, string>();
        Examples = new List<string>();
    }

    public void AddPartOfSpeech(PartOfSpeech pos, string definition, string example = "")
    {
        if (!PossiblePartsOfSpeech.Contains(pos))
        {
            PossiblePartsOfSpeech.Add(pos);
        }
        Definitions[pos.ToString()] = definition;
        if (!string.IsNullOrEmpty(example))
        {
            Examples.Add(example);
        }
    }

    public bool IsPolysemous => PossiblePartsOfSpeech.Count > 1;
}

/// <summary>
/// Polysemous Word Dictionary with context-based POS disambiguation.
/// Handles words like 'will' (noun, verb, auxiliary, modal)
/// </summary>
public sealed class PolysemousWordDictionary
{
    private readonly Dictionary<string, DictionaryEntry> _entries = new();

    // Inlined word sets for pattern matching (100% deterministic)
    private static readonly HashSet<string> CommonVerbs = new(StringComparer.OrdinalIgnoreCase)
    {
        "eat", "drink", "sleep", "walk", "run", "talk", "think", "know", "love", "hate",
        "see", "hear", "feel", "become", "remain", "live", "die", "go", "come", "have",
        "do", "make", "take", "give", "get", "say", "tell", "speak", "read", "write",
        "swim", "help", "work", "play", "start", "stop", "begin", "end", "leave", "arrive"
    };

    private static readonly HashSet<string> Determiners = new(StringComparer.OrdinalIgnoreCase)
    {
        "a", "an", "the", "this", "that", "these", "those", "my", "your", "his", "her",
        "its", "our", "their", "some", "any", "each", "every", "all", "both", "few", "many"
    };

    private static readonly HashSet<string> Pronouns = new(StringComparer.OrdinalIgnoreCase)
    {
        "i", "you", "he", "she", "it", "we", "they", "me", "him", "her", "us", "them",
        "who", "whom", "whose", "which", "what"
    };

    private static readonly HashSet<string> CommonAdjectives = new(StringComparer.OrdinalIgnoreCase)
    {
        "good", "bad", "big", "small", "happy", "sad", "old", "new", "young", "ancient",
        "modern", "smart", "wise", "foolish", "beautiful", "ugly", "true", "false", "valid",
        "invalid", "free", "last", "deep", "military", "great"
    };

    private static readonly HashSet<string> Prepositions = new(StringComparer.OrdinalIgnoreCase)
    {
        "in", "on", "at", "by", "for", "with", "about", "against", "between", "into",
        "through", "during", "before", "after", "above", "below", "to", "from", "up", "down", "of", "off"
    };

    private static readonly HashSet<string> Auxiliaries = new(StringComparer.OrdinalIgnoreCase)
    {
        "has", "have", "had", "do", "does", "did", "will", "would", "shall", "should",
        "may", "might", "can", "could", "must"
    };

    private static readonly HashSet<string> CommonNouns = new(StringComparer.OrdinalIgnoreCase)
    {
        "human", "person", "man", "woman", "child", "animal", "dog", "cat", "mortal",
        "entity", "object", "thing", "concept", "idea", "fact", "socrates", "plato",
        "aristotle", "alice", "bob", "charlie", "philosopher", "student", "teacher",
        "doctor", "engineer", "river", "power"
    };

    public PolysemousWordDictionary()
    {
        BuildComprehensiveDictionary();
    }

    private void BuildComprehensiveDictionary()
    {
        // WILL - 5 meanings
        AddEntry("will", PartOfSpeech.Modal, "Modal: future tense", "I will go");
        AddEntry("will", PartOfSpeech.Noun, "Noun: volition/choice", "Free will");
        AddEntry("will", PartOfSpeech.Noun, "Noun: legal document", "Last will");
        AddEntry("will", PartOfSpeech.Verb, "Verb: to desire", "What will you?");
        AddEntry("will", PartOfSpeech.Auxiliary, "Auxiliary: willingness", "Will you help?");

        // CAN - 3 meanings
        AddEntry("can", PartOfSpeech.Modal, "Modal: ability", "I can swim");
        AddEntry("can", PartOfSpeech.Noun, "Noun: metal container", "A can of soda");
        AddEntry("can", PartOfSpeech.Verb, "Verb: preserve food", "Can tomatoes");

        // MAY - 2 meanings
        AddEntry("may", PartOfSpeech.Modal, "Modal: permission", "May I leave?");
        AddEntry("may", PartOfSpeech.Noun, "Noun: month", "In May");

        // MIGHT - 2 meanings
        AddEntry("might", PartOfSpeech.Modal, "Modal: possibility", "It might rain");
        AddEntry("might", PartOfSpeech.Noun, "Noun: strength", "Military might");

        // MUST - 3 meanings
        AddEntry("must", PartOfSpeech.Modal, "Modal: necessity", "You must go");
        AddEntry("must", PartOfSpeech.Noun, "Noun: requirement", "A must for success");
        AddEntry("must", PartOfSpeech.Adjective, "Adjective: required", "Must-have");

        // RUN - 4 meanings
        AddEntry("run", PartOfSpeech.Verb, "Verb: move on foot", "I run daily");
        AddEntry("run", PartOfSpeech.Noun, "Noun: act of running", "A 5K run");
        AddEntry("run", PartOfSpeech.Noun, "Noun: sequence", "A run of luck");
        AddEntry("run", PartOfSpeech.Verb, "Verb: operate", "Machine runs well");

        // BOOK - 2 meanings
        AddEntry("book", PartOfSpeech.Noun, "Noun: written work", "Read a book");
        AddEntry("book", PartOfSpeech.Verb, "Verb: reserve", "Book a flight");

        // LEFT - 3 meanings
        AddEntry("left", PartOfSpeech.Adjective, "Adjective: opposite right", "Left hand");
        AddEntry("left", PartOfSpeech.Verb, "Verb: past of leave", "He left early");
        AddEntry("left", PartOfSpeech.Noun, "Noun: left side", "Turn to the left");

        // RIGHT - 4 meanings
        AddEntry("right", PartOfSpeech.Adjective, "Adjective: opposite left", "Right hand");
        AddEntry("right", PartOfSpeech.Adjective, "Adjective: correct", "Right answer");
        AddEntry("right", PartOfSpeech.Noun, "Noun: entitlement", "Human rights");
        AddEntry("right", PartOfSpeech.Adverb, "Adverb: correctly", "Got it right");

        // FAIR - 3 meanings
        AddEntry("fair", PartOfSpeech.Adjective, "Adjective: just", "Fair decision");
        AddEntry("fair", PartOfSpeech.Noun, "Noun: exhibition", "County fair");
        AddEntry("fair", PartOfSpeech.Adjective, "Adjective: light color", "Fair skin");

        // BANK - 3 meanings
        AddEntry("bank", PartOfSpeech.Noun, "Noun: financial", "Deposit at bank");
        AddEntry("bank", PartOfSpeech.Noun, "Noun: river edge", "River bank");
        AddEntry("bank", PartOfSpeech.Verb, "Verb: deposit money", "Bank the funds");

        // SPRING - 4 meanings
        AddEntry("spring", PartOfSpeech.Noun, "Noun: season", "Spring flowers");
        AddEntry("spring", PartOfSpeech.Noun, "Noun: elastic coil", "Metal spring");
        AddEntry("spring", PartOfSpeech.Verb, "Verb: jump", "Spring into action");
        AddEntry("spring", PartOfSpeech.Noun, "Noun: water source", "Natural spring");

        // WELL - 4 meanings
        AddEntry("well", PartOfSpeech.Adverb, "Adverb: good manner", "Do well");
        AddEntry("well", PartOfSpeech.Noun, "Noun: water hole", "Deep well");
        AddEntry("well", PartOfSpeech.Adjective, "Adjective: healthy", "I feel well");
        AddEntry("well", PartOfSpeech.Interjection, "Interjection", "Well, let's begin");
    }

    private void AddEntry(string word, PartOfSpeech pos, string definition, string example = "")
    {
        var lowerWord = word.ToLowerInvariant();
        if (!_entries.TryGetValue(lowerWord, out var entry))
        {
            entry = new DictionaryEntry(word);
            _entries[lowerWord] = entry;
        }
        entry.AddPartOfSpeech(pos, definition, example);
    }

    public DictionaryEntry? GetEntry(string word) =>
        _entries.TryGetValue(word.ToLowerInvariant(), out var e) ? e : null;

    /// <summary>
    /// Deterministic context-based POS disambiguation.
    /// Zero hallucination - pure syntactic pattern matching.
    /// </summary>
    public PartOfSpeech DisambiguatePOS(string word, List<string> contextWords, int wordPosition)
    {
        var entry = GetEntry(word);
        if (entry == null || !entry.IsPolysemous)
            return entry?.PossiblePartsOfSpeech.FirstOrDefault() ?? PartOfSpeech.Unknown;

        var lowerWord = word.ToLowerInvariant();
        var prevWord = wordPosition > 0 ? contextWords[wordPosition - 1].ToLowerInvariant() : "";
        var nextWord = wordPosition < contextWords.Count - 1 ? contextWords[wordPosition + 1].ToLowerInvariant() : "";

        // ============================================
        // WILL - context rules
        // ============================================
        if (lowerWord == "will")
        {
            if (CommonVerbs.Contains(nextWord)) return PartOfSpeech.Modal;
            if (CommonAdjectives.Contains(prevWord) || Determiners.Contains(prevWord)) return PartOfSpeech.Noun;
            if (Pronouns.Contains(nextWord) && wordPosition == 0) return PartOfSpeech.Auxiliary;
            return PartOfSpeech.Modal;
        }

        // ============================================
        // CAN - context rules
        // ============================================
        if (lowerWord == "can")
        {
            if (Determiners.Contains(prevWord)) return PartOfSpeech.Noun;
            if (CommonVerbs.Contains(nextWord)) return PartOfSpeech.Modal;
            return PartOfSpeech.Modal;
        }

        // ============================================
        // MAY - context rules
        // ============================================
        if (lowerWord == "may")
        {
            if (Prepositions.Contains(prevWord)) return PartOfSpeech.Noun;
            if (CommonVerbs.Contains(nextWord) || Pronouns.Contains(nextWord)) return PartOfSpeech.Modal;
            return PartOfSpeech.Modal;
        }

        // ============================================
        // MIGHT - context rules
        // ============================================
        if (lowerWord == "might")
        {
            if (CommonAdjectives.Contains(prevWord)) return PartOfSpeech.Noun;
            return PartOfSpeech.Modal;
        }

        // ============================================
        // LEFT - context rules
        // ============================================
        if (lowerWord == "left")
        {
            if (Pronouns.Contains(prevWord) || CommonNouns.Contains(prevWord)) return PartOfSpeech.Verb;
            if (Determiners.Contains(prevWord)) return PartOfSpeech.Adjective;
            return PartOfSpeech.Adjective;
        }

        // ============================================
        // WELL - context rules
        // ============================================
        if (lowerWord == "well")
        {
            if (wordPosition == 0) return PartOfSpeech.Interjection;
            if (CommonVerbs.Contains(prevWord)) return PartOfSpeech.Adverb;
            if (Determiners.Contains(prevWord) || CommonAdjectives.Contains(prevWord)) return PartOfSpeech.Noun;
            return PartOfSpeech.Adverb;
        }

        // ============================================
        // BOOK - context rules
        // ============================================
        if (lowerWord == "book")
        {
            if (Pronouns.Contains(prevWord) || Auxiliaries.Contains(prevWord)) return PartOfSpeech.Verb;
            return PartOfSpeech.Noun;
        }

        // Fallback: most common POS
        return entry.PossiblePartsOfSpeech.First();
    }

    /// <summary>
    /// Full analysis with confidence scores
    /// </summary>
    public (PartOfSpeech BestPOS, List<(PartOfSpeech POS, float Confidence, string Reason)> Candidates) 
        AnalyzeWordWithContext(string word, List<string> contextWords, int wordPosition)
    {
        var entry = GetEntry(word);
        if (entry == null)
            return (PartOfSpeech.Unknown, new List<(PartOfSpeech, float, string)>());

        var bestPOS = DisambiguatePOS(word, contextWords, wordPosition);
        var candidates = new List<(PartOfSpeech, float, string)>();

        foreach (var pos in entry.PossiblePartsOfSpeech)
        {
            var confidence = pos == bestPOS ? 0.95f : 0.05f;
            var reason = entry.Definitions.TryGetValue(pos.ToString(), out var def) ? def : "";
            candidates.Add((pos, confidence, reason));
        }

        return (bestPOS, candidates);
    }

    public List<string> GetAllPolysemousWords() =>
        _entries.Where(kv => kv.Value.IsPolysemous).Select(kv => kv.Key).OrderBy(x => x).ToList();
}
