using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QuikGraph.Algorithms.ShortestPath;
using QuikGraph;

public class MAPFAgent : MonoBehaviour
{
    float timer = 0;
    [SerializeField]MAPFNode _destinationNode;
    public MAPFNode destinationNode { get =>_destinationNode; }

    [SerializeField] MAPFNode _currentNode;
    public MAPFNode currentNode { get => _currentNode; }

    [SerializeField] MAPFNode _nextNode;
    public MAPFNode nextNode { get => _nextNode; }

    public List<MAPFNode> path;

    Vector3 _nextVector;

    public void SetPath(List<MAPFNode> path)
    {
        if (path.Count == 0) return;
        this.path = path;
        _nextNode = path[1];
        _nextVector = new Vector3(_nextNode.position.x, 0, _nextNode.position.y);
    }
    public void SetDestination(MAPFNode node)
    {
        _destinationNode = node;
        node.isTargeted = true;
        
    }
    public void SetCurrent(MAPFNode node)
    {
        _currentNode = node;
        Debug.Log("CURRENT NODE SET");
    }
    void ArriveAtNode()
    {
        _currentNode = _nextNode;

        if (path.Count == 0)return; //if there is no path do not do anything
        path.RemoveAt(0);  //remove the node agent just arrived at from path
        if (path.Count == 0) //if path is now empty
        {
            //GraphGrid.agentArrived(this); //agent arrived at destination
            destinationNode.isTargeted = false;
            return;
        }
        _nextNode = path[1];
        _nextVector = new Vector3(_nextNode.position.x, 0, _nextNode.position.y);

    }
    /*private void Update()
    {
        timer += Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, _nextVector, 10*Time.deltaTime);
        if (timer>=.5f)
        {
            //transform.position = _nextVector;
            timer -= .5f;
            ArriveAtNode();
        }
    }
    */
}
