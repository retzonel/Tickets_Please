using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IntroScreen : MonoBehaviour
{
    [Header("Name Input")] [SerializeField]
    private GameObject nameInputCanvas;

    [SerializeField] private TMP_InputField nameInputField;
    [SerializeField] private Button submitButton;

    [Header("Story Display")] [SerializeField]
    private GameObject storyCanvas;

    [SerializeField] private TextMeshProUGUI storyText;
    [SerializeField] private Button skipButton;

    [Header("Settings")] [SerializeField] private float typeSpeed = 0.04f;
    [SerializeField] private int linesPerPage = 3;
    [SerializeField] private AudioClip typeSound;

    private string[] storyLines;
    private bool isTyping = false;
    private bool skipRequested = false;
    private string currentLine = "";
    private int currentCharIndex;
    private void Start()
    {
        storyCanvas.SetActive(false);
        submitButton.onClick.AddListener(OnSubmitName);
        skipButton.onClick.AddListener(CompleteLine);
    }

    private void OnDestroy()
    {
        skipButton.onClick.RemoveListener(CompleteLine);
    }

    private void OnSubmitName()
    {
        string username = nameInputField.text.Trim();
        if (string.IsNullOrEmpty(username)) username = "Friend";

        PlayerPrefs.SetString("PlayerName", username);

        storyLines = new string[]
        {
            $"Welcome, {username}.",
            "You are the Gatekeeper of Fun Park — where joy knows no borders.",
            "Guests arrive here to laugh, play, and create shared memories.",
            "\"Play should be safe, joyful, and open to all who protect it.\"",
            "But not every path to play is the same.",
            "Some conditions affect how safely and fairly guests can participate.",
            "Poor decisions or unstable guests can disrupt the experience for everyone.",
            $"Your mission {username} is to assess each guest before entry.",
            "VALID ENTRY GUIDELINES:",
            "- Guest must have a GOLDEN ticket.",
            "- Guest must be under 25 years old.",
            "- Guest must have LOW cortisol levels.",
            "- Guest must have HIGH dopamine levels.",
            "- Guest must NOT be sick.",
            "To decide, drag the to approve or to reject.",
            "Place your stamp on the guest document to shape their access to play.",
            "Incorrect judgments will reduce park harmony.",
            "Protect borderless fun.",
            "Remember: every decision shapes what 'without borders' truly means.",
            "Keep play flowing. Good luck, Gatekeeper."
        };

        nameInputCanvas.SetActive(false);
        storyCanvas.SetActive(true);
        StartCoroutine(PlayStory());
    }

    private IEnumerator PlayStory()
    {
        for (int i = 0; i < storyLines.Length; i += linesPerPage)
        {
            if (skipRequested)
                break;

            storyText.text = "";
            isTyping = true;

            int end = Mathf.Min(i + linesPerPage, storyLines.Length);

            for (int j = i; j < end; j++)
            {
                yield return StartCoroutine(TypeLine(storyLines[j]));
                storyText.text += "\n"; // spacing between lines inside page
            }

            isTyping = false;

            yield return new WaitForSeconds(0.6f);
        }

        LoadNextLevel();
    }

    private IEnumerator TypeLine(string line)
    {
        currentLine = line;
        currentCharIndex = 0;

        while (currentCharIndex < currentLine.Length)
        {
            if (skipRequested)
            {
                // Complete remaining text instantly
                storyText.text += currentLine.Substring(currentCharIndex);

                skipRequested = false;
                yield break;
            }

            char c = currentLine[currentCharIndex];

            storyText.text += c;

            if (c != ' ' && c != '\n' && c != '\t')
            {
                AudioManager.Instance?.PlaySFX(typeSound);
            }

            currentCharIndex++;

            yield return new WaitForSeconds(typeSpeed);
        }

        storyText.text += "\n";
    }

    private void CompleteLine()
    {
        if (isTyping)
        {
            skipRequested = true;
        }
    }

    public void LoadNextLevel()
    {
        LevelLoader.LoadLevel(3);
    }

}