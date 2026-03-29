using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    [Header("Mixer Reference")]
    public AudioMixer masterMixer;

    // These will be auto-filled by the script whenever a scene loads
    private Slider musicSlider;
    private Slider sfxSlider;

    private bool isMusicMuted = false;
    private bool isSFXMuted = false;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            // This tells Unity: "Every time a scene is loaded, run the 'OnSceneLoaded' function"
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // This runs automatically whenever you switch scenes
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        RefreshSliderReferences();
    }

    public void RefreshSliderReferences()
    {
        // 1. Find the sliders in the NEW scene by their names
        GameObject mObj = GameObject.Find("MusicSlider");
        GameObject sObj = GameObject.Find("SFXSlider");

        if (mObj != null)
        {
            musicSlider = mObj.GetComponent<Slider>();

            // 2. Set the slider handle to the SAVED volume immediately
            musicSlider.value = PlayerPrefs.GetFloat("MusicVol", 0.75f);

            // 3. Tell the slider to talk to THIS script when moved
            musicSlider.onValueChanged.RemoveAllListeners(); // Clear old links
            musicSlider.onValueChanged.AddListener(SetMusicVolume);
        }

        if (sObj != null)
        {
            sfxSlider = sObj.GetComponent<Slider>();
            sfxSlider.value = PlayerPrefs.GetFloat("SFXVol", 0.75f);

            sfxSlider.onValueChanged.RemoveAllListeners();
            sfxSlider.onValueChanged.AddListener(SetSFXVolume);
        }
    }

    public void SetMusicVolume(float value)
    {
        if (!isMusicMuted)
        {
            float dB = Mathf.Log10(Mathf.Max(value, 0.0001f)) * 20;
            masterMixer.SetFloat("MusicVol", dB);
        }
        PlayerPrefs.SetFloat("MusicVol", value);
    }

    public void SetSFXVolume(float value)
    {
        if (!isSFXMuted)
        {
            float dB = Mathf.Log10(Mathf.Max(value, 0.0001f)) * 20;
            masterMixer.SetFloat("SFXVol", dB);
        }
        PlayerPrefs.SetFloat("SFXVol", value);
    }
}