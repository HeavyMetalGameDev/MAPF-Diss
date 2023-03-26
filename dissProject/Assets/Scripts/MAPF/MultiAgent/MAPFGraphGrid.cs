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
    Dictionary<Vector2, MAPFNode> _nodeDict = new Dictionary<Vector2, MAPFNode>();
    List<List<MAPFNode>> _gridGraph = new List<List<MAPFNode>>();
    List<List<MAPFNode>> _gridGraphCopy;
    MAPFMapReader _mapReader = new MAPFMapReader();
    [SerializeField] string _mapName;

    STAStar _stAStar;

    public delegate void AgentArrived(MAPFAgent agent);
    public static AgentArrived agentArrived;

    int _maxPathLength = 0;
    Vector2 _mapDimensions;

    private void Start()
    {
        GetDataFromMapReader();
        //GetNodesInChildren();
        AddNodesToGraph();
        CreateRandomAgents(20);
        //SetupAgents();
        RandomDestinationAllAgents();
        STAStarAllAgents();
        //CreateAllRenderEdges();
    }
    private void OnGridRefresh(MAPFNode node)
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

    private void RemoveNodeAndEdges(MAPFNode node)
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
        foreach (List<MAPFNode> nodeList in _gridGraph)
        {
            int counter = 0;
            foreach (MAPFNode node in nodeList)
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
        _stAStar = new STAStar(_gridGraph, _mapDimensions);
    }
    private void NewDestinationAgent(MAPFAgent agent)
    {
        //SetNodeMaterial(agent.destinationNode, _defaultMaterial);
        MAPFNode randomNode;
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
        MAPFNode randomNode = null;
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
        UnityEngine.Debug.Log("VALID LOCATION FOUND");
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
            if (!SetRandomAgentLocation(agent))
            {
                UnityEngine.Debug.Log("NO MORE SPACE FOR AGENTS");
                break;
            }
            agentsList.Add(agent);

        }
        _MAPFAgents = agentsList.ToArray();
    }

    private void STAStarAllAgents()
    {
        foreach(MAPFAgent agent in _MAPFAgents)
        {
            
            _gridGraphCopy = _gridGraph;
            
            _stAStar = new STAStar(_gridGraphCopy,_mapDimensions);
            agent.SetPath(_stAStar.GetSingleAgentPath(agent));
        }
        
        
    }
    
}
