using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SteeringBehaviour : MonoBehaviour
{
    public virtual SteeringOutput GetSteering()
    {
        return null;
    }
}
