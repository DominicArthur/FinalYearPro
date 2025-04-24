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
    public Seeker seekerScript;
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
    private const float MAX_EPISODE_TIME = 60f;
    private const float STUCK_THRESHOLD = 3f;

    public Transform seeker;
    public LayerMask wallLayer;
    //public Transform target;

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
         Vector2 agentPosition = GetSafeRandomPosition();
         transform.position = agentPosition;
         lastPosition = agentPosition;

        // Moves target to a new random position
        //seeker.position = GetSafeRandomPosition();
        Vector2 seekerPosition;
        do 
        {
            seekerPosition = new Vector2(
                Random.Range(agentPosition.x - 2f, agentPosition.x + 2f), 
                Random.Range(agentPosition.y - 2f, agentPosition.y + 2f)
                );
        }       
        while (Vector2.Distance(seekerPosition, agentPosition) < 2f);
        
        seeker.position = seekerPosition;

        if(seekerScript != null)
        {   
            Debug.Log("[Agent] Calling Seeker.OnEpisodeBegin()");
            seekerScript.OnEpisodeBegin();
        }

        Debug.Log($"New Episode! Agent Reset to: {transform.position}, Target at {seeker.position}");
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log($"DIRECT COLLISION with {collision.gameObject.name}, layer: {collision.gameObject.layer}");

    if (collision.gameObject.layer == LayerMask.NameToLayer("Walls"))
    {
        Debug.Log("Agent hit a wall!");
        AddReward(-0.1f);
    }
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
        LayerMask wallLayerMask = LayerMask.GetMask("Walls");
        Collider2D[] colliders = Physics2D.OverlapCircleAll(position, 0.5f, wallLayerMask);
         return colliders.Length > 0;

       //Collider2D[] colliders = Physics2D.OverlapCircleAll(position, 0.5f);
       //return colliders.Length > 0;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Agent's position as observations
        sensor.AddObservation(transform.position.x / boundarySize);  // 1st observation (X position)
        sensor.AddObservation(transform.position.y / boundarySize);  // 2nd observation (Y position)

        // Target Position
        sensor.AddObservation(seeker.position.x / boundarySize);
        sensor.AddObservation(seeker.position.y / boundarySize);

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
        float distanceToSeeker = Vector2.Distance(transform.position, seeker.position);
        sensor.AddObservation(distanceToSeeker / boundarySize);

        // Time remaining in episode
        sensor.AddObservation(episodeTimer / MAX_EPISODE_TIME);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {   
        Debug.Log("OnActionReceived is running!");
        System.Console.WriteLine("****Testing");
       //Heuristic(actions);

       // Updates Timer
       episodeTimer += Time.fixedDeltaTime;

       // Get movement actions
       float moveX = actions.ContinuousActions[0];
       float moveY = actions.ContinuousActions[1];
       
       // Prevent agent from standing still when it gets zero actions
       //if (Mathf.Abs(moveX) < 0.05f) moveX += Mathf.Sign(Random.Range(-1f, 1f)) * 0.1f;
       //if (Mathf.Abs(moveY) < 0.05f) moveY += Mathf.Sign(Random.Range(-1f, 1f)) * 0.1f;

       //Debug.Log($"Actions Received - X: {moveX}, Y: {moveY}");

       Vector2 moveDirection = new Vector2(moveX, moveY).normalized;

       Vector2 desiredVelocity = moveDirection * moveSpeed;
       rb.velocity = Vector2.Lerp(rb.velocity, desiredVelocity, 0.1f);
    
       // Debug Rigidbody movement
       //Debug.Log($"Applying Velocity: {rb.velocity}");

       // Clamp Velocity
       rb.velocity = Vector2.ClampMagnitude(rb.velocity, maxVelocity);

       // Calculate movement metrics
       Vector2 currentPosition = transform.position;
       float deltaDistance = Vector2.Distance(lastPosition, currentPosition);
       totalDistance += deltaDistance;

       // Calculate Rewards
       CalculateRewards(currentPosition, deltaDistance);

       // Reward for surviving time
       AddReward(Time.fixedDeltaTime * 0.01f);

       // Update last position and velocity
       lastPosition = currentPosition;
       lastVelocity = rb.velocity;

       // Check for episode and conditions
       CheckEndEpisode();
    }

    private void CalculateRewards(Vector2 currentPosition, float deltaDistance)
    {
        float totalReward = 0f;
        float cornerPenalty = 0f;

        // Goal Reward System
        float distanceToSeeker = Vector2.Distance(currentPosition,seeker.position);

        // Reward for moving toward the target
        //float distanceToTarget = Vector2.Distance(currentPosition, target.position);
        //float movementReward = (1.0f - (distanceToTarget / boundarySize)) * 0.5f;
        //totalReward += movementReward;

        if (distanceToSeeker < 1.0f)
         {
            AddReward(-1.0f);
            EndEpisode();
            return;             
        }

        // Penilty for standing still near the goal
        else if(distanceToSeeker < 0.8f && rb.velocity.magnitude < 0.1f)
        {
            totalReward -= 0.1f;
        }

        // Encourage staying far away
        float escapeReward = Mathf.Clamp(distanceToSeeker / boundarySize, 0f, 1f) * 0.2f;
        totalReward += escapeReward;

        // Bonus for approaching the goal
        //float approachReward = Mathf.Clamp((1f - (distanceToSeeker / boundarySize)) * 0.2f, 0f, 0.2f);
        //float approachReward = (1f - (distanceToTarget / boundarySize)) * 0.1f;
        //totalReward += approachReward;

        // Movement Reward - encourages controlled movement
        if (deltaDistance > minMovementThreshold)
        {
            totalReward += Mathf.Clamp(deltaDistance * 0.1f, 0.01f, 0.1f);  
        }

        // Exploration reward - encourage moving over time
        float explorationReward = 0.02f;
        totalReward += explorationReward;
        
        float distanceFromBoundary = Mathf.Min(
            boundarySize - Mathf.Abs(currentPosition.x),
            boundarySize - Mathf.Abs(currentPosition.y)
        );

        // Wall Proximity Reward - encourages staying near walls
        float wallReward = 0f;
        if(distanceFromBoundary < 2.0f)
        {
            wallReward = 0.3f * (1.0f - distanceFromBoundary/2.0f);
            totalReward += wallReward;
        }

        if (distanceToSeeker < 0.8f && stuckTimer >= 2.0f)
        {

         Debug.Log("Agent loitered near the seeker too long — ending episode.");
         AddReward(-0.5f);
         EndEpisode();
         
        }       

        // Stability reward - encourages smooth changes in direction
        float accelerationMagnitude = ((rb.velocity - lastVelocity) / Time.fixedDeltaTime).magnitude;
        float stabilityReward = 0.05f * (1.0f - Mathf.Min(accelerationMagnitude/ maxVelocity, 1.0f));
        totalReward += stabilityReward;

        // Hiding reward - encorages hider to hide behind walls
        int wallsBetween = CountWallsBetweenSeekerAndHider();
        float hidingReward = Mathf.Clamp(Mathf.Pow(wallsBetween, 1.5f), 0f, 5f) * 0.04f;
        totalReward += hidingReward;
        Debug.Log($"Hiding Reward (walls between): {hidingReward}");

        //Debug.Log($"Reward Breakdown -> Move: {deltaDistance * 0.5f}, Explore: {explorationReward}, Wall: {wallReward}, Stability: {stabilityReward}, Total: {totalReward}");

        if (Mathf.Abs(transform.position.x) > boundarySize * 0.9f && Mathf.Abs(transform.position.y) > boundarySize * 0.9f)
        {
            cornerPenalty = -0.2f;
        }
        AddReward(cornerPenalty);
        AddReward(totalReward);
    }
    
     private void CheckEndEpisode()
    {  
       Debug.Log("CheckEndEpisode() is being called!");
       //Debug.Log($"Checking Out-of-Bounds: Position {transform.position}, Boundary: {boundarySize}");
       
    /*
       bool outOfBounds = Mathf.Abs(transform.position.x) > boundarySize || 
                       Mathf.Abs(transform.position.y) > boundarySize;

      if (outOfBounds)
      {   
        Debug.Log("Agent went out of bounds! Ending episode.");
        AddReward(-1.0f);  // Penalize going out of bounds
        EndEpisode();  // Properly end the episode instead of teleporting
        return;
      }

    */

     // Check if the agent is stuck for too long
    if (rb.velocity.magnitude < 0.05f && Vector2.Distance(lastPosition, transform.position) < 0.1f)
     // Small movement still counts as movement
    {
        stuckTimer += Time.fixedDeltaTime;

        float distanceToSeeker = Vector2.Distance(transform.position, seeker.position);

        if (distanceToSeeker < 1.5f && stuckTimer >= STUCK_THRESHOLD)
        {
            Debug.Log("Agent has been near seeker too long! Ending episode.");
            AddReward(-0.1f);
            EndEpisode();
        }

        if(stuckTimer >= 1.0f)
        {
            Debug.Log($"Agent Velocity: {rb.velocity.magnitude}, Stuck Timer: {stuckTimer}");
        }
    }
    else
    {
        stuckTimer = 0f; // Reset timer when the agent moves
    }

     // Check if the episode timer has exceeded the max time
    if (episodeTimer >= MAX_EPISODE_TIME)
    {
        Debug.Log("Time's up! Ending episode.");
        EndEpisode();  // End the episode after the timer runs out
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

    private int CountWallsBetweenSeekerAndHider()
    {   
        Vector2 direction = seeker.position - transform.position;
        float distance = direction.magnitude;

         Debug.DrawRay(transform.position, direction.normalized * distance, Color.green, 0.1f);

        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, direction.normalized, distance, wallLayer);
       
        int wallCount = 0;
        foreach(var hit in hits)
        {
            if(hit.collider != null && hit.collider.gameObject != seeker.gameObject)
            {   
                Debug.Log($"[Ray Hit] {hit.collider.gameObject.name}");
                wallCount++;
            }
        }
        return wallCount;
    }
    private void FixedUpdate()
    {
        RequestDecision();  // Tells the agent to request a new action
    }

    private void OnDrawGizmos()
    {   
        //Debug.Log("Boundary Size: " + boundarySize);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(boundarySize * 2, boundarySize * 2, 0));
    }
}
