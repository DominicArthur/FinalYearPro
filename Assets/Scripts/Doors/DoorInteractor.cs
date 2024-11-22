using UnityEngine;

public class DoorInteractor : MonoBehaviour
{
    public float interactionRange = 2f; // Distance within which the player can interact with a door
    public LayerMask doorLayer;        // Layer to identify doors

    void Update()
    {
        // Check for player input to interact with doors
        if (Input.GetKeyDown(KeyCode.E)) // Press 'E' to interact
        {   
            Debug.Log("E key pressed");
            InteractWithDoor();
        }
    }

   private void InteractWithDoor()
{
    Collider2D doorCollider = Physics2D.OverlapCircle(transform.position, interactionRange, doorLayer);

    if (doorCollider != null)
    {
        Debug.Log("Door detected: " + doorCollider.name); // Log to confirm door detection
        Door door = doorCollider.GetComponent<Door>();
        if (door != null)
        {
            Debug.Log("Door component found"); // Log to confirm the Door component is detected
            door.ToggleDoor();
        }
    }
    else
    {
        Debug.Log("No door detected within range."); // Log if no door is detected
    }
    }
 }
