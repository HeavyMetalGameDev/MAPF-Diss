using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class MAPFNode : MonoBehaviour, IComparable<MAPFNode>
{
    [SerializeField] public Vector2 position;
    [SerializeField] public NodeTypeEnum nodeType;
    public GridMarker _nodeMarker;
    public bool isTargeted;
    public bool isOccupied;
    public float cost = 1;

    public MAPFNode(Vector2 position, NodeTypeEnum nodeType)
    {
        this.position = position;
        this.nodeType = nodeType;
    }
    public int CompareTo(MAPFNode compareNode)
    {
       // if(position.x > compareNode.position.x || position.y > compareNode.position.y)
       // {
            return -1;
       // }
       // else if (position.x < compareNode.position.x || position.y < compareNode.position.y)
       // {
      //      return -1;
      //  }
      //  else
      //  {
       //     return 0;
      //  }
    }
    private void Awake()
    {
        position = new Vector2(transform.position.x, transform.position.z);
    }

    public override string ToString()
    {
        return "Node: "+position + " isOccupied: " + isOccupied + " NodeType: " + nodeType;
    }
}
