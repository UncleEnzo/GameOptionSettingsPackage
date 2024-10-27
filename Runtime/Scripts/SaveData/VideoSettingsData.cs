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
        const bool DEFAULT_FULL_SCREEN = false;
        const int DEFAULT_TARGET_FPS = 120;
        DesiredResolution DEFAULT_RESOLUTION = new DesiredResolution()
        {
            width = 1920,
            height = 1080,
        };

        int GRAPHICS_DEFAULT()
        {
            return QualitySettings.names.Length - 1;
        }

        public bool VSync
        {
            get => IntToBool(PlayerPrefs.GetInt(VSYNC, BoolToInt(DEFAULT_VSYNC)));
            set => PlayerPrefs.SetInt(VSYNC, BoolToInt(value));
        }

        public bool FullScreen
        {
            get => IntToBool(PlayerPrefs.GetInt(FULL_SCREEN, BoolToInt(DEFAULT_FULL_SCREEN)));
            set => PlayerPrefs.SetInt(FULL_SCREEN, BoolToInt(value));
        }

        public DesiredResolution Resolution
        {
            get => StringToResolution(PlayerPrefs.GetString(RESOLUTION, ResolutionToString(DEFAULT_RESOLUTION)));
            set => PlayerPrefs.SetString(RESOLUTION, ResolutionToString(value));
        }

        public int TargetFPS
        {
            get => PlayerPrefs.GetInt(TARGET_FPS, DEFAULT_TARGET_FPS);
            set => PlayerPrefs.SetInt(TARGET_FPS, value);
        }

        public int Graphics
        {
            get => PlayerPrefs.GetInt(GRAPHICS, GRAPHICS_DEFAULT());
            set => PlayerPrefs.SetInt(GRAPHICS, value);
        }

        /// <summary>
        /// Constructs the class for global game settings.  
        /// </summary>
        public VideoSettingsData() { }

        static int BoolToInt(bool isTrue)
        {
            return isTrue ? 1 : 0;
        }

        static bool IntToBool(int isTrue)
        {
            if (isTrue != 1 && isTrue != 0)
            {
                throw new UnityException("Incorrect Value");
            }

            return isTrue == 1;
        }

        static DesiredResolution StringToResolution(string value)
        {
            int width = int.Parse(value.Substring(0, value.IndexOf(" ")));
            int height = int.Parse(value.Split('x')[1]);
            return new DesiredResolution(width, height);
        }

        static string ResolutionToString(DesiredResolution value)
        {
            return $"{value.width} x {value.height}";
        }
    }
}