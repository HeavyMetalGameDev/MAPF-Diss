using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class Node : MonoBehaviour, IComparable<Node>
{
    [SerializeField] public Vector2Int position;
    [SerializeField] public NodeTypeEnum nodeType;
    public GridMarker _nodeMarker;
    public bool isTargeted;
    public bool isOccupied;
    public float cost = 1;

    public Node(Vector2Int position, NodeTypeEnum nodeType)
    {
        this.position = position;
        this.nodeType = nodeType;
    }
    public int CompareTo(Node compareNode)
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
        position = new Vector2Int((int)transform.position.x, (int)transform.position.z);
    }
}
