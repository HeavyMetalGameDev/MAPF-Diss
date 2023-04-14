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
    public Dictionary<string,MAPFAgent> rTable = new Dictionary<string, MAPFAgent>(); //reservation table for node positions
    public Dictionary<string, MAPFAgent> edgeTable = new Dictionary<string, MAPFAgent>(); //reservation table for edge traversal
    public int startingTimestep=0;
    RRAStar rraStar;
    Stopwatch _sw = new Stopwatch();
    public STAStar()
    {

    }
    public void SetSTAStar(List<List<MapNode>> Graph, Vector2 Dimensions, RRAStar rraStar)
    {
        _graph = Graph;
        dimensions = Dimensions;
        this.rraStar = rraStar;
    }
    public void SetSTAStar(List<List<MapNode>> Graph, Vector2 Dimensions)
    {
        _graph = Graph;
        dimensions = Dimensions;
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


                return path;
            }
            closedList.Add(workingNode.node.position, workingNode);
            
            foreach (MapNode adjNode in GetAdjacentNodes(workingNode))
            {
                if (!closedList.ContainsKey(adjNode.position))
                {
                    MAPFNode newNode = new MAPFNode(adjNode, workingNode.g + 5, CalculateManhattan(adjNode, agent.destinationNode), 0, workingNode);
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

    public List<MapNode> GetSTAStarPath(MAPFAgent agent, bool shouldReservePath)
    {
        MAPFNode source = new MAPFNode(agent.currentNode, 0, 0, startingTimestep, null);
        List<MapNode> path = new List<MapNode>();
        SimplePriorityQueue<MAPFNode> openList = new SimplePriorityQueue<MAPFNode>();
        Dictionary<(Vector2,int),MAPFNode> closedList = new Dictionary<(Vector2, int), MAPFNode>();
        
        MAPFNode workingNode;
        openList.Enqueue(source, source.GetCost());
        while (openList.Count != 0)
        {
            workingNode = openList.Dequeue();
            if (workingNode.PositionIsEqualTo(agent.destinationNode))
            {
                MAPFNode prevNode;
                while (workingNode.parent != null)
                {

                    //Debug.Log(workingNode + " - " + workingNode.parent);
                    path.Add(workingNode.node);

                    if (shouldReservePath)
                    {
                        rTable.Add(workingNode.node + "" + workingNode.time, agent);
                        prevNode = workingNode;
                        workingNode = workingNode.parent;
                        edgeTable.Add(workingNode.node + "" + prevNode.node + "" + workingNode.time, agent);
                        if (!workingNode.PositionIsEqualTo(prevNode.node))
                        {
                            edgeTable.Add(prevNode.node + "" + workingNode.node + "" + workingNode.time, agent);
                        }
                    }
                    else
                    {
                        workingNode = workingNode.parent;
                    }
                    

                }
                path.Add(agent.currentNode);
                path.Reverse();
                //UnityEngine.Debug.Log(openList.Count);
                //UnityEngine.Debug.Log(closedList.Count);
                return path;
            }
            closedList.Add((workingNode.node.position, workingNode.time),workingNode);
            foreach (MapNode adjNode in GetAdjacentNodes(workingNode))
            {
                MAPFNode newNode = new MAPFNode(adjNode, workingNode.g + 5, rraStar.GetNodeHeuristic(adjNode), workingNode.time + 1, workingNode);
                if (closedList.ContainsKey((adjNode.position, workingNode.time+1)))
                {
                    closedList.TryGetValue((adjNode.position, workingNode.time+1), out MAPFNode closedListNode);
                    if (closedListNode.GetCost() > newNode.GetCost())
                    {
                        
                        closedList[(adjNode.position, newNode.time)] = newNode;
                        openList.Enqueue(newNode, newNode.GetCost());
                    }
                    continue;
                }
                if (rTable.ContainsKey(adjNode + "" + (workingNode.time + 1))) //if this time position is reserved at the nest timestep, dont consider it
                {
                    //UnityEngine.Debug.Log(agent.agentId +" AVOID AT " + adjNode.position + "" + (workingNode.time + 1));
                    continue;
                }
                if (edgeTable.ContainsKey(workingNode.node + "" + adjNode + "" + workingNode.time)) //if this edge is reserved, dont consider it
                {
                    //UnityEngine.Debug.Log(agent.agentId + " AVOID EDGE AT " + workingNode.position + "" + adjNode.position + "" +workingNode.time);
                    continue;
                }

                bool found = false;
                foreach (MAPFNode node in openList)
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
        //UnityEngine.Debug.Log("NO PATH FOUND");
        path = null;
        return path;

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
