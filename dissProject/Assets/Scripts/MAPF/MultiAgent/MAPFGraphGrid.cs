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
    [SerializeField] List<MAPFAgent> _MAPFAgents;
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
        CBSAllAgents(true);
        //CoopAStarAllAgents();
        //CreateAllRenderEdges();
        SolutionChecker();
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
        _MAPFAgents = agentsList;
    }

    private void CoopAStarAllAgents()
    {
        
        _sw.Start();
        bool isValid = false;
        int iterationsAllowed = 20;
        while (!isValid)
        {
            _stAStar = new STAStar();
            iterationsAllowed--;
            if (iterationsAllowed <= 0)
            {
                break;
            }
            foreach (MAPFAgent agent in _MAPFAgents)
            {
                isValid = false;
                _stAStar.SetSTAStar(_gridGraph, _mapDimensions, agentRRAStarDict[agent.agentId]);
                List<MapNode> newPath = _stAStar.GetSTAStarPath(agent, true);
                if (newPath == null)
                {
                    UnityEngine.Debug.Log("REPLAN");
                    _MAPFAgents.Remove(agent);
                    _MAPFAgents.Insert(0, agent); //increase an agents priority then replan
                    break;
                }
                else
                {
                    
                    isValid = true;
                }
                agent.SetPath(newPath);

            }
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
    
    private void CBSAllAgents(bool disjoint)
    {
        _sw.Start();
        _cbsManager = new CBSManager(_gridGraph,_MAPFAgents,_mapDimensions,disjoint);
        _cbsManager.agentRRAStarDict = agentRRAStarDict;
        Dictionary<MAPFAgent, List<MapNode>> solution = _cbsManager.Plan();
        if (solution == null)
        {
            UnityEngine.Debug.Log("FAILED TO FIND CBS SOLUTION");
            return;
        }
        _sw.Stop();
        UnityEngine.Debug.Log(_sw.ElapsedMilliseconds);
        foreach (MAPFAgent agent in _MAPFAgents)
        {
            agent.SetPath(solution[agent]);
        }
    }

    private void SolutionChecker()
    {
        Hashtable positionsAtTimestep; //stores the positions of all checked agents at a timestep, so if there is duplicates then there is a collision
        Hashtable edgesAtTimestep;
        int maxPathLength = 0;
        foreach(MAPFAgent agent in _MAPFAgents)
        {
            if (agent.path.Count > maxPathLength)
            {
                maxPathLength = agent.path.Count;
            }
        }
        for (int t = 0; t < maxPathLength; t++)
        {

            positionsAtTimestep = new Hashtable();
            edgesAtTimestep = new Hashtable();
            foreach (MAPFAgent agent in _MAPFAgents)
            {
                if (agent.path.Count <= t) continue;//if the agents path is shorter than t there cant be a collision so go to next agent
                if (positionsAtTimestep.ContainsKey(agent.path[t].position))
                {
                    MAPFAgent[] agents = { agent, (MAPFAgent)positionsAtTimestep[agent.path[t].position] };
                    UnityEngine.Debug.Log("COLLISION WHEN PLANNING: Agent " + agent.agentId + " and Agent " + agents[1].agentId + " at " + agent.path[t].position +" time " + (t));
                }
                else
                {
                    positionsAtTimestep.Add(agent.path[t].position, agent);
                }
                if (agent.path.Count <= t + 1) continue; //if there isnt a node at the next timestep continue
                if (edgesAtTimestep.ContainsKey(agent.path[t].position + "" + agent.path[t + 1].position))
                {
                    MAPFAgent[] agents = { agent, (MAPFAgent)edgesAtTimestep[agent.path[t].position + "" + agent.path[t + 1].position] };
                    UnityEngine.Debug.Log("EDGE COLLISION WHEN PLANNING: Agent " + agent.agentId + " and Agent " + agents[1].agentId + " edge " + agent.path[t].position +""+ agent.path[t+1].position + "time " + (t));
                }
                else
                {
                    if (!agent.path[t].position.Equals(agent.path[t + 1].position)) //only add an edge if the agent is travelling to a different node
                    {
                        edgesAtTimestep.Add(agent.path[t].position + "" + agent.path[t + 1].position, agent);
                        edgesAtTimestep.Add(agent.path[t + 1].position + "" + agent.path[t].position, agent); //reserve edge in opposite direction too
                    }
                }



            }

        }
    }
}
