using RankHub;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.LowLevelPhysics2D.PhysicsLayers;

public class Leaderboard : MonoBehaviour
{
    public bool debugMode;
    [Tooltip("Number of scores to display per page")]
    [SerializeField] private int scoresPerPage = 10;

    [Tooltip("Shows current status (loading, page info, etc.)")]
    [SerializeField] private TMP_Text statusText;

    [Tooltip("Container where leaderboard entries will be instantiated")]
    [SerializeField] private Transform leaderboardContainer;

    [Tooltip("Prefab for each leaderboard entry (must have 3 TMP_Text: Rank, Player, Score)")]
    [SerializeField] private GameObject scoreEntryPrefab;

    [SerializeField] private Button nextPageButton;
    [SerializeField] private Button prevPageButton;

    private string playerId;
    private int lastSubmittedScore;       // Last score successfully submitted
    private int currentPlayerScore;      // Local score (can be higher than submitted)
    private bool hasSavedScore;

    private int currentPage = 1;
    private bool isSubmitting;

    private const string PLAYER_ID_KEY = "PlayerID";
    private const string PLAYER_NAME_KEY = "PlayerName";
    private const string PLAYER_SCORE_KEY = "TotalScore";


    private IEnumerator Start()
    {
        yield return new WaitUntil(() => RankHubManager.Instance != null);
        
        LoadOrCreatePlayerId();
        LoadSavedData();
        SetupButtonListeners();
        SubmitPlayerScore();
        // Initial leaderboard load
        RefreshLeaderboard();
    }

    private void Update()
    {
        
    }

    private void LoadOrCreatePlayerId()
    {
        if (PlayerPrefs.HasKey(PLAYER_ID_KEY))
        {
            playerId = PlayerPrefs.GetString(PLAYER_ID_KEY);
            if(debugMode)Debug.Log($"Player ID loaded: {playerId}");
        }
        else
        {
            playerId = RankHubManager.Instance.GeneratePlayerId();
            PlayerPrefs.SetString(PLAYER_ID_KEY, playerId);
            PlayerPrefs.Save();
            if(debugMode) Debug.Log($"New Player ID created: {playerId}");
        }
    }

    private void LoadSavedData()
    {
        // Load saved score (last submitted score)
        lastSubmittedScore = PlayerPrefs.GetInt(PLAYER_SCORE_KEY, 0);
        currentPlayerScore = lastSubmittedScore; // Start with submitted score
        hasSavedScore = lastSubmittedScore > 0;
    }

    private void SetupButtonListeners()
    {
        if (nextPageButton != null)
            nextPageButton.onClick.AddListener(NextPage);

        if (prevPageButton != null)
            prevPageButton.onClick.AddListener(PrevPage);
    }

    private string GetCurrentPlayerName()
    {
        return PlayerPrefs.GetString(PLAYER_NAME_KEY, "PlayerName");
    }

    public void SubmitPlayerScore()
    {
        if (isSubmitting)
        {
            if (debugMode) Debug.Log("Please wait...");
            return;
        }

        int currentScore = PlayerPrefs.GetInt(PLAYER_SCORE_KEY, 0);
        string playerId = PlayerPrefs.GetString(PLAYER_ID_KEY);
        string playerName = GetCurrentPlayerName();

        isSubmitting = true;
        SetStatus("Submitting...", Color.white);

        RankHubManager.Instance.AddScore(
            playerName,
            currentScore,
            playerId,
            OnSubmitSuccess,
            (error) => OnSubmitError(error, playerName)

        );
    }

    private void RefreshLeaderboard()
    {
        SetStatus("Loading...", Color.white);

        RankHubManager.Instance.GetLeaderboard(
            scoresPerPage,
            currentPage,
            OnLeaderboardLoaded,
            OnError
        );
    }

    private void OnSubmitSuccess(AddScoreResponse response)
    {
        isSubmitting = false;

        if (response.success)
        {
            // =========================================================
            // HANDLE SCORE SUBMISSION FEEDBACK
            // =========================================================
            if (response.message.Contains("not updated"))
            {
                // Score was lower than best - didn't enter leaderboard
                if (debugMode) Debug.Log($"Score not improved. Your best is still {lastSubmittedScore}. Keep playing!");
            }
            else if (response.message.Contains("replaced"))
            {
                // New score entered leaderboard and replaced someone
                lastSubmittedScore = currentPlayerScore;
                PlayerPrefs.SetInt(PLAYER_SCORE_KEY, lastSubmittedScore);
                PlayerPrefs.Save();

                if (debugMode) Debug.Log($"NEW RECORD! You beat {response.replaced_player}'s score of {response.replaced_score}!");
            }
            else if (response.message.Contains("updated"))
            {
                // New personal best
                lastSubmittedScore = currentPlayerScore;
                PlayerPrefs.SetInt(PLAYER_SCORE_KEY, lastSubmittedScore);
                PlayerPrefs.Save();

                if (debugMode) Debug.Log($"New personal best: {currentPlayerScore} pts!");
            }
            else if (response.message.Contains("added"))
            {
                // First score submission
                lastSubmittedScore = currentPlayerScore;
                PlayerPrefs.SetInt(PLAYER_SCORE_KEY, lastSubmittedScore);
                PlayerPrefs.Save();

                if (debugMode) Debug.Log($"First score recorded: {currentPlayerScore} pts!");
            }

            RefreshLeaderboard();
        }
    }

    private void OnLeaderboardLoaded(LeaderboardResponse response)
    {
        // Clear existing entries
        foreach (Transform child in leaderboardContainer)
        {
            Destroy(child.gameObject);
        }

        int playerRank = 0;
        int playerScore = 0;
        int startRank = (currentPage - 1) * scoresPerPage + 1;

        // Create new entries
        for (int i = 0; i < response.scores.Length; i++)
        {
            var score = response.scores[i];
            int rank = startRank + i;

            var entry = Instantiate(scoreEntryPrefab, leaderboardContainer);
            var texts = entry.GetComponentsInChildren<TMP_Text>();

            if (texts.Length >= 3)
            {
                texts[0].text = rank.ToString();
                texts[1].text = score.player;
                texts[2].text = score.score.ToString();
            }

            // Track player's data
            if (score.player_id == playerId)
            {
                playerRank = rank;
                playerScore = score.score;

                // Update last submitted score if leaderboard shows higher
                if (playerScore > lastSubmittedScore)
                {
                    lastSubmittedScore = playerScore;
                    PlayerPrefs.SetInt(PLAYER_SCORE_KEY, lastSubmittedScore);
                    PlayerPrefs.Save();
                }

                // Highlight player's entry
                var image = entry.GetComponent<Image>();
                if (image != null)
                {
                    image.color = new Color(1, 1, 0, 0.2f);
                }
            }
        }

        // Update pagination info
        int totalPages = Mathf.CeilToInt((float)response.total_scores / scoresPerPage);

        if (playerRank > 0)
        {
            //SetStatus($"Page {currentPage} of {totalPages} · You're #{playerRank} with {playerScore} pts · Total: {response.total_scores} scores", Color.white);
        }
        else
        {
            SetStatus($"Page {currentPage} of {totalPages} · Total: {response.total_scores} scores", Color.white);
        }

        // Update pagination buttons
        if (prevPageButton != null)
            prevPageButton.interactable = currentPage > 1;

        if (nextPageButton != null)
            nextPageButton.interactable = currentPage < totalPages;
    }

    private void NextPage()
    {
        currentPage++;
        RefreshLeaderboard();
    }

    private void PrevPage()
    {
        currentPage--;
        RefreshLeaderboard();
    }

    private void OnError(string error)
    {
        isSubmitting = false;

        if (error.Contains("403") || error.Contains("Forbidden"))
        {
            if (debugMode) Debug.Log("Leaderboard full. Keep playing to beat the lowest score!");
        }
        else
        {
            if (debugMode) Debug.Log($"Error: {error}");
        }

        SetStatus("Error loading", Color.red);
    }

    private void OnSubmitError(string error, string attemptedName)
    {
        isSubmitting = false;

        if (error.Contains("403") || error.Contains("Forbidden"))
        {
            if (debugMode) Debug.Log("Leaderboard full. Your score isn't high enough to enter the top.");
        }
        else
        {
            if (debugMode) Debug.Log($"Error: {error}");
        }

        SetStatus("Error", Color.red);
    }

    private void SetStatus(string message, Color color)
    {
        if (statusText != null)
        {
            statusText.text = message;
            statusText.color = color;
        }
    }

}
