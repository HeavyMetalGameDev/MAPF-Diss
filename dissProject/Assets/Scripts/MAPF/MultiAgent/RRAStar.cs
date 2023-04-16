using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Priority_Queue;

public class RRAStar
{
    List<List<MapNode>> graph = new List<List<MapNode>>();
    Vector2 dimensions;
    Dictionary<int, Dictionary<MapNode, int>> agent;
    MAPFNode source;
    SimplePriorityQueue<MAPFNode> openList = new SimplePriorityQueue<MAPFNode>();
    public Dictionary<Vector2, MAPFNode> closedList = new Dictionary<Vector2, MAPFNode>();
    public Dictionary<Vector2, MAPFNode> openListDict = new Dictionary<Vector2, MAPFNode>();
    MAPFNode workingNode;
    public RRAStar(MapNode destinationNode, List<List<MapNode>> graph, Vector2 dimensions)
    {
        source = new MAPFNode(destinationNode, 0, 0, 0, null);
        this.graph = graph;
        this.dimensions = dimensions;
        openList.Enqueue(source, 0);
    }
    int Resume(MapNode targetNode)
    {
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
                    if (!openListDict.ContainsKey(adjNode.position))
                    {
                        openList.Enqueue(newNode, newNode.GetCost());
                        openListDict.Add(newNode.node.position, newNode);
                    }
                    else
                    {
                        if(openListDict[adjNode.position].GetCost() < newNode.GetCost())
                        {
                            openListDict[adjNode.position] = newNode;
                        }
                    }
                }
            }
        }
        return -1;
    }
    public int GetNodeHeuristic(MapNode node)
    {
        if(closedList.TryGetValue(node.position, out MAPFNode returnedNode))
        {
            return returnedNode.g;
        }
        else
        {
            return Resume(node);
        }
    }
    public int CalculateManhattan(MapNode start, MapNode end)
    {
        return (int)(Mathf.Abs(start.position.x - end.position.x) + (int)Mathf.Abs(start.position.y - end.position.y));
    }
    public List<MapNode> GetAdjacentNodes(MAPFNode node)
    {
        List<MapNode> adjacentNodes = new List<MapNode>();
        int nodeX = (int)(node.node.position.x * .2f);
        int nodeY = (int)(node.node.position.y * .2f);
        MapNode potentialNode;

        if (nodeX + 1 < dimensions.x)
        {
            potentialNode = graph[nodeY][nodeX + 1];
            if (potentialNode.nodeType.Equals(NodeTypeEnum.WALKABLE))
            {
                adjacentNodes.Add(potentialNode);
            }

        }

        if (nodeX != 0)
        {
            potentialNode = graph[nodeY][nodeX - 1];
            if (potentialNode.nodeType.Equals(NodeTypeEnum.WALKABLE))
            {
                adjacentNodes.Add(potentialNode);
            }

        }

        if (nodeY + 1 < dimensions.y)
        {
            potentialNode = graph[nodeY + 1][nodeX];
            if (potentialNode.nodeType.Equals(NodeTypeEnum.WALKABLE))
            {
                adjacentNodes.Add(potentialNode);
            }

        }


        if (nodeY != 0)
        {
            potentialNode = graph[nodeY - 1][nodeX];
            if (potentialNode.nodeType.Equals(NodeTypeEnum.WALKABLE))
            {
                adjacentNodes.Add(potentialNode);
            }

        }

        return adjacentNodes;
    }
}
