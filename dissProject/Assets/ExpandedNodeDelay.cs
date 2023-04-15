using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExpandedNodeDelay : MonoBehaviour
{
    [SerializeField] MeshRenderer mr;
    public int order;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(DelayAppear());
    }

    IEnumerator DelayAppear()
    {
        yield return new WaitForSeconds(.2f * order);
        mr.enabled = true;
    }
}
