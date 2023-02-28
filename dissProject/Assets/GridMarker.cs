using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridMarker : MonoBehaviour
{
    public void ToggleMarker(bool isWalkable)
    {
        if (isWalkable)
        {
            GetComponentInChildren<SpriteRenderer>().color = Color.green;
        }
        else
        {
            GetComponentInChildren<SpriteRenderer>().color = Color.red;
        }
    }
}
