using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QuikGraph;

public class GraphGrid : MonoBehaviour
{
    [SerializeField] Node[] _nodes;
    [SerializeField] GameObject _nodeMarker;
    Vector2[] _dirs = { new Vector2(0, 5), new Vector2(0, -5), new Vector2(5, 0), new Vector2(-5, 0) };
    UndirectedGraph<Node,TaggedUndirectedEdge<Node,int>> _gridGraph = new UndirectedGraph<Node, TaggedUndirectedEdge<Node, int>>();

    private void Start()
    {
        foreach (Node node in _nodes)
        {
            Debug.Log(node._position);
            _gridGraph.AddVertex(node);
            Instantiate(_nodeMarker, node.transform);
        }
        Debug.Log(_gridGraph.VertexCount);
    }

}
