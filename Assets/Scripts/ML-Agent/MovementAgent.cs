using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine;

public class HiderAgent : Agent
{
    public float moveSpeed = 3f;  // Movement speed

    public override void OnEpisodeBegin()
    {
        // Reset the hider's position at the start of training
        transform.position = new Vector3(Random.Range(-5, 5), 0, Random.Range(-5, 5));
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Observe the hider's position
        sensor.AddObservation(transform.position);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveX = actions.ContinuousActions[0];  // Left & Right movement
        float moveZ = actions.ContinuousActions[1];  // Forward & Backward movement

        Vector3 move = new Vector3(moveX, 0, moveZ) * moveSpeed * Time.deltaTime;
        transform.position += move;

        // Reward the hider for moving so it dosent just stand still 
        SetReward(0.01f);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxis("Horizontal");
        continuousActions[1] = Input.GetAxis("Vertical");
    }
}
