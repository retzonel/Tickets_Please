using DG.Tweening;
using KBCore.Refs;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UI;

public class FeedbackManager : MonoBehaviour
{
    public static FeedbackManager Instance;

    [Header("Stamp")] [SerializeField] private Image stampImage;
    [SerializeField] private Sprite approvedStamp;
    [SerializeField] private Sprite deniedStamp;

    [Header("Screen Flash")] [SerializeField]
    private Image flashOverlay; // fullscreen transparent image

    [Header("Audio")] [SerializeField] private AudioClip approveSound;
    [SerializeField] private AudioClip denySound;
    [SerializeField] private AudioClip wrongSound; // buzzer for mistakes

    [Header("VFX")] [SerializeField, Self] CinemachineImpulseSource shakeSource;

    public System.Action OnFeedbackComplete;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        stampImage.gameObject.SetActive(false);
        flashOverlay.color = Color.clear;
    }

    public void PlayCorrect(bool wasApproved)
    {
        stampImage.sprite = wasApproved ? approvedStamp : deniedStamp;
        AudioManager.Instance?.PlaySFX(approveSound, 0.35f);
        DoStampAnimation();
        DoFlash(new Color(0f, 1f, 0f, 0.25f)); // green
    }

    public void PlayWrong(bool wasApproved, bool timeout = false)
    {
        stampImage.sprite = wasApproved ? approvedStamp : deniedStamp;
        AudioManager.Instance?.PlaySFX(wrongSound, 0.35f);
        if(!timeout)    DoStampAnimation();
        DoFlash(new Color(1f, 0f, 0f, 0.35f)); // red
    }

    private void DoStampAnimation()
    {
        stampImage.gameObject.SetActive(true);
        stampImage.transform.localScale = Vector3.one * 3f; // start big
        stampImage.color = new Color(1, 1, 1, 0);

        Sequence seq = DOTween.Sequence();
        seq.Append(stampImage.transform.DOScale(1f, 0.15f).SetEase(Ease.OutBounce));
        seq.Join(stampImage.DOFade(1f, 0.1f));
        seq.AppendInterval(0.6f);
        seq.Append(stampImage.DOFade(0f, 0.2f));
        seq.OnComplete(() =>
        { 
            stampImage.gameObject.SetActive(false);
            OnFeedbackComplete?.Invoke();
        });

        shakeSource.GenerateImpulse();
    }

    private void DoFlash(Color flashColor)
    {
        flashOverlay.color = flashColor;
        flashOverlay.DOFade(0f, 0.4f).SetEase(Ease.OutQuad);
    }
}