using RankHub;
using System.Collections;
using UnityEngine;
using static UnityEngine.LowLevelPhysics2D.PhysicsLayers;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public bool debugMode;
    [Header("---------- UI ----------")]
    [SerializeField] private UserConsole userConsole;
    [SerializeField] private EndDayScreen endDayScreen;
    [SerializeField] private BriefingScreen briefingScreen;

    [Header("---------- Days ----------")]
    [SerializeField] private DayRuleSet[] days;
    [SerializeField] private int currentDayIndex;

    [Header("---------- Score Tracking ----------")]
    [SerializeField] private float maxTimePerGuest = 15f; 
    [SerializeField] private int baseScorePerGuest = 100;
    [SerializeField] private int totalScore;
    [SerializeField] private int totalFunKillersCaught;
    [SerializeField] private int mistakes;
    [SerializeField] private int score;
    [SerializeField] private int funKillersCaught;
    [SerializeField] private int funKillersMissed;
    [Header("Fun Meter")]
    [SerializeField] private float funIncreasePerCorrectApproval = 30f;
    [SerializeField] private float funDecreasePerWrongApproval = -50f;
    [SerializeField] private float currentFunAmount = 50f;
    [SerializeField] private float maxFun = 200f;
    [SerializeField] private float minFun = 0f;

    [Header("---------- Sound Effects ----------")]
    [SerializeField] private AudioClip dayStartSFX;
    [SerializeField] private AudioClip dayEndSFX;
    [SerializeField] private AudioClip nextGuestSFX;
    [SerializeField] private AudioClip correctDecisionSFX;
    [SerializeField] private AudioClip wrongDecisionSFX;
    [SerializeField] private AudioClip timerTickSFX;
    [SerializeField] private AudioClip timerEndSFX;

    public DayRuleSet CurrentDay => days[currentDayIndex];
    public bool IsLastDay => currentDayIndex >= days.Length - 1;

    private GuestProfile[] queue;
    private int currentIndex;
    private bool isDayOver;

    private float guestStartTime;
    private bool timerRunning;
    private float lastTickSecond;

    private bool isFeedbackPlaying;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

    }

    private void Start()
    {
        FeedbackManager.Instance.OnFeedbackComplete += HandleFeedbackComplete;

        ShowBriefing();
    }

    private void Update()
    {
        if (!timerRunning)
            return;

        float elapsed = Time.time - guestStartTime;
        float remaining = maxTimePerGuest - elapsed;

        // UI update
        userConsole.UpdateTimer(remaining);

        // Per second tick SFX
        float currentSecond = Mathf.Ceil(remaining);

        if (currentSecond < lastTickSecond && remaining > 0f)
        {
            lastTickSecond = currentSecond;
            AudioManager.Instance?.PlaySFX(timerTickSFX, 0.6f);
        }

        // Timeout trigger
        if (remaining <= 0f)
        {
            timerRunning = false;

            AudioManager.Instance?.PlaySFX(timerEndSFX);
            HandleTimeout(playerApproved: false);

            StartCoroutine(AdvanceToNextGuest());

            return;
        }
    }

    private void OnDestroy()
    {
        FeedbackManager.Instance.OnFeedbackComplete -= HandleFeedbackComplete;
    }

    private void ShowBriefing()
    {
        userConsole.Hide();
        endDayScreen.Hide();

        briefingScreen.UpdateBriefingText(CurrentDay.dailyBriefing);
        briefingScreen.Show();

        userConsole.UpdateStatsText(CurrentDay.dayNumber, currentIndex, CurrentDay.guestQueue.Length);
    }

    private void StartDay()
    {
        AudioManager.Instance?.PlaySFX(dayStartSFX);

        ResetDayStats();

        queue = CurrentDay.guestQueue;
        currentIndex = 0;
        isDayOver = false;

        userConsole.UpdateFunMeter(currentFunAmount / maxFun);

        ShowNextGuest();
    }

    private void EndDay()
    {
        AudioManager.Instance?.PlaySFX(dayEndSFX);

        totalScore += score;
        PlayerPrefs.SetInt("TotalScore", totalScore);
        PlayerPrefs.Save();
        totalFunKillersCaught += funKillersCaught;

        userConsole.Hide();

        endDayScreen.UpdateResultsUI(score, mistakes, funKillersCaught);
        endDayScreen.Show(IsLastDay);
    }

    private void ResetDayStats()
    {
        mistakes = 0;
        score = 0;
        funKillersCaught = 0;
        funKillersMissed = 0;
    }

    private void ShowNextGuest()
    {
        if (currentIndex >= queue.Length)
        {
            HandleEndOfQueue();
            return;
        }

        userConsole.DisplayGuest(queue[currentIndex]);

        AudioManager.Instance?.PlaySFX(nextGuestSFX);

        userConsole.EnableConsoleButtons(true);

        guestStartTime = Time.time;
        timerRunning = true;
        lastTickSecond = maxTimePerGuest;

        userConsole.ResetTimerDisplay(maxTimePerGuest);

        currentIndex++;
    }

    private int CalculateScore(float timeTaken)
    {
        float timeFactor = Mathf.Clamp01(1f - (timeTaken / maxTimePerGuest));

        // ensures player always gets something, but rewards speed
        return Mathf.RoundToInt(Mathf.Lerp(30f, baseScorePerGuest, timeFactor));
    }

    private void HandleEndOfQueue()
    {
        isDayOver = true;
        timerRunning = false;

        userConsole.EnableConsoleButtons(false);
        userConsole.ShowQueueEmptyState();

        userConsole.ResetTimerDisplay(0f);
    }

    public void ProcessDecision(bool playerApproved)
    {
        if (isFeedbackPlaying)
            return;

        timerRunning = false;

        userConsole.EnableConsoleButtons(false);

        GuestProfile guest = CurrentDay.guestQueue[currentIndex - 1];
        bool shouldEnter = IsGuestAllowed(guest);

        float timeTaken = Time.time - guestStartTime;

        if (timeTaken >= maxTimePerGuest)
        {
            mistakes++;
            isFeedbackPlaying = true;
            FeedbackManager.Instance?.PlayWrong(playerApproved);
            AudioManager.Instance?.PlaySFX(wrongDecisionSFX);
            return;
        }

        bool correct = playerApproved == shouldEnter;
        isFeedbackPlaying = true;

        if (correct)
        {
            HandleCorrectDecision(playerApproved, guest);
        }
        else
        {
            HandleWrongDecision(playerApproved, guest);
        }
    }

    private void HandleCorrectDecision(bool playerApproved, GuestProfile guest)
    {
        float timeTaken = Time.time - guestStartTime;
        int earnedScore = CalculateScore(timeTaken);

        score += earnedScore;

        if (!playerApproved && guest.isFunKiller)
            funKillersCaught++;

        FeedbackManager.Instance?.PlayCorrect(playerApproved);
        AudioManager.Instance?.PlaySFX(correctDecisionSFX);

        AddFun(funIncreasePerCorrectApproval);

        if(debugMode) Debug.Log($"Time: {timeTaken:F2}s | Score Earned: {earnedScore}");
    }

    private void HandleWrongDecision(bool playerApproved, GuestProfile guest)
    {
        mistakes++;

        score -= 50;
        score = Mathf.Max(0, score);

        if (playerApproved && guest.isFunKiller)
            funKillersMissed++;

        AddFun(funDecreasePerWrongApproval);

        FeedbackManager.Instance?.PlayWrong(playerApproved);
        AudioManager.Instance?.PlaySFX(wrongDecisionSFX);
    }

    private void HandleTimeout(bool playerApproved)
    {
        mistakes++;

        AddFun(funDecreasePerWrongApproval / 2);

        FeedbackManager.Instance?.PlayWrong(playerApproved, true);
    }

    private void HandleFeedbackComplete()
    {
        ClearUserConsole();
        ResetStamps();
        isFeedbackPlaying = false;
    }

    private IEnumerator AdvanceToNextGuest()
    {
        userConsole.EnableConsoleButtons(false);

        yield return new WaitForSeconds(1f);

        ShowNextGuest();
    }

    private void AddFun(float amount)
    {
        currentFunAmount = Mathf.Clamp(currentFunAmount + amount, minFun, maxFun);
        userConsole.UpdateFunMeter(currentFunAmount / maxFun);
    }

    private bool IsGuestAllowed(GuestProfile guest)
    {
        if (!guest.hasValidTicket) return false;
        if (guest.scanResult.sick) return false;
        if (guest.personalInfo.age < CurrentDay.minAge) return false;
        if (guest.personalInfo.age > CurrentDay.maxAge) return false;
        if (!guest.scanResult.highDopamine) return false;
        if (guest.isFunKiller) return false;

        return true;
    }

    public void OnStartDayPressed()
    {
        briefingScreen.Hide();
        userConsole.Show();
        StartDay();
    }

    public void OnNextGuestPressed()
    {
        if (isFeedbackPlaying)
            return; // Ignore input during feedback

        if (isDayOver)
        {
            EndDay();
            return;
        }

        ShowNextGuest();
    }

    public void OnRetryPressed()
    {
        endDayScreen.Hide();
        userConsole.Show();
        StartDay();
    }

    public void OnProceedPressed()
    {
        if (IsLastDay)
        {
            PlayerPrefs.SetInt("TotalScore", totalScore);
            PlayerPrefs.SetInt("FunKillersCaught", totalFunKillersCaught);

            LevelLoader.LoadLevel(4);
            return;
        }

        currentDayIndex++;
        ShowBriefing();
    }

    public void ClearUserConsole()
    {
        userConsole.ClearConsole();
    }

    public void ResetStamps()
    {
        userConsole.ResetStamps();
    }

    public bool DayFailed()
    {
        return mistakes >= CurrentDay.maxMistakesAllowed;
    }

}
