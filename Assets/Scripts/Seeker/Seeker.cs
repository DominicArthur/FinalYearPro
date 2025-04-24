using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Seeker : MonoBehaviour
{
    public Transform hider;  
    public float detectionRange = 5f;
    public float moveSpeed = 2f;  // Speed of the seeker
    public float boundarySize = 7f;
    private Vector2 targetPosition;
    private bool seekerInDelay = true;
    private float delayTime = 10f;
    private float delayTimer = 0f;
    private float timeAtTarget = 0f;

    void Start()
    {
        PickNewSpot();
    }
    
    public void OnEpisodeBegin()
    {
        seekerInDelay = true;
        delayTimer = 0f;
        PickNewSpot();
        Debug.Log("[Seeker] OnEpisodeBegin() was called. Delay reset.");
    }
    void Update()
    {
        if (seekerInDelay)
        {
            delayTimer += Time.deltaTime;
            float timeRemaining = delayTime - delayTimer;
            Debug.Log($"Seeker waiting... {timeRemaining:F1} seconds remaining");
            if (delayTimer >= delayTime)
            {
                seekerInDelay = false;
                Debug.Log("Seeker delay ended - starting search.");
            }
            return;
        }

        float distanceToHider = Vector2.Distance(transform.position, hider.position);
        float distanceToTarget = Vector2.Distance(transform.position, targetPosition);
    
        //Debug.Log($"Distance to hider: {distanceToHider}, Distance to target: {distanceToTarget}");
    
       if(distanceToHider < detectionRange)
       {
             Debug.Log("Chasing hider!");
             ChaseHider();
             timeAtTarget = 0f;
       }
       else if(distanceToTarget < 0.2f)
       {
            timeAtTarget += Time.deltaTime;
            if(timeAtTarget > 1f)
            {
                Debug.Log("Reached target, picking new spot");
                PickNewSpot();
                timeAtTarget = 0f;
            }
       }
       else
       {
            MoveToTarget(targetPosition);
            timeAtTarget = 0f;
       }
    }

    private void PickNewSpot()
    {   
        targetPosition = new Vector2(
            Random.Range(-boundarySize, boundarySize),
            Random.Range(-boundarySize, boundarySize)
        );
    }

    private void MoveToTarget(Vector2 target)
    {
        Vector2 direction = (target - (Vector2)transform.position).normalized;
        transform.Translate(direction * moveSpeed * Time.deltaTime);
    }

    private void ChaseHider()
    {   
        if (Vector2.Distance(transform.position, hider.position) < detectionRange)
        {
            Vector2 direction = (hider.position - transform.position).normalized;
            transform.Translate(direction * moveSpeed *Time.deltaTime);
        }
        else
        {
            PickNewSpot();
        }
    }

    private bool IsPositionObstructed(Vector2 position)
    {
        LayerMask wallLayerMask = LayerMask.GetMask("Walls");
        Collider2D[] colliders = Physics2D.OverlapCircleAll(position, 0.5f, wallLayerMask);
        return colliders.Length > 0;
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(boundarySize * 2, boundarySize * 2, 0));
    }
}
