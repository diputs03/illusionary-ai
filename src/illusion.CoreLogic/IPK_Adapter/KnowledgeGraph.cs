using illusion.Common.Types;
using illusion.CoreLogic.Graph;
using illusion.CoreLogic.Prover;
using System.Text.Json.Nodes;
using System.Collections.Frozen;
using System.Text;
using illusion.CoreLogic.Kernel;


namespace illusion.CoreLogic.IPK_Adapter;

public class TNode<Statement_T, LNode_T> : Node
    where Statement_T : IStatement<Statement_T>
    where LNode_T : Node
{
    public Statement_T Statement { get; set; }
    public new JsonObject ToJson()
    {
        var obj = base.ToJson();
        obj["statement"] = Statement?.ToString();
        return obj;
    }
}

public class LNode<Statement_T, TNode_T> : Node
    where Statement_T : IStatement<Statement_T>
    where TNode_T : Node
{
    public List<LNode<Statement_T, TNode_T>> Comp = new List<LNode<Statement_T, TNode_T>>();
    public new JsonObject ToJson()
    {
        var obj = base.ToJson();
        return obj;
    }
}
public class EdgeL2T : BiEdge<LNode, TNode>
{
    public EdgeL2T(LNode from, TNode to) : base(from, to) { }
}
public class EdgeT2L : BiEdge<TNode, LNode>
{
    public EdgeT2L(TNode from, LNode to) : base(from, to) { }
}
public class TNode : TNode<AST, LNode>;
public class LNode : LNode<AST, TNode>;
public class KnowledgeGraph : ReadOnlyBipartiteGraph<KnowledgeGraph, TNode, LNode, EdgeT2L, EdgeL2T>
{
    public KnowledgeGraph(
        FrozenDictionary<string, TNode> truthNodes,
        FrozenDictionary<string, LNode> linkNodes,
        FrozenDictionary<string, List<EdgeT2L>> t2ledges,
        FrozenDictionary<string, List<EdgeL2T>> l2tedges)
        : base(truthNodes, linkNodes, t2ledges, l2tedges) { }
    public static Builder CreateBuilder() => new Builder((a, b, ab, ba) =>
        new KnowledgeGraph(a, b, ab, ba));
}
