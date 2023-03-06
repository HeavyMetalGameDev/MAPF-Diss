using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QuikGraph;
using System.Linq;

public class GraphGrid : MonoBehaviour
{
    [SerializeField] Node[] _nodes;
    [SerializeField] GameObject _nodeMarker;
    [SerializeField] GameObject _edgeRenderer;
    [SerializeField] Transform _renderedEdgesParent;
    [SerializeField] MAPFAgent[] _MAPFAgents;
    public delegate void RefreshGrid(Node node);
    public static RefreshGrid refreshGrid;
    Dictionary<Vector2, Node> _nodeDict = new Dictionary<Vector2, Node>();
    Vector2[] _dirs = { new Vector2(0, 5), new Vector2(5, 0)};
    AStarManager aStarManager = new AStarManager();
    BidirectionalGraph<Node, UndirectedEdge<Node>> _gridGraph = new BidirectionalGraph<Node, UndirectedEdge<Node>>(true);

    public delegate void AgentArrived(MAPFAgent agent);
    public static AgentArrived agentArrived;

    private void Start()
    {
        GetNodesInChildren();
        AddNodesToGraph();
        AddEdgesToGraph();
        CreateAllRenderEdges();
        Debug.Log(_gridGraph.EdgeCount);
        AStarAlgorithmAllAgents();
    }

    private void OnEnable()
    {
        refreshGrid += OnGridRefresh;
        agentArrived += NewDestinationAgent;
    }
    private void OnDisable()
    {
        refreshGrid -= OnGridRefresh;
        agentArrived -= NewDestinationAgent;
    }
    private void GetNodesInChildren()
    {
        _nodes = GetComponentsInChildren<Node>();
    }

    private void OnGridRefresh(Node node)
    {
        if (node.nodeType.Equals(NodeTypeEnum.NOT_WALKABLE))
        {
            RemoveNodeAndEdges(node);
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
                        _gridGraph.AddEdge(new UndirectedEdge<Node>(node, value));
                    }
                }
                if (_nodeDict.TryGetValue(node.position - dir, out Node value2))
                {
                    if (value2.nodeType.Equals(NodeTypeEnum.WALKABLE))
                    {
                        _gridGraph.AddEdge(new UndirectedEdge<Node>(value2, node));
                    }
                }

            }
        }
        foreach(LineRenderer child in _renderedEdgesParent.GetComponentsInChildren<LineRenderer>())
        {
            Destroy(child.gameObject);
        }
        CreateAllRenderEdges();
        AStarAlgorithmAllAgents();
    }

    private void RemoveNodeAndEdges(Node node)
    {
        _gridGraph.ClearEdges(node);
        _gridGraph.RemoveVertex(node);
        _nodeDict[node.position] = node;
        node._nodeMarker.ToggleMarker(false);
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
    private void CreateAllRenderEdges()
    {
        foreach (UndirectedEdge<Node> edge in _gridGraph.Edges)
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
                    _gridGraph.AddEdge(new UndirectedEdge<Node>(node, value));
                    _gridGraph.AddEdge(new UndirectedEdge<Node>(value, node));
                }
            }
        }
    }

    private void AStarAlgorithmAllAgents()
    {
        foreach (MAPFAgent agent in _MAPFAgents)
        {
            aStarManager.AttachGraph(_gridGraph);
            Debug.Log(agent.currentNode);
            List<UndirectedEdge<Node>> path = aStarManager.ComputeAStarPath(agent.currentNode, agent.destinationNode).ToList();
            agent.SetPath(path);
            Debug.Log(path);

            /*foreach (LineRenderer child in _renderedEdgesParent.GetComponentsInChildren<LineRenderer>())
            {
                Destroy(child.gameObject);
            }
            foreach (UndirectedEdge<Node> edge in _gridGraph.Edges)
            {
                LineRenderer lineRenderer = Instantiate(_edgeRenderer, _renderedEdgesParent).GetComponent<LineRenderer>();
                lineRenderer.SetPositions(new Vector3[2] { edge.Source.position, edge.Target.position });
                if (path.Contains(edge))
                {
                    lineRenderer.startColor = Color.green;
                    lineRenderer.endColor = Color.green;
                }
            }
            */
        }

    }

    private void NewDestinationAgent(MAPFAgent agent)
    {
        agent.SetDestination(_nodes[Random.Range(0, _nodes.Length)]);
        AStarAlgorithmAllAgents();
    }
}
