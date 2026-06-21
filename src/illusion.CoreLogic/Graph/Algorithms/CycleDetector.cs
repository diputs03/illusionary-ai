namespace illusion.CoreLogic.Graph.Algorithms;

/// <summary>
/// Cycle detection and topological sorting for directed graphs
/// Critical for proof consistency: circular reasoning = logical fallacy
/// </summary>
public static class CycleDetector
{
    /// <summary>
    /// Detect if graph contains any cycles
    /// Uses DFS-based coloring algorithm: O(V+E)
    /// </summary>
    public static bool HasCycle(KnowledgeGraph graph)
        => FindCycle(graph) != null;

    /// <summary>
    /// Find a cycle in the graph (returns null if acyclic)
    /// Returns list of node IDs forming the cycle
    /// </summary>
    public static List<string>? FindCycle(KnowledgeGraph graph)
    {
        var visited = new HashSet<string>();
        var recursionStack = new HashSet<string>();
        var path = new Stack<string>();

        foreach (var node in graph.Nodes.Select(n => n.Id))
        {
            if (DfsCycle(node, graph, visited, recursionStack, path, out var cycle))
                return cycle;
        }
        return null;
    }

    private static bool DfsCycle(
        string current,
        KnowledgeGraph graph,
        HashSet<string> visited,
        HashSet<string> recursionStack,
        Stack<string> path,
        out List<string> cycle)
    {
        cycle = null!;

        if (recursionStack.Contains(current))
        {
            // Found cycle: reconstruct from recursion stack
            cycle = path.Reverse().SkipWhile(n => n != current).ToList();
            cycle.Add(current); // Close the cycle
            return true;
        }

        if (visited.Contains(current))
            return false;

        visited.Add(current);
        recursionStack.Add(current);
        path.Push(current);

        foreach (var neighbor in graph.GetNeighbors(current))
        {
            if (DfsCycle(neighbor, graph, visited, recursionStack, path, out cycle))
                return true;
        }

        recursionStack.Remove(current);
        path.Pop();
        return false;
    }

    /// <summary>
    /// Kahn's algorithm for topological sort
    /// Returns topological order OR null if graph has cycle
    /// Critical for proof ordering: premises must come before conclusions
    /// </summary>
    public static List<string>? TopologicalSort(KnowledgeGraph graph)
    {
        var inDegree = new Dictionary<string, int>();
        foreach (var node in graph.Nodes.Select(n => n.Id))
            inDegree[node] = graph.GetInEdges(node).Count;

        var queue = new Queue<string>();
        foreach (var node in inDegree.Where(kv => kv.Value == 0))
            queue.Enqueue(node.Key);

        var result = new List<string>();
        var visitedCount = 0;

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            result.Add(current);
            visitedCount++;

            foreach (var neighbor in graph.GetNeighbors(current))
            {
                inDegree[neighbor]--;
                if (inDegree[neighbor] == 0)
                    queue.Enqueue(neighbor);
            }
        }

        // If not all nodes visited, there's a cycle
        return visitedCount == graph.NodeCount ? result : null;
    }

    /// <summary>
    /// Check if proof DAG is valid (no circular reasoning)
    /// In formal logic: A cannot be derived from B if B is derived from A
    /// </summary>
    public static bool IsValidProofDAG(KnowledgeGraph proofGraph)
    {
        // Filter to only DerivedFrom edges (proof derivation)
        var derivationEdges = proofGraph.Nodes
            .SelectMany(n => proofGraph.GetOutEdges(n.Id))
            .Where(e => e.Type == "DerivedFrom")
            .ToList();

        if (!derivationEdges.Any())
            return true; // Trivially valid

        // Build derivation subgraph
        var builder = KnowledgeGraph.CreateBuilder();
        foreach (var node in proofGraph.Nodes)
            builder.AddNode(node);
        foreach (var edge in derivationEdges)
            builder.AddEdge(edge);

        var derivationGraph = builder.Build();
        return !HasCycle(derivationGraph);
    }

    /// <summary>
    /// Find strongly connected components (SCCs) using Tarjan's algorithm
    /// Each SCC is a maximal cycle
    /// </summary>
    public static List<List<string>> FindStronglyConnectedComponents(KnowledgeGraph graph)
    {
        var index = 0;
        var stack = new Stack<string>();
        var indices = new Dictionary<string, int>();
        var lowlink = new Dictionary<string, int>();
        var onStack = new HashSet<string>();
        var sccs = new List<List<string>>();

        foreach (var node in graph.Nodes.Select(n => n.Id))
        {
            if (!indices.ContainsKey(node))
                StrongConnect(node, graph, ref index, stack, indices, lowlink, onStack, sccs);
        }

        return sccs;
    }

    private static void StrongConnect(
        string v,
        KnowledgeGraph graph,
        ref int index,
        Stack<string> stack,
        Dictionary<string, int> indices,
        Dictionary<string, int> lowlink,
        HashSet<string> onStack,
        List<List<string>> sccs)
    {
        indices[v] = index;
        lowlink[v] = index;
        index++;
        stack.Push(v);
        onStack.Add(v);

        foreach (var w in graph.GetNeighbors(v))
        {
            if (!indices.ContainsKey(w))
            {
                StrongConnect(w, graph, ref index, stack, indices, lowlink, onStack, sccs);
                lowlink[v] = Math.Min(lowlink[v], lowlink[w]);
            }
            else if (onStack.Contains(w))
            {
                lowlink[v] = Math.Min(lowlink[v], indices[w]);
            }
        }

        if (lowlink[v] == indices[v])
        {
            var scc = new List<string>();
            string w;
            do
            {
                w = stack.Pop();
                onStack.Remove(w);
                scc.Add(w);
            } while (w != v);
            sccs.Add(scc);
        }
    }

    /// <summary>
    /// Get all nodes reachable from start (transitive closure)
    /// </summary>
    public static HashSet<string> ReachableFrom(KnowledgeGraph graph, string start)
    {
        var visited = new HashSet<string>();
        var queue = new Queue<string>();
        queue.Enqueue(start);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (visited.Contains(current)) continue;
            visited.Add(current);

            foreach (var neighbor in graph.GetNeighbors(current))
            {
                if (!visited.Contains(neighbor))
                    queue.Enqueue(neighbor);
            }
        }
        return visited;
    }

    /// <summary>
    /// Find shortest path using BFS
    /// </summary>
    public static List<string>? ShortestPath(KnowledgeGraph graph, string from, string to)
    {
        var parent = new Dictionary<string, string>();
        var visited = new HashSet<string>();
        var queue = new Queue<string>();
        queue.Enqueue(from);
        visited.Add(from);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (current == to)
            {
                // Reconstruct path
                var path = new List<string>();
                var node = to;
                while (node != null)
                {
                    path.Add(node);
                    node = parent.TryGetValue(node, out var p) ? p : null;
                }
                path.Reverse();
                return path;
            }

            foreach (var neighbor in graph.GetNeighbors(current))
            {
                if (!visited.Contains(neighbor))
                {
                    visited.Add(neighbor);
                    parent[neighbor] = current;
                    queue.Enqueue(neighbor);
                }
            }
        }
        return null;
    }
}
