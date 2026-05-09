using DG.Tweening;
using TMPro;
using UnityEngine;

public class GameCompleteScreen : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI totalScoreText;
    [SerializeField] private TextMeshProUGUI funKillersText;
    [SerializeField] private TextMeshProUGUI rankText;
    [SerializeField] private CanvasGroup contentGroup;
    [SerializeField] private UnityEngine.UI.Button mainMenuBtn;

    private string playerName;

    private void Start()
    {
        playerName = PlayerPrefs.GetString("PlayerName", "Player");
        int totalScore = PlayerPrefs.GetInt("TotalScore", 0);
        int funKillersCaught = PlayerPrefs.GetInt("FunKillersCaught", 0);

        nameText.text = $"Thanks for playing, {playerName}!";
        totalScoreText.text = $"Total Score\n{totalScore}";
        funKillersText.text = $"Fun Killers Stopped\n{funKillersCaught}";
        rankText.text = GetRank(totalScore);

        // Animate in
        contentGroup.alpha = 0;
        contentGroup.DOFade(1f, 0.8f).SetDelay(0.3f);

        mainMenuBtn.onClick.AddListener(() => LevelLoader.LoadLevel(0));
    }

    private string GetRank(int score)
    {
        if (score >= 2500) return "rank: GUARDIAN OF FUN 🏆";
        if (score >= 1800) return "rank: SOLID GATEKEEPER ✅";
        if (score >= 1000) return "rank: NEEDS IMPROVEMENT 😬";
        return "rank: THE FUN KILLERS WON 💀";
    }

}
