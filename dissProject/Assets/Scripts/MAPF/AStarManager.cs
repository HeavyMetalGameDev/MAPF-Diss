using System;
using System.Collections.Generic;
using UnityEngine;
using QuikGraph;
using QuikGraph.Algorithms.ShortestPath;
using QuikGraph.Algorithms.Observers;

public class AStarManager
{  
    DijkstraShortestPathAlgorithm<Node, Edge<Node>> dijkstrasAlgorithm;
    AStarShortestPathAlgorithm<Node, Edge<Node>> aStarAlgorithm;
    Func<Edge<Node>, double> edgeCost = edge => 5 ;
    Func<Node, double> costHeuristic = vertex => 1; //TODO program heuristic function for A*
    BidirectionalGraph<Node, Edge<Node>> _gridGraph;


    public IEnumerable<Edge<Node>> ComputeDijkstraPath(Node source, Node destination)
    {
        dijkstrasAlgorithm = new DijkstraShortestPathAlgorithm<Node, Edge<Node>>(_gridGraph, edgeCost);
        VertexPredecessorRecorderObserver<Node, Edge<Node>> predecessors = new VertexPredecessorRecorderObserver<Node, Edge<Node>>();

        using (predecessors.Attach(dijkstrasAlgorithm))
        {
            dijkstrasAlgorithm.Compute(source);
        }

        if(predecessors.TryGetPath(destination,out IEnumerable<Edge<Node>> path))
        {
            foreach(Edge<Node> edge in path)
            {
                Debug.Log(edge);
            }
        }
        return path;
    }

    public IEnumerable<Edge<Node>> ComputeAStarPath(Node source, Node destination)
    {
        aStarAlgorithm = new AStarShortestPathAlgorithm<Node, Edge<Node>>(_gridGraph, edgeCost, costHeuristic);
        VertexPredecessorRecorderObserver<Node, Edge<Node>> predecessors = new VertexPredecessorRecorderObserver<Node, Edge<Node>>();

        using (predecessors.Attach(aStarAlgorithm))
        {
            aStarAlgorithm.Compute(source);
        }
        

        if (predecessors.TryGetPath(destination, out IEnumerable<Edge<Node>> path))
        {
            foreach (Edge<Node> edge in path)
            {
                Debug.Log(edge);
            }
        }
        return path;
    }

    public void AttachGraph(BidirectionalGraph<Node, Edge<Node>> graph)
    {
        _gridGraph = graph;
    }
}
