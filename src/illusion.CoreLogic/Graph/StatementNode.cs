using illusion.CoreLogic.Kernel;

namespace illusion.CoreLogic.Graph;

/// <summary>Graph node that stores a kernel statement and exposes l = ln(out_degree).</summary>
public sealed record StatementNode(string Id, Statement Statement)
{
    public double LogicalOutDegreeWeight(int outDegree) => outDegree <= 0 ? double.NegativeInfinity : Math.Log(outDegree);

    public GraphNode ToGraphNode() => GraphNode.Create(Id, "Statement", Statement.ToString());
}
