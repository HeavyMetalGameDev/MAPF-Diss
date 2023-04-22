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
    [SerializeField] GameObject _camera;
    [SerializeField] List<MAPFAgent> _MAPFAgents;
    [SerializeField] Material _defaultMaterial;
    Dictionary<Vector2Int, MapNode> _nodeDict = new Dictionary<Vector2Int, MapNode>();
    List<List<MapNode>> _gridGraph = new List<List<MapNode>>();
    public Dictionary<int, RRAStar> agentRRAStarDict = new();
    MAPFMapReader _mapReader = new MAPFMapReader();
    Stopwatch _sw = new Stopwatch();

    [SerializeField] public string _mapName;
    [SerializeField] public int _agentCount;
    int prevAgentCount = 0;
    public string algorithmToUse;
    public int scenarioNum;

    STAStar _stAStar;
    CBSManager _cbsManager;

    public delegate void AgentArrived(MAPFAgent agent);
    public static AgentArrived agentArrived;

    Vector2Int _mapDimensions;

    float executionTime;
    int sumOfCosts;

    public void Execute()
    {
        bool success = true;
        GetDataFromMapReader();
        AddNodesToGraph();
        CombineMapMeshes();
        
        //CreateRandomAgents(_agentCount);
        //RandomDestinationAllAgents();
        
        while (success)
        {
            foreach (MAPFAgent agent in _MAPFAgents)
            {
                Destroy(agent.gameObject);
            }
            prevAgentCount = _agentCount;
            SetupScenario(scenarioNum);
            
            switch (algorithmToUse)
            {
                case "AStar":
                    success = AStarAllAgents();
                    break;
                case "CAStar":
                    SetupRRAStar();
                    success = CoopAStarAllAgents(false);
                    break;
                case "HCAStar":
                    SetupRRAStar();
                    success = CoopAStarAllAgents(true);
                    break;
                case "CBS":
                    SetupRRAStar();
                    success = CBSAllAgents(false);
                    break;
                case "CBS-DS":
                    SetupRRAStar();
                    success = CBSAllAgents(true);
                    break;
            }
            SolutionChecker();
            WriteResultsToFile();
            _agentCount++;
            if (prevAgentCount == _agentCount)
            {
                break;
            }
        }

    }
    private void GetDataFromMapReader()
    {
        _mapDimensions = _mapReader.ReadMapFromFile(_mapName);
        _gridGraph = _mapReader.GetNodesFromMap();

        _camera.transform.position = new Vector3(_mapDimensions.x * 2.5f, _mapDimensions.y * 3f, -_mapDimensions.y * 2.5f);
        _camera.transform.LookAt(new Vector3(_mapDimensions.x * 2.5f, 0, _mapDimensions.y * 2.5f));
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
        agentRRAStarDict.Clear();
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

    private void SetupScenario(int scenarioID)
    {
        ScenarioReader sr = new ScenarioReader();
        _MAPFAgents = sr.ReadScenarioAgents(_mapName, scenarioID, _agentCount, _gridGraph, _agentPrefab);
        _agentCount = _MAPFAgents.Count;
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

    private bool CoopAStarAllAgents(bool useImprovedHeuristic)
    {
        _sw.Reset();
        _sw.Start();
        int iterationsAllowed = 20;
        sumOfCosts = 0;
        _stAStar = new STAStar();
        iterationsAllowed--;
        foreach (MAPFAgent agent in _MAPFAgents)
        {
            _stAStar.SetSTAStar(_gridGraph, _mapDimensions, agentRRAStarDict[agent.agentId]);
            if (_sw.ElapsedMilliseconds >= 30000)
            {
                return false;
            }
            List<MapNode> newPath = _stAStar.GetSTAStarPath(agent, true, useImprovedHeuristic, 0);
            if (newPath == null)
            {
                Destroy(agent.gameObject);
                return false;
            }
            else
            {
                sumOfCosts += _stAStar.finalTimestep;
            }
            agent.SetPath(newPath);

        }
            

        _sw.Stop();
        executionTime = _sw.ElapsedMilliseconds;
        return true;

    }
    private bool AStarAllAgents()
    {
        _sw.Reset();
        _sw.Start();
        sumOfCosts = 0;
        _stAStar = new STAStar();
        foreach (MAPFAgent agent in _MAPFAgents)
        {
            _stAStar.SetSTAStar(_gridGraph, _mapDimensions);
            List<MapNode> path = _stAStar.GetAStarPath(agent);
            if (path == null) return false;
            agent.SetPath(path);
            sumOfCosts += agent.path.Count;
            if (_sw.ElapsedMilliseconds >= 30000)
            {
                return false;
            }
        }
        _sw.Stop();
        executionTime = _sw.ElapsedMilliseconds;
        return true;
    }

    private bool CBSAllAgents(bool disjoint)
    {
        _sw.Reset();
        sumOfCosts = 0;
        _cbsManager = new CBSManager(_gridGraph,_MAPFAgents,_mapDimensions,disjoint);
        _cbsManager.agentRRAStarDict = agentRRAStarDict;
        Dictionary<MAPFAgent, List<MapNode>> solution = _cbsManager.Plan();
        if (solution == null)
        {
            UnityEngine.Debug.Log("FAILED TO FIND CBS SOLUTION");
            return false;
        }

        foreach (MAPFAgent agent in _MAPFAgents)
        {
            agent.SetPath(solution[agent]);
        }
        sumOfCosts = _cbsManager.sumOfCosts;
        executionTime = _cbsManager.executionTime;
        return true;
    }

    private void SolutionChecker()
    {
        int maxPathLength = 0;
        foreach(MAPFAgent agent in _MAPFAgents)
        {
            if (agent.path.Count > maxPathLength)
            {
                maxPathLength = agent.path.Count;
            }
        }
        Dictionary<Vector2Int, MAPFAgent> positionsAtTimestep; //stores the positions of all checked agents at a timestep, so if there is duplicates then there is a collision
        Dictionary<(Vector2Int, Vector2Int), MAPFAgent> edgesAtTimestep;
        for (int t = 0; t <= maxPathLength; t++)
        {

            positionsAtTimestep = new();
            edgesAtTimestep = new();
            foreach (MAPFAgent agent in _MAPFAgents)
            {

                List<MapNode> agentPath = agent.path;
                if (agentPath.Count <= t)
                {
                    //positionsAtTimestep.Add(agentPath[^1].position, agent);
                    continue;
                }
                //if the agents path is shorter than t there cant be a collision so go to next agent
                if (positionsAtTimestep.ContainsKey(agentPath[t].position))
                {
                    MAPFAgent[] agents = { agent, positionsAtTimestep[agentPath[t].position] };
                    //UnityEngine.Debug.Log("COLLISION WHEN PLANNING: Agent " + agent.agentId + " and Agent " + agents[1].agentId + " at " + agentPath[t].position + " time " + (t));
                }

                positionsAtTimestep.TryAdd(agentPath[t].position, agent);
                if (agentPath.Count <= t + 1) continue; //if there isnt a node at the next timestep continue
                if (edgesAtTimestep.ContainsKey((agentPath[t].position, agentPath[t + 1].position)))
                {
                    MAPFAgent[] agents = { agent, edgesAtTimestep[(agentPath[t].position, agentPath[t + 1].position)] };
                    //UnityEngine.Debug.Log("EDGE COLLISION WHEN PLANNING: Agent " + agent.agentId + " and Agent " + agents[1].agentId + " edge " + agentPath[t].position + "" + agentPath[t + 1].position + "time " + (t));
                }

                if (!agentPath[t].position.Equals(agentPath[t + 1].position)) //only add an edge if the agent is travelling to a different node
                {
                    edgesAtTimestep.TryAdd((agentPath[t].position, agentPath[t + 1].position), agent);
                    edgesAtTimestep.TryAdd((agentPath[t + 1].position, agentPath[t].position), agent); //reserve edge in opposite direction too
                } 

            }

        }
    }

    private void WriteResultsToFile()
    {
        ResultsWriter.WriteResult( _agentCount+ "\t" + executionTime + "\t" + sumOfCosts, algorithmToUse,_mapName);
        //UnityEngine.Debug.Log(executionTime);
        //UnityEngine.Debug.Log("SUM OF COSTS: " + sumOfCosts );
    }
    private void CombineMapMeshes()
    {
        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];

        int i = 0;
        while (i < meshFilters.Length)
        {
            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
            meshFilters[i].gameObject.SetActive(false);

            i++;
        }

        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.CombineMeshes(combine);
        transform.GetComponent<MeshFilter>().sharedMesh = mesh;
        transform.gameObject.SetActive(true);
    }

}
