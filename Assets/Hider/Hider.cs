using UnityEngine;
public class Hider : MonoBehaviour
{
    public GameObject winMessage;
    void Start()
    {
        if(winMessage != null)
        {
            winMessage.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("Player"))
        {
            ShowWinMessage();
        }
    }

    private void ShowWinMessage()
    {
        if(winMessage != null)
        {
            winMessage.SetActive(true);
        }

        Time.timeScale = 0;
        Debug.Log("Seeker Wins!");
    }
}
  
