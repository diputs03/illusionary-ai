using illusion.CoreLogic.IPK_Adapter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace illusion.CoreLogic.Graph;

public class Edge
{
    private KeyValuePair<Node, Node> _inner;
    public Edge(Node from, Node to) => _inner = new KeyValuePair<Node, Node>(from, to);
    public Node Key => _inner.Key;
    public Node Value => _inner.Value;
}

public class BiEdge<NodeA_T, NodeB_T> : Edge
    where NodeA_T : Node
    where NodeB_T : Node
{
    public BiEdge(NodeA_T from, NodeB_T to) : base(from, to) { }
    public new NodeA_T Key => (NodeA_T)base.Key;
    public new NodeB_T Value => (NodeB_T)base.Value;
}