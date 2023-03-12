using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QuikGraph;
using System.Linq;

public class GraphGrid : MonoBehaviour
{
    [SerializeField] Node[] _nodes;
    [SerializeField] GameObject _nodePrefab;
    [SerializeField] GameObject _nodeMarker;
    [SerializeField] GameObject _edgeRenderer;
    [SerializeField] Transform _renderedEdgesParent;
    [SerializeField] MAPFAgent[] _MAPFAgents;
    [SerializeField] Material _defaultMaterial;
    public delegate void RefreshGrid(Node node);
    public static RefreshGrid refreshGrid;
    Dictionary<Vector2, Node> _nodeDict = new Dictionary<Vector2, Node>();
    Vector2[] _dirs = { new Vector2(0, 5), new Vector2(5, 0)};
    AStarManager aStarManager = new AStarManager();
    BidirectionalGraph<Node, Edge<Node>> _gridGraph = new BidirectionalGraph<Node, Edge<Node>>(true);

    MapReader _mapReader = new MapReader();
    [SerializeField] string _mapName;
    public delegate void AgentArrived(MAPFAgent agent);
    public static AgentArrived agentArrived;

    private void Start()
    {
        GetNodesFromMapReader();
        //GetNodesInChildren();
        AddNodesToGraph();
        AddEdgesToGraph();
        SetupAgents();
        RandomDestinationAllAgents();
        //CreateAllRenderEdges();
        Debug.Log(_gridGraph.EdgeCount);
        AStarAlgorithmAllAgents();
    }

    private void OnEnable()
    {
        refreshGrid += OnGridRefresh;
        agentArrived += NewDestinationAgentAndAStar;
    }
    private void OnDisable()
    {
        refreshGrid -= OnGridRefresh;
        agentArrived -= NewDestinationAgentAndAStar;
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
                        _gridGraph.AddEdge(new Edge<Node>(node, value));
                    }
                }
                if (_nodeDict.TryGetValue(node.position - dir, out Node value2))
                {
                    if (value2.nodeType.Equals(NodeTypeEnum.WALKABLE))
                    {
                        _gridGraph.AddEdge(new Edge<Node>(value2, node));
                    }
                }

            }
        }
        foreach(LineRenderer child in _renderedEdgesParent.GetComponentsInChildren<LineRenderer>())
        {
            Destroy(child.gameObject);
        }
        //CreateAllRenderEdges();
        AStarAlgorithmAllAgents();
    }

    private void RemoveNodeAndEdges(Node node)
    {
        _gridGraph.ClearEdges(node);
        _gridGraph.RemoveVertex(node);
        _nodeDict[node.position] = node;
        node._nodeMarker.ToggleMarker(false);
    }

    private void GetNodesFromMapReader()
    {
        _nodes = _mapReader.ReadNodesFromFile(_mapName);
    }
    private void AddNodesToGraph() //function will instantiate node gameobjects and add them to graph. additionally will set the correcsponding node address to be the new GameO.
    {
        int counter = 0;
        foreach (Node node in _nodes)
        {
            bool isWalkable = false; //used to toggle marker colour later in function
            GameObject createdNode = Instantiate(_nodePrefab,transform);

            //Set node values
            Node createdNodeComponent = createdNode.GetComponent<Node>();
            createdNodeComponent.position = node.position;
            createdNodeComponent.nodeType = node.nodeType;
            createdNode.transform.position = new Vector3(node.position.x,0, node.position.y);

            if (node.nodeType == NodeTypeEnum.WALKABLE)
            {
                isWalkable = true;
                _gridGraph.AddVertex(createdNodeComponent);
                
                _nodeDict.Add(createdNodeComponent.position, createdNodeComponent);
            }
            //createdNodeComponent._nodeMarker = Instantiate(_nodeMarker, createdNode.transform).GetComponent<GridMarker>();
            //createdNodeComponent._nodeMarker.ToggleMarker(isWalkable);
            _nodes[counter] = createdNodeComponent;
            counter += 1;
        }
    }
    private void CreateAllRenderEdges()
    {
        foreach (Edge<Node> edge in _gridGraph.Edges)
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
                    _gridGraph.AddEdge(new Edge<Node>(node, value));
                    _gridGraph.AddEdge(new Edge<Node>(value, node));
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
            List<Edge<Node>> path = aStarManager.ComputeAStarPath(agent.currentNode, agent.destinationNode).ToList();
            agent.SetPath(path);
            Debug.Log(path);

            /*foreach (LineRenderer child in _renderedEdgesParent.GetComponentsInChildren<LineRenderer>())
            {
                Destroy(child.gameObject);
            }
            foreach (Edge<Node> edge in _gridGraph.Edges)
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

    private void NewDestinationAgentAndAStar(MAPFAgent agent) //called when an agent arrives at their destination and therefore A* needs to be ran again.
    {
        NewDestinationAgent(agent);
        AStarAlgorithmAllAgents();
    }

    private void NewDestinationAgent(MAPFAgent agent)
    {
        SetNodeMaterial(agent.destinationNode, _defaultMaterial);
        Node randomNode = agent.destinationNode;
        int timeout = 20; //times to attempt random node asignment to avoid deadlock
        while (randomNode == agent.destinationNode || randomNode.nodeType == NodeTypeEnum.NOT_WALKABLE || randomNode.isTargeted)
        {
            randomNode = _nodes[Random.Range(0, _nodes.Length)];
            timeout--;
            if (timeout <= 0) break;
        }
        agent.SetDestination(randomNode);

        SetNodeMaterial(randomNode, agent.GetComponentInChildren<MeshRenderer>().material);
    }

    private void SetNodeMaterial(Node node,Material material)
    {
        node.GetComponent<MeshRenderer>().material = material;
    }

    private void RandomDestinationAllAgents()
    {
        foreach(MAPFAgent agent in _MAPFAgents)
        {
            NewDestinationAgent(agent);
        }
    }

    private void SetupAgents()
    {
        foreach(MAPFAgent agent in _MAPFAgents)
        {
            Vector2 agentPos = new Vector2(agent.transform.position.x,agent.transform.position.z);
            if (_nodeDict.TryGetValue(agentPos, out Node outNode))
            {
                agent.SetCurrent(outNode);
            }
            else
            {
                Debug.LogError("AGENT SETUP FAILED: AGENT NOT ON GRID POSITION");
            }

        }
    }
}
