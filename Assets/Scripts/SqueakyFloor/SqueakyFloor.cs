using System;
using UnityEngine;

public class SqueakyFloor : MonoBehaviour
{
    public AudioClip squeakSound; // Remember squeak sound effect in inspector
    private AudioSource audioSource;
    private AudioClip hornSound;
    void Start()
    {   
        // Add AudioSource component to the GameObject
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false; // Stops the sound from playing automatically
        audioSource.clip = squeakSound;
    }

   private void OnTriggerEnter2D(Collider2D other)
   {    
        // Checks if the player or hider collides with floor
        if(other.CompareTag("Player") || other.CompareTag("Hider"))
        {   
            // Plays the squeak sound effect
            audioSource.Play();

            // Prints to console for debugging
            Debug.Log($"{other.name} stepped on a squeaky floor!");
        }
   }
}
