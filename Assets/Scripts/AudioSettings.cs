using UnityEngine;

public static class AudioSettings {
    private static bool loadedSettings = false;

    private static float masterVolume;
    private static float musicVolume;
    private static float sfxVolume;

    public static float MasterVolume {
        get {
            if (!loadedSettings) {
                LoadSettings();
            }
            return masterVolume;
        }
        set {
            PlayerPrefs.SetFloat("masterVolume", value);
            masterVolume = value;
        }
    }

    public static float MusicVolume {
        get {
            if (!loadedSettings) {
                LoadSettings();
            }
            return musicVolume;
        }
        set {
            PlayerPrefs.SetFloat("musicVolume", value);
            musicVolume = value;
        }
    }

    public static float SfxVolume {
        get {
            if (!loadedSettings) {
                LoadSettings();
            }
            return sfxVolume;
        }
        set {
            PlayerPrefs.SetFloat("sfxVolume", value);
            sfxVolume = value;
        }
    }

    private static void LoadSettings() {
        if (PlayerPrefs.HasKey("masterVolume")) {
            masterVolume = PlayerPrefs.GetFloat("masterVolume");
        } else {
            masterVolume = 1;
        }

        if (PlayerPrefs.HasKey("musicVolume")) {
            musicVolume = PlayerPrefs.GetFloat("musicVolume");
        } else {
            musicVolume = 1;
        }

        if (PlayerPrefs.HasKey("sfxVolume")) {
            sfxVolume = PlayerPrefs.GetFloat("sfxVolume");
        } else {
            sfxVolume = 1;
        }
        
        loadedSettings = true;
    }
}