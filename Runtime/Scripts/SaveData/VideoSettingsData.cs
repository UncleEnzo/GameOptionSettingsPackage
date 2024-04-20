using UnityEngine;

namespace Nevelson.GameSettingOptions
{
    public class VideoSettingsData
    {
        const string VSYNC = "VSYNC";
        const string FULL_SCREEN = "FULLSCREEN";
        const string RESOLUTION = "RESOLUTION";
        const string TARGET_FPS = "FPS";
        const string GRAPHICS = "GRAPHICS";
        const bool DEFAULT_VSYNC = false;
        const bool DEFAULT_FULL_SCREEN = true;
        const int DEFAULT_TARGET_FPS = 120;
        const int DEFAULT_GRAPHICS = -1;

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

        public int Graphics
        {
            get => PlayerPrefs.GetInt(GRAPHICS);
            set => PlayerPrefs.SetInt(GRAPHICS, value);
        }

        /// <summary>
        /// Constructs the class for global game settings.  
        /// </summary>
        public VideoSettingsData()
        {
            if (!PlayerPrefs.HasKey(VSYNC) ||
                !PlayerPrefs.HasKey(FULL_SCREEN) ||
                !PlayerPrefs.HasKey(RESOLUTION) ||
                !PlayerPrefs.HasKey(TARGET_FPS) ||
                !PlayerPrefs.HasKey(GRAPHICS)
                )

            {
                Debug.Log("Initializing Settings Data");
                VSync = DEFAULT_VSYNC;
                FullScreen = DEFAULT_FULL_SCREEN;
                Resolution = Screen.currentResolution;
                TargetFPS = DEFAULT_TARGET_FPS;
                Graphics = DEFAULT_GRAPHICS;
            }
            else
            {
                Debug.Log($"Found Settings Data");
            }
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
            int height = int.Parse(value.Split('x')[1].Split('@')[0].TrimStart(' ').TrimEnd(' '));
            int refreshRate = int.Parse(value.Split('@')[1].Split('H')[0].TrimStart(' '));

            foreach (var resolution in Screen.resolutions)
            {
                if (resolution.width == width &&
                   resolution.height == height &&
                   resolution.refreshRate == refreshRate)
                {
                    return resolution;
                }
            }
            Debug.LogError($"Could not find resolution {value}");
            return Screen.currentResolution;
        }

        static string ResolutionToString(Resolution value)
        {
            return $"{value.width} x {value.height} @ {value.refreshRate} Hz";
        }
    }
}