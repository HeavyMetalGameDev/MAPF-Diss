using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QuikGraph;
using System.Linq;
using Priority_Queue;

public class STAStar
{
    List<List<MAPFNode>> _graph;
    Vector2[] _dirs = 
        {Vector2.left,Vector2.right,Vector2.up,Vector2.down };
    Vector2 dimensions;
    public STAStar(List<List<MAPFNode>> Graph, Vector2 Dimensions)
    {
        _graph = Graph;
        dimensions = Dimensions;
    }

    public List<MAPFNode> GetSingleAgentPath(MAPFAgent agent)
    {
        MAPFNode source = agent.currentNode;
        MAPFNode destination = agent.destinationNode;
        List<MAPFNode> path = new List<MAPFNode>();
        SimplePriorityQueue<MAPFNode> openList = new SimplePriorityQueue<MAPFNode>();
        List<MAPFNode> closedList = new List<MAPFNode>();
        MAPFNode workingNode = new MAPFNode();
        openList.Enqueue(source,source.f);

        while (openList.Count!= 0)
        {
            workingNode = openList.Dequeue();
            if (workingNode.IsEqualTo(destination))
            {
                break;
            }
            closedList.Add(workingNode);

            foreach(MAPFNode adjNode in GetAdjacentNodes(workingNode))
            {
                if (closedList.Contains(adjNode)) continue;
                if (!openList.Contains(adjNode))
                {
                    adjNode.g = workingNode.g + 1;
                    adjNode.h = CalculateManhattan(adjNode, destination);
                    adjNode.parent = workingNode;

                    openList.Enqueue(adjNode, adjNode.GetFCost());
                }
                else
                {
                    if (adjNode.GetFCost() > workingNode.g + 1 + adjNode.h)
                    {
                        adjNode.g = workingNode.g + 1;
                        adjNode.GetFCost();
                        adjNode.parent = workingNode;
                    }
                }
            }
        }
        while (workingNode.parent != null)
        {
            path.Add(workingNode);
            workingNode = workingNode.parent;
        }
        path.Reverse();
        return path;
    }
    

    public int CalculateManhattan(MAPFNode start, MAPFNode end)
    {
        return (int)Mathf.Abs(start.position.x - end.position.x) + (int)Mathf.Abs(start.position.y - end.position.y);
    }

    public List<MAPFNode> GetAdjacentNodes(MAPFNode node)
    {
        List<MAPFNode> adjacentNodes = new List<MAPFNode>();
        int nodeX = (int)(node.position.x*.2f);
        int nodeY = (int)(node.position.y * .2f);
        //adjacentNodes.Add(node); Add back when performing STA* as this introduces a wait action.
        MAPFNode potentialNode;

        if(nodeX + 1 < dimensions.x)
        {
            potentialNode = _graph[nodeY][nodeX + 1];
            if (potentialNode.nodeType.Equals(NodeTypeEnum.WALKABLE))
            {
                adjacentNodes.Add(potentialNode);
                Debug.Log(potentialNode);
            }

        }

        if (nodeX!=0)
        {
            potentialNode = _graph[nodeY][nodeX - 1];
            if (potentialNode.nodeType.Equals(NodeTypeEnum.WALKABLE))
            {
                adjacentNodes.Add(potentialNode);
                Debug.Log(potentialNode);
            }

        }

        if (nodeY + 1 < dimensions.y)
        {
            potentialNode = _graph[nodeY+1][nodeX];
            if (potentialNode.nodeType.Equals(NodeTypeEnum.WALKABLE))
            {
                adjacentNodes.Add(potentialNode);
                Debug.Log(potentialNode);
            }

        }
        

        if (nodeY!=0)
        {
            potentialNode = _graph[nodeY-1][nodeX];
            if (potentialNode.nodeType.Equals(NodeTypeEnum.WALKABLE))
            {
                adjacentNodes.Add(potentialNode);
                Debug.Log(potentialNode);
            }

        }

        return adjacentNodes;
    }
}
