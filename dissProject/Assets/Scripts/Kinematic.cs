using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Kinematic : MonoBehaviour
{
    public Vector2 xzPos;
    Vector2 velocity = Vector2.zero; //units per second
    float rotation = 0f; //degrees per second around y axis

    [SerializeField] float maxAcceleration = 5f;
    [SerializeField] float maxRotation = 45f;
    [SerializeField] float maxVelocity = 10f;

    [SerializeField] BehaviourAndWeight[] behaviours;

    private void Update()
    {
        xzPos = new Vector2(transform.position.x, transform.position.z);
        SteeringOutput steering = GetBlendedSteering();
        UpdateKinematicValues(steering);
    }

    private void UpdateKinematicValues(SteeringOutput steeringOutput)
    {
        float deltaTime = Time.deltaTime;
        transform.position += new Vector3(velocity.x * deltaTime,0,velocity.y*deltaTime);
        transform.Rotate(Vector3.up * rotation * deltaTime);

        velocity += steeringOutput.linear * deltaTime;
        rotation += steeringOutput.angular * deltaTime;

        if (velocity.magnitude > maxVelocity)
        {
            velocity = velocity.normalized * maxVelocity;
        }
    }

    private SteeringOutput GetBlendedSteering()
    {
        SteeringOutput steeringResult = new SteeringOutput();
        foreach (BehaviourAndWeight behaviour in behaviours)
        {
            SteeringOutput behaviourSteering = behaviour.behaviour.GetSteering();
            steeringResult.linear += behaviourSteering.linear * behaviour.weight;
            steeringResult.angular += behaviourSteering.angular * behaviour.weight;
        }

        steeringResult.linear = steeringResult.linear.normalized * Mathf.Min(steeringResult.linear.magnitude, maxAcceleration);
        steeringResult.angular = Mathf.Min(steeringResult.angular, maxRotation);

        return steeringResult;
    }

}
