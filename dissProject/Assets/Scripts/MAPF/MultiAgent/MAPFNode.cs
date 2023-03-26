using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

[Serializable]
public class MAPFNode : IEquatable<MAPFNode>
{
    [SerializeField] public Vector2 position;
    [SerializeField] public NodeTypeEnum nodeType;
    public GridMarker _nodeMarker;
    public bool isTargeted;
    public bool isOccupied;
    public MAPFNode parent;
    public int time;
    public int g;
    public int h;
    public int f;

    public MAPFNode(Vector2 position, NodeTypeEnum nodeType)
    {
        this.position = position;
        this.nodeType = nodeType;
    }
    public MAPFNode()
    {
    }
    public bool IsEqualTo(MAPFNode compareNode)
    {
        if (position.Equals(compareNode.position))
        {
            return true;
        }
        return false;
    }
    public void SetPos(Vector2 pos)
    {
        position = pos;
    }

    public override string ToString()
    {
        return "Node: "+position + " isOccupied: " + isOccupied + " NodeType: " + nodeType;
    }
    public int GetFCost()
    {
        f = g + h;
        return f;
    }

    public bool Equals(MAPFNode node)
    {
        if (node.position.Equals(position))
        {
            return true;
        }
        return false;
    }
}
