using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    [Header("Mixer Reference")]
    public AudioMixer masterMixer;

    [Header("Button Sprites")]
    public Sprite musicOnSprite;
    public Sprite musicOffSprite;
    public Sprite sfxOnSprite;
    public Sprite sfxOffSprite;

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
            SceneManager.sceneLoaded += OnSceneLoaded;

            // Load mute states from memory
            isMusicMuted = PlayerPrefs.GetInt("MusicMuted", 0) == 1;
            isSFXMuted = PlayerPrefs.GetInt("SFXMuted", 0) == 1;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Initial volume application on game start
        ApplyCurrentVolumes();
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // A small delay ensures UI is fully initialized before searching
        Invoke("RefreshUIReferences", 0.1f);
    }

    public void RefreshUIReferences()
    {
        // --- 1. SLIDER LOGIC ---
        // Using helper to find sliders even if they are inactive (inside a closed panel)
        Slider mSlider = FindObjectIncludingInactive<Slider>("MusicSlider") ?? FindObjectIncludingInactive<Slider>("MusicSlider 1");
        if (mSlider != null)
        {
            musicSlider = mSlider;
            musicSlider.value = PlayerPrefs.GetFloat("MusicVol", 0.75f);
            musicSlider.onValueChanged.RemoveAllListeners();
            musicSlider.onValueChanged.AddListener(SetMusicVolume);
        }

        Slider sSlider = FindObjectIncludingInactive<Slider>("SFXSlider") ?? FindObjectIncludingInactive<Slider>("SFXSlider 1");
        if (sSlider != null)
        {
            sfxSlider = sSlider;
            sfxSlider.value = PlayerPrefs.GetFloat("SFXVol", 0.75f);
            sfxSlider.onValueChanged.RemoveAllListeners();
            sfxSlider.onValueChanged.AddListener(SetSFXVolume);
        }

        // --- 2. BUTTON LOGIC ---
        // Using helper to find buttons even if they are inactive
        GameObject mBtnObj = FindObjectIncludingInactive("MusicButton") ?? FindObjectIncludingInactive("MusicButton 1");
        if (mBtnObj != null)
        {
            Button b = mBtnObj.GetComponent<Button>();
            Image img = mBtnObj.GetComponent<Image>();

            // Set initial sprite based on saved mute state
            img.sprite = isMusicMuted ? musicOffSprite : musicOnSprite;

            b.onClick.RemoveAllListeners();
            b.onClick.AddListener(() => ToggleMusic(img));
        }

        GameObject sBtnObj = FindObjectIncludingInactive("SFXButton") ?? FindObjectIncludingInactive("SFXButton 1");
        if (sBtnObj != null)
        {
            Button b = sBtnObj.GetComponent<Button>();
            Image img = sBtnObj.GetComponent<Image>();

            img.sprite = isSFXMuted ? sfxOffSprite : sfxOnSprite;

            b.onClick.RemoveAllListeners();
            b.onClick.AddListener(() => ToggleSFX(img));
        }
    }

    public void ToggleMusic(Image buttonImage)
    {
        isMusicMuted = !isMusicMuted;
        PlayerPrefs.SetInt("MusicMuted", isMusicMuted ? 1 : 0);

        buttonImage.sprite = isMusicMuted ? musicOffSprite : musicOnSprite;
        ApplyCurrentVolumes();
    }

    public void ToggleSFX(Image buttonImage)
    {
        isSFXMuted = !isSFXMuted;
        PlayerPrefs.SetInt("SFXMuted", isSFXMuted ? 1 : 0);

        buttonImage.sprite = isSFXMuted ? sfxOffSprite : sfxOnSprite;
        ApplyCurrentVolumes();
    }

    private void ApplyCurrentVolumes()
    {
        // Apply Music
        float mVol = isMusicMuted ? 0.0001f : PlayerPrefs.GetFloat("MusicVol", 0.75f);
        masterMixer.SetFloat("MusicVol", Mathf.Log10(Mathf.Max(mVol, 0.0001f)) * 20);

        // Apply SFX
        float sVol = isSFXMuted ? 0.0001f : PlayerPrefs.GetFloat("SFXVol", 0.75f);
        masterMixer.SetFloat("SFXVol", Mathf.Log10(Mathf.Max(sVol, 0.0001f)) * 20);
    }

    public void SetMusicVolume(float value)
    {
        PlayerPrefs.SetFloat("MusicVol", value);
        if (!isMusicMuted) ApplyCurrentVolumes();
    }

    public void SetSFXVolume(float value)
    {
        PlayerPrefs.SetFloat("SFXVol", value);
        if (!isSFXMuted) ApplyCurrentVolumes();
    }

    // --- HELPER FUNCTIONS ---

    // Finds any GameObject by name, even if it's deactivated
    private GameObject FindObjectIncludingInactive(string name)
    {
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if (obj.name == name) return obj;
        }
        return null;
    }

    // Finds a specific component type by name, even if deactivated
    private T FindObjectIncludingInactive<T>(string name) where T : Component
    {
        T[] allComponents = Resources.FindObjectsOfTypeAll<T>();
        foreach (T comp in allComponents)
        {
            if (comp.gameObject.name == name) return comp;
        }
        return null;
    }
}