using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

[Serializable]
public class MapNode: IEquatable<MapNode>
{
    [SerializeField] public Vector2 position;
    [SerializeField] public NodeTypeEnum nodeType;
    public GridMarker _nodeMarker;
    public bool isTargeted;
    public bool isOccupied;

    public override string ToString()
    {
        return position.ToString();
    }
    public MapNode(Vector2 position, NodeTypeEnum nodeType)
    {
        this.position = position;
        this.nodeType = nodeType;
    }
    public MapNode(MapNode nodeToCopy) //constructor for a  copy
    {
        position = nodeToCopy.position;
        nodeType = nodeToCopy.nodeType;
    }
    public MapNode()
    {
    }
    public bool Equals(MapNode compareNode)
    {
        if (position.Equals(compareNode.position)) return true;
        return false;
    }
    public void SetPos(Vector2 pos)
    {
        position = pos;
    }
}

public class MAPFNode : IEquatable<MAPFNode>
{
    public MapNode node;
    public int g;
    public int h;
    public int time;
    public MAPFNode parent;

    public MAPFNode(MapNode node, int g, int h, int time, MAPFNode parent)
    {
        this.node = node;
        this.g = g;
        this.h = h;
        this.time = time;
        this.parent = parent;
    }

    public int GetCost()
    {
        return g + h;
    }

    public bool Equals(MAPFNode compareNode)
    {
        if (node.position.Equals(compareNode.node.position) && time == compareNode.time) return true;
        return false;
    }

    public bool PositionIsEqualTo(MapNode compareNode)
    {
        return node.position.Equals(compareNode.position);
    }
}
