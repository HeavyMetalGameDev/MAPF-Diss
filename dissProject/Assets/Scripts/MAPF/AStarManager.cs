using System;
using System.Collections.Generic;
using UnityEngine;
using QuikGraph;
using QuikGraph.Algorithms.ShortestPath;
using QuikGraph.Algorithms.Observers;

public class AStarManager
{  
    DijkstraShortestPathAlgorithm<Node, TaggedUndirectedEdge<Node, int>> dijkstrasAlgorithm;
    AStarShortestPathAlgorithm<Node, TaggedUndirectedEdge<Node, int>> aStarAlgorithm;
    Func<TaggedUndirectedEdge<Node, int>, double> edgeCost = edge => 5 ;
    Func<Node, double> costHeuristic = edge => 5;
    BidirectionalGraph<Node, TaggedUndirectedEdge<Node, int>> _gridGraph;

    public IEnumerable<TaggedUndirectedEdge<Node, int>> ComputeDijkstraPath(Node source, Node destination)
    {
        dijkstrasAlgorithm = new DijkstraShortestPathAlgorithm<Node, TaggedUndirectedEdge<Node, int>>(_gridGraph, edgeCost);
        VertexPredecessorRecorderObserver<Node, TaggedUndirectedEdge<Node, int>> predecessors = new VertexPredecessorRecorderObserver<Node, TaggedUndirectedEdge<Node, int>>();

        using (predecessors.Attach(dijkstrasAlgorithm))
        {
            dijkstrasAlgorithm.Compute(source);
        }

        if(predecessors.TryGetPath(destination,out IEnumerable<TaggedUndirectedEdge<Node, int>> path))
        {
            foreach(TaggedUndirectedEdge<Node, int> edge in path)
            {
                Debug.Log(edge);
            }
        }
        return path;
    }

    public IEnumerable<TaggedUndirectedEdge<Node, int>> ComputeAStarPath(Node source, Node destination)
    {
        aStarAlgorithm = new AStarShortestPathAlgorithm<Node, TaggedUndirectedEdge<Node, int>>(_gridGraph, edgeCost, costHeuristic);
        VertexPredecessorRecorderObserver<Node, TaggedUndirectedEdge<Node, int>> predecessors = new VertexPredecessorRecorderObserver<Node, TaggedUndirectedEdge<Node, int>>();

        using (predecessors.Attach(aStarAlgorithm))
        {
            aStarAlgorithm.Compute(source);
        }

        if (predecessors.TryGetPath(destination, out IEnumerable<TaggedUndirectedEdge<Node, int>> path))
        {
            foreach (TaggedUndirectedEdge<Node, int> edge in path)
            {
                Debug.Log(edge);
            }
        }
        return path;
    }

    public void AttachGraph(BidirectionalGraph<Node, TaggedUndirectedEdge<Node, int>> graph)
    {
        _gridGraph = graph;
    }
}
