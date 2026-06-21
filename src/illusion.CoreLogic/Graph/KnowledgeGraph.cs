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
