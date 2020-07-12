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
    public Transform resetAreaLowerLeft;
    public Transform resetAreaUpperRight;
    public GameObject target;
    public GameObject sensorOrigin;
    public GameObject obistcaleCars;
    DateTime episodeBeginTime;
    float initialDistanceToTarget;
    float previousDistanceToTarget;
    Rigidbody rigidbody;
    bool isColliding = false;
    bool reachedTarget = false;

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
        transform.position = GetRandomStartLocation();
        transform.rotation = Quaternion.Euler(new Vector3(0, UnityEngine.Random.Range(0, 360), 0));

        obistcaleCars.GetComponent<ResetCars>().ResetCarPositions();

        episodeBeginTime = System.DateTime.Now;
        initialDistanceToTarget = (target.transform.position - transform.position).magnitude;
        previousDistanceToTarget = initialDistanceToTarget;
        isColliding = false;
        reachedTarget = false;
        StartCoroutine(CheckForPositionChange(1));
    }

    // Generates a random start position inside a specific area
    Vector3 GetRandomStartLocation(){
        float xSpawnPosition = UnityEngine.Random.Range(resetAreaLowerLeft.position.x, resetAreaUpperRight.position.x);
        float zSpawnPosition = UnityEngine.Random.Range(resetAreaLowerLeft.position.z, resetAreaUpperRight.position.z);
        return new Vector3(xSpawnPosition,0, zSpawnPosition);
    }

    /**
     * The observations consist of:
     * - 8 proximity sensors in all directions
     *  - clockwise in 45 degree steps
     * - current speed
     * - position of car
     * - position of parking lot
     * - angle of car relative to parking spot
    **/
    public override void CollectObservations(VectorSensor sensor)
    {
        // Current speed
        float currentspeed = GetComponent<Rigidbody>().velocity.magnitude;
        sensor.AddObservation(currentspeed);

        // Car position
        sensor.AddObservation(transform.position.x);
        sensor.AddObservation(transform.position.z);


        if (target != null)
        {          
            // Target position
            sensor.AddObservation(target.transform.position.x);
            sensor.AddObservation(target.transform.position.z);

            // Angle as scalar to parking spot
            Vector3 directionToTarget = target.transform.position - transform.position;
            float scalarToTarget = Vector3.Dot(transform.forward.normalized, directionToTarget.normalized);
            sensor.AddObservation(scalarToTarget);
        }

        // 8 proximity sensors
        foreach (int proximitySensorId in Enumerable.Range(0, 8))
        {
            int sensorAngle = proximitySensorId * 45;
            Quaternion sensorDirection = transform.rotation;
            sensorDirection = Quaternion.AngleAxis(sensorAngle, Vector3.up);
            float distanceToObstacle = SenseDistance(sensorDirection * transform.forward);
            if (distanceToObstacle == 0)
            {
                sensor.AddObservation(1000f);
            }
            else
            {
                sensor.AddObservation(distanceToObstacle);
            }
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

        // Negative reward if colliding with other object
        if (isColliding)
        {
            AddReward(-0.01f);
        }

        // Negative reward for each time step
        AddReward(-0.01f);

        // Reached target
        if (distanceToTarget < 1.5f)
        {
            // A single reward for just reaching the target
            if (!reachedTarget)
            {
                AddReward(1);
                reachedTarget = true;
            }

            // Coming to a stop at the target
            if (rigidbody.velocity.magnitude < 0.01f)
            {
                // If almost parallel, agent receives additional reward
                Vector3 targetMarkingDirection = new Vector3(0, 0, 1);
                float dotProduct = Vector3.Dot(targetMarkingDirection, rigidbody.transform.forward);

                if (dotProduct > 0.9)
                {
                    AddReward(1);
                }

                AddReward(1);
                EndEpisode();
            }
        }

        // Fell from platform
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

    private IEnumerator<WaitForSeconds> CheckForPositionChange(int waitTime)
    {
        Vector3 oldPosition = new Vector3(0, 0, 0);
        while (true)
        {
            yield return new WaitForSeconds(waitTime);
            if ((transform.position - oldPosition).magnitude < 0.5){
                Debug.Log("Not moving. Terminating Episode");
                EndEpisode();
            }
            oldPosition = transform.position;
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

    public void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("terminator")){
            EndEpisode();
        }
    }
}
