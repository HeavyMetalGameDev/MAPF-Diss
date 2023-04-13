using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QuikGraph;
using System.Linq;
using Priority_Queue;
using System.Diagnostics;

public class STAStar
{
    public List<List<MapNode>> _graph = new List<List<MapNode>>();
    Vector2 dimensions;
    public Hashtable rTable = new Hashtable(); //reservation table for node positions
    public Hashtable edgeTable = new Hashtable(); //reservation table for edge traversal
    public int startingTimestep=0;
    public STAStar()
    {

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
                        }
                    }
                    if (!found)
                    {
                        openList.Enqueue(newNode, newNode.GetCost());
                    }
                }

            }
            
        }
        //UnityEngine.Debug.Log("NO PATH FOUND");
        return null;
    }

    public List<MapNode> GetSTAStarPath(MAPFAgent agent)
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
                    rTable.Add(workingNode.node + "" + workingNode.time, agent);
                    

                    prevNode = workingNode;
                    workingNode = workingNode.parent;
                    edgeTable.Add(workingNode.node + "" + prevNode.node + "" + workingNode.time, agent);
                }
                path.Add(agent.currentNode);
                path.Reverse();

                return path;
            }
            
            foreach (MapNode adjNode in GetAdjacentNodes(workingNode))
            {
                MAPFNode newNode = new MAPFNode(adjNode, workingNode.g + 5, CalculateManhattan(adjNode, agent.destinationNode), workingNode.time + 1, workingNode);
                if (closedList.ContainsKey((adjNode.position, newNode.time)))
                {
                    MAPFNode closedListNode = closedList[(adjNode.position, newNode.time)];
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
                if (edgeTable.ContainsKey(adjNode + "" + workingNode.node + "" + workingNode.time)) //if this edge in the opposite direction is reserved, dont consider it
                {
                    //UnityEngine.Debug.Log(agent.agentId + " AVOID EDGE AT " + adjNode.position + "" + workingNode.position + "" + workingNode.time);
                    continue;
                }

                bool found = false;
                foreach (MAPFNode node in openList)
                {
                    if (node.PositionIsEqualTo(newNode.node))
                    {
                        found = true;
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

    public int CalculateNodeHeuristic(MapNode targetNode, MapNode destination)
    {
        MAPFNode source = new MAPFNode(destination, 0, 0, startingTimestep, null);
        SimplePriorityQueue<MAPFNode> openList = new SimplePriorityQueue<MAPFNode>();
        Dictionary<Vector2, MAPFNode> closedList = new Dictionary<Vector2, MAPFNode>();
        MAPFNode workingNode;
        openList.Enqueue(source, source.GetCost());
        while (openList.Count != 0)
        {
            workingNode = openList.Dequeue();
            if (workingNode.PositionIsEqualTo(targetNode))
            {
                return workingNode.g;
            }
            closedList.Add(workingNode.node.position, workingNode);
            foreach (MapNode adjNode in GetAdjacentNodes(workingNode))
            {
                if (!closedList.ContainsKey(adjNode.position))
                {
                    MAPFNode newNode = new MAPFNode(adjNode, workingNode.g + 5, CalculateManhattan(adjNode, targetNode), 0, workingNode);
                    bool found = false;
                    foreach (MAPFNode node in openList)
                    {
                        if (node.PositionIsEqualTo(newNode.node))
                        {
                            found = true;
                        }
                    }
                    if (!found)
                    {
                        openList.Enqueue(newNode, newNode.GetCost());
                    }
                }

            }

        }
        //UnityEngine.Debug.Log("NO PATH FOUND");
        return -1;
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
