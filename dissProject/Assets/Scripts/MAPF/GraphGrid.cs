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
    public delegate void RefreshGrid(Node node);
    public static RefreshGrid refreshGrid;
    Dictionary<Vector2, Node> _nodeDict = new Dictionary<Vector2, Node>();
    Vector2[] _dirs = { new Vector2(0, 5), new Vector2(5, 0)};
    AStarManager aStarManager = new AStarManager();
    BidirectionalGraph<Node,TaggedUndirectedEdge<Node,int>> _gridGraph = new BidirectionalGraph<Node, TaggedUndirectedEdge<Node, int>>();

    private void Start()
    {
        GetNodesInChildren();
        AddNodesToGraph();
        AddEdgesToGraph();
        RenderEdges();
        Debug.Log(_gridGraph.EdgeCount);
        AStarAlgorithm();
    }

    private void OnEnable()
    {
        refreshGrid += OnGridRefresh;
    }
    private void OnDisable()
    {
        refreshGrid -= OnGridRefresh;
    }
    private void GetNodesInChildren()
    {
        _nodes = GetComponentsInChildren<Node>();
    }

    private void OnGridRefresh(Node node)
    {
        if (node.nodeType.Equals(NodeTypeEnum.NOT_WALKABLE))
        {
            _gridGraph.ClearEdges(node);
            _gridGraph.RemoveVertex(node);
            _nodeDict[node.position] = node;
            node._nodeMarker.ToggleMarker(false);
        }
        else if (node.nodeType.Equals(NodeTypeEnum.WALKABLE))
        {
            _gridGraph.AddVertex(node);
            _nodeDict[node.position] = node;
            node._nodeMarker.ToggleMarker(true);
            foreach (Vector2 dir in _dirs)
            {
                if (_nodeDict.TryGetValue(node.position + dir, out Node value))
                {
                    if (value.nodeType.Equals(NodeTypeEnum.WALKABLE))
                    {
                        _gridGraph.AddEdge(new TaggedUndirectedEdge<Node, int>(node, value, 5));
                    }
                }
                if (_nodeDict.TryGetValue(node.position - dir, out Node value2))
                {
                    if (value2.nodeType.Equals(NodeTypeEnum.WALKABLE))
                    {
                        _gridGraph.AddEdge(new TaggedUndirectedEdge<Node, int>(value2, node, 5));
                    }
                }

            }
        }
        foreach(LineRenderer child in _renderedEdgesParent.GetComponentsInChildren<LineRenderer>())
        {
            Destroy(child.gameObject);
        }
        RenderEdges();
    }
    private void AddNodesToGraph()
    {
        foreach (Node node in _nodes)
        {
            if (node.nodeType == NodeTypeEnum.WALKABLE)
            {
                _gridGraph.AddVertex(node);
                node._nodeMarker = Instantiate(_nodeMarker, node.transform).GetComponent<GridMarker>();

                _nodeDict.Add(node.position, node);
            }
        }
    }
    private void RenderEdges()
    {
        foreach (TaggedUndirectedEdge<Node, int> edge in _gridGraph.Edges)
        {
            LineRenderer lineRenderer = Instantiate(_edgeRenderer, _renderedEdgesParent).GetComponent<LineRenderer>();
            lineRenderer.SetPositions(new Vector3[2] { edge.Source.position, edge.Target.position });
        }
    }

    private void AddEdgesToGraph()
    {
        foreach (Node node in _gridGraph.Vertices)
        {
            foreach (Vector2 dir in _dirs)
            {
                if (_nodeDict.TryGetValue(node.position + dir, out Node value) && value.nodeType.Equals(NodeTypeEnum.WALKABLE))
                {
                    _gridGraph.AddEdge(new TaggedUndirectedEdge<Node, int>(node, value, 5));
                }
            }
        }
    }

    private void AStarAlgorithm()
    {
        aStarManager.AttachGraph(_gridGraph);
        aStarManager.ComputeAStarPath(_nodes[0], _nodes[21]);
    }
}
