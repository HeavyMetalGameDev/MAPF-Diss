using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QuikGraph;
using System.Linq;
using Priority_Queue;

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
        SimplePriorityQueue<Node> openList = new SimplePriorityQueue<Node>();
        List<Node> closedList = new List<Node>();

        openList.Enqueue(source);
    }
    */
}
