using System;
using DG.Tweening;
using KBCore.Refs;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class UserConsole : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI statsText; //day, count of proccesed tickets

    [Space]
    [SerializeField] private DraggableUI acceptStamp;
    [SerializeField] private DraggableUI rejectStamp;
    [SerializeField] private Button nextGuestButton;

    [Space] [SerializeField] private Image portraitImage;
    [SerializeField] private Image portraitBg;
    [SerializeField] private Image ticketImage;

    [Space] [SerializeField] private TextMeshProUGUI personalInfoText;
    [SerializeField] private TextMeshProUGUI scanResultsText;

    [Space]
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color warningColor = Color.yellow;
    [SerializeField] private Color dangerColor = Color.red;

    [Space]
    [SerializeField] private RectTransform funNeedle;
    [SerializeField] private float minAngle = -90f;
    [SerializeField] private float maxAngle = 90f;

    [Space] [SerializeField] private Sprite validTicketSprite;
    [SerializeField] private Sprite[] invalidTicketsSprites;

    [Space] [SerializeField] AudioClip buttonClickSFX;
    [SerializeField] AudioClip clearConsoleSFX;
    [Space]
    [SerializeField] AudioClip stampDownSFX;

    [Space]
    [SerializeField] private AudioClip validstampSFX;
    [SerializeField] private AudioClip invalidStampSFX;

    [Space]
    [SerializeField, Self] private CanvasGroup contentGroup;
    [SerializeField] AudioClip showPanelSFX;
    [SerializeField] AudioClip hidePanelSFX;

    
    private void Start()
    {
        acceptStamp.onValidDrop.AddListener(OnAccept);
        rejectStamp.onValidDrop.AddListener(OnReject);
        acceptStamp.onValidDrop.AddListener(ValidStamp);
        rejectStamp.onValidDrop.AddListener(ValidStamp);
        acceptStamp.onInvalidDrop.AddListener(InvalidStamp);
        rejectStamp.onInvalidDrop.AddListener(InvalidStamp);

        nextGuestButton.onClick.AddListener(GameManager.Instance.OnNextGuestPressed);
    }

    private void OnDestroy()
    {
        acceptStamp.onValidDrop.RemoveListener(OnAccept);
        rejectStamp.onValidDrop.RemoveListener(OnReject);
        acceptStamp.onValidDrop.RemoveListener(ValidStamp);
        rejectStamp.onValidDrop.RemoveListener(ValidStamp);
        acceptStamp.onInvalidDrop.RemoveListener(InvalidStamp);
        rejectStamp.onInvalidDrop.RemoveListener(InvalidStamp);
    }

    private void OnAccept()
    {
        AudioManager.Instance?.PlaySFX(buttonClickSFX);
        GameManager.Instance.ProcessDecision(true);
    }

    private void OnReject()
    {
        AudioManager.Instance?.PlaySFX(buttonClickSFX);
        GameManager.Instance.ProcessDecision(false);
    }

    private void ValidStamp() => AudioManager.Instance?.PlaySFX(validstampSFX);
    private void InvalidStamp() => AudioManager.Instance?.PlaySFX(invalidStampSFX);

    public void DisplayGuest(GuestProfile guest)
    {
        UpdatePersonalInfo(guest);
        UpdateScanResults(guest);
        UpdateTicketInfo(guest);
    }


    public void EnableConsoleButtons(bool isAttending)
    {
        if (isAttending)
        {
            acceptStamp.Interactable = true;
            rejectStamp.Interactable = true;
            nextGuestButton.interactable = false;
        }
        else
        {
            acceptStamp.Interactable = false;
            rejectStamp.Interactable = false;
            nextGuestButton.interactable = true;
        }
    }

    private void UpdatePersonalInfo(GuestProfile guestProfile)
    {
        var personalInfo = guestProfile.personalInfo;
        var text =
            $"name: {personalInfo.name} \n \nage: {personalInfo.age} \n \nreason for coming: {personalInfo.reasonForVisit}";
        personalInfoText.text = text;
        portraitBg.enabled = true;
        portraitImage.enabled = true;
        portraitImage.sprite = personalInfo.portrait;
    }

    private void UpdateScanResults(GuestProfile guestProfile)
    {
        var scanResult = guestProfile.scanResult;
        var results = "";
        results += $"Sick: {(scanResult.sick ? "Yes" : "No")}\n \n";
        results += $"Low Cortisol: {(scanResult.lowCortisol ? "Yes" : "No")}\n \n";
        results += $"High Dopamine: {(scanResult.highDopamine ? "Yes" : "No")}";

        scanResultsText.text = results;
    }

    private void UpdateTicketInfo(GuestProfile guestProfile)
    {
        if (guestProfile.hasValidTicket)
        {
            ticketImage.sprite = validTicketSprite;
            ticketImage.gameObject.SetActive(true);
        }
        else if (invalidTicketsSprites is { Length: > 0 })
        {
            var index = Random.Range(0, invalidTicketsSprites.Length);
            ticketImage.sprite = invalidTicketsSprites[index];
            ticketImage.gameObject.SetActive(true);
        }
    }

    public void UpdateStatsText(int day, int currentGuestIndex, int totalGuests)
    {
        statsText.text = $"Day {day}\n{currentGuestIndex}/{totalGuests}";
    }

    public void ClearConsole()
    {
        scanResultsText.text = "";
        personalInfoText.text = "";
        portraitImage.sprite = null;
        portraitImage.enabled = false;
        portraitBg.enabled = false;
        ticketImage.sprite = null;
        ticketImage.gameObject.SetActive(false);
        AudioManager.Instance?.PlaySFX(clearConsoleSFX);
    }

    public void ResetStamps()
    {
        acceptStamp.ResetPosition();
        rejectStamp.ResetPosition();
    }

    public void ShowQueueEmptyState()
    {
        personalInfoText.text = "No more guests today.";
        scanResultsText.text = "";
        portraitImage.sprite = null;
        portraitImage.enabled = false;
        portraitBg.enabled = false;
        ticketImage.sprite = null;
        ticketImage.gameObject.SetActive(false);
        //nextGuestButton active so player can press to end day
        AudioManager.Instance?.PlaySFX(clearConsoleSFX);
    }

    public void Show()
    {
        gameObject.SetActive(true);
        contentGroup.alpha = 0;
        contentGroup.DOKill(); // kill any previous tween on this target
        contentGroup.DOFade(1f, 0.8f).SetDelay(0.0f);
        AudioManager.Instance?.PlaySFX(showPanelSFX);
    }

    public void Hide()
    {
        contentGroup.DOKill(); // kill any previous tween on this target
        contentGroup.DOFade(0f, 0.1f).SetDelay(0.0f).OnComplete(() => gameObject.SetActive(false));
        AudioManager.Instance?.PlaySFX(hidePanelSFX);
    }

    public void UpdateTimer(float remainingTime)
    {
        remainingTime = Mathf.Max(0f, remainingTime);

        int minutes = Mathf.FloorToInt(remainingTime / 60f);
        int seconds = Mathf.FloorToInt(remainingTime % 60f);
        int milliseconds = Mathf.FloorToInt((remainingTime * 100f) % 100f);

        timerText.text = $"{minutes:00}:{seconds:00}:{milliseconds:00}";

        // Color warning states
        if (remainingTime <= 3f)
        {
            timerText.color = dangerColor;
        }
        else if (remainingTime <= 5f)
        {
            timerText.color = warningColor;
        }
        else
        {
            timerText.color = normalColor;
        }
    }

    public void ResetTimerDisplay(float time)
    {
        UpdateTimer(time);
    }

    public void UpdateFunMeter(float normalizedValue)
    {
        normalizedValue = Mathf.Clamp01(normalizedValue);

        float angle = Mathf.Lerp(minAngle, maxAngle, normalizedValue);

        funNeedle.localRotation = Quaternion.Euler(0f, 0f, angle);
    }

}