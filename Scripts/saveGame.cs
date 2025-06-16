using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // Add SceneManagement namespace
using System.IO;
using System.Linq;
using System.Collections.Generic;
using TMPro;
using System.Collections; // Add TextMeshPro namespace

public class saveGame : MonoBehaviour
{
    [SerializeField]
    private TMP_InputField usernameInput; // Reference to the TextMeshPro InputField
    [SerializeField]
    private GameNeccessities gameNeccessities; // Reference to GameNeccessities
    [SerializeField]
    private TextMeshProUGUI leaderboardText; // Add reference for leaderboard display
    [SerializeField]
    private TextMeshProUGUI leaderboardTextPlayerUI; // Add reference for leaderboard display

    // File paths for our save data
    private string savePath;
    private string leaderboardPath;
    private const int MaxLeaderboardEntries = 10; // Store top 10 scores

    private readonly float transitionTime = 1f;

    void Awake()
    {
        // Set the save file paths in the persistent data path
        savePath = Path.Combine(Application.persistentDataPath, "playerData.txt");
        leaderboardPath = Path.Combine(Application.persistentDataPath, "leaderboard.txt");

        // If GameNeccessities reference is not set, try to find it
        if (gameNeccessities == null)
        {
            gameNeccessities = Object.FindFirstObjectByType<GameNeccessities>();
            if (gameNeccessities == null)
            {
                Debug.LogError("GameNeccessities reference not found!");
            }
        }
    }

    // New method to get username from input field
    public string GetUsername()
    {
        if (usernameInput == null)
        {
            Debug.LogError("Username InputField reference not set!");
            return "";
        }

        string username = usernameInput.text.Trim();
        if (string.IsNullOrEmpty(username))
        {
            Debug.LogWarning("Username is empty!");
            return "";
        }

        return username;
    }

    // Save player data and update leaderboard
    public void SavePlayerData(string playerName, float survivedTime)
    {
        // Don't save if playerName is empty or just whitespace
        if (string.IsNullOrWhiteSpace(playerName))
        {
            Debug.LogWarning("Save cancelled: Player name is empty");
            return;
        }

        try
        {
            // Save current game data
            string saveData = $"{playerName},{survivedTime}";
            File.WriteAllText(savePath, saveData);

            // Update leaderboard
            UpdateLeaderboard(playerName, survivedTime);
            Debug.Log("Game data and leaderboard updated successfully!");
            
            // Start scene transition
            StartCoroutine(RestartSceneWithTransition());
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error saving game data: {e.Message}");
        }
    }

    private IEnumerator RestartSceneWithTransition()
    {
        // Create a black overlay
        GameObject overlay = new GameObject("Transition");
        Canvas canvas = overlay.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999; // Ensure it's on top
        
        Image fadeImage = overlay.AddComponent<Image>();
        fadeImage.color = Color.clear;
        RectTransform rt = fadeImage.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;

        // Fade to black
        float elapsedTime = 0;
        while (elapsedTime < transitionTime)
        {
            elapsedTime += Time.deltaTime;
            fadeImage.color = Color.Lerp(Color.clear, Color.black, elapsedTime / transitionTime);
            yield return null;
        }

        // Reload scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // Modified SaveCurrentPlayerScore to get time from GameNeccessities
    public void SaveCurrentPlayerScore()
    {
        if (gameNeccessities == null)
        {
            Debug.LogError("Cannot save score: GameNeccessities reference not set!");
            return;
        }

        string username = GetUsername();
        if (string.IsNullOrWhiteSpace(username))
        {
            Debug.LogWarning("Save cancelled: Please enter a username");
            return;
        }
        
        float survivedTime = gameNeccessities.GetSurvivalTime();
        SavePlayerData(username, survivedTime);
    }

    private void UpdateLeaderboard(string playerName, float survivedTime)
    {
        var leaderboardEntries = new List<(string name, float time)>();

        // Load existing leaderboard if it exists
        if (File.Exists(leaderboardPath))
        {
            string[] lines = File.ReadAllLines(leaderboardPath);
            foreach (string line in lines)
            {
                string[] data = line.Split(',');
                if (data.Length == 2 && float.TryParse(data[1], out float time))
                {
                    leaderboardEntries.Add((data[0], time));
                }
            }
        }

        // Add new entry
        leaderboardEntries.Add((playerName, survivedTime));

        // Sort by time (descending) and take top entries
        leaderboardEntries = leaderboardEntries
            .OrderByDescending(entry => entry.time)
            .Take(MaxLeaderboardEntries)
            .ToList();

        // Save updated leaderboard
        var leaderboardLines = leaderboardEntries
            .Select(entry => $"{entry.name},{entry.time}");
        File.WriteAllLines(leaderboardPath, leaderboardLines);
    }

    // Load player data
    public (string playerName, float survivedTime) LoadPlayerData()
    {
        try
        {
            if (File.Exists(savePath))
            {
                string saveData = File.ReadAllText(savePath);
                string[] data = saveData.Split(',');
                string playerName = data[0];
                float survivedTime = float.Parse(data[1]);
                Debug.Log("Game data loaded successfully!");
                return (playerName, survivedTime);
            }
            else
            {
                Debug.Log("No save file found!");
                return ("", 0f);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error loading game data: {e.Message}");
            return ("", 0f);
        }
    }

    // Get leaderboard entries
    public List<(string name, float time)> GetLeaderboard()
    {
        var leaderboard = new List<(string name, float time)>();
        try
        {
            if (File.Exists(leaderboardPath))
            {
                string[] lines = File.ReadAllLines(leaderboardPath);
                foreach (string line in lines)
                {
                    string[] data = line.Split(',');
                    if (data.Length == 2 && float.TryParse(data[1], out float time))
                    {
                        leaderboard.Add((data[0], time));
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error loading leaderboard: {e.Message}");
        }
        return leaderboard;
    }

    // Add this method to update the leaderboard display
    public void DisplayLeaderboard()
    {
        if (leaderboardText == null)
        {
            Debug.LogError("Leaderboard Text component not assigned!");
            return;
        }

        var leaderboard = GetLeaderboard();
        if (leaderboard.Count == 0)
        {
            leaderboardText.text = "No scores yet!";
            leaderboardTextPlayerUI.text = "No scores yet!";
            return;
        }

        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine("LEADERBOARD");
        sb.AppendLine("----------------");

        for (int i = 0; i < leaderboard.Count; i++)
        {
            var entry = leaderboard[i];
            // Format time as HH:MM:SS
            int totalSeconds = Mathf.FloorToInt(entry.time);
            int hours = totalSeconds / 3600;
            int minutes = (totalSeconds % 3600) / 60;
            int seconds = totalSeconds % 60;
            
            sb.AppendLine($"{i + 1}. {entry.name} - {hours:D2}:{minutes:D2}:{seconds:D2}");
        }

        leaderboardText.text = sb.ToString();
        leaderboardTextPlayerUI.text = sb.ToString();
    }

    // Example usage in Start method
    void Start()
    {
        // Example of saving data and updating leaderboard
        // SavePlayerData("Player1", 120.5f);

        // Example of reading leaderboard
        // var leaderboard = GetLeaderboard();
        // foreach (var entry in leaderboard)
        // {
        //     Debug.Log($"Player: {entry.name}, Time: {entry.time}");
        // }

        DisplayLeaderboard(); // Display the leaderboard when the game starts
    }
}
