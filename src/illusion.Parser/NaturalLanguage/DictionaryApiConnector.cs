using System.Net.Http.Json;
using System.Text.Json;
using System.Collections.Frozen;

namespace illusion.Parser.NaturalLanguage;

/// <summary>
/// Connector to authoritative dictionary APIs (Free Dictionary API / Wiktionary)
/// 100% deterministic, no hallucination, zero ML
/// </summary>
public class DictionaryApiConnector
{
    private static readonly HttpClient _httpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(10)
    };
    
    /// <summary>
    /// Static singleton instance for global access
    /// </summary>
    public static DictionaryApiConnector Instance { get; } = new();
    
    /// <summary>
    /// Static wrapper for FetchWordEntry
    /// </summary>
    public static Task<DictionaryWordEntry?> FetchWordEntry(string word)
        => Instance.FetchWordEntryInternal(word);

    private const string FreeDictionaryApi = "https://api.dictionaryapi.dev/api/v2/entries/en/{0}";
    private const string WiktionaryApi = "https://en.wiktionary.org/api/rest_v1/page/definition/{0}";

    /// <summary>
    /// Fetch complete dictionary entry from Free Dictionary API
    /// Returns all parts of speech, definitions, and examples for a word
    /// </summary>
    private async Task<DictionaryWordEntry?> FetchWordEntryInternal(string word)
    {
        try
        {
            var url = string.Format(FreeDictionaryApi, word.ToLowerInvariant());
            var response = await _httpClient.GetFromJsonAsync<JsonElement[]>(url);
            
            if (response == null || response.Length == 0)
                return null;

            var entry = response[0];
            var result = new DictionaryWordEntry
            {
                Word = entry.GetProperty("word").GetString() ?? word,
                Phonetic = entry.TryGetProperty("phonetic", out var p) ? p.GetString() : null,
                Meanings = new List<WordMeaning>()
            };

            if (entry.TryGetProperty("meanings", out var meanings))
            {
                foreach (var meaning in meanings.EnumerateArray())
                {
                    var posText = meaning.GetProperty("partOfSpeech").GetString() ?? "unknown";
                    var pos = MapPartOfSpeech(posText);
                    
                    var wordMeaning = new WordMeaning
                    {
                        PartOfSpeech = pos,
                        PartOfSpeechText = posText,
                        Definitions = new List<WordDefinition>()
                    };

                    if (meaning.TryGetProperty("definitions", out var defs))
                    {
                        foreach (var def in defs.EnumerateArray())
                        {
                            wordMeaning.Definitions.Add(new WordDefinition
                            {
                                Definition = def.GetProperty("definition").GetString() ?? "",
                                Example = def.TryGetProperty("example", out var ex) ? ex.GetString() : null
                            });
                        }
                    }

                    if (meaning.TryGetProperty("synonyms", out var syns))
                    {
                        wordMeaning.Synonyms = syns.EnumerateArray()
                            .Select(s => s.GetString()!)
                            .Where(s => s != null)
                            .ToList();
                    }

                    if (meaning.TryGetProperty("antonyms", out var ants))
                    {
                        wordMeaning.Antonyms = ants.EnumerateArray()
                            .Select(a => a.GetString()!)
                            .Where(a => a != null)
                            .ToList();
                    }

                    result.Meanings.Add(wordMeaning);
                }
            }

            return result;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Get all possible parts of speech for a word (polysemy detection)
    /// </summary>
    public async Task<List<PartOfSpeech>> GetPossiblePartsOfSpeech(string word)
    {
        var entry = await FetchWordEntryInternal(word);
        return entry?.Meanings.Select(m => m.PartOfSpeech).Distinct().ToList() 
               ?? new List<PartOfSpeech>();
    }

    /// <summary>
    /// Check if a word is polysemous (has multiple parts of speech)
    /// </summary>
    public async Task<bool> IsPolysemous(string word)
    {
        var poses = await GetPossiblePartsOfSpeech(word);
        return poses.Count > 1;
    }

    /// <summary>
    /// Get definitions for a specific part of speech
    /// </summary>
    public async Task<List<string>> GetDefinitions(string word, PartOfSpeech pos)
    {
        var entry = await FetchWordEntryInternal(word);
        return entry?.Meanings
                   .Where(m => m.PartOfSpeech == pos)
                   .SelectMany(m => m.Definitions.Select(d => d.Definition))
                   .ToList() 
               ?? new List<string>();
    }

    /// <summary>
    /// Get examples for a word + part of speech
    /// </summary>
    public async Task<List<string>> GetExamples(string word, PartOfSpeech pos)
    {
        var entry = await FetchWordEntryInternal(word);
        return entry?.Meanings
                   .Where(m => m.PartOfSpeech == pos)
                   .SelectMany(m => m.Definitions.Select(d => d.Example))
                   .Where(e => e != null)
                   .ToList()! 
               ?? new List<string>();
    }

    /// <summary>
    /// Map API part of speech string to our enum
    /// </summary>
    private static PartOfSpeech MapPartOfSpeech(string posText)
    {
        return posText.ToLowerInvariant() switch
        {
            "noun" => PartOfSpeech.Noun,
            "verb" => PartOfSpeech.Verb,
            "adjective" => PartOfSpeech.Adjective,
            "adverb" => PartOfSpeech.Adverb,
            "preposition" => PartOfSpeech.Preposition,
            "pronoun" => PartOfSpeech.Pronoun,
            "conjunction" => PartOfSpeech.Conjunction,
            "determiner" => PartOfSpeech.Determiner,
            "interjection" => PartOfSpeech.Interjection,
            "modal" => PartOfSpeech.Modal,
            "auxiliary" => PartOfSpeech.Auxiliary,
            "auxiliary verb" => PartOfSpeech.Auxiliary,
            _ => PartOfSpeech.Unknown
        };
    }

    /// <summary>
    /// Batch fetch multiple words (with rate limiting)
    /// </summary>
    public async Task<Dictionary<string, DictionaryWordEntry>> BatchFetch(IEnumerable<string> words)
    {
        var result = new Dictionary<string, DictionaryWordEntry>();
        foreach (var word in words.Distinct())
        {
            var entry = await FetchWordEntryInternal(word);
            if (entry != null)
                result[word] = entry;
            await Task.Delay(100); // Rate limiting
        }
        return result;
    }
}

/// <summary>
/// Complete dictionary entry for a word
/// </summary>
public class DictionaryWordEntry
{
    public string Word { get; set; } = "";
    public string? Phonetic { get; set; }
    public List<WordMeaning> Meanings { get; set; } = new();
    public bool IsPolysemous => Meanings.Select(m => m.PartOfSpeech).Distinct().Count() > 1;
    public FrozenSet<PartOfSpeech> PossiblePartsOfSpeech => Meanings.Select(m => m.PartOfSpeech).Distinct().ToFrozenSet();
}

/// <summary>
/// Meaning of a word for a specific part of speech
/// </summary>
public class WordMeaning
{
    public PartOfSpeech PartOfSpeech { get; set; }
    public string PartOfSpeechText { get; set; } = "";
    public List<WordDefinition> Definitions { get; set; } = new();
    public List<string> Synonyms { get; set; } = new();
    public List<string> Antonyms { get; set; } = new();
}

/// <summary>
/// Single definition with example
/// </summary>
public class WordDefinition
{
    public string Definition { get; set; } = "";
    public string? Example { get; set; }
}
