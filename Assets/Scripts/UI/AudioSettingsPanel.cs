using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AudioSettingsPanel : MonoBehaviour
{
    [SerializeField]
    private Slider masterSlider;
    [SerializeField]
    private Slider musicSlider;
    [SerializeField]
    private Slider sfxSlider;
    [SerializeField]
    private Button saveButton;
    
    // Start is called before the first frame update
    void Start()
    {
        saveButton.onClick.AddListener(() => PlayerPrefs.Save());
        masterSlider.value = AudioSettings.MasterVolume;
        musicSlider.value = AudioSettings.MusicVolume;
        sfxSlider.value = AudioSettings.SfxVolume;
        masterSlider.onValueChanged.AddListener((value) => AudioSettings.MasterVolume = value);
        musicSlider.onValueChanged.AddListener((value) => AudioSettings.MusicVolume = value);
        sfxSlider.onValueChanged.AddListener((value) => AudioSettings.SfxVolume = value);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
