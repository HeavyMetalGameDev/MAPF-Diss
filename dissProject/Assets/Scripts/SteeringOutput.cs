using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SteeringOutput
{
    public Vector2 linear { get; set; }
    public float angular { get; set; }

    public SteeringOutput(Vector3 linear, float angular)
    {
        this.linear = linear;
        this.angular = angular;
    }
    public SteeringOutput()
    {
    }

}
