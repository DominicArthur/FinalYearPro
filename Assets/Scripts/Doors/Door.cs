using UnityEngine;

public class Door : MonoBehaviour
{
    public Sprite closedDoorSprite; // Sprite for the closed door
    public Sprite openDoorSprite;   // Sprite for the open door

    private bool isOpen = false;    // Tracks whether the door is open or closed
    private SpriteRenderer spriteRenderer;
    private Collider2D doorCollider;

    void Start()
    {
        // Get the SpriteRenderer and Collider2D components
        spriteRenderer = GetComponent<SpriteRenderer>();
        doorCollider = GetComponent<Collider2D>();

        // Initialize the door to be closed
        SetDoorState(isOpen);
    }

    public void ToggleDoor()
    {
        // Toggle the door's open/closed state
        isOpen = !isOpen;
         Debug.Log("isOpen is now: " + isOpen);
        // Update the door's appearance and functionality
        SetDoorState(isOpen);

        Debug.Log("Door toggled to " + (isOpen ? "Open" : "Closed"));
    }

    private void SetDoorState(bool open)
    {
        if (open)
        {   
            Debug.Log("Set to Open");
            // Open the door: Change sprite and disable the collider
            spriteRenderer.sprite = openDoorSprite;
            doorCollider.enabled = false;
        }
        else
        {   
            Debug.Log("Set to Closed");
            // Close the door: Change sprite and enable the collider
            spriteRenderer.sprite = closedDoorSprite;
            doorCollider.enabled = true;
        }
    }
}

