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
    public DateTime episodeBeginTime;

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

        transform.position = resetPosition.position;

        episodeBeginTime = System.DateTime.Now;
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
        float currentspeed = GetComponent<Rigidbody>().velocity.magnitude;
        sensor.AddObservation(currentspeed);

        Vector3 directionToTarget = target.transform.position - transform.position;
        sensor.AddObservation(directionToTarget.x);
        sensor.AddObservation(directionToTarget.z);

        foreach (int proximitySensorId in Enumerable.Range(0, 8))
        {
            int sensorAngle = proximitySensorId * 45;
            Quaternion sensorDirection = transform.rotation;
            sensorDirection.SetAxisAngle(transform.up, sensorAngle * (Mathf.PI/180));
            sensor.AddObservation(SenseDistance(sensorDirection * transform.forward));
        }
    }

    private float SenseDistance(Vector3 direction){
        RaycastHit hit;
        float viewDistance = Mathf.Infinity;
        if (Physics.Raycast(transform.position, direction, out hit, viewDistance))
        {
            Debug.DrawRay(transform.position, direction * hit.distance, Color.green);
        }
        else
        {
            Debug.DrawRay(transform.position, direction * Mathf.Min(1000, viewDistance), Color.white);
        }
        return hit.distance;
    }

    // Is called every thime actions are received (from human or neural network)
    public override void OnActionReceived(float[] vectorAction)
    {
        int maxEpisodeLength = 60;
        float secondsRemaining = (float) (maxEpisodeLength - System.DateTime.Now.Subtract(episodeBeginTime).TotalSeconds);

        foreach (WheelCollider wheel in speedWheels)
        {
            wheel.motorTorque = strengthCoefficient * Time.deltaTime * vectorAction[0]; //speed
        }

        foreach (WheelCollider wheel in steeringWheels)
        {
            wheel.steerAngle = maxTurn * vectorAction[1]; // steering
        }

        // reached target
        if ((target.transform.position - transform.position).magnitude < 0.2f)
        {
            SetReward(secondsRemaining/maxEpisodeLength);
            EndEpisode();
        }

        // fell from platform
        if (this.transform.localPosition.y < -5)
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
