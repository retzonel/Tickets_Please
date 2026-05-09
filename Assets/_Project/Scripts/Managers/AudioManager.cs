using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    public bool IsMusicOn { get; private set; } = true;
    public bool IsSFXOn { get; private set; } = true;


    void Awake()
    {
        if (Instance != null)
        {
            //Debug.Log("More than one instance of Auido Manager found!");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    private void Start()
    {
        IsMusicOn = PlayerPrefs.GetInt("MusicOn", 1) == 1;
        IsSFXOn = PlayerPrefs.GetInt("SFXOn", 1) == 1;

        musicSource.mute = !IsMusicOn;
        musicSource.loop = true;
        sfxSource.mute = !IsSFXOn;
    }

    public void PlayMusic(AudioClip musicClip)
    {
        if (musicClip == null) return;
        musicSource.clip = musicClip;
        musicSource.Play();
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    public void PlaySFX(AudioClip clip, float volume)
    {
        if (clip != null)
        {
            sfxSource.PlayOneShot(clip, volume);
        }
    }

    public void ToggleMusic()
    {
        IsMusicOn = !IsMusicOn;
        musicSource.mute = !IsMusicOn;
        PlayerPrefs.SetInt("MusicOn", IsMusicOn ? 1 : 0);
    }

    public void ToggleSFX()
    {
        IsSFXOn = !IsSFXOn;
        sfxSource.mute = !IsSFXOn;
        PlayerPrefs.SetInt("SFXOn", IsSFXOn ? 1 : 0);
    }
}