using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MAPFAgent : MonoBehaviour
{
    public int agentId;
    float timer = 0;
    [SerializeField] GameObject _pathmarkerPrefab;
    [SerializeField] GameObject _completemarkerPrefab;
    public MapNode destinationNode;
    public MapNode currentNode;
    public MapNode nextNode;
    public List<MapNode> path;
    public int timesteps;
    public int goalTimestep;
    bool atDestination = false;
    Vector3 _nextVector;
    Quaternion lookRotation;

    public void SetPath(List<MapNode> path)
    {
        if (path == null || path.Count == 0)
        {
            atDestination = true;
            return;
        }

        path.RemoveAt(0);
        this.path = path;
        if (path.Count == 0)
        {
            atDestination = true;
            return;
        }
        nextNode = path[0];
        _nextVector = new Vector3(nextNode.position.x, 0, nextNode.position.y);
        lookRotation = Quaternion.LookRotation((_nextVector - transform.position).normalized);
    }
    public void SetDestination(MapNode node)
    {
        destinationNode = node;
        node.isTargeted = true;
        
    }
    public void SetCurrent(MapNode node)
    {
        currentNode = node;
        //Debug.Log("CURRENT NODE SET");
    }
    void ArriveAtNode()
    {
        currentNode = nextNode;
        timesteps += 1;

        if (path.Count == 0)return; //if there is no path do not do anything
        path.RemoveAt(0);  //remove the node agent just arrived at from path
        if (path.Count == 0) //if path is now empty
        {
            Instantiate(_completemarkerPrefab, new Vector3(transform.position.x, 0, transform.position.z), Quaternion.identity);
            destinationNode.isTargeted = false;
            return;
        }
        nextNode = path[0];
        _nextVector = new Vector3(nextNode.position.x, 0, nextNode.position.y);
        lookRotation = Quaternion.LookRotation((_nextVector - transform.position).normalized);

    }
    private void Update()
    {
        if (atDestination) return;
        timer += Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, _nextVector, 10*Time.deltaTime);
        RotateWalkDirection();
        if (timer>=.5f)
        {
            //transform.position = _nextVector;
            timer -= .5f;
            ArriveAtNode();
        }
    }
    
    private void RotateWalkDirection()
    {
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5);
    }
}
