using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using System.Collections.Generic;
using System;
using System.Linq;

public class CarAgent : Agent
{
    public List<WheelCollider> speedWheels;
    public List<WheelCollider> steeringWheels;
    public float strengthCoefficient;
    public float maxTurn;
    public Transform resetPosition;
    public GameObject target;
    private DateTime episodeBeginTime;
    private float previousDistanceToTarget;

    // Used for resetting
    public override void OnEpisodeBegin()
    {
        foreach (WheelCollider wheel in speedWheels)
        {
            wheel.motorTorque = 0.0f;
        }

        foreach (WheelCollider wheel in steeringWheels)
        {
            wheel.steerAngle = 0.0f;
        }
        GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        GetComponent<Rigidbody>().velocity = Vector3.zero;
        transform.position = resetPosition.position;
        transform.rotation = new Quaternion(0, 0, 0, 0);

        episodeBeginTime = System.DateTime.Now;
        previousDistanceToTarget = (target.transform.position - transform.position).magnitude;
    }

    /**
     * The observations consist of:
     * - 8 proximity sensors in all directions
     *  - clockwise in 45 degree steps
     * - current speed
     * - position relative to parking spot
    **/
    public override void CollectObservations(VectorSensor sensor)
    {
        // current speed
        float currentspeed = GetComponent<Rigidbody>().velocity.magnitude;
        sensor.AddObservation(currentspeed);

        // position relative to parking spot
        Vector3 directionToTarget = target.transform.position - transform.position;
        sensor.AddObservation(directionToTarget.x);
        sensor.AddObservation(directionToTarget.z);

        // 8 proximity sensors
        foreach (int proximitySensorId in Enumerable.Range(0, 8))
        {
            int sensorAngle = proximitySensorId * 45;
            Quaternion sensorDirection = transform.rotation;
            sensorDirection = Quaternion.AngleAxis(sensorAngle, Vector3.up);
            sensor.AddObservation(SenseDistance(sensorDirection * transform.forward));
        }
    }

    private float SenseDistance(Vector3 direction){
        RaycastHit hit;
        float viewDistance = 10f;
        if (Physics.Raycast(transform.position + new Vector3(0, 0.2f, 0), direction, out hit, viewDistance))
        {
            Debug.DrawRay(transform.position + new Vector3(0, 0.2f, 0), direction * hit.distance, Color.green);
        }
        else
        {
            Debug.DrawRay(transform.position + new Vector3(0, 0.2f, 0), direction * viewDistance, Color.white);
        }
        return hit.distance;
    }

    // Is called every thime actions are received (from human or neural network)
    public override void OnActionReceived(float[] vectorAction)
    {
        int maxEpisodeLength = 60;
        float distanceToTarget = (target.transform.position - transform.position).magnitude;
        float secondsRemaining = (float) (maxEpisodeLength - System.DateTime.Now.Subtract(episodeBeginTime).TotalSeconds);

        foreach (WheelCollider wheel in speedWheels)
        {
            wheel.motorTorque = strengthCoefficient * Time.deltaTime * vectorAction[0]; //speed
        }

        foreach (WheelCollider wheel in steeringWheels)
        {
            wheel.steerAngle = maxTurn * vectorAction[1]; // steering
        }
        
        // distance to target
        if (distanceToTarget < previousDistanceToTarget)
        {
            // Positive reward if getting 5 units closer to target
            for (int step = 15; step >= 5; step -= 5)
            {
                if (distanceToTarget < step && previousDistanceToTarget > step)
                {
                    SetReward(0.1f);
                    break;
                }
            }            
        } else
        {
            // Negative reward if getting 5 units farther away from target
            for (int step = 15; step >= 5; step -= 5)
            {
                if (distanceToTarget > step && previousDistanceToTarget < step)
                {
                    SetReward(-0.1f);
                    break;
                }
            }
        }
        previousDistanceToTarget = distanceToTarget;

        // reached target
        if (distanceToTarget < 1.5f)
        {            
            SetReward(secondsRemaining/maxEpisodeLength);
            EndEpisode();
        }

        // fell from platform
        if (this.transform.localPosition.y < -1)
        {
            EndEpisode();
        }

        // Time frame exceeded
        if (secondsRemaining < 0)
        {
            EndEpisode();
        }
    }

    // Used for human player input
    public override void Heuristic(float[] actionsOut)
    {
        actionsOut[0] = Input.GetAxis("Vertical");
        actionsOut[1] = Input.GetAxis("Horizontal");
    }

}
