using illusion.CoreLogic.IPK_Adapter;
using illusion.Common.Utils;
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
    /// Tree isomorphism check for rooted trees using canonical labeling (AHU algorithm)
    /// </summary>
    /// <param name="g1"></param>
    /// <param name="root1"></param>
    /// <param name="g2"></param>
    /// <param name="root2"></param>
    /// <returns></returns>
    public static bool AreRootedTreesIsomorphic(
        KnowledgeGraph g1, string root1,
        KnowledgeGraph g2, string root2)
    {
        var hash1 = ComputeTreeCanonicalHash(g1, root1);
        var hash2 = ComputeTreeCanonicalHash(g2, root2);
        return hash1 == hash2;
    }
    /// <summary>
    /// Compute canonical hash for a rooted tree using post-order traversal and hashing
    /// </summary>
    /// <param name="graph"></param>
    /// <param name="root"></param>
    /// <returns></returns>
    public static string ComputeTreeCanonicalHash(KnowledgeGraph graph, string root)
    {
        var labelMap = new Dictionary<string, string>();
        ComputeHashRecursive(graph, root, new HashSet<string>(), labelMap);
        return labelMap[root];
    }
    /// <summary>
    /// Hash computation for a node in the tree, recursively computing children's hashes first (post-order)
    /// </summary>
    /// <param name="graph"></param>
    /// <param name="node"></param>
    /// <param name="visited"></param>
    /// <param name="labelMap"></param>
    private static void ComputeHashRecursive(
        KnowledgeGraph graph,
        string node,
        HashSet<string> visited,
        Dictionary<string, string> labelMap)
    {
        if (visited.Contains(node)) return;
        visited.Add(node);

        var children = graph.Edge[node]
            .Where(n => !visited.Contains(n.Value.Id))
            .Select(n => n.Value.Id)
            .ToList();

        // Recursively compute children first (post-order)
        foreach (var child in children)
            ComputeHashRecursive(graph, child, visited, labelMap);

        // Sort children's labels for canonical form (order doesn't matter)
        var childLabels = children.Select(c => labelMap[c]).OrderBy(l => l).ToList();

        // Compute canonical label: nodeType + sorted(childLabels)
        var nodeType = graph.Node[node].GetType()?.ToString() ?? "Unknown";

        var canonical = $"{nodeType}({string.Join(",", childLabels)})";
        labelMap[node] = Convert.ToBase64String(CryptoUtils.ComputeSha256(Encoding.UTF8.GetBytes(canonical)));
    }

    #endregion

    #region Subgraph Isomorphism (Ullmann's Algorithm)
    /// <summary>
    /// Subgraph isomorphism check using backtracking (Ullmann's algorithm)
    /// </summary>
    /// <param name="pattern"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    public static List<Dictionary<string, string>> FindSubgraphIsomorphisms(
        KnowledgeGraph pattern,
        KnowledgeGraph target)
    {
        var patternNodes = pattern.Node.Values.ToList();
        var targetNodes = target.Node.Values.ToList();

        // Initial candidate mapping: pattern node -> compatible target nodes
        var candidates = new Dictionary<string, List<Node>>();
        foreach (var pNode in patternNodes)
        {
            candidates[pNode.Id] = targetNodes
                .Where(t => target.Node[t.Id]?.GetType() == pattern.Node[pNode.Id]?.GetType())
                .ToList();
        }

        var results = new List<Dictionary<string, string>>();
        var mapping = new Dictionary<string, string>();
        BacktrackMatch(0, patternNodes, pattern, target, candidates, mapping, results);
        return results;
    }
    /// <summary>
    /// Backtracking search for subgraph isomorphism
    /// </summary>
    /// <param name="depth"></param>
    /// <param name="patternNodes"></param>
    /// <param name="pattern"></param>
    /// <param name="target"></param>
    /// <param name="candidates"></param>
    /// <param name="mapping"></param>
    /// <param name="results"></param>
    private static void BacktrackMatch(
        int depth,
        List<Node> patternNodes,
        KnowledgeGraph pattern,
        KnowledgeGraph target,
        Dictionary<string, List<Node>> candidates,
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
        foreach (var tNode in candidates[pNode.Id])
        {
            if (mapping.Values.Contains(tNode.Id))
                continue; // Already mapped

            mapping[pNode.Id] = tNode.Id;

            // Prune: check partial mapping consistency
            if (IsPartialMappingValid(pattern, target, mapping, depth))
                BacktrackMatch(depth + 1, patternNodes, pattern, target, candidates, mapping, results);

            mapping.Remove(pNode.Id);
        }
    }
    /// <summary>
    /// Check if the current partial mapping is consistent with the edges in the pattern and target graphs
    /// </summary>
    /// <param name="pattern"></param>
    /// <param name="target"></param>
    /// <param name="mapping"></param>
    /// <param name="depth"></param>
    /// <returns></returns>
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
                var hasPatternEdge = pattern.Edge[from].Any(e => e.Value.Id == to);
                var hasTargetEdge = target.Edge[mapping[from]].Any(e => e.Value.Id == mapping[to]);

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
        foreach (var pNode in pattern.Node.Values.Select(n => n.Id))
        {
            foreach (var edge in pattern.Edge[pNode])
            {
                var hasTargetEdge = target.Edge[mapping[pNode]]
                    .Any(e => e.Key.Id == mapping[edge.Key.Id] && e.GetType() == edge.GetType());

                if (!hasTargetEdge)
                    return false;
            }
        }
        return true;
    }
    #endregion

    #region Common Subtree Extraction
    /// <summary>
    /// Find the maximum common subtree between two rooted trees (not necessarily isomorphic)
    /// </summary>
    /// <param name="g1"></param>
    /// <param name="root1"></param>
    /// <param name="g2"></param>
    /// <param name="root2"></param>
    /// <returns></returns>
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
            if (g1.Node[n1]?.GetType() != g2.Node[n2]?.GetType())
                continue;

            visited1.Add(n1);
            visited2.Add(n2);
            common.Add(n1);

            // Match children by degree
            var children1 = g1.Edge[n1].Where(c => !visited1.Contains(c.Value.Id)).ToList();
            var children2 = g2.Edge[n2].Where(c => !visited2.Contains(c.Value.Id)).ToList();

            // Greedy matching by type
            foreach (var c1 in children1)
            {
                var c1Type = g1.Node[c1.Value.Id]?.GetType();
                var match = children2.FirstOrDefault(c2 =>
                    g2.Node[c2.Value.Id]?.GetType() == c1Type && !visited2.Contains(c2.Value.Id));

                if (match != null)
                    queue.Enqueue((c1.Value.Id, match.Value.Id));
            }
        }
        return common;
    }

    #endregion
}