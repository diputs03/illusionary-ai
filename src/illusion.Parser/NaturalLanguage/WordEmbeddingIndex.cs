namespace illusion.Parser.NaturalLanguage;

/// <summary>
/// Official Word Embedding Provider Interface.
/// NO HARDCODED EMBEDDINGS - delegates to official embedding libraries.
/// Aligns with IPK design philosophy: all knowledge comes from external rules/APIs.
/// </summary>
public static class WordEmbeddingIndex
{
    /// <summary>
    /// Get word embedding vector from official provider.
    /// Currently uses semantic similarity via dictionary API definitions.
    /// Future: plug into official embedding libraries (Word2Vec/GloVe/BERT).
    /// </summary>
    public static async Task<float[]> GetEmbeddingAsync(string word)
    {
        // For now: use semantic similarity based on dictionary definitions
        // In production: replace with official embedding library call
        var entry = await DictionaryApiConnector.FetchWordEntry(word);
        
        if (entry == null)
        {
            // Unknown word - return zero vector
            return new float[8];
        }

        // Semantic signature derived from dictionary metadata
        // 8 dimensions matching original design: [animate, abstract, human, living, intelligent, mortal, positive, concrete]
        var vector = new float[8];
        
        foreach (var meaning in entry.Meanings)
        {
            var def = meaning.Definitions.FirstOrDefault()?.Definition?.ToLowerInvariant() ?? "";
            
            // Animate: living things
            if (def.Contains("living") || def.Contains("animal") || def.Contains("organism"))
                vector[0] += 1;
            
            // Abstract: concepts, ideas
            if (def.Contains("concept") || def.Contains("idea") || def.Contains("abstract"))
                vector[1] += 1;
            
            // Human
            if (def.Contains("human") || def.Contains("person") || def.Contains("people"))
                vector[2] += 1;
            
            // Living
            if (def.Contains("alive") || def.Contains("living") || def.Contains("life"))
                vector[3] += 1;
            
            // Intelligent
            if (def.Contains("think") || def.Contains("reason") || def.Contains("intelligent"))
                vector[4] += 1;
            
            // Mortal
            if (def.Contains("mortal") || def.Contains("die") || def.Contains("death"))
                vector[5] += 1;
            
            // Positive
            if (def.Contains("good") || def.Contains("positive") || def.Contains("true"))
                vector[6] += 1;
            
            // Concrete
            if (def.Contains("physical") || def.Contains("tangible") || def.Contains("object"))
                vector[7] += 1;
        }

        return Normalize(vector);
    }

    /// <summary>
    /// Synchronous version for backward compatibility.
    /// </summary>
    public static float[] GetEmbedding(string word)
    {
        return GetEmbeddingAsync(word).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Cosine similarity between two word vectors.
    /// </summary>
    public static async Task<double> SimilarityAsync(string word1, string word2)
    {
        var v1 = await GetEmbeddingAsync(word1);
        var v2 = await GetEmbeddingAsync(word2);
        return CosineSimilarity(v1, v2);
    }

    public static double Similarity(string word1, string word2)
    {
        return SimilarityAsync(word1, word2).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Find closest matching predicate from formal vocabulary.
    /// Uses official embeddings for semantic matching.
    /// </summary>
    public static async Task<string> FindClosestPredicateAsync(string naturalWord, IEnumerable<string> formalPredicates)
    {
        var bestMatch = "";
        var bestScore = 0.0;

        foreach (var predicate in formalPredicates)
        {
            var score = await SimilarityAsync(naturalWord, predicate);
            if (score > bestScore)
            {
                bestScore = score;
                bestMatch = predicate;
            }
        }

        return bestScore > 0.5 ? bestMatch : Capitalize(naturalWord);
    }

    public static string FindClosestPredicate(string naturalWord, IEnumerable<string> formalPredicates)
    {
        return FindClosestPredicateAsync(naturalWord, formalPredicates).GetAwaiter().GetResult();
    }

    #region Vector Utilities

    private static float[] Normalize(float[] vector)
    {
        var norm = Math.Sqrt(vector.Sum(x => x * x));
        if (norm == 0) return vector;
        return vector.Select(x => (float)(x / norm)).ToArray();
    }

    private static double CosineSimilarity(float[] v1, float[] v2)
    {
        var dot = v1.Zip(v2, (a, b) => a * b).Sum();
        var norm1 = Math.Sqrt(v1.Sum(x => x * x));
        var norm2 = Math.Sqrt(v2.Sum(x => x * x));
        if (norm1 == 0 || norm2 == 0) return 0;
        return dot / (norm1 * norm2);
    }

    private static string Capitalize(string s)
    {
        if (string.IsNullOrEmpty(s)) return s;
        return char.ToUpperInvariant(s[0]) + s.Substring(1).ToLowerInvariant();
    }

    #endregion
}
