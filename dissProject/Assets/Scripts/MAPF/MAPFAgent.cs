using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QuikGraph.Algorithms.ShortestPath;
using QuikGraph;

public class MAPFAgent : MonoBehaviour
{
    [SerializeField]Node _destinationNode;
    public Node destinationNode { get =>_destinationNode; }

    [SerializeField] Node _currentNode;
    public Node currentNode { get => _currentNode; }

    Node _nextNode;
    public Node nextNode { get => _nextNode; }

    IEnumerable<UndirectedEdge<Node>> path;

    public void SetPath(IEnumerable<UndirectedEdge<Node>> path)
    {
        this.path = path;
    }
}
