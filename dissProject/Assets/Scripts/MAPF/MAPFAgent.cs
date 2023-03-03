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

    [SerializeField] Node _nextNode;
    public Node nextNode { get => _nextNode; }

    public List<UndirectedEdge<Node>> path;

    public void SetPath(List<UndirectedEdge<Node>> path)
    {
        this.path = path;
        _nextNode = path[0].Target;
    }
    void ArriveAtNode()
    {
        transform.position = new Vector3(_nextNode.position.x, 0, _nextNode.position.y);
        if (path.Count == 0)return;
        path.RemoveAt(0);
        if (path.Count == 0) return;
        _nextNode = path[0].Target;
        
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ArriveAtNode();
        }
    }
}
