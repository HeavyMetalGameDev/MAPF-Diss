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

    /*public MAPFNode[] GetSingleAgentPath(MAPFAgent agent)
    {
        MAPFNode source = agent.currentNode;
        MAPFNode destination = agent.destinationNode;
        SimplePriorityQueue<MAPFNode> openList = new SimplePriorityQueue<MAPFNode>();
        List<Node> closedList = new List<Node>();

        openList.Enqueue(source,source.f);

        while (openList.Count!= 0 && )
    }*/
    

    public int CalculateManhattan(MAPFNode start, MAPFNode end)
    {
        return (int)Mathf.Abs(start.position.x - end.position.x) + (int)Mathf.Abs(start.position.y - end.position.y);
    }

    public List<MAPFNode> GetAdjacentNodes(MAPFNode node)
    {
        List<MAPFNode> adjacentNodes = new List<MAPFNode>();
        int nodeX = (int)(node.position.x*.2f);
        int nodeY = (int)(node.position.y * .2f);
        adjacentNodes.Add(node);
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
