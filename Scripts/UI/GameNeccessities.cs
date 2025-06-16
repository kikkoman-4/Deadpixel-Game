using TMPro;
using UnityEngine;

public class GameNeccessities : MonoBehaviour
{
    // Timer to track user survival time
    private float survivalTime = 0f;
    private bool isTiming = true;

    // Reference to the UI Text component that displays the game time
    [SerializeField] private TextMeshProUGUI gameTimeText;
    [SerializeField] private TextMeshProUGUI waveText;
    [SerializeField] private int wave;
    [SerializeField] private int playerHealth;
    [SerializeField] private bool godMode;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        survivalTime = 0f;
        isTiming = true;
        UpdateGameTimeText();
    }

    // Update is called once per frame
    void Update()
    {
        if (isTiming)
        {
            survivalTime += Time.deltaTime;
            UpdateGameTimeText();
        }
    }

    // Call this method to stop the timer (e.g., when the player dies)
    public void StopTimer()
    {
        isTiming = false;
    }

    // Call this method to get the current survival time
    public float GetSurvivalTime()
    {
        return survivalTime;
    }

    // Updates the game time text UI
    private void UpdateGameTimeText()
    {
        if (gameTimeText != null)
        {
            int totalSeconds = Mathf.FloorToInt(survivalTime);
            int hours = totalSeconds / 3600;
            int minutes = (totalSeconds % 3600) / 60;
            int seconds = totalSeconds % 60;
            gameTimeText.text = $"TIME SURVIVED\n{hours:D2}:{minutes:D2}:{seconds:D2}";
        }
    }
}
