using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using System.Collections.Generic;

public class CarAgent : Agent
{
    public List<WheelCollider> speedWheels;
    public List<WheelCollider> steeringWheels;
    public float strengthCoefficient;
    public float maxTurn;

    // Used for resetting
    public override void OnEpisodeBegin()
    {
       //TODO
    }

    /**
     * The Agent sends the information we collect to the Brain, which uses it to make a decision.
     * When you train the Agent (or use a trained model), the data is fed into a neural network as
     * a feature vector.
    **/
    public override void CollectObservations(VectorSensor sensor)
    {
        //TODO
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
