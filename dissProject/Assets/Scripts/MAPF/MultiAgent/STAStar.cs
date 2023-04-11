using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QuikGraph;
using System.Linq;
using Priority_Queue;
using System.Diagnostics;

public class STAStar
{
    public List<List<MAPFNode>> _graph = new List<List<MAPFNode>>();
    Vector2 dimensions;
    public Hashtable rTable = new Hashtable(); //reservation table for node positions
    public Hashtable edgeTable = new Hashtable(); //reservation table for edge traversal
    public int startingTimestep=0;
    public STAStar(List<List<MAPFNode>> Graph, Vector2 Dimensions)
    {
        int yCount = 0;
        foreach(List<MAPFNode> yNodes in Graph)
        {
            int xCount = 0;
            _graph.Add(new List<MAPFNode>());
            foreach(MAPFNode xNodes in yNodes)
            {
                _graph[yCount].Add(new MAPFNode(Graph[yCount][xCount].position, Graph[yCount][xCount].nodeType));
                xCount += 1;
            }
            yCount += 1;
        }
        dimensions = Dimensions;
    }
    public STAStar()
    {

    }
    public void SetSTAStar(List<List<MAPFNode>> Graph, Vector2 Dimensions)
    {
        int yCount = 0;
        _graph = new List<List<MAPFNode>>();
        foreach (List<MAPFNode> yNodes in Graph)
        {
            int xCount = 0;
            _graph.Add(new List<MAPFNode>());
            foreach (MAPFNode xNodes in yNodes)
            {
                _graph[yCount].Add(new MAPFNode(Graph[yCount][xCount].position, Graph[yCount][xCount].nodeType));
                xCount += 1;
            }
            yCount += 1;
        }
        dimensions = Dimensions;
    }

    public List<MAPFNode> GetSingleAgentPath(MAPFAgent agent)
    {
        Stopwatch sw = new Stopwatch();
        MAPFNode source = agent.currentNode;
        MAPFNode destination = agent.destinationNode;
        List<MAPFNode> path = new List<MAPFNode>();
        SimplePriorityQueue<MAPFNode> openList = new SimplePriorityQueue<MAPFNode>();
        List<MAPFNode> closedList = new List<MAPFNode>();
        MAPFNode workingNode = new MAPFNode();
        openList.Enqueue(source,source.f);
        sw.Start();
        while (openList.Count!= 0)
        {
            workingNode = openList.Dequeue();
            if (workingNode.PositionIsEqualTo(destination))
            {
                break;
            }
            closedList.Add(workingNode);
            
            foreach (MAPFNode adjNode in GetAdjacentNodes(workingNode))
            {
                if (closedList.Contains(adjNode))
                {
                    
                    continue;
                }
                
                if (!openList.Contains(adjNode))
                {
                    adjNode.g = workingNode.g + 5;
                    adjNode.h = CalculateManhattan(adjNode, destination);
                    adjNode.parent = workingNode;

                    openList.Enqueue(adjNode, adjNode.GetFCost());
                }
                else
                {
                    if (adjNode.GetFCost() >= workingNode.g + 5 + adjNode.h)
                    {
                        adjNode.g = workingNode.g + 5;
                        openList.UpdatePriority(adjNode,adjNode.GetFCost());
                        adjNode.parent = workingNode;
                    }
                }

            }
            
        }
        sw.Stop();
        UnityEngine.Debug.Log(sw.ElapsedMilliseconds);
        sw.Reset();

        while (workingNode.parent != null)
        {
            //Debug.Log(workingNode + " - " + workingNode.parent);
            path.Add(workingNode);
            workingNode = workingNode.parent;
        }
        path.Reverse();

        return path;
    }

    public List<MAPFNode> GetSTAStarPath(MAPFAgent agent)
    {
        Stopwatch sw = new Stopwatch();
        MAPFNode source = _graph[(int)(agent.currentNode.position.y*.2f)][(int)(agent.currentNode.position.x * .2f)];
        source.time = startingTimestep;
        MAPFNode destination = _graph[(int)(agent.destinationNode.position.y * .2f)][(int)(agent.destinationNode.position.x * .2f)];
        List<MAPFNode> path = new List<MAPFNode>();
        SimplePriorityQueue<MAPFNode> openList = new SimplePriorityQueue<MAPFNode>();
        List<MAPFNode> closedList = new List<MAPFNode>();
        MAPFNode workingNode = new MAPFNode();
        openList.Enqueue(source, source.f);
        sw.Start();
        int iterations = 0;
        int pathPadding = 0;
        while (openList.Count != 0)
        {
            iterations++;
            //UnityEngine.Debug.Log(workingNode.position + " " + workingNode.time);
            if (iterations >= 10000)
            {
                UnityEngine.Debug.Log("EXPANDED 10000 TIMES");
                return null;
            }
            workingNode = openList.Dequeue();
            if (workingNode.PositionIsEqualTo(destination))
            {
                pathPadding++;
                if (pathPadding <= 20)
                {
                    if (rTable.ContainsKey(workingNode.position + "" + (workingNode.time + 1))) //if this time position is reserved at the nest timestep, dont consider it
                    {
                        //UnityEngine.Debug.Log(agent.agentId +" AVOID AT " + adjNode.position + "" + (workingNode.time + 1));
                        //ProcessNewAdjacentNodes();
                        continue;
                    }
                    else
                    {
                        MAPFNode newNode = new MAPFNode(workingNode);
                        newNode.time++;
                        newNode.parent = workingNode;
                        openList.Enqueue(newNode, newNode.GetFCost());
                    }
                    continue;
                }
                //UnityEngine.Debug.Log(sw.ElapsedMilliseconds);
                MAPFNode prevNode;
                while (workingNode.parent != null)
                {

                    //Debug.Log(workingNode + " - " + workingNode.parent);
                    path.Add(workingNode);
                    rTable.Add(workingNode.position + "" + workingNode.time, agent);
                    

                    prevNode = workingNode;
                    workingNode = workingNode.parent;
                    edgeTable.Add(workingNode.position + "" + prevNode.position + "" + workingNode.time, agent);
                }
                path.Add(source);
                path.Reverse();

                return path;
            }
            closedList.Add(workingNode);

            foreach (MAPFNode adjNode in GetAdjacentNodes(workingNode))
            {
                if (closedList.Contains(adjNode))
                {
                   continue;
                } 
                if (rTable.ContainsKey(adjNode.position + "" + (workingNode.time+1))) //if this time position is reserved at the nest timestep, dont consider it
                {
                    //UnityEngine.Debug.Log(agent.agentId +" AVOID AT " + adjNode.position + "" + (workingNode.time + 1));
                    continue;
                }
                if (edgeTable.ContainsKey(workingNode.position +""+ adjNode.position + "" + workingNode.time)) //if this edge is reserved, dont consider it
                {
                    //UnityEngine.Debug.Log(agent.agentId + " AVOID EDGE AT " + workingNode.position + "" + adjNode.position + "" +workingNode.time);
                    continue;
                }
                if (edgeTable.ContainsKey(adjNode.position + "" + workingNode.position + "" + workingNode.time)) //if this edge in the opposite direction is reserved, dont consider it
                {
                    //UnityEngine.Debug.Log(agent.agentId + " AVOID EDGE AT " + adjNode.position + "" + workingNode.position + "" + workingNode.time);
                    continue;
                }
                if (!openList.Contains(adjNode) || adjNode.PositionIsEqualTo(workingNode))
                {
                    //UnityEngine.Debug.Log("SAME NODE GENERATED");
                    adjNode.g = workingNode.g + 5;
                    adjNode.time = workingNode.time + 1;
                    adjNode.h = CalculateManhattan(adjNode, destination);
                    adjNode.parent = workingNode;

                    openList.Enqueue(adjNode, adjNode.GetFCost());
                }
                else
                {
                    if (adjNode.GetFCost() >= workingNode.g + 5 + adjNode.h)
                    {
                        adjNode.g = workingNode.g + 5;
                        adjNode.time = workingNode.time + 1;
                        openList.UpdatePriority(adjNode, adjNode.GetFCost());
                        adjNode.parent = workingNode;
                    }
                }

            }
            void ProcessNewAdjacentNodes()
            {
                foreach (MAPFNode adjNode in GetAdjacentNodes(workingNode))
                {
                    MAPFNode newNode = new MAPFNode(adjNode);
                    if (closedList.Contains(newNode))
                    {
                        continue;
                    }
                    if (rTable.ContainsKey(newNode.position + "" + (workingNode.time + 1))) //if this time position is reserved at the nest timestep, dont consider it
                    {
                        //UnityEngine.Debug.Log(agent.agentId +" AVOID AT " + adjNode.position + "" + (workingNode.time + 1));
                        continue;
                    }
                    if (edgeTable.ContainsKey(workingNode.position + "" + newNode.position + "" + workingNode.time)) //if this edge is reserved, dont consider it
                    {
                        //UnityEngine.Debug.Log(agent.agentId + " AVOID EDGE AT " + workingNode.position + "" + adjNode.position + "" +workingNode.time);
                        continue;
                    }
                    if (edgeTable.ContainsKey(newNode.position + "" + workingNode.position + "" + workingNode.time)) //if this edge in the opposite direction is reserved, dont consider it
                    {
                        //UnityEngine.Debug.Log(agent.agentId + " AVOID EDGE AT " + adjNode.position + "" + workingNode.position + "" + workingNode.time);
                        continue;
                    }
                    if (!openList.Contains(newNode))
                    {
                        //UnityEngine.Debug.Log("SAME NODE GENERATED");
                        newNode.g = workingNode.g + 5;
                        newNode.time = workingNode.time + 1;
                        newNode.h = CalculateManhattan(newNode, destination);
                        newNode.parent = workingNode;

                        openList.Enqueue(newNode, newNode.GetFCost());
                    }
                    else
                    {
                        if (newNode.GetFCost() >= workingNode.g + 5 + newNode.h)
                        {
                            newNode.g = workingNode.g + 5;
                            newNode.time = workingNode.time + 1;
                            openList.UpdatePriority(newNode, newNode.GetFCost());
                            newNode.parent = workingNode;
                        }
                    }

                }
            }
        }
        //UnityEngine.Debug.Log("NO PATH FOUND");
        path = null;
        return path;

    }
    public int CalculateManhattan(MAPFNode start, MAPFNode end)
    {
        return (int)(Mathf.Abs(start.position.x - end.position.x) + (int)Mathf.Abs(start.position.y - end.position.y));
    }

    public List<MAPFNode> GetAdjacentNodes(MAPFNode node)
    {
        List<MAPFNode> adjacentNodes = new List<MAPFNode>();
        int nodeX = (int)(node.position.x*.2f);
        int nodeY = (int)(node.position.y * .2f);
        MAPFNode potentialNode; 

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
