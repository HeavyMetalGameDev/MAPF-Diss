using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Priority_Queue;
using System.Diagnostics;

public class STAStar
{
    public List<List<MapNode>> _graph = new List<List<MapNode>>();
    Vector2Int dimensions;
    public Dictionary<(Vector2Int,int),MAPFAgent> rTable = new Dictionary<(Vector2Int, int), MAPFAgent>(); //reservation table for node positions
    public Dictionary<(Vector2Int,Vector2Int, int), MAPFAgent> edgeTable = new Dictionary<(Vector2Int, Vector2Int, int), MAPFAgent>(); //reservation table for edge traversal
    public Dictionary<Vector2Int, int> goalReservations = new Dictionary<Vector2Int, int>(); //dict holding goal reservations. key is position and value is timestep the goal is reached, all agents should avoid after that time
    public int startingTimestep=0;
    RRAStar rraStar;
    UnityEngine.Object marker;
    int iterator;
    public int finalTimestep;
    int longestTimestep;

    public STAStar()
    {
        marker = Resources.Load("expanded marker");
    }
    public void SetSTAStar(List<List<MapNode>> Graph, Vector2Int Dimensions, RRAStar rraStar)
    {
        _graph = Graph;
        dimensions = Dimensions;
        this.rraStar = rraStar;
    }
    public void SetSTAStar(List<List<MapNode>> Graph, Vector2Int Dimensions)
    {
        _graph = Graph;
        dimensions = Dimensions;
        marker = Resources.Load("expanded marker");
    }

    public List<MapNode> GetAStarPath(MAPFAgent agent)
    {
        MAPFNode source = new MAPFNode(agent.currentNode, 0, 0, 0, null);
        SimplePriorityQueue<MAPFNode> openList = new SimplePriorityQueue<MAPFNode>();
        Dictionary<Vector2Int, MAPFNode> closedList = new Dictionary<Vector2Int, MAPFNode>();
        Dictionary<Vector2Int, MAPFNode> openListDict = new Dictionary<Vector2Int, MAPFNode>();
        List<MapNode> path = new List<MapNode>();
        MAPFNode workingNode;

        openList.Enqueue(source, source.GetCost());
        while (openList.Count != 0)
        {
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
                    if (!openListDict.ContainsKey(adjNode.position))
                    {
                        openList.Enqueue(newNode, newNode.GetCost());
                        openListDict.Add(newNode.node.position, newNode);
                    }
                    else
                    {
                        if (openListDict[adjNode.position].GetCost() < newNode.GetCost())
                        {
                            openListDict[adjNode.position] = newNode;
                        }
                    }
                }

            }

        }
        //UnityEngine.Debug.Log("NO PATH FOUND");
        return null;
    }

    public List<MapNode> GetSTAStarPath(MAPFAgent agent, bool shouldReservePath, bool useImprovedHeuristic, int timeOfLastConstraint)
    {
        MAPFNode source = new MAPFNode(agent.currentNode, 0, 0, startingTimestep, null);
        List<MapNode> path = new List<MapNode>();
        SimplePriorityQueue<MAPFNode> openList = new SimplePriorityQueue<MAPFNode>();
        Dictionary<(Vector2Int,int),MAPFNode> closedList = new Dictionary<(Vector2Int, int), MAPFNode>();
        Dictionary<(Vector2Int, int), MAPFNode> openListDict = new Dictionary<(Vector2Int, int), MAPFNode>();
        MAPFNode workingNode;
        bool reachedDestination = false;
        Stopwatch sw = new();
        sw.Start();
  
        openList.Enqueue(source, source.GetCost());
        while (openList.Count != 0)
        {
            if (sw.ElapsedMilliseconds > 30000) return null;
            workingNode = openList.Dequeue();
            openListDict.Remove((workingNode.node.position, workingNode.time));
            if (workingNode.PositionIsEqualTo(agent.destinationNode))
            {
                if (!reachedDestination)
                {
                    finalTimestep = workingNode.time;
                    reachedDestination = true;
                }
                if (workingNode.time < timeOfLastConstraint)
                {
                    ProcessAdjacentNodes();
                    continue;
                }
                else
                {
                    openList = new();
                    if (workingNode.time < longestTimestep)
                    {
                        ProcessAdjacentNodes();
                        continue;
                    }
                    if (finalTimestep > longestTimestep) //if this path is longer than any previous path, update longest timestep
                    {
                        longestTimestep = finalTimestep;
                    }
                }


                MAPFNode prevNode;
                if (shouldReservePath) goalReservations.Add(agent.destinationNode.position, workingNode.time);
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
                int nodeHValue = 0;
                if (rTable.ContainsKey((adjNode.position, workingNode.time + 1))) //if this time position is reserved at the nest timestep, dont consider it
                {
                    //UnityEngine.Debug.Log(agent.agentId +" AVOID AT " + adjNode.position + "" + (workingNode.time + 1));
                    continue;
                }
                if (edgeTable.ContainsKey((workingNode.node.position, adjNode.position, workingNode.time))) //if this edge is reserved, dont consider it
                {
                    //UnityEngine.Debug.Log(agent.agentId + " AVOID EDGE AT " + workingNode.position + "" + adjNode.position + "" +workingNode.time);
                    continue;
                }
                if (shouldReservePath) //if this is CA* or HCA* we check for goal reservations
                {
                    if (goalReservations.ContainsKey(adjNode.position)) //if this position is another previously planned agent's goal
                    {
                        if (goalReservations[adjNode.position] <= workingNode.time + 1) //if the goal has been reached before this timestep
                        {
                            continue;
                        }
                    }
                }

                if (useImprovedHeuristic)
                {
                    nodeHValue = rraStar.GetNodeHeuristic(adjNode);
                }
                else
                {
                    nodeHValue = CalculateManhattan(adjNode, agent.destinationNode);
                }
                MAPFNode newNode = new MAPFNode(adjNode, workingNode.g + 1, nodeHValue, workingNode.time + 1, workingNode);
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



                if (!openListDict.ContainsKey((newNode.node.position, newNode.time)))
                {
                    openList.Enqueue(newNode, newNode.GetCost());
                    openListDict.Add((newNode.node.position , newNode.time), newNode);
                }
                else 
                {
                    if(openListDict[(newNode.node.position, newNode.time)].GetCost()> newNode.GetCost()) //update path to the node if a better one is found
                    {
                        openListDict[(newNode.node.position, newNode.time)].g = newNode.g;
                        openListDict[(newNode.node.position, newNode.time)].parent = workingNode;
                        //openListDict[(newNode.node.position, newNode.time)] = newNode;
                    }
                    
                }

            }
        }
    }
    public int CalculateManhattan(MapNode start, MapNode end)
    {
        return (Mathf.Abs(start.position.x - end.position.x) + Mathf.Abs(start.position.y - end.position.y));
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
