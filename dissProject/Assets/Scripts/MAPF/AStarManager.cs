using System;
using System.Collections.Generic;
using UnityEngine;
using QuikGraph;
using QuikGraph.Algorithms.ShortestPath;
using QuikGraph.Algorithms.Observers;

public class AStarManager
{  
    DijkstraShortestPathAlgorithm<Node, UndirectedEdge<Node>> dijkstrasAlgorithm;
    AStarShortestPathAlgorithm<Node, UndirectedEdge<Node>> aStarAlgorithm;
    Func<UndirectedEdge<Node>, double> edgeCost = edge => 5 ;
    Func<Node, double> costHeuristic = edge => 5; //TODO program heuristic function for A*
    BidirectionalGraph<Node, UndirectedEdge<Node>> _gridGraph;

    public IEnumerable<UndirectedEdge<Node>> ComputeDijkstraPath(Node source, Node destination)
    {
        dijkstrasAlgorithm = new DijkstraShortestPathAlgorithm<Node, UndirectedEdge<Node>>(_gridGraph, edgeCost);
        VertexPredecessorRecorderObserver<Node, UndirectedEdge<Node>> predecessors = new VertexPredecessorRecorderObserver<Node, UndirectedEdge<Node>>();

        using (predecessors.Attach(dijkstrasAlgorithm))
        {
            dijkstrasAlgorithm.Compute(source);
        }

        if(predecessors.TryGetPath(destination,out IEnumerable<UndirectedEdge<Node>> path))
        {
            foreach(UndirectedEdge<Node> edge in path)
            {
                Debug.Log(edge);
            }
        }
        return path;
    }

    public IEnumerable<UndirectedEdge<Node>> ComputeAStarPath(Node source, Node destination)
    {
        aStarAlgorithm = new AStarShortestPathAlgorithm<Node, UndirectedEdge<Node>>(_gridGraph, edgeCost, costHeuristic);
        VertexPredecessorRecorderObserver<Node, UndirectedEdge<Node>> predecessors = new VertexPredecessorRecorderObserver<Node, UndirectedEdge<Node>>();

        using (predecessors.Attach(aStarAlgorithm))
        {
            aStarAlgorithm.Compute(source);
        }

        if (predecessors.TryGetPath(destination, out IEnumerable<UndirectedEdge<Node>> path))
        {
            foreach (UndirectedEdge<Node> edge in path)
            {
                Debug.Log(edge);
            }
        }
        return path;
    }

    public void AttachGraph(BidirectionalGraph<Node, UndirectedEdge<Node>> graph)
    {
        _gridGraph = graph;
    }
}
