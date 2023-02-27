using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Seek : SteeringBehaviour
{
    [SerializeField] Kinematic character;
    [SerializeField] Kinematic target;

    public override SteeringOutput GetSteering()
    {
        SteeringOutput result = new SteeringOutput();
        
        result.linear = target.xzPos - character.xzPos;
        result.angular = 0;
        return result;
    }
}
