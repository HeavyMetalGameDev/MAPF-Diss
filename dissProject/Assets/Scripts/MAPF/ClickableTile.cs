using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickableTile : MonoBehaviour
{
    Node node;
    private void Awake()
    {
        node = GetComponent<Node>();
    }
    private void OnMouseDown()
    {
        node.nodeType = (NodeTypeEnum)(((int)node.nodeType +1)%2);
        GraphGrid.refreshGrid(node);
    }
}
