using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QuikGraph;

public class GraphGrid : MonoBehaviour
{
    [SerializeField] Node[] _nodes;
    [SerializeField] GameObject _nodeMarker;
    [SerializeField] GameObject _edgeRenderer;
    [SerializeField] Transform _renderedEdgesParent;
    Dictionary<Vector2, Node> _nodeDict = new Dictionary<Vector2, Node>();
    Vector2[] _dirs = { new Vector2(0, 5), new Vector2(5, 0)};
    UndirectedGraph<Node,TaggedUndirectedEdge<Node,int>> _gridGraph = new UndirectedGraph<Node, TaggedUndirectedEdge<Node, int>>();

    private void Start()
    {
        AddNodesToGraph();
        AddEdgesToGraph();
        Debug.Log(_gridGraph.EdgeCount);
    }

    private void AddNodesToGraph()
    {
        foreach (Node node in _nodes)
        {
            if (node.nodeType == NodeTypeEnum.WALKABLE)
            {
                _gridGraph.AddVertex(node);
                Instantiate(_nodeMarker, node.transform);

                _nodeDict.Add(node._position, node);
            }
        }
    }

    private void AddEdgesToGraph()
    {
        foreach (Node node in _gridGraph.Vertices)
        {
            foreach (Vector2 dir in _dirs)
            {
                if (_nodeDict.TryGetValue(node._position + dir, out Node value))
                {
                    _gridGraph.AddEdge(new TaggedUndirectedEdge<Node, int>(node, value, 5));
                }
            }
        }
    }

}
