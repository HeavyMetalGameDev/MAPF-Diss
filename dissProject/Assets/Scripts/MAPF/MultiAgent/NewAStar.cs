using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QuikGraph;
using System.Linq;

public class NewAStar
{
    BidirectionalGraph<Node, Edge<Node>> graph;

    public NewAStar(BidirectionalGraph<Node, Edge<Node>> Graph)
    {
        graph = Graph;
    }

    /*public Node[] GetSingleAgentPath(MAPFAgent agent)
    {
        Node source = agent.currentNode;
        Node destination = agent.destinationNode;
        Queue<Node> openList = new Queue<Node>();
        List<Node> closedList = new List<Node>();

        openList.Enqueue(source);
    }
    */
}
