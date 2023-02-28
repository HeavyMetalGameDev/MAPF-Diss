using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class Node : MonoBehaviour, IComparable<Node>
{
    [SerializeField] public Vector2 _position;
    [SerializeField] public NodeTypeEnum nodeType;

    public int CompareTo(Node compareNode)
    {
        if(_position.x > compareNode._position.x || _position.y > compareNode._position.y)
        {
            return 1;
        }
        else if (_position.x < compareNode._position.x || _position.y < compareNode._position.y)
        {
            return -1;
        }
        else
        {
            return 0;
        }
    }
    private void Awake()
    {
        _position = new Vector2(transform.position.x, transform.position.z);
    }
}
