using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QuikGraph;
using System.Linq;
using Priority_Queue;
using System.Diagnostics;
using Unity.Profiling;

public class STAStar
{
    public List<List<MapNode>> _graph = new List<List<MapNode>>();
    Vector2 dimensions;
    public Dictionary<(Vector2,int),MAPFAgent> rTable = new Dictionary<(Vector2, int), MAPFAgent>(); //reservation table for node positions
    public Dictionary<(Vector2,Vector2, int), MAPFAgent> edgeTable = new Dictionary<(Vector2, Vector2, int), MAPFAgent>(); //reservation table for edge traversal
    public Dictionary<(Vector2,int), MAPFAgent> positiveConstraints = new Dictionary<(Vector2,int), MAPFAgent>();
    public Dictionary<(Vector2, Vector2, int), MAPFAgent> positiveEdgeConstraints = new Dictionary<(Vector2, Vector2, int), MAPFAgent>();
    public int startingTimestep=0;
    RRAStar rraStar;
    UnityEngine.Object marker;
    Stopwatch _sw = new Stopwatch();
    bool considerPositiveConstraints = false;
    int iterator;
    public STAStar()
    {
        marker = Resources.Load("expanded marker");
    }
    public void SetSTAStar(List<List<MapNode>> Graph, Vector2 Dimensions, RRAStar rraStar, bool positive)
    {
        _graph = Graph;
        dimensions = Dimensions;
        this.rraStar = rraStar;
        considerPositiveConstraints = positive;
    }
    public void SetSTAStar(List<List<MapNode>> Graph, Vector2 Dimensions)
    {
        _graph = Graph;
        dimensions = Dimensions;
        marker = Resources.Load("expanded marker");
    }

    public List<MapNode> GetSingleAgentPath(MAPFAgent agent)
    {
        MAPFNode source = new MAPFNode(agent.currentNode, 0, 0, 0, null);
        SimplePriorityQueue<MAPFNode> openList = new SimplePriorityQueue<MAPFNode>();
        Dictionary<Vector2, MAPFNode> closedList = new Dictionary<Vector2, MAPFNode>();
        List<MapNode> path = new List<MapNode>();
        MAPFNode workingNode;
        openList.Enqueue(source, source.GetCost());
        while (openList.Count != 0)
        {
            _sw.Start();
            workingNode = openList.Dequeue();
            if (workingNode.PositionIsEqualTo(agent.destinationNode))
            {
                
                while (workingNode.parent != null)
                {

                    //Debug.Log(workingNode + " - " + workingNode.parent);
                    path.Add(workingNode.node);
                    workingNode = workingNode.parent;
                }
                path.Add(agent.currentNode);
                path.Reverse();

                //UnityEngine.Debug.Log("OPEN: " + openList.Count);
                //UnityEngine.Debug.Log("CLOSED: " + closedList.Count);
                return path;
            }
            closedList.Add(workingNode.node.position, workingNode);
            foreach (MapNode adjNode in GetAdjacentNodes(workingNode))
            {
                if (!closedList.ContainsKey(adjNode.position))
                {
                    MAPFNode newNode = new MAPFNode(adjNode, workingNode.g+5, CalculateManhattan(adjNode, agent.destinationNode), 0, workingNode);
                    bool found = false;
                    foreach(MAPFNode node in openList)
                    {
                        if (node.PositionIsEqualTo(newNode.node))
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        openList.Enqueue(newNode, newNode.GetCost());
                    }
                }

            }
            //_sw.Stop();
            //UnityEngine.Debug.Log(_sw.ElapsedMilliseconds);

        }
        //UnityEngine.Debug.Log("NO PATH FOUND");
        return null;
    }

    public List<MapNode> GetSTAStarPath(MAPFAgent agent, bool shouldReservePath, bool useImprovedHeuristic)
    {
        MAPFNode source = new MAPFNode(agent.currentNode, 0, 0, startingTimestep, null);
        List<MapNode> path = new List<MapNode>();
        SimplePriorityQueue<MAPFNode> openList = new SimplePriorityQueue<MAPFNode>();
        Dictionary<(Vector2,int),MAPFNode> closedList = new Dictionary<(Vector2, int), MAPFNode>();
        Dictionary<(Vector2, int), MAPFNode> openListDict = new Dictionary<(Vector2, int), MAPFNode>();

        MAPFNode workingNode;
        openList.Enqueue(source, source.GetCost());
        int pathPadding = 20;
        while (openList.Count != 0)
        {
            workingNode = openList.Dequeue();
            if (workingNode.PositionIsEqualTo(agent.destinationNode))
            {
                /*openList = new();
                if (pathPadding > 0)
                {
                    pathPadding--;
                    ProcessAdjacentNodes();
                    continue;
                }*/
                MAPFNode prevNode;
                while (workingNode.parent != null)
                {
                    //Debug.Log(workingNode + " - " + workingNode.parent);
                    path.Add(workingNode.node);

                    if (shouldReservePath)
                    {
                        rTable.Add((workingNode.node.position,workingNode.time), agent);
                        prevNode = workingNode;
                        workingNode = workingNode.parent;
                        edgeTable.Add((workingNode.node.position,prevNode.node.position, workingNode.time), agent);
                        if (!workingNode.PositionIsEqualTo(prevNode.node))
                        {
                            edgeTable.Add((prevNode.node.position, workingNode.node.position, workingNode.time), agent);
                        }
                    }
                    else
                    {
                        workingNode = workingNode.parent;
                    }


                }
                path.Add(agent.currentNode);
                path.Reverse();
                //UnityEngine.Debug.Log("OPEN: " + openList.Count);
                //UnityEngine.Debug.Log("CLOSED: " + closedList.Count);
                return path;
            }
            closedList.Add((workingNode.node.position, workingNode.time), workingNode);

            /*ExpandedNodeDelay xND = ((GameObject)GameObject.Instantiate(marker, new Vector3(workingNode.node.position.x, .4f, workingNode.node.position.y), Quaternion.identity)).GetComponent<ExpandedNodeDelay>();
            iterator++;
            xND.order = iterator;*/
            ProcessAdjacentNodes();
        }
        //UnityEngine.Debug.Log("NO PATH FOUND");
        path = null;
        return path;

        void ProcessAdjacentNodes()
        {
            foreach (MapNode adjNode in GetAdjacentNodes(workingNode))
            {
                int nodeHValue;
                if (useImprovedHeuristic)
                {
                    nodeHValue = rraStar.GetNodeHeuristic(adjNode);
                }
                else
                {
                    nodeHValue = CalculateManhattan(adjNode, agent.destinationNode);
                }
                 
                MAPFNode newNode = new MAPFNode(adjNode, workingNode.g + 5, nodeHValue, workingNode.time + 1, workingNode);
                if (closedList.ContainsKey((adjNode.position, workingNode.time + 1)))
                {
                    closedList.TryGetValue((adjNode.position, workingNode.time + 1), out MAPFNode closedListNode);
                    if (closedListNode.GetCost() > newNode.GetCost())
                    {

                        closedList[(adjNode.position, newNode.time)] = newNode;
                        openList.Enqueue(newNode, newNode.GetCost());
                        openListDict.Add((newNode.node.position, newNode.time), newNode);
                    }
                    continue;
                }
                if (rTable.ContainsKey((adjNode.position,workingNode.time + 1))) //if this time position is reserved at the nest timestep, dont consider it
                {
                    //UnityEngine.Debug.Log(agent.agentId +" AVOID AT " + adjNode.position + "" + (workingNode.time + 1));
                    continue;
                }
                if (edgeTable.ContainsKey((workingNode.node.position,adjNode.position,workingNode.time))) //if this edge is reserved, dont consider it
                {
                    //UnityEngine.Debug.Log(agent.agentId + " AVOID EDGE AT " + workingNode.position + "" + adjNode.position + "" +workingNode.time);
                    continue;
                }


                if (!openListDict.ContainsKey((newNode.node.position, newNode.time)))
                {
                    openList.Enqueue(newNode, newNode.GetCost());
                    openListDict.Add((newNode.node.position, newNode.time), newNode);
                }
                else
                {
                    if(openListDict[(newNode.node.position, newNode.time)].GetCost()> newNode.GetCost()) //update path to the node if a better one is found
                    {
                        openListDict[(newNode.node.position, newNode.time)].g = newNode.g;
                        openListDict[(newNode.node.position, newNode.time)].parent = workingNode;
                        openListDict[(newNode.node.position, newNode.time)] = newNode;
                    }
                    
                }

            }
        }
    }
    public int CalculateManhattan(MapNode start, MapNode end)
    {
        return (int)(Mathf.Abs(start.position.x - end.position.x) + (int)Mathf.Abs(start.position.y - end.position.y));
    }

    public List<MapNode> GetAdjacentNodes(MAPFNode node)
    {
        List<MapNode> adjacentNodes = new List<MapNode>();
        int nodeX = (int)(node.node.position.x*.2f);
        int nodeY = (int)(node.node.position.y * .2f);
        adjacentNodes.Add(node.node);
        MapNode potentialNode; 

        if(nodeX + 1 < dimensions.x)
        {
            potentialNode = _graph[nodeY][nodeX + 1];
            if (potentialNode.nodeType.Equals(NodeTypeEnum.WALKABLE))
            {
                adjacentNodes.Add(potentialNode);
                //Debug.Log(potentialNode);
            }

        }

        if (nodeX!=0)
        {
            potentialNode = _graph[nodeY][nodeX - 1];
            if (potentialNode.nodeType.Equals(NodeTypeEnum.WALKABLE))
            {
                adjacentNodes.Add(potentialNode);
                //Debug.Log(potentialNode);
            }

        }

        if (nodeY + 1 < dimensions.y)
        {
            potentialNode = _graph[nodeY+1][nodeX];
            if (potentialNode.nodeType.Equals(NodeTypeEnum.WALKABLE))
            {
                adjacentNodes.Add(potentialNode);
                //Debug.Log(potentialNode);
            }

        }
        

        if (nodeY!=0)
        {
            potentialNode = _graph[nodeY-1][nodeX];
            if (potentialNode.nodeType.Equals(NodeTypeEnum.WALKABLE))
            {
                adjacentNodes.Add(potentialNode);
                //Debug.Log(potentialNode);
            }

        }

        return adjacentNodes;
    }

}
