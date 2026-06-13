namespace illusion.Parser.NaturalLanguage;

/// <summary>
/// Deterministic word embedding index for semantic similarity.
/// Uses pre-computed, lightweight semantic vectors.
/// No training at runtime - 100% deterministic lookup.
/// </summary>
public sealed class WordEmbeddingIndex
{
    private readonly Dictionary<string, float[]> _embeddings = new(StringComparer.OrdinalIgnoreCase);
    private const int Dimensions = 8; // Lightweight 8D semantic space

    /// <summary>
    /// Initialize with built-in semantic categories.
    /// Vectors are hand-crafted for deterministic similarity.
    /// </summary>
    public WordEmbeddingIndex()
    {
        InitializeBuiltInEmbeddings();
    }

    /// <summary>
    /// Get embedding vector for a word.
    /// Returns zero vector if word not found.
    /// </summary>
    public float[] GetEmbedding(string word)
    {
        if (_embeddings.TryGetValue(word.ToLowerInvariant(), out var vec))
        {
            return (float[])vec.Clone();
        }
        return new float[Dimensions];
    }

    /// <summary>
    /// Cosine similarity between two words.
    /// Deterministic - same inputs always give same output.
    /// </summary>
    public float Similarity(string word1, string word2)
    {
        var vec1 = GetEmbedding(word1);
        var vec2 = GetEmbedding(word2);
        return CosineSimilarity(vec1, vec2);
    }

    /// <summary>
    /// Find the most semantically similar predicate from candidates.
    /// Used for mapping natural language to formal predicates.
    /// </summary>
    public string FindClosestPredicate(string naturalWord, IEnumerable<string> formalPredicates)
    {
        var bestMatch = formalPredicates.First();
        var bestScore = -1f;

        foreach (var predicate in formalPredicates)
        {
            var score = Similarity(naturalWord, predicate);
            if (score > bestScore)
            {
                bestScore = score;
                bestMatch = predicate;
            }
        }

        return bestMatch;
    }

    private static float CosineSimilarity(float[] a, float[] b)
    {
        float dot = 0, normA = 0, normB = 0;
        for (var i = 0; i < Dimensions; i++)
        {
            dot += a[i] * b[i];
            normA += a[i] * a[i];
            normB += b[i] * b[i];
        }

        if (normA == 0 || normB == 0) return 0;
        return dot / (MathF.Sqrt(normA) * MathF.Sqrt(normB));
    }

    /// <summary>
    /// Initialize deterministic embeddings based on semantic categories.
    /// Each dimension represents a semantic feature:
    /// [animate, abstract, human, living, intelligent, mortal, positive, concrete]
    /// </summary>
    private void InitializeBuiltInEmbeddings()
    {
        // Category: Humans/Persons
        AddEmbedding("human", 1, 0, 1, 1, 1, 1, 0, 1);
        AddEmbedding("person", 1, 0, 1, 1, 1, 1, 0, 1);
        AddEmbedding("man", 1, 0, 1, 1, 1, 1, 0, 1);
        AddEmbedding("woman", 1, 0, 1, 1, 1, 1, 0, 1);
        AddEmbedding("child", 1, 0, 1, 1, 1, 1, 0, 1);
        AddEmbedding("socrates", 1, 0, 1, 1, 1, 1, 0, 1);
        AddEmbedding("plato", 1, 0, 1, 1, 1, 1, 0, 1);
        AddEmbedding("aristotle", 1, 0, 1, 1, 1, 1, 0, 1);
        AddEmbedding("philosopher", 1, 0, 1, 1, 1, 1, 0, 1);

        // Category: Mortality
        AddEmbedding("mortal", 1, 0, 0, 1, 0, 1, 0, 0);
        AddEmbedding("immortal", 1, 0, 0, 1, 0, 0, 0, 0);
        AddEmbedding("die", 1, 0, 0, 1, 0, 1, -1, 0);
        AddEmbedding("live", 1, 0, 0, 1, 0, 1, 1, 0);

        // Category: Animals
        AddEmbedding("animal", 1, 0, 0, 1, 0, 1, 0, 1);
        AddEmbedding("dog", 1, 0, 0, 1, 0, 1, 0, 1);
        AddEmbedding("cat", 1, 0, 0, 1, 0, 1, 0, 1);

        // Category: Attributes
        AddEmbedding("wise", 0, 0, 0, 0, 1, 0, 1, 0);
        AddEmbedding("smart", 0, 0, 0, 0, 1, 0, 1, 0);
        AddEmbedding("intelligent", 0, 0, 0, 0, 1, 0, 1, 0);
        AddEmbedding("good", 0, 0, 0, 0, 0, 0, 1, 0);
        AddEmbedding("bad", 0, 0, 0, 0, 0, 0, -1, 0);
        AddEmbedding("true", 0, 1, 0, 0, 0, 0, 1, 0);
        AddEmbedding("false", 0, 1, 0, 0, 0, 0, -1, 0);

        // Category: Abstract concepts
        AddEmbedding("idea", 0, 1, 0, 0, 0, 0, 0, 0);
        AddEmbedding("concept", 0, 1, 0, 0, 0, 0, 0, 0);
        AddEmbedding("fact", 0, 1, 0, 0, 0, 0, 1, 0);
        AddEmbedding("truth", 0, 1, 0, 0, 0, 0, 1, 0);

        // Formal predicates (for mapping)
        AddEmbedding("Human", 1, 0, 1, 1, 1, 1, 0, 1);
        AddEmbedding("Mortal", 1, 0, 0, 1, 0, 1, 0, 0);
        AddEmbedding("Animal", 1, 0, 0, 1, 0, 1, 0, 1);
        AddEmbedding("Wise", 0, 0, 0, 0, 1, 0, 1, 0);
        AddEmbedding("Living", 1, 0, 0, 1, 0, 1, 1, 0);
    }

    private void AddEmbedding(string word, params float[] values)
    {
        _embeddings[word.ToLowerInvariant()] = values;
    }

    /// <summary>
    /// Add custom domain-specific embeddings.
    /// </summary>
    public void AddDomainEmbedding(string word, params float[] values)
    {
        if (values.Length != Dimensions)
            Array.Resize(ref values, Dimensions);
        _embeddings[word.ToLowerInvariant()] = values;
    }
}
