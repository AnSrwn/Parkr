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
    public GameObject sensorOrigin;
    public GameObject obistcaleCars;
    DateTime episodeBeginTime;
    float previousDistanceToTarget;
    Rigidbody rigidbody;
    bool isColliding = false;

    // Used for resetting
    public override void OnEpisodeBegin()
    {
        rigidbody = GetComponent<Rigidbody>();

        foreach (WheelCollider wheel in speedWheels)
        {
            wheel.motorTorque = 0.0f;
        }

        foreach (WheelCollider wheel in steeringWheels)
        {
            wheel.steerAngle = 0.0f;
        }
        rigidbody.angularVelocity = Vector3.zero;
        rigidbody.velocity = Vector3.zero;
        transform.position = resetPosition.position;
        transform.rotation = new Quaternion(0, 0, 0, 0);

        obistcaleCars.GetComponent<ResetCars>().ResetCarPositions();

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
        if (target != null)
        {
            Vector3 directionToTarget = target.transform.position - transform.position;
            sensor.AddObservation(directionToTarget.x);
            sensor.AddObservation(directionToTarget.z);
        }

        // 8 proximity sensors
        foreach (int proximitySensorId in Enumerable.Range(0, 8))
        {
            int sensorAngle = proximitySensorId * 45;
            Quaternion sensorDirection = transform.rotation;
            sensorDirection = Quaternion.AngleAxis(sensorAngle, Vector3.up);
            sensor.AddObservation(SenseDistance(sensorDirection * transform.forward));
        }
    }

    float SenseDistance(Vector3 direction){
        Transform originTransform = sensorOrigin.GetComponent<Transform>();
        RaycastHit hit;
        float viewDistance = 10f;

        if (Physics.Raycast(originTransform.position, direction, out hit, viewDistance))
        {
            Debug.DrawRay(originTransform.position, direction * hit.distance, Color.green);
        }
        else
        {
            Debug.DrawRay(originTransform.position, direction * viewDistance, Color.grey);
        }
        return hit.distance;
    }

    // Is called every time actions are received (from human or neural network)
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

        // negative reward if colliding with other object
        if (isColliding)
        {
            SetReward(-0.1f);
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
        if (distanceToTarget < 1.5f && rigidbody.velocity.magnitude < 0.01f)
        {
            // if almost parallel, agent receives additional reward
            Vector3 targetMarkingDirection = new Vector3(0, 0, 1);
            float dotProduct = Vector3.Dot(targetMarkingDirection, rigidbody.transform.forward);

            if (dotProduct > 0.9)
            {
                SetReward(0.5f);
            }        

            // minimum reward of 1 and additional reward depending on time needed
            SetReward(1 + secondsRemaining/maxEpisodeLength);
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

    public void OnCollisionEnter(Collision collision)
    {
        isColliding = true;
    }

    public void OnCollisionExit(Collision collision)
    {
        isColliding = false;
    }
}
