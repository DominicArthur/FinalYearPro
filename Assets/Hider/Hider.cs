using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Hider : MonoBehaviour
{
    public GameObject winMessage; // Win message UI
    public Transform player; // Player's Transform
    public Light2D playerLight; // Reference to Player's Light2D

    private SpriteRenderer spriteRenderer; // Reference to Hider's SpriteRenderer
    private Vector3 hidingSpot; // Current hiding spot

    void Start()
    {
        // Hide win message initially
        if (winMessage != null)
        {
            winMessage.SetActive(false);
        }

        // Get the Hider's SpriteRenderer
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false; // Start as invisible
        }

        // Choose an initial hiding spot
        ChooseHidingSpot();
    }

    void Update()
    {
        CheckVisibility(); // Check visibility every frame
    }

    private void CheckVisibility()
    {
        if (player == null || spriteRenderer == null || playerLight == null)
        {
            Debug.LogWarning("Player, SpriteRenderer, or PlayerLight is missing!");
            return;
        }

        // Check if the Hider is within the light's radius
        if (IsInLightArea())
        {
            // Make the Hider visible
            spriteRenderer.enabled = true;
            Debug.Log("Hider is now visible!");
        }
        else
        {
            // Hide the Hider if it's not within the light's radius
            spriteRenderer.enabled = false;
            Debug.Log("Hider is not visible.");
        }
    }

    private bool IsInLightArea()
    {
        Vector3 lightPosition = playerLight.transform.position;
        float lightRadius = playerLight.pointLightOuterRadius;

        float distanceToLight = Vector3.Distance(transform.position, lightPosition);

        //Debug.Log($"Distance to Light: {distanceToLight}, Light Radius: {lightRadius}");

        return distanceToLight <= lightRadius;
    }

    private void ChooseHidingSpot()
    {
        // Choose a random hiding spot within a certain range
        hidingSpot = new Vector3(Random.Range(-5.0f, 5.0f), Random.Range(-5.0f, 5.0f), 0);
        transform.position = hidingSpot; // Move to the hiding spot immediately
        Debug.Log($"New hiding spot: {hidingSpot}");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            ShowWinMessage(); // Display win message if Player collides
        }
    }

    private void ShowWinMessage()
    {
        if (winMessage != null)
        {
            winMessage.SetActive(true); // Show win message UI
        }

        Time.timeScale = 0; // Pause the game
        Debug.Log("Seeker Wins!");
    }

    private void OnDrawGizmos()
    {
        // Visualize the light radius in the Scene view for debugging
        if (playerLight == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(playerLight.transform.position, playerLight.pointLightOuterRadius);
    }
}
