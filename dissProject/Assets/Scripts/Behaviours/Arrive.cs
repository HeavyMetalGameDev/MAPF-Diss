using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrive : SteeringBehaviour
{
    [SerializeField] Kinematic character;
    [SerializeField] Kinematic target;
    [SerializeField] float targetRadius;
    [SerializeField] float slowRadius;
    [SerializeField] float timeToTarget;

    public override SteeringOutput GetSteering()
    {
        SteeringOutput result = new SteeringOutput();

        Vector2 direction = target.xzPos - character.xzPos;
        float distance = direction.magnitude;

        if (distance < targetRadius)
        {
            return null;
        }
        if (distance > slowRadius)
        {
        }
        result.angular = 0;
        return result;
    }
}
