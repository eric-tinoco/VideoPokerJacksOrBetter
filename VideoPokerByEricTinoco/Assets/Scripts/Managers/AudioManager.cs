using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;


    private const string MUSIC_ON = "Music On";
    private const string MUSIC_OFF = "Music Off";
    private const string SFX_ON = "SFX On";
    private const string SFX_OFF = "SFX Off";

    [Header("Audio Source & Clip References")]
    [SerializeField] private AudioSource musicAudioSource;
    [SerializeField] private AudioSource sfxAudioSource;
    [SerializeField] private AudioClip winningClip;
    [SerializeField] private AudioClip loosingClip;
    [SerializeField] private AudioClip buttonPressedClip;

    [Header("Music/SFX Button & Text References")]
    [SerializeField] private Button musicButton;
    [SerializeField] private Button sfxButton;
    [SerializeField] private Text musicText;
    [SerializeField] private Text sfxText;


    [SerializeField] private Dictionary<SFXType, AudioClip> soundClipDictionary = new Dictionary<SFXType, AudioClip>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this.gameObject);
    }

    private void Start()
    {
        // intialize button listeners
        musicButton.onClick.AddListener(delegate { ToggleMusic(); });
        sfxButton.onClick.AddListener(delegate { ToggleSFX(); });

        // initialize text at start of game
        musicText.text = MUSIC_ON;
        sfxText.text = SFX_ON;

        // add clips to dictionary
        soundClipDictionary.Add(SFXType.ButtonPressed, buttonPressedClip);
        soundClipDictionary.Add(SFXType.Winning, winningClip);
        soundClipDictionary.Add(SFXType.Loosing, loosingClip);
    }

    /// <summary>
    /// Toggles between on and off music, labels button accordingly
    /// </summary>
    public void ToggleMusic()
    {
        if (!musicAudioSource.enabled)
            musicText.text = MUSIC_ON;
        else
            musicText.text = MUSIC_OFF;

        musicAudioSource.enabled = !musicAudioSource.enabled;
    }

    /// <summary>
    /// Toggles between on and off sfx, labels button accordingly
    /// </summary>
    public void ToggleSFX()
    {
        if (!sfxAudioSource.enabled)
            sfxText.text = SFX_ON;
        else
            sfxText.text = SFX_OFF;

        sfxAudioSource.enabled = !sfxAudioSource.enabled;
    }

    /// <summary>
    /// Plays the soundEffect according to its type
    /// </summary>
    /// <param name="sfxType"></param>
    public void PlaySoundEffect(SFXType sfxType)
    {
        sfxAudioSource.PlayOneShot(soundClipDictionary[sfxType]);
    }

}

public enum SFXType
{
    Winning,
    Loosing,
    ButtonPressed
}
