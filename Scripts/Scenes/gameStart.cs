using UnityEngine;

public class start : MonoBehaviour
{
    [SerializeField] private GameObject startScreen; // Reference to the start screen GameObject

    private void Start()
    {
        // Pause the game on start
        PauseGame();
        startScreen.SetActive(true);
    }

    private void Update()
    {
        // Check for mouse click (left, right, or middle button)
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2))
        {
            ResumeGame();

            // Disable the start screen
            if (startScreen != null)
            {
                startScreen.SetActive(false);
            }
            else
            {
                Debug.LogWarning("Start screen GameObject is not assigned in the inspector.");
            }
        }
    }

    private void PauseGame()
    {
        Time.timeScale = 0f; // 0 means the game is paused
    }

    private void ResumeGame()
    {
        Time.timeScale = 1f; // 1 means normal time scale
        // Optionally, you can disable this script after the game starts
        this.enabled = false;
    }
}
