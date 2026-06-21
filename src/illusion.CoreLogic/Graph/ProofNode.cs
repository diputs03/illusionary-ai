namespace illusion.CoreLogic.Graph;

/// <summary>
/// Hyper-edge-like proof node: premise statement nodes point into the proof node,
/// and the proof node has one outgoing edge to its conclusion statement node.
/// </summary>
public sealed record ProofNode(string Id, IReadOnlyList<string> PremiseIds, string ConclusionId, string RuleName)
{
    public GraphNode ToGraphNode() => GraphNode.Create(Id, "Proof", RuleName);

    public IEnumerable<GraphEdge> ToEdges()
    {
        foreach (var premise in PremiseIds)
            yield return GraphEdge.Create(premise, Id, "PremiseOf", "premise");
        yield return GraphEdge.Create(Id, ConclusionId, "Concludes", "conclusion");
    }
}
