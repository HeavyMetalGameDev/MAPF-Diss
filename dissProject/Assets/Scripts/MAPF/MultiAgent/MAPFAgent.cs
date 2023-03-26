using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MAPFAgent : MonoBehaviour
{
    float timer = 0;
    [SerializeField] GameObject _pathmarkerPrefab;
    public MAPFNode destinationNode;
    public MAPFNode currentNode;
    public MAPFNode nextNode;
    public List<MAPFNode> path;

    Vector3 _nextVector;

    public void SetPath(List<MAPFNode> path)
    {
        if (path.Count == 0) return;
        this.path = path;
        nextNode = path[1];
        _nextVector = new Vector3(nextNode.position.x, 0, nextNode.position.y);
        foreach(MAPFNode node in path)
        {
            Instantiate(_pathmarkerPrefab, new Vector3(node.position.x, 0, node.position.y), Quaternion.identity);
        }
    }
    public void SetDestination(MAPFNode node)
    {
        destinationNode = node;
        node.isTargeted = true;
        
    }
    public void SetCurrent(MAPFNode node)
    {
        currentNode = node;
        Debug.Log("CURRENT NODE SET");
    }
    void ArriveAtNode()
    {
        currentNode = nextNode;

        if (path.Count == 0)return; //if there is no path do not do anything
        path.RemoveAt(0);  //remove the node agent just arrived at from path
        if (path.Count == 0) //if path is now empty
        {
            //GraphGrid.agentArrived(this); //agent arrived at destination
            destinationNode.isTargeted = false;
            return;
        }
        nextNode = path[0];
        _nextVector = new Vector3(nextNode.position.x, 0, nextNode.position.y);

    }
    private void Update()
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
    
}
