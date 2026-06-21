using System.Collections.Frozen;
using System.Text;

namespace illusion.CoreLogic.Graph;

/// <summary>
/// Immutable directed labeled knowledge graph
/// Core data structure for all reasoning operations
/// </summary>
public class KnowledgeGraph
{
    private readonly FrozenDictionary<string, GraphNode> _nodes;
    private readonly FrozenDictionary<string, List<GraphEdge>> _outEdges;  // from -> edges
    private readonly FrozenDictionary<string, List<GraphEdge>> _inEdges;   // to -> edges

    public IReadOnlyCollection<GraphNode> Nodes => _nodes.Values;
    public IReadOnlyCollection<GraphEdge> Edges => _outEdges.Values.SelectMany(e => e).ToList();
    public int NodeCount => _nodes.Count;
    public int EdgeCount => _outEdges.Values.Sum(e => e.Count);

    private KnowledgeGraph(
        FrozenDictionary<string, GraphNode> nodes,
        FrozenDictionary<string, List<GraphEdge>> outEdges,
        FrozenDictionary<string, List<GraphEdge>> inEdges)
    {
        _nodes = nodes;
        _outEdges = outEdges;
        _inEdges = inEdges;
    }

    /// <summary>
    /// Get node by ID
    /// </summary>
    public GraphNode? GetNode(string id) => _nodes.TryGetValue(id, out var n) ? n : null;

    /// <summary>
    /// Get all outgoing edges from a node
    /// </summary>
    public List<GraphEdge> GetOutEdges(string nodeId)
        => _outEdges.TryGetValue(nodeId, out var edges) ? edges : new List<GraphEdge>();

    /// <summary>
    /// Get all incoming edges to a node
    /// </summary>
    public List<GraphEdge> GetInEdges(string nodeId)
        => _inEdges.TryGetValue(nodeId, out var edges) ? edges : new List<GraphEdge>();

    /// <summary>
    /// Get all neighbors (reachable in one step)
    /// </summary>
    public List<string> GetNeighbors(string nodeId)
        => GetOutEdges(nodeId).Select(e => e.ToId).Distinct().ToList();

    /// <summary>
    /// Get predecessors (nodes pointing to this node)
    /// </summary>
    public List<string> GetPredecessors(string nodeId)
        => GetInEdges(nodeId).Select(e => e.FromId).Distinct().ToList();

    /// <summary>
    /// Get in-degree (number of incoming edges)
    /// </summary>
    public int GetInDegree(string nodeId)
        => GetInEdges(nodeId).Count;

    /// <summary>
    /// Get out-degree (number of outgoing edges)
    /// </summary>
    public int GetOutDegree(string nodeId)
        => GetOutEdges(nodeId).Count;

    /// <summary>
    /// Check if graph contains a node with given ID
    /// </summary>
    public bool HasNode(string nodeId)
        => _nodes.ContainsKey(nodeId);

    /// <summary>
    /// Enumerate statement nodes whose l = ln(out_degree) is at least the
    /// requested threshold. Isolated statement nodes have l = -infinity.
    /// </summary>
    public IReadOnlyList<GraphNode> GetStatementNodesByLogicalOutDegree(double minimumL)
        => _nodes.Values
            .Where(n => string.Equals(n.Type, "Statement", StringComparison.Ordinal))
            .Where(n =>
            {
                var outDegree = GetOutDegree(n.Id);
                var l = outDegree <= 0 ? double.NegativeInfinity : Math.Log(outDegree);
                return l >= minimumL;
            })
            .OrderBy(n => n.Id, StringComparer.Ordinal)
            .ToList();

    /// <summary>
    /// Verify that every proof node has all declared premise in-edges and
    /// exactly one conclusion out-edge. This structural verifier intentionally
    /// stays rule-agnostic; rule semantics belong to kernel/prover adapters.
    /// </summary>
    public bool VerifyProofStructure()
    {
        foreach (var proof in _nodes.Values.Where(n => string.Equals(n.Type, "Proof", StringComparison.Ordinal)))
        {
            var premises = GetInEdges(proof.Id).Where(e => e.Type == "PremiseOf").ToList();
            var conclusions = GetOutEdges(proof.Id).Where(e => e.Type == "Concludes").ToList();
            if (premises.Count == 0 || conclusions.Count != 1)
                return false;
            if (premises.Any(e => !_nodes.ContainsKey(e.FromId)) || !_nodes.ContainsKey(conclusions[0].ToId))
                return false;
        }

        return true;
    }

    /// <summary>
    /// Compress a topologically-near node set into adjacent directed balls B(x,d).
    /// A greedy deterministic set-cover is used so the serialized shape is
    /// compact and stable across runs.
    /// </summary>
    public IReadOnlyList<(string CenterId, int Distance)> CompressAsAdjacentBalls(IEnumerable<string> nodeIds, int maximumDistance)
    {
        ArgumentNullException.ThrowIfNull(nodeIds);
        if (maximumDistance < 0)
            throw new ArgumentOutOfRangeException(nameof(maximumDistance));

        var remaining = nodeIds.Where(HasNode).ToHashSet(StringComparer.Ordinal);
        var balls = new Dictionary<(string CenterId, int Distance), HashSet<string>>();
        foreach (var node in _nodes.Keys.OrderBy(id => id, StringComparer.Ordinal))
        {
            for (var distance = 0; distance <= maximumDistance; distance++)
                balls[(node, distance)] = DirectedBall(node, distance).Where(remaining.Contains).ToHashSet(StringComparer.Ordinal);
        }

        var result = new List<(string CenterId, int Distance)>();
        while (remaining.Count > 0)
        {
            var best = balls
                .Where(kv => kv.Value.Overlaps(remaining))
                .OrderByDescending(kv => kv.Value.Count(remaining.Contains))
                .ThenBy(kv => kv.Key.Distance)
                .ThenBy(kv => kv.Key.CenterId, StringComparer.Ordinal)
                .First();

            result.Add(best.Key);
            remaining.ExceptWith(best.Value);
        }

        return result;
    }

    public IReadOnlySet<string> DirectedBall(string centerId, int distance)
    {
        if (!HasNode(centerId))
            return new HashSet<string>(StringComparer.Ordinal);
        if (distance < 0)
            throw new ArgumentOutOfRangeException(nameof(distance));

        var visited = new HashSet<string>(StringComparer.Ordinal) { centerId };
        var queue = new Queue<(string Id, int Depth)>();
        queue.Enqueue((centerId, 0));
        while (queue.Count > 0)
        {
            var (id, depth) = queue.Dequeue();
            if (depth == distance)
                continue;
            foreach (var next in GetNeighbors(id).Concat(GetPredecessors(id)))
            {
                if (visited.Add(next))
                    queue.Enqueue((next, depth + 1));
            }
        }

        return visited;
    }

    /// <summary>
    /// Builder for constructing immutable graphs
    /// </summary>
    public class Builder
    {
        private readonly Dictionary<string, GraphNode> _nodes = new();
        private readonly Dictionary<string, List<GraphEdge>> _outEdges = new();
        private readonly Dictionary<string, List<GraphEdge>> _inEdges = new();

        public Builder AddNode(GraphNode node)
        {
            _nodes[node.Id] = node;
            return this;
        }

        public Builder AddEdge(GraphEdge edge)
        {
            if (!_outEdges.ContainsKey(edge.FromId))
                _outEdges[edge.FromId] = new List<GraphEdge>();
            if (!_inEdges.ContainsKey(edge.ToId))
                _inEdges[edge.ToId] = new List<GraphEdge>();

            _outEdges[edge.FromId].Add(edge);
            _inEdges[edge.ToId].Add(edge);
            return this;
        }

        public Builder AddNodes(IEnumerable<GraphNode> nodes)
        {
            foreach (var n in nodes) AddNode(n);
            return this;
        }

        public Builder AddEdges(IEnumerable<GraphEdge> edges)
        {
            foreach (var e in edges) AddEdge(e);
            return this;
        }

        public KnowledgeGraph Build()
        {
            return new KnowledgeGraph(
                _nodes.ToFrozenDictionary(),
                _outEdges.ToFrozenDictionary(kv => kv.Key, kv => kv.Value.ToList()),
                _inEdges.ToFrozenDictionary(kv => kv.Key, kv => kv.Value.ToList()));
        }
    }

    public static Builder CreateBuilder() => new();

    /// <summary>
    /// Export to DOT format for visualization
    /// </summary>
    public string ToDot()
    {
        var sb = new StringBuilder();
        sb.AppendLine("digraph KnowledgeGraph {");
        sb.AppendLine("  node [shape=box, style=filled, fontname=Arial];");
        
        foreach (var node in _nodes.Values)
        {
            var color = node.Type switch
            {
                "Object" => "lightblue",
                "Predicate" => "lightgreen",
                "Rule" => "gold",
                "Axiom" => "orange",
                "Contradiction" => "red",
                _ => "white"
            };
            sb.AppendLine($"  \"{node.Id}\" [label=\"{node.Label}\", fillcolor=\"{color}\"];");
        }

        foreach (var (from, edges) in _outEdges)
        {
            foreach (var edge in edges)
            {
                var style = edge.Type switch
                {
                    "Implies" => "color=blue, penwidth=2",
                    "Contradicts" => "color=red, style=dashed",
                    "SubclassOf" => "color=darkgreen",
                    _ => "color=gray"
                };
                sb.AppendLine($"  \"{edge.FromId}\" -> \"{edge.ToId}\" [label=\"{edge.Label}\", {style}];");
            }
        }

        sb.AppendLine("}");
        return sb.ToString();
    }
}
