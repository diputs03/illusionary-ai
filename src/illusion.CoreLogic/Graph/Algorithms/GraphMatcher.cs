using System.Security.Cryptography;
using System.Text;

namespace illusion.CoreLogic.Graph.Algorithms;

/// <summary>
/// Tree isomorphism and subgraph matching algorithms
/// For pattern matching in knowledge graphs: find proof patterns, rule instances
/// </summary>
public static class GraphMatcher
{
    #region Tree Isomorphism (AHU Algorithm)

    /// <summary>
    /// Check if two rooted trees are isomorphic
    /// Using AHU (Aho-Hopcroft-Ullman) algorithm with canonical hashing
    /// </summary>
    public static bool AreRootedTreesIsomorphic(
        KnowledgeGraph g1, string root1,
        KnowledgeGraph g2, string root2)
    {
        var hash1 = ComputeTreeCanonicalHash(g1, root1);
        var hash2 = ComputeTreeCanonicalHash(g2, root2);
        return hash1 == hash2;
    }

    /// <summary>
    /// Compute canonical hash for rooted tree (deterministic, isomorphism-invariant)
    /// Two trees are isomorphic iff they have the same canonical hash
    /// </summary>
    public static string ComputeTreeCanonicalHash(KnowledgeGraph graph, string root)
    {
        var labelMap = new Dictionary<string, string>();
        ComputeHashRecursive(graph, root, new HashSet<string>(), labelMap);
        return labelMap[root];
    }

    private static void ComputeHashRecursive(
        KnowledgeGraph graph,
        string node,
        HashSet<string> visited,
        Dictionary<string, string> labelMap)
    {
        if (visited.Contains(node)) return;
        visited.Add(node);

        var children = graph.GetNeighbors(node)
            .Where(n => !visited.Contains(n))
            .ToList();

        // Recursively compute children first (post-order)
        foreach (var child in children)
            ComputeHashRecursive(graph, child, visited, labelMap);

        // Sort children's labels for canonical form (order doesn't matter)
        var childLabels = children.Select(c => labelMap[c]).OrderBy(l => l).ToList();

        // Compute canonical label: nodeType + sorted(childLabels)
        var nodeObj = graph.GetNode(node);
        var nodeType = nodeObj?.Type.ToString() ?? "Unknown";

        var canonical = $"{nodeType}({string.Join(",", childLabels)})";
        labelMap[node] = Sha256Hash(canonical);
    }

    #endregion

    #region Subgraph Isomorphism (Ullmann's Algorithm)

    /// <summary>
    /// Find all occurrences of pattern graph within target graph
    /// Uses Ullmann's algorithm for subgraph isomorphism
    /// Critical for rule matching: find where inference rules apply
    /// </summary>
    public static List<Dictionary<string, string>> FindSubgraphIsomorphisms(
        KnowledgeGraph pattern,
        KnowledgeGraph target)
    {
        var results = new List<Dictionary<string, string>>();
        var patternNodes = pattern.Nodes.Select(n => n.Id).ToList();
        var targetNodes = target.Nodes.Select(n => n.Id).ToList();

        // Initial candidate mapping: pattern node -> compatible target nodes
        var candidates = new Dictionary<string, List<string>>();
        foreach (var pNode in patternNodes)
        {
            var pType = pattern.GetNode(pNode)?.Type;
            candidates[pNode] = targetNodes
                .Where(t => target.GetNode(t)?.Type == pType)
                .ToList();
        }

        var mapping = new Dictionary<string, string>();
        BacktrackMatch(0, patternNodes, pattern, target, candidates, mapping, results);
        return results;
    }

    private static void BacktrackMatch(
        int depth,
        List<string> patternNodes,
        KnowledgeGraph pattern,
        KnowledgeGraph target,
        Dictionary<string, List<string>> candidates,
        Dictionary<string, string> mapping,
        List<Dictionary<string, string>> results)
    {
        if (depth == patternNodes.Count)
        {
            // Found complete mapping - verify all edges
            if (VerifyMapping(pattern, target, mapping))
                results.Add(new Dictionary<string, string>(mapping));
            return;
        }

        var pNode = patternNodes[depth];
        foreach (var tNode in candidates[pNode])
        {
            if (mapping.Values.Contains(tNode))
                continue; // Already mapped

            mapping[pNode] = tNode;

            // Prune: check partial mapping consistency
            if (IsPartialMappingValid(pattern, target, mapping, depth))
                BacktrackMatch(depth + 1, patternNodes, pattern, target, candidates, mapping, results);

            mapping.Remove(pNode);
        }
    }

    private static bool IsPartialMappingValid(
        KnowledgeGraph pattern,
        KnowledgeGraph target,
        Dictionary<string, string> mapping,
        int depth)
    {
        // Check all edges between already-mapped nodes
        var mapped = mapping.Keys.ToList();
        foreach (var from in mapped)
        {
            foreach (var to in mapped)
            {
                var hasPatternEdge = pattern.GetOutEdges(from).Any(e => e.ToId == to);
                var hasTargetEdge = target.GetOutEdges(mapping[from]).Any(e => e.ToId == mapping[to]);

                if (hasPatternEdge != hasTargetEdge)
                    return false;
            }
        }
        return true;
    }

    private static bool VerifyMapping(
        KnowledgeGraph pattern,
        KnowledgeGraph target,
        Dictionary<string, string> mapping)
    {
        // Verify every edge in pattern exists in target
        foreach (var pNode in pattern.Nodes.Select(n => n.Id))
        {
            foreach (var edge in pattern.GetOutEdges(pNode))
            {
                var hasTargetEdge = target.GetOutEdges(mapping[pNode])
                    .Any(e => e.ToId == mapping[edge.ToId] && e.Type == edge.Type);

                if (!hasTargetEdge)
                    return false;
            }
        }
        return true;
    }

    #endregion

    #region Rule Pattern Matching

    /// <summary>
    /// Find all places where an inference rule can be applied
    /// Pattern: A → B (implication edge)
    /// Returns all (A, B) pairs where rule matches
    /// </summary>
    public static List<(string Premise, string Conclusion)> FindRuleApplications(
        KnowledgeGraph graph,
        string rulePatternNode)
    {
        var results = new List<(string, string)>();

        // Find all implication edges matching the rule pattern
        foreach (var node in graph.Nodes)
        {
            foreach (var edge in graph.GetOutEdges(node.Id)
                .Where(e => e.Type == "Implies"))
            {
                results.Add((node.Id, edge.ToId));
            }
        }
        return results;
    }

    /// <summary>
    /// Find all modus ponens opportunities
    /// Pattern: We have A, and A→B, therefore we can derive B
    /// Returns all derivable conclusions with their proof paths
    /// </summary>
    public static List<ModusPensMatch> FindModusPonensOpportunities(KnowledgeGraph graph)
    {
        var results = new List<ModusPensMatch>();
        var existingFacts = new HashSet<string>(graph.Nodes
            .Where(n => n.Type is "Predicate" or "Axiom")
            .Select(n => n.Id));

        foreach (var fact in existingFacts)
        {
            foreach (var edge in graph.GetOutEdges(fact)
                .Where(e => e.Type == "Implies"))
            {
                var conclusion = edge.ToId;
                if (!existingFacts.Contains(conclusion))
                {
                    results.Add(new ModusPensMatch
                    {
                        Premise = fact,
                        Conclusion = conclusion,
                        ProofPath = new List<string> { fact, conclusion }
                    });
                }
            }
        }
        return results;
    }

    #endregion

    #region Common Subtree Extraction

    /// <summary>
    /// Find maximum common subtree between two graphs
    /// Useful for proof analogy: find similar proof structures
    /// </summary>
    public static List<string> FindMaximumCommonSubtree(
        KnowledgeGraph g1, string root1,
        KnowledgeGraph g2, string root2)
    {
        // BFS from both roots, find largest matching subtree
        var visited1 = new HashSet<string>();
        var visited2 = new HashSet<string>();
        var common = new List<string>();

        var queue = new Queue<(string, string)>();
        queue.Enqueue((root1, root2));

        while (queue.Count > 0)
        {
            var (n1, n2) = queue.Dequeue();
            if (visited1.Contains(n1) || visited2.Contains(n2))
                continue;

            // Check if nodes are compatible
            var type1 = g1.GetNode(n1)?.Type;
            var type2 = g2.GetNode(n2)?.Type;
            if (type1 != type2)
                continue;

            visited1.Add(n1);
            visited2.Add(n2);
            common.Add(n1);

            // Match children by degree
            var children1 = g1.GetNeighbors(n1).Where(c => !visited1.Contains(c)).ToList();
            var children2 = g2.GetNeighbors(n2).Where(c => !visited2.Contains(c)).ToList();

            // Greedy matching by type
            foreach (var c1 in children1)
            {
                var c1Type = g1.GetNode(c1)?.Type;
                var match = children2.FirstOrDefault(c2 =>
                    g2.GetNode(c2)?.Type == c1Type && !visited2.Contains(c2));

                if (match != null)
                    queue.Enqueue((c1, match));
            }
        }
        return common;
    }

    #endregion

    #region Helpers

    private static string Sha256Hash(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToBase64String(bytes).Substring(0, 16);
    }

    #endregion
}

/// <summary>
/// Result of modus ponens pattern matching
/// </summary>
public class ModusPensMatch
{
    public string Premise { get; set; } = "";
    public string Conclusion { get; set; } = "";
    public List<string> ProofPath { get; set; } = new();
    public string RuleName { get; set; } = "";
}
