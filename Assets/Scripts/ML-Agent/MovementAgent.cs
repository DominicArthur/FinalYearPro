using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine;
using System.Xml.Schema;
using UnityEngine.UIElements;
using System.Data.Common;
using UnityEditor;

public class MovementAgent : Agent
{
    public float moveSpeed = 3f;  // Movement speed
    public float maxVelocity = 5f;
    public float boundarySize = 7f;
    public float minMovementThreshold = 0.1f;
    private float stuckTimer = 0f;
    
    //private float totalEpisodeReward = 0f;  // Track rewards per episode

    private Rigidbody2D rb;
    private Vector2 lastPosition;
    private Vector2 lastVelocity;
    private float totalDistance;
    private float episodeTimer;
    private const float MAX_EPISODE_TIME = 20f;
    private const float STUCK_THRESHOLD = 3f;
    public Transform target;

    public override void Initialize()
    {   
        Debug.Log("Agent Initialized!");

         // Check if Academy is running
         if (!Academy.IsInitialized)
         {
             Debug.LogError("ML-Agents Academy is not initialized! This could prevent the agent from receiving actions.");
         }

        rb = GetComponent<Rigidbody2D>();

         if (rb == null)
        {
             Debug.LogError("No Rigidbody2D found on agent!");
             return;
        }

        rb.gravityScale = 0;
        rb.drag = 0.5f;
        Physics2D.queriesStartInColliders = false;
    }
    public override void OnEpisodeBegin()
    {
        // Reset Metrics
        totalDistance = 0f;
        episodeTimer = 0f;
        stuckTimer = 0f;

        // Reset Physics
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
        lastVelocity = Vector2.zero;

        // Reset Position
         Vector2 safePosition = GetSafeRandomPosition();
         transform.position = safePosition;
         lastPosition = safePosition;

        // Moves target to a new random position
        target.position = GetSafeRandomPosition();

        Debug.Log($"New Episode! Agent Reset to: {transform.position}, Target at {target.position}");
    }

    private Vector2 GetSafeRandomPosition()
    {
        Vector2 position;
        int maxAttempts = 10;
        int attempts = 0;

       do{
        position = new Vector2(
            Random.Range(-boundarySize + 1, boundarySize - 1),
            Random.Range(-boundarySize + 1, boundarySize - 1)
        );
        attempts++;
        Debug.Log($"Trying Position: {position} (Attempt {attempts})");
       }
       while (IsPositionObstructed(position) && attempts < maxAttempts);

       Debug.Log($"Final Spawn Position: {position}");
       return position;
    }

    private bool IsPositionObstructed(Vector2 position)
    {  
        // Checks if the position is clear of obstacles
       Collider2D[] colliders = Physics2D.OverlapCircleAll(position, 0.5f);
       return colliders.Length > 0;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Agent's position as observations
        sensor.AddObservation(transform.position.x / boundarySize);  // 1st observation (X position)
        sensor.AddObservation(transform.position.y / boundarySize);  // 2nd observation (Y position)

        // Target Position
        sensor.AddObservation(target.position.x / boundarySize);
        sensor.AddObservation(target.position.y / boundarySize);

         // Agent's velocity to allow it to learn acceleration & stopping
        sensor.AddObservation(rb.velocity.x / maxVelocity);
        sensor.AddObservation(rb.velocity.y / maxVelocity);

        // Distance from boundaries
        float distanceFromBoundary = Mathf.Min( 
            (boundarySize - Mathf.Abs(transform.position.x)) / boundarySize,
            (boundarySize - Mathf.Abs(transform.position.y)) / boundarySize
        );
        sensor.AddObservation(distanceFromBoundary);

        // Distance from target
        float distanceToTarget = Vector2.Distance(transform.position, target.position);
        sensor.AddObservation(distanceToTarget / boundarySize);

        // Time remaining in episode
        sensor.AddObservation(episodeTimer / MAX_EPISODE_TIME);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {   
        Debug.Log("OnActionReceived is running!");
       //Heuristic(actions);

       // Updates Timer
       episodeTimer += Time.fixedDeltaTime;

       // Get movement actions
       float moveX = actions.ContinuousActions[0];
       float moveY = actions.ContinuousActions[1];

       // Prevent agent from standing still when it gets zero actions
       if (Mathf.Abs(moveX) < 0.05f) moveX += Mathf.Sign(Random.Range(-1f, 1f)) * 0.1f;
       if (Mathf.Abs(moveY) < 0.05f) moveY += Mathf.Sign(Random.Range(-1f, 1f)) * 0.1f;

       Debug.Log($"Actions Received - X: {moveX}, Y: {moveY}");

       Vector2 moveDirection = new Vector2(moveX, moveY);

       // Apply Movement
       rb.velocity = moveDirection * moveSpeed;

       // Debug Rigidbody movement
       Debug.Log($"Applying Velocity: {rb.velocity}");

       // Clamp Velocity
       rb.velocity = Vector2.ClampMagnitude(rb.velocity, maxVelocity);

       // Calculate movement metrics
       Vector2 currentPosition = transform.position;
       float deltaDistance = Vector2.Distance(lastPosition, currentPosition);
       totalDistance += deltaDistance;

       // Calculate Rewards
       CalculateRewards(currentPosition, deltaDistance);

       // Update last position and velocity
       lastPosition = currentPosition;
       lastVelocity = rb.velocity;

       // Check for episode and conditions
       CheckEndEpisode();
    }

    private void CalculateRewards(Vector2 currentPosition, float deltaDistance)
    {
        float totalReward = 0f;

        // Reward for moving toward the target
        float distanceToTarget = Vector2.Distance(currentPosition, target.position);
        float movementReward = (1.0f - (distanceToTarget / boundarySize)) * 0.5f;
        totalReward += movementReward;

        // Extra reward for reaching the target
        if (distanceToTarget < 0.5f)
         {
             totalReward += 2.0f; // Big reward for reaching the target
             Debug.Log("Agent reached the goal! Ending episode.");
             EndEpisode();
        }

        // Movement Reward - encourages controlled movement
        if (deltaDistance > minMovementThreshold)
        {
            totalReward += Mathf.Clamp(deltaDistance * 0.5f, 0.01f, 0.5f);  
        }

        // Exploration reward - encourage moving over time
        float explorationReward = 0.02f;
        totalReward += explorationReward;

        // Wall Proximity Reward - encourages staying near walls
        float distanceFromBoundary = Mathf.Min(
            boundarySize - Mathf.Abs(currentPosition.x),
            boundarySize - Mathf.Abs(currentPosition.y)
        );

        float wallReward = 0f;
        if(distanceFromBoundary < 2.0f)
        {
            wallReward = 0.3f * (1.0f - distanceFromBoundary/2.0f);
            totalReward += wallReward;
        }

        // Stability reward - encourages smooth changes in direction
        float accelerationMagnitude = ((rb.velocity - lastVelocity) / Time.fixedDeltaTime).magnitude;
        float stabilityReward = 0.05f * (1.0f - Mathf.Min(accelerationMagnitude/ maxVelocity, 1.0f));
        totalReward += stabilityReward;

        Debug.Log($"Reward Breakdown -> Move: {deltaDistance * 0.5f}, Explore: {explorationReward}, Wall: {wallReward}, Stability: {stabilityReward}, Total: {totalReward}");

        AddReward(totalReward);
    }
    
     private void CheckEndEpisode()
    {  
       Debug.Log("CheckEndEpisode() is being called!");
       Debug.Log($"Checking Out-of-Bounds: Position {transform.position}, Boundary: {boundarySize}");

       bool outOfBounds = Mathf.Abs(transform.position.x) > boundarySize || 
                       Mathf.Abs(transform.position.y) > boundarySize;

      if (outOfBounds)
      {   
        Debug.Log("Agent went out of bounds! Ending episode.");
        AddReward(-1.0f);  // Penalize going out of bounds
        EndEpisode();  // Properly end the episode instead of teleporting
        return;
      }

     // Check if the agent is stuck for too long
    if (rb.velocity.magnitude < 0.05f)  // Small movement still counts as movement
    {
        stuckTimer += Time.fixedDeltaTime;

        Debug.Log($"Agent Velocity: {rb.velocity.magnitude}, Stuck Timer: {stuckTimer}");

        if (stuckTimer >= STUCK_THRESHOLD)
        {
            Debug.Log("Agent has been stuck for too long! Ending episode.");
            AddReward(-0.1f);
            EndEpisode();
        }
    }
    else
    {
        stuckTimer = 0f; // Reset timer when the agent moves
    }
    }
    public override void Heuristic(in ActionBuffers actionsOut)
    {   
        Debug.Log("Heuristic mode active. Checking input...");

        var continuousActions = actionsOut.ContinuousActions;

        // Get input and immediately log it
         float horizontalInput = Input.GetAxisRaw("Horizontal");
         float verticalInput = Input.GetAxisRaw("Vertical");
         Debug.Log($"Raw Input - Horizontal: {horizontalInput}, Vertical: {verticalInput}");

        continuousActions[0] = Input.GetAxisRaw("Horizontal");
        continuousActions[1] = Input.GetAxisRaw("Vertical");

        Debug.Log($"Heuristic Controls - X: {continuousActions[0]}, Y: {continuousActions[1]}");
    }

    private void FixedUpdate()
    {
        RequestDecision();  // Tells the agent to request a new action
    }
}
