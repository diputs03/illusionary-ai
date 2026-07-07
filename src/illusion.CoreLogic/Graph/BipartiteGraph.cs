using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace illusion.CoreLogic.Graph;
public abstract class ReadOnlyGraph<Node_T, Edge_T>
    where Node_T : Node
    where Edge_T : Edge
{
    public abstract IReadOnlyDictionary<string, Node_T> Node { get; }
    public abstract IReadOnlyDictionary<string, List<Edge_T>> Edge { get; }
    public abstract IReadOnlyList<Node_T> PrevNodes<TempNode_T>(TempNode_T n) where TempNode_T : Node;
    public class Builder;
}
public abstract class ReadOnlyBipartiteGraph<Self_T, NodeA_T, NodeB_T, EdgeA2B_T, EdgeB2A_T> : ReadOnlyGraph<Node, Edge>
    where Self_T : ReadOnlyBipartiteGraph<Self_T, NodeA_T, NodeB_T, EdgeA2B_T, EdgeB2A_T>
    where NodeA_T : Node
    where NodeB_T : Node
    where EdgeA2B_T : BiEdge<NodeA_T, NodeB_T>
    where EdgeB2A_T : BiEdge<NodeB_T, NodeA_T>
{
    private readonly FrozenDictionary<string, NodeA_T> _anodes;
    private readonly FrozenDictionary<string, NodeB_T> _bnodes;

    private readonly FrozenDictionary<string, List<EdgeA2B_T>> _a2bedges;
    private readonly Dictionary<string, List<NodeA_T>> _ra2bedges; // b to prev-a

    private readonly FrozenDictionary<string, List<EdgeB2A_T>> _b2aedges;
    private readonly Dictionary<string, List<NodeB_T>> _rb2aedges; // a to prev-b
    public bool HasNode<Node_T>(Node_T n) where Node_T : Node
    {
        switch (n)
        {
            case NodeA_T a: return _anodes.ContainsKey(a.Id);
            case NodeB_T b: return _bnodes.ContainsKey(b.Id);
            default: throw new ArgumentException($"Node type {n.GetType().Name} is not supported.");
        }
    }
    public override IReadOnlyDictionary<string, Node> Node
        => _anodes.Select(kv => kv).ToDictionary(kv => kv.Key, kv => kv.Value as Node).Concat(
            _bnodes.Select(kv => kv).ToDictionary(kv => kv.Key, kv => kv.Value as Node)
            ).ToDictionary(kv => kv.Key, kv => kv.Value);
    public override IReadOnlyDictionary<string, List<Edge>> Edge =>
        _a2bedges.Select(kv => kv).ToDictionary(kv => kv.Key, kv => kv.Value.Select(e => e as Edge).ToList()).Concat(
            _b2aedges.Select(kv => kv).ToDictionary(kv => kv.Key, kv => kv.Value.Select(e => e as Edge).ToList())
            ).ToDictionary(kv => kv.Key, kv => kv.Value);
    public override IReadOnlyList<Node> PrevNodes<Node_T>(Node_T n)
    {
        switch (n)
        {
            case NodeA_T a: return _rb2aedges[n.Id];
            case NodeB_T b: return _ra2bedges[n.Id];
            default: throw new ArgumentException($"Node type {n.GetType().Name} is not supported.");
        }
    }

    public ReadOnlyBipartiteGraph(
        FrozenDictionary<string, NodeA_T> anodes,
        FrozenDictionary<string, NodeB_T> bnodes,
        FrozenDictionary<string, List<EdgeA2B_T>> a2bedges,
        FrozenDictionary<string, List<EdgeB2A_T>> b2aedges)
    {
        _anodes = anodes;
        _bnodes = bnodes;
        _a2bedges = a2bedges;
        _b2aedges = b2aedges;
        _ra2bedges = new();
        _rb2aedges = new();

        foreach (var (b, s) in _b2aedges)
        {
            foreach (var e in s)
            {
                if (!_rb2aedges.TryGetValue(e.Value.Id, out var tmp))
                    _rb2aedges.Add(e.Value.Id, new List<NodeB_T>());
                _rb2aedges[e.Value.Id].Add(_bnodes.GetValueOrDefault(b)!);
            }
        }
        foreach (var (a, e) in _a2bedges)
        {
            foreach (var b in e)
            {
                if (!_ra2bedges.TryGetValue(b.Value.Id, out var tmp))
                    _ra2bedges.Add(b.Value.Id, new List<NodeA_T>());
                _ra2bedges[b.Value.Id].Add(_anodes.GetValueOrDefault(a)!);
            }
        }
    }

    public new class Builder
    {
        private readonly Dictionary<string, NodeA_T> _anodes = new();
        private readonly Dictionary<string, NodeB_T> _bnodes = new();
        private readonly Dictionary<string, List<EdgeA2B_T>> _a2bedges = new();
        private readonly Dictionary<string, List<EdgeB2A_T>> _b2aedges = new();

        private readonly Func<
            FrozenDictionary<string, NodeA_T>,
            FrozenDictionary<string, NodeB_T>,
            FrozenDictionary<string, List<EdgeA2B_T>>,
            FrozenDictionary<string, List<EdgeB2A_T>>,
            Self_T> _factory;

        internal Builder(Func<
            FrozenDictionary<string, NodeA_T>,
            FrozenDictionary<string, NodeB_T>,
            FrozenDictionary<string, List<EdgeA2B_T>>,
            FrozenDictionary<string, List<EdgeB2A_T>>,
            Self_T> factory) => _factory = factory;
        public Self_T Build()
        {
            return _factory(
                _anodes.ToFrozenDictionary(),
                _bnodes.ToFrozenDictionary(),
                _a2bedges.ToFrozenDictionary(),
                _b2aedges.ToFrozenDictionary()
            );
        }
        public Builder AddNode<T>(T node) where T : Node
        {
            switch (node)
            {
                case NodeA_T a: _anodes[a.Id] = a; break;
                case NodeB_T b: _bnodes[b.Id] = b; break;
                default: throw new ArgumentException($"Node type {node.GetType().Name} is not supported.");
            }
            return this;
        }
        public Builder AddNodes<T>(IEnumerable<T> nodes) where T : Node
        {
            foreach (var n in nodes) AddNode(n);
            return this;
        }
        public Builder AddEdge<T>(T edge) where T : Edge
        {
            switch (edge)
            {
                case EdgeA2B_T e:
                    if (!_a2bedges.ContainsKey(e.Key.Id))
                        _a2bedges[e.Key.Id] = new List<EdgeA2B_T>();
                    _a2bedges[e.Key.Id].Add(e);
                    return this;
                case EdgeB2A_T e:
                    if (!_b2aedges.ContainsKey(e.Key.Id))
                        _b2aedges[e.Key.Id] = new List<EdgeB2A_T>();
                    _b2aedges[e.Key.Id].Add(e);
                    return this;
                default: throw new ArgumentException($"Edge type {edge.GetType().Name} is not supported.");
            }
        }
        public Builder AddEdges<T>(IEnumerable<T> edges) where T : Edge
        {
            foreach (var e in edges) AddEdge(e);
            return this;
        }
    }
}