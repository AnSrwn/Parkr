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
        Vector2 directionToTarget = target.transform.position - transform.position;
        List<float> proximitySensors = new List<float>(new float[8]);

        foreach (int proximitySensorId in Enumerable.Range(0, 8))
        {
            int sensorAngle = proximitySensorId * 45;
            Debug.Log(sensorAngle);
            Quaternion sensorDirection = transform.rotation;
            sensorDirection.SetAxisAngle(transform.up, sensorAngle * (Mathf.PI/180));
            proximitySensors[proximitySensorId] = SenseDistance(sensorDirection * transform.forward);
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
        foreach (WheelCollider wheel in speedWheels)
        {
            wheel.motorTorque = strengthCoefficient * Time.deltaTime * vectorAction[0]; //speed
        }

        foreach (WheelCollider wheel in steeringWheels)
        {
            wheel.steerAngle = maxTurn * vectorAction[1]; // steering
        }

        //TODO call EndEpisode() if conditions are fulfilled
    }

    // Used for human player input
    public override void Heuristic(float[] actionsOut)
    {
        actionsOut[0] = Input.GetAxis("Vertical");
        actionsOut[1] = Input.GetAxis("Horizontal");
    }

}
