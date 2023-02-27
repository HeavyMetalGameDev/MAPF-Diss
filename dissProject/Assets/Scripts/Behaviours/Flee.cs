using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flee : SteeringBehaviour
{
    [SerializeField] Kinematic character;
    [SerializeField] Kinematic target;

    public override SteeringOutput GetSteering()
    {
        SteeringOutput result = new SteeringOutput();

        result.linear = character.xzPos - target.xzPos;
        result.angular = 0;
        return result;
    }
}
