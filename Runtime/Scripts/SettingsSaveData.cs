using UnityEngine;

namespace Nevelson.GameSettingOptions
{
    public class SettingsSaveData
    {
        const string MASTER_VOLUME = "MASTER";
        const string MUSIC_VOLUME = "MUSIC";
        const string SFX_VOLUME = "SFX";
        const string VSYNC = "VSYNC";
        const string FULL_SCREEN = "FULLSCREEN";
        const string RESOLUTION = "RESOLUTION";
        const string TARGET_FPS = "FPS";
        const float DEFAULT_VOLUME = 1f;
        const bool DEFAULT_VSYNC = false;
        const bool DEFAULT_FULL_SCREEN = true;
        const int DEFAULT_TARGET_FPS = 120;

        public float MasterVolume
        {
            get => PlayerPrefs.GetFloat(MASTER_VOLUME);
            set => PlayerPrefs.SetFloat(MASTER_VOLUME, value);
        }

        public float MusicVolume
        {
            get => PlayerPrefs.GetFloat(MUSIC_VOLUME);
            set => PlayerPrefs.SetFloat(MUSIC_VOLUME, value);
        }

        public float SFXVolume
        {
            get => PlayerPrefs.GetFloat(SFX_VOLUME);
            set => PlayerPrefs.SetFloat(SFX_VOLUME, value);
        }

        public bool VSync
        {
            get => SaveValToBool(PlayerPrefs.GetInt(VSYNC));
            set => PlayerPrefs.SetInt(VSYNC, BoolToSaveVal(value));
        }

        public bool FullScreen
        {
            get => SaveValToBool(PlayerPrefs.GetInt(FULL_SCREEN));
            set => PlayerPrefs.SetInt(FULL_SCREEN, BoolToSaveVal(value));
        }

        public Resolution Resolution
        {
            get => StringToResolution(PlayerPrefs.GetString(RESOLUTION));
            set => PlayerPrefs.SetString(RESOLUTION, ResolutionToString(value));
        }

        public int TargetFPS
        {
            get => PlayerPrefs.GetInt(TARGET_FPS);
            set => PlayerPrefs.SetInt(TARGET_FPS, value);
        }

        /// <summary>
        /// Constructs the class for global game settings.  
        /// </summary>
        public SettingsSaveData()
        {
            if (!PlayerPrefs.HasKey(MASTER_VOLUME) ||
                !PlayerPrefs.HasKey(MUSIC_VOLUME) ||
                !PlayerPrefs.HasKey(SFX_VOLUME) ||
                !PlayerPrefs.HasKey(VSYNC) ||
                !PlayerPrefs.HasKey(FULL_SCREEN) ||
                !PlayerPrefs.HasKey(RESOLUTION) ||
                !PlayerPrefs.HasKey(TARGET_FPS)
                )

            {
                Debug.Log("Initializing Settings Data");
                MasterVolume = DEFAULT_VOLUME;
                MusicVolume = DEFAULT_VOLUME;
                SFXVolume = DEFAULT_VOLUME;
                VSync = DEFAULT_VSYNC;
                FullScreen = DEFAULT_FULL_SCREEN;
                Resolution = Screen.currentResolution;
                TargetFPS = DEFAULT_TARGET_FPS;
            }
            else
            {
                Debug.Log($"Found Settings Data");
            }

            Debug.Log($"Master Volume: {MasterVolume}");
            Debug.Log($"Music Volume: {MusicVolume}");
            Debug.Log($"SFX Volume: {SFXVolume}");
            Debug.Log($"VSync: {VSync}");
            Debug.Log($"Full Screen: {FullScreen}");
            Debug.Log($"Resolution {Resolution}");
            Debug.Log($"Target FPS: {TargetFPS}");
        }

        static int BoolToSaveVal(bool isTrue)
        {
            return isTrue ? 1 : 0;
        }

        static bool SaveValToBool(int isTrue)
        {
            if (isTrue != 1 && isTrue != 0)
            {
                throw new UnityException("Incorrect Value");
            }

            return isTrue == 1;
        }

        static Resolution StringToResolution(string value)
        {
            int width = int.Parse(value.Substring(0, value.IndexOf(" ")));
            int height = int.Parse(value.Split('x')[1].TrimStart(' '));
            foreach (var resolution in Screen.resolutions)
            {
                if (resolution.width == width &&
                   resolution.height == height)
                {
                    return resolution;
                }
            }
            Debug.LogError($"Could not find resolution {value}");
            return Screen.currentResolution;
        }

        static string ResolutionToString(Resolution value)
        {
            return $"{value.width} x {value.height}";
        }
    }
}