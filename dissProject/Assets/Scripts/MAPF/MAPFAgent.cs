using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QuikGraph.Algorithms.ShortestPath;
using QuikGraph;

public class MAPFAgent : MonoBehaviour
{
    float timer = 0;
    [SerializeField]Node _destinationNode;
    public Node destinationNode { get =>_destinationNode; }

    [SerializeField] Node _currentNode;
    public Node currentNode { get => _currentNode; }

    [SerializeField] Node _nextNode;
    public Node nextNode { get => _nextNode; }

    public List<UndirectedEdge<Node>> path;

    Vector3 _nextVector;

    public void SetPath(List<UndirectedEdge<Node>> path)
    {
        this.path = path;
        _nextNode = path[0].Target;
        _nextVector = new Vector3(_nextNode.position.x, 0, _nextNode.position.y);
    }
    public void SetDestination(Node node)
    {
        _destinationNode = node;
        
    }
    void ArriveAtNode()
    {
        _currentNode = _nextNode;

        if (path.Count == 0)return;
        path.RemoveAt(0);
        if (path.Count == 0)
        {
            GraphGrid.agentArrived(this);
            return;
        }
        _nextNode = path[0].Target;
        _nextVector = new Vector3(_nextNode.position.x, 0, _nextNode.position.y);

    }
    private void Update()
    {
        timer += Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, _nextVector, 5*Time.deltaTime);
        if (timer>=1)
        {
            timer -= 1;
            ArriveAtNode();
        }
    }
}
