using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Toggle = UnityEngine.UI.Toggle;

namespace Nevelson.GameSettingOptions
{
    [Serializable]
    public struct DesiredResolution
    {
        public int width;
        public int height;

        public DesiredResolution(int width, int height)
        {
            this.width = width;
            this.height = height;
        }

        public override string ToString()
        {
            return $"{width} x {height}";
        }
    }

    public class VideoSettings : SettingsBase
    {
        [SerializeField] Toggle vsyncToggle;
        [SerializeField] Toggle fullScreenToggle;
        [SerializeField] TMP_Dropdown resolutionDropdown;
        [SerializeField] TMP_Dropdown targetFPSDropdown;
        [SerializeField] TMP_Dropdown graphicsDropdown;
        [SerializeField] bool fullScreenResolutionChanging;
        [SerializeField] bool vSyncFPSChanging;
        [SerializeField] DesiredResolution[] desiredResolutions;

        DesiredResolution m_resolution;
        VideoSettingsData settingsData;

        string[] graphicsSettingNames;
        bool isVSync;
        bool isFullScreen;
        int targetFPS;
        int graphics;
        int[] fpsCaps = new int[]
        {
            15,
            30,
            60,
            120,
            200,
            300,
            500,
            1000,
        };

        /// <summary>
        /// Sets the vSync to enabled or disabled.
        /// Enabled vSync = 120 fps  
        /// </summary>
        /// <param name="value"></param>
        public void SetVsyncValue(bool value)
        {
            Debug.Log($"Setting vSync to: {value}");
            QualitySettings.vSyncCount = value ? 1 : 0;
            isVSync = value;

            if (!vSyncFPSChanging)
            {
                SetUIElementActive(isVSync, targetFPSDropdown);
            }
        }

        /// <summary>
        /// Sets whether full screen is enabled or disabled.
        /// Enabled full screen locks resolution to match the screen the user is currently using
        /// </summary>
        /// <param name="value"></param>
        public void SetFullScreenValue(bool value)
        {
            isFullScreen = value;
            Screen.fullScreen = isFullScreen;

            //Setting res to match the current resolution of the screen
            if (isFullScreen)
            {
                int screenWidth = Display.main.systemWidth;
                int screenHeight = Display.main.systemHeight;
                DesiredResolution resolution = new DesiredResolution(screenWidth, screenHeight);
                Screen.SetResolution(resolution.width, resolution.height, isFullScreen);
                m_resolution = resolution;
            }

            if (!fullScreenResolutionChanging)
            {
                SetUIElementActive(isFullScreen, resolutionDropdown);
            }

            Debug.Log($"Setting fullscreen: {isFullScreen} | resolution to: {m_resolution}");
        }

        /// <summary>
        /// Sets the value of the game's resolution.
        /// </summary>
        /// <param name="indexValue"></param>
        public void SetResolutionValue(int indexValue)
        {
            DesiredResolution resolution = desiredResolutions[indexValue];
            Screen.SetResolution(resolution.width, resolution.height, isFullScreen);
            m_resolution = resolution;
            Debug.Log($"Setting resolution to: {m_resolution} | fullscreen: {isFullScreen}");
        }

        /// <summary>
        /// Sets the target framerate for the game.
        /// If vSync is enabled, you cannot select a target framerate
        /// </summary>
        /// <param name="indexValue"></param>
        public void SetTargetFrameRateValue(int indexValue)
        {
            if (isVSync)
            {
                Debug.Log("Cannot set Target FrameRate when Vsync enabled");
                return;
            }

            int targetFPS = fpsCaps[indexValue];
            Application.targetFrameRate = targetFPS;
            this.targetFPS = targetFPS;
            SetUIElementActive(isVSync, targetFPSDropdown);
            Debug.Log($"Setting Target FPS Limit to: {Application.targetFrameRate}");
        }

        /// <summary>
        /// Sets the Graphics level for the game.
        /// If no graphics are selected then defaults to middle of the road graphics option
        /// </summary>
        /// <param name="indexValue"></param>
        public void SetGraphicsValue(int indexValue)
        {
            Debug.Log($"Setting Graphics to {graphicsSettingNames[indexValue]}");
            graphics = indexValue;
            QualitySettings.SetQualityLevel(graphics);
        }

        /// <summary>
        /// Saves the current settings to player prefs. 
        /// This is done automatically when this component is disabled or destroyed.
        /// </summary>
        public override void SaveAllData()
        {
            Debug.Log($"Saving VSync to: {isVSync}");
            settingsData.VSync = isVSync;
            Debug.Log($"Saving Full Screen to: {isFullScreen}");
            settingsData.FullScreen = isFullScreen;
            Debug.Log($"Saving resolution to: {m_resolution}");
            settingsData.Resolution = m_resolution;
            Debug.Log($"Saving Target FPS to: {targetFPS}");
            settingsData.TargetFPS = targetFPS;
            Debug.Log($"Saving Graphics level to: {graphicsSettingNames[graphics]}");
            settingsData.Graphics = graphics;
        }

        /// <summary>
        /// Refreshes if resolution and fps dropdowns are interactable based on the vSync and fullscreen toggles.
        /// </summary>
        public void RefreshUIElementInteractables()
        {
            Debug.Log("Refreshing interactables.");
            if (!vSyncFPSChanging)
            {
                SetUIElementActive(isVSync, targetFPSDropdown);
            }

            if (!fullScreenResolutionChanging)
            {
                SetUIElementActive(isFullScreen, resolutionDropdown);
            }
        }

        protected override void Awake()
        {
            base.Awake();
            settingsData = new VideoSettingsData();

            Debug.Log($"Video settings: VSync {settingsData.VSync}");
            Debug.Log($"Video settings: Full Screen {settingsData.FullScreen}");
            Debug.Log($"Video settings: Resolution {settingsData.Resolution}");
            Debug.Log($"Video settings: Target FPS {settingsData.TargetFPS}");
            graphicsSettingNames = QualitySettings.names;
            Debug.Log($"Video settings: Graphics {settingsData.Graphics} | {graphicsSettingNames[settingsData.Graphics]}");
            if (resolutionDropdown) PopulateResolutionOptions();
            if (targetFPSDropdown) PopulateAvailableTargetFPS();
            if (graphicsDropdown) PopulateAvailableGraphicalLevels();

            //I call this here once because if the values don't change from UI below they don't get set on init
            if (targetFPSDropdown) SetTargetFrameRateValue(TargetFPSToDropdownIndex(settingsData.TargetFPS));
            if (vsyncToggle) SetVsyncValue(settingsData.VSync);
            if (resolutionDropdown) SetResolutionValue(ResolutionToDropdownIndex(settingsData.Resolution));
            if (fullScreenToggle) SetFullScreenValue(settingsData.FullScreen);
            if (graphicsDropdown) SetGraphicsValue(settingsData.Graphics);
            else SetGraphicsValue(settingsData.Graphics);
        }

        protected override void Start()
        {
            base.Start();
            if (graphicsDropdown) SetUIDropdown(settingsData.Graphics, graphicsDropdown);
            if (targetFPSDropdown) SetUIDropdown(TargetFPSToDropdownIndex(settingsData.TargetFPS), targetFPSDropdown);
            if (vsyncToggle) SetUIToggle(settingsData.VSync, vsyncToggle);
            //this needs to be called before full screen toggle to populate the currentResolution
            if (resolutionDropdown) SetUIDropdown(ResolutionToDropdownIndex(settingsData.Resolution), resolutionDropdown);
            if (fullScreenToggle) SetUIToggle(settingsData.FullScreen, fullScreenToggle);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            SaveAllData();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            SaveAllData();
        }

        void PopulateResolutionOptions()
        {
            resolutionDropdown.ClearOptions();
            List<string> resolutionsList = new List<string>();
            foreach (var resolution in desiredResolutions)
            {
                resolutionsList.Add(resolution.ToString());
            }
            resolutionDropdown.AddOptions(resolutionsList);
        }

        void PopulateAvailableTargetFPS()
        {
            targetFPSDropdown.ClearOptions();
            List<string> fpsList = new List<string>();
            for (int i = 0; i < fpsCaps.Length; i++)
            {
                int fpsCap = fpsCaps[i];
                string fps = fpsCap == -1 ? "Unlimited" : $"{fpsCap}";
                fpsList.Add(fps);
            }
            targetFPSDropdown.AddOptions(fpsList);
        }

        void PopulateAvailableGraphicalLevels()
        {
            graphicsDropdown.ClearOptions();
            List<string> graphicsList = new List<string>();
            for (int i = 0; i < graphicsSettingNames.Length; i++)
            {
                string name = graphicsSettingNames[i];
                graphicsList.Add(name);
            }
            graphicsDropdown.AddOptions(graphicsList);
        }

        void SetUIToggle(bool value, Toggle toggle)
        {
            toggle.isOn = value;
        }

        void SetUIDropdown(int value, TMP_Dropdown dropdown)
        {
            dropdown.value = value;
            dropdown.RefreshShownValue();
        }

        void SetUIElementActive(bool value, TMP_Dropdown dropdown)
        {
            dropdown.interactable = value ? false : true;
        }

        int ResolutionToDropdownIndex(DesiredResolution value)
        {
            return Array.IndexOf(desiredResolutions, value);
        }

        int TargetFPSToDropdownIndex(int value)
        {
            return Array.IndexOf(fpsCaps, value);
        }
    }
}