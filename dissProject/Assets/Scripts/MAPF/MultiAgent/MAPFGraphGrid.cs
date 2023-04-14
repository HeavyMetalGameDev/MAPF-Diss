using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QuikGraph;
using System.Linq;
using System.Diagnostics;

public class MAPFGraphGrid : MonoBehaviour
{
    //[SerializeField] List<MAPFNode> _nodes;
    [SerializeField] GameObject _agentPrefab;
    [SerializeField] GameObject _nodePrefab;
    [SerializeField] MAPFAgent[] _MAPFAgents;
    [SerializeField] Material _defaultMaterial;
    public delegate void RefreshGrid(Node node);
    public static RefreshGrid refreshGrid;
    Dictionary<Vector2, MapNode> _nodeDict = new Dictionary<Vector2, MapNode>();
    List<List<MapNode>> _gridGraph = new List<List<MapNode>>();
    public Dictionary<int, RRAStar> agentRRAStarDict = new();
    MAPFMapReader _mapReader = new MAPFMapReader();
    Stopwatch _sw = new Stopwatch();
    [SerializeField] string _mapName;

    [SerializeField] int _agentCount;

    STAStar _stAStar;
    CBSManager _cbsManager;

    public delegate void AgentArrived(MAPFAgent agent);
    public static AgentArrived agentArrived;

    Vector2 _mapDimensions;

    private void OnEnable()
    {
        agentArrived += OnAgentArrived;
    }
    private void OnDisable()
    {
        agentArrived -= OnAgentArrived;
    }
    private void Start()
    {
        GetDataFromMapReader();
        //GetNodesInChildren();
        AddNodesToGraph();
        CreateRandomAgents(_agentCount);
        //SetupAgents();
        RandomDestinationAllAgents();
        SetupRRAStar();
        //AStarAllAgents();
        CBSAllAgents();
        //CoopAStarAllAgents();
        //CreateAllRenderEdges();
    }
    private void OnGridRefresh(MapNode node)
    {
        if (node.nodeType.Equals(NodeTypeEnum.NOT_WALKABLE))
        {
            RemoveNodeAndEdges(node);
        }
        else if (node.nodeType.Equals(NodeTypeEnum.WALKABLE))
        {
            //TODO
            _nodeDict[node.position] = node;
            node._nodeMarker.ToggleMarker(true);
        }
    }

    private void RemoveNodeAndEdges(MapNode node)
    {
        ///TODO
        _nodeDict[node.position] = node;
        node._nodeMarker.ToggleMarker(false);
    }

    private void GetDataFromMapReader()
    {
        _mapDimensions = _mapReader.ReadMapFromFile(_mapName);
        _gridGraph = _mapReader.GetNodesFromMap();
    }
    private void AddNodesToGraph() //function will instantiate node gameobjects and add them to graph. additionally will set the correcsponding node address to be the new GameO.
    {
        //TODO
        foreach (List<MapNode> nodeList in _gridGraph)
        {
            int counter = 0;
            foreach (MapNode node in nodeList)
            {
                if (node.nodeType == NodeTypeEnum.WALKABLE)
                {
                    GameObject createdNode = Instantiate(_nodePrefab, transform);
                    createdNode.transform.position = new Vector3(node.position.x, 0, node.position.y);
                    _nodeDict.Add(node.position, node);
                }
                //createdNodeComponent._nodeMarker = Instantiate(_nodeMarker, createdNode.transform).GetComponent<GridMarker>();
                //createdNodeComponent._nodeMarker.ToggleMarker(isWalkable);
                //nodeList[counter] = createdNodeComponent;
                counter += 1;
            }

        }
    }
    private void CreateSTAStar()
    {
        _stAStar = new STAStar();
        _stAStar.SetSTAStar(_gridGraph, _mapDimensions);
    }
    private void SetupRRAStar()
    {
        foreach (MAPFAgent agent in _MAPFAgents) //setup RRA star for each agent since the calculated heuristics can be reused in each Conflict Node low level
        {
            agentRRAStarDict.Add(agent.agentId, new RRAStar(agent.destinationNode, _gridGraph, _mapDimensions));
        }
    }
    private void NewDestinationAgent(MAPFAgent agent)
    {
        //SetNodeMaterial(agent.destinationNode, _defaultMaterial);
        MapNode randomNode;
        if (agent.destinationNode != null)
        {
            randomNode = agent.destinationNode;
        }
        else
        {
            randomNode = null;
        }

        int timeout = 100; //times to attempt random node asignment to avoid deadlock
        while (randomNode == agent.destinationNode || randomNode.nodeType == NodeTypeEnum.NOT_WALKABLE || randomNode.isTargeted || randomNode == agent.currentNode)
        {
            randomNode = _gridGraph[Random.Range(0, (int)_mapDimensions.y)][Random.Range(0,(int) _mapDimensions.x)];
            timeout--;
            if (timeout <= 0) break;
        }
        agent.SetDestination(randomNode);

        //SetNodeMaterial(randomNode, agent.GetComponentInChildren<MeshRenderer>().material);
    }
    private bool SetRandomAgentLocation(MAPFAgent agent)
    {
        MapNode randomNode = null;
        int timeout = 100; //times to attempt random node asignment to avoid deadlock
        while (randomNode == null || randomNode.nodeType == NodeTypeEnum.NOT_WALKABLE || randomNode.isOccupied)
        {
            randomNode = _gridGraph[Random.Range(0, (int)_mapDimensions.y)][Random.Range(0, (int)_mapDimensions.x)];
            timeout--;
            if (randomNode.nodeType == NodeTypeEnum.WALKABLE && !randomNode.isOccupied)
            {
                break;
            }
            if (timeout <= 0)
            {
                Destroy(agent.gameObject);
                return false;
            }
        }
        //UnityEngine.Debug.Log("VALID LOCATION FOUND");
        agent.SetCurrent(randomNode);
        randomNode.isOccupied = true;
        agent.transform.position = new Vector3(randomNode.position.x, 0, randomNode.position.y);
        return true;
    }

    private void RandomDestinationAllAgents()
    {
        foreach (MAPFAgent agent in _MAPFAgents)
        {
            NewDestinationAgent(agent);
        }
    }

    private void CreateRandomAgents(int agentCount)
    {
        List<MAPFAgent> agentsList = new List<MAPFAgent>();

        for (int i = 0; i < agentCount; i++)
        {
            MAPFAgent agent = Instantiate(_agentPrefab).GetComponent<MAPFAgent>();
            agent.agentId = i;
            if (!SetRandomAgentLocation(agent))
            {
                UnityEngine.Debug.Log("NO MORE SPACE FOR AGENTS");
                break;
            }
            agentsList.Add(agent);

        }
        _MAPFAgents = agentsList.ToArray();
    }

    private void CoopAStarAllAgents()
    {
        _stAStar = new STAStar();
        _sw.Start();
        foreach (MAPFAgent agent in _MAPFAgents)
        {
            _stAStar.SetSTAStar(_gridGraph, _mapDimensions, agentRRAStarDict[agent.agentId]);
            agent.SetPath(_stAStar.GetSTAStarPath(agent,true));
        }
        _sw.Stop();
        UnityEngine.Debug.Log(_sw.ElapsedMilliseconds);


    }
    private void AStarAllAgents()
    {
        _stAStar = new STAStar();
        _sw.Start();
        foreach (MAPFAgent agent in _MAPFAgents)
        {
            _stAStar.SetSTAStar(_gridGraph, _mapDimensions);
            
            agent.SetPath(_stAStar.GetSingleAgentPath(agent));
        }
        _sw.Stop();
        UnityEngine.Debug.Log(_sw.ElapsedMilliseconds);


    }

    private void OnAgentArrived(MAPFAgent agent)
    {

        NewDestinationAgent(agent);
        //CBSAllAgents();
        /*_gridGraphCopy = _gridGraph;

        _stAStar.SetSTAStar(_gridGraphCopy, _mapDimensions);
        _stAStar.startingTimestep = agent.timesteps;
        agent.SetPath(_stAStar.GetSTAStarPath(agent));*/
    }
    
    private void CBSAllAgents()
    {
        _cbsManager = new CBSManager(_gridGraph,_MAPFAgents,_mapDimensions);
        _cbsManager.agentRRAStarDict = agentRRAStarDict;
        Dictionary<MAPFAgent, List<MapNode>> solution = _cbsManager.Plan();
        if (solution == null)
        {
            UnityEngine.Debug.Log("FAILED TO FIND CBS SOLUTION");
            return;
        }
        foreach(MAPFAgent agent in _MAPFAgents)
        {
            agent.SetPath(solution[agent]);
        }
    }
}
