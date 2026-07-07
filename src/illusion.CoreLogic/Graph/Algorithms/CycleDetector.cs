using illusion.CoreLogic.IPK_Adapter;

namespace illusion.CoreLogic.Graph.Algorithms;

/// <summary>
/// cycle, topological sort, and strongly connected components, reachable from, shortest path
/// </summary>
public static class CycleDetector
{
    /// <summary>
    /// Cycle detection using DFS
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="graph"></param>
    /// <returns></returns>
    public static bool HasCycle<T>(T graph) where T : ReadOnlyGraph<Node, Edge>
    {
        var visited = new HashSet<string>();
        var recStack = new HashSet<string>();
        bool DFS(Node node)
        {
            if (recStack.Contains(node.Id))
                return true;
            if (visited.Contains(node.Id))
                return false;
            visited.Add(node.Id);
            recStack.Add(node.Id);
            foreach (var neighbor in graph.Edge[node.Id])
            {
                if (DFS(neighbor.Value))
                    return true;
            }
            recStack.Remove(node.Id);
            return false;
        }
        foreach (var node in graph.Node.Values)
        {
            if (DFS(node))
                return true;
        }
        return false;
    }
    /// <summary>
    /// Strongly connected components (SCCs) using Tarjan's algorithm
    /// </summary>
    /// <param name="graph"></param>
    /// <returns></returns>
    public static List<List<string>> FindStronglyConnectedComponents(KnowledgeGraph graph)
    {
        var index = 0;
        var stack = new Stack<string>();
        var indices = new Dictionary<string, int>();
        var lowlink = new Dictionary<string, int>();
        var onStack = new HashSet<string>();
        var sccs = new List<List<string>>();

        foreach (var node in graph.Node.Values.Select(n => n.Id))
        {
            if (!indices.ContainsKey(node))
                StrongConnect(node, graph, ref index, stack, indices, lowlink, onStack, sccs);
        }

        return sccs;
    }
    /// <summary>
    /// Strongly connected components (SCCs) using Tarjan's algorithm
    /// </summary>
    /// <param name="v"></param>
    /// <param name="graph"></param>
    /// <param name="index"></param>
    /// <param name="stack"></param>
    /// <param name="indices"></param>
    /// <param name="lowlink"></param>
    /// <param name="onStack"></param>
    /// <param name="sccs"></param>
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

        foreach (var w in graph.Edge[graph.Node[v].Id])
        {
            if (!indices.ContainsKey(w.Value.Id))
            {
                StrongConnect(w.Value.Id, graph, ref index, stack, indices, lowlink, onStack, sccs);
                lowlink[v] = Math.Min(lowlink[v], lowlink[w.Value.Id]);
            }
            else if (onStack.Contains(w.Value.Id))
            {
                lowlink[v] = Math.Min(lowlink[v], indices[w.Value.Id]);
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
    /// Connectiveness: Find all nodes reachable from a given node using BFS
    /// </summary>
    /// <param name="graph"></param>
    /// <param name="start"></param>
    /// <returns></returns>
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

            foreach (var neighbor in graph.Edge[graph.Node[current].Id])
            {
                if (!visited.Contains(neighbor.Value.Id))
                    queue.Enqueue(neighbor.Value.Id);
            }
        }
        return visited;
    }
    /// <summary>
    /// Shortest path using BFS (unweighted graph)
    /// </summary>
    /// <param name="graph"></param>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <returns></returns>
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

            foreach (var neighbor in graph.Edge[graph.Node[current].Id])
            {
                if (!visited.Contains(neighbor.Value.Id))
                {
                    visited.Add(neighbor.Value.Id);
                    parent[neighbor.Value.Id] = current;
                    queue.Enqueue(neighbor.Value.Id);
                }
            }
        }
        return null;
    }
}
