using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QuikGraph;

public class GraphGrid : MonoBehaviour
{
    [SerializeField] Node[] _nodes;
    Vector2[] _dirs = { new Vector2(0, 1), new Vector2(0, -1), new Vector2(1, 0), new Vector2(-1, 0) };
    UndirectedGraph<Node,TaggedUndirectedEdge<Node,int>> _gridGraph;

    private void Start()
    {
        foreach (Node node in _nodes)
        {
            _gridGraph.AddVertex(node);
        }
        Debug.Log(_gridGraph.VertexCount);
    }

}
