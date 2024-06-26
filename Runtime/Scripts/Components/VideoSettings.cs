using System;
using System.Collections.Generic;
using System.Linq;
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
    }

    public class VideoSettings : SettingsBase
    {
        [SerializeField] Toggle vsyncToggle;
        [SerializeField] Toggle fullScreenToggle;
        [SerializeField] TMP_Dropdown resolutionDropdown;
        [SerializeField] TMP_Dropdown targetFPSDropdown;
        [SerializeField] TMP_Dropdown graphicsDropdown;
        [SerializeField] bool fullScreenResolutionChanging;
        [SerializeField] DesiredResolution[] desiredResolutions;

        VideoSettingsData settingsData;
        Dictionary<int, int> localResToScreenRes = new Dictionary<int, int>();
        Resolution[] _resolutions;
        Resolution resolution;
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
            -1,
        };

        Resolution[] ScreenResolutions
        {
            get
            {
                if (_resolutions == null || Screen.resolutions.Length != _resolutions.Length)
                {
                    RefreshResolutionDropdownOptions();
                    _resolutions = Screen.resolutions;
                }
                return _resolutions;
            }
        }

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
            SetUIElementInteractable(isVSync, targetFPSDropdown);
        }

        /// <summary>
        /// Sets whether full screen is enabled or disabled.
        /// Enabled full screen locks resolution to match the screen the user is currently using
        /// </summary>
        /// <param name="value"></param>
        public void SetFullScreenValue(bool value)
        {
            Debug.Log($"Setting full screen to: {value}");
            isFullScreen = value;
            Screen.fullScreen = isFullScreen;

            //sets to full screen with native display size resolution
            if (!fullScreenResolutionChanging && isFullScreen)
            {
                SetUIDropdown(ResolutionToDropdownIndex(Screen.currentResolution), resolutionDropdown);
            }
            //Sets to full screen with current resolutions
            else if (fullScreenResolutionChanging && isFullScreen)
            {
                SetUIDropdown(ResolutionToDropdownIndex(resolution), resolutionDropdown);
            }
            //sets to previously saved resolution (display size could be one of them)
            else if (!fullScreenResolutionChanging && !isFullScreen)
            {
                SetUIDropdown(ResolutionToDropdownIndex(resolution.width, resolution.height), resolutionDropdown);
            }
            //Sets to previously saved resolution
            else if (fullScreenResolutionChanging && !isFullScreen)
            {
                SetUIDropdown(ResolutionToDropdownIndex(resolution.width, resolution.height), resolutionDropdown);
            }
            else
            {
                Debug.LogError($"Unaccounted: fullscreenResChange = {fullScreenResolutionChanging}. isFullScreen = {isFullScreen}");
            }

            if (!fullScreenResolutionChanging)
            {
                SetUIElementInteractable(isFullScreen, resolutionDropdown);
            }
        }

        /// <summary>
        /// Sets the value of the game's resolution.
        /// </summary>
        /// <param name="indexValue"></param>
        public void SetResolutionValue(int indexValue)
        {
            int screenResIndex = localResToScreenRes[indexValue];
            Resolution resolution = ScreenResolutions[screenResIndex];
            Screen.SetResolution(resolution.width, resolution.height, isFullScreen);
            this.resolution = resolution;
            Debug.Log($"Setting resolution to: {this.resolution}");
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
            SetUIElementInteractable(isVSync, targetFPSDropdown);
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
            Debug.Log($"Saving resolution to: {resolution}");
            settingsData.Resolution = resolution;
            Debug.Log($"Saving Target FPS to: {targetFPS}");
            settingsData.TargetFPS = targetFPS;
            Debug.Log($"Saving Graphics to: {graphicsSettingNames[graphics]}");
            settingsData.Graphics = graphics;
        }

        /// <summary>
        /// Refreshes if resolution and fps dropdowns are interactable based on the vSync and fullscreen toggles.
        /// </summary>
        public void RefreshUIElementInteractables()
        {
            Debug.Log("Refreshing interactables.");
            if (targetFPSDropdown && vsyncToggle)
            {
                targetFPSDropdown.interactable = !settingsData.VSync;
            }

            if (resolutionDropdown && fullScreenToggle)
            {
                resolutionDropdown.interactable = !settingsData.FullScreen;
            }
        }

        protected override void Awake()
        {
            base.Awake();
            settingsData = new VideoSettingsData();

            Debug.Log($"Found VSync: {settingsData.VSync}");
            Debug.Log($"Found Full Screen: {settingsData.FullScreen}");
            Debug.Log($"Found Resolution {settingsData.Resolution}");
            Debug.Log($"Found Target FPS: {settingsData.TargetFPS}");
            Debug.Log($"Found Graphics: {settingsData.Graphics}");
            graphicsSettingNames = QualitySettings.names;
            if (resolutionDropdown) RefreshResolutionDropdownOptions();
            if (targetFPSDropdown) PopulateAvailableTargetFPS();
            if (graphicsDropdown) PopulateAvailableGraphicalLevels();
            if (graphicsDropdown) SetCorrectGraphicalDefault();

            //I call this here once because if the values don't change from UI below they don't get set on init
            if (targetFPSDropdown) SetTargetFrameRateValue(TargetFPSToDropdownIndex(settingsData.TargetFPS));
            if (vsyncToggle) SetVsyncValue(settingsData.VSync);
            if (resolutionDropdown) SetResolutionValue(ResolutionToDropdownIndex(settingsData.Resolution));
            if (fullScreenToggle) SetFullScreenValue(settingsData.FullScreen);
            if (graphicsDropdown) SetGraphicsValue(settingsData.Graphics);
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

        void FixedUpdate()
        {
            RepopulateDropdownOptionsOnDisplayChange();
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

        void SetCorrectGraphicalDefault()
        {
            if (settingsData.Graphics == -1)
            {
                //rounds to nearest if odd
                settingsData.Graphics = graphicsSettingNames.Length - 1 / 2;
            }
        }

        void RepopulateDropdownOptionsOnDisplayChange()
        {
            if (_resolutions != null && Screen.resolutions.Length == _resolutions.Length)
            {
                return;
            }

            RefreshResolutionDropdownOptions();
            _resolutions = Screen.resolutions;
            SetUIDropdown(0, resolutionDropdown);
        }

        void RefreshResolutionDropdownOptions()
        {
            if (desiredResolutions == null || desiredResolutions.Length == 0)
            {
                PopulateAvailableResolutionsDropdown();
            }
            else
            {
                PopulateApprovedResolutionsDropdown();
            }
        }

        void PopulateApprovedResolutionsDropdown()
        {
            //Add the current screen resolution as an option
            List<DesiredResolution> desiredRes = desiredResolutions.ToList();
            bool match = desiredRes.Exists(
                res => res.width == Screen.currentResolution.width &&
                res.height == Screen.currentResolution.height
                );
            if (!match)
            {
                desiredRes.Add(new DesiredResolution(Screen.currentResolution.width,
                    Screen.currentResolution.height));
            }
            desiredResolutions = desiredRes.ToArray();

            //only extract ones that are on the approved list
            resolutionDropdown.ClearOptions();
            localResToScreenRes.Clear();
            List<string> resolutionsList = new List<string>();
            for (int i = 0; i < Screen.resolutions.Length; i++)
            {
                Resolution resolution = Screen.resolutions[i];
                foreach (var approvedResolution in desiredResolutions)
                {
                    if (approvedResolution.width == resolution.width &&
                        approvedResolution.height == resolution.height)
                    {
                        string res = $"{resolution.width} x {resolution.height} @ {resolution.refreshRate}";
                        resolutionsList.Add(res);
                        localResToScreenRes.Add(resolutionsList.Count - 1, i);
                    }
                }
            }
            resolutionDropdown.AddOptions(resolutionsList);
        }

        void PopulateAvailableResolutionsDropdown()
        {
            resolutionDropdown.ClearOptions();
            localResToScreenRes.Clear();
            List<string> resolutionsList = new List<string>();
            for (int i = 0; i < Screen.resolutions.Length; i++)
            {
                Resolution resolution = Screen.resolutions[i];
                string res = $"{resolution.width} x {resolution.height} @ {resolution.refreshRate}";
                resolutionsList.Add(res);
                localResToScreenRes.Add(resolutionsList.Count - 1, i);
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

        void SetUIElementInteractable(bool value, TMP_Dropdown dropdown)
        {
            dropdown.interactable = value ? false : true;
        }

        int ResolutionToDropdownIndex(Resolution value)
        {
            return ResolutionToDropdownIndex(value.width, value.height);
        }

        int ResolutionToDropdownIndex(int width, int height)
        {
            Dictionary<Resolution, int> resolutionContendors = new Dictionary<Resolution, int>();
            for (int i = 0; i < ScreenResolutions.Length; i++)
            {
                Resolution resolution = ScreenResolutions[i];
                if (resolution.width == width &&
                    resolution.height == height)
                {

                    resolutionContendors.Add(resolution, i);
                }
            }

            if (resolutionContendors.Count == 0)
            {
                throw new UnityException($"Could not find any resolutions with correct {width} x {height}");
            }

            KeyValuePair<Resolution, int> largestHz = resolutionContendors.First();
            foreach (var resolution in resolutionContendors)
            {
                if (resolution.Key.refreshRate > largestHz.Key.refreshRate)
                {
                    largestHz = resolution;
                }
            }

            foreach (var pair in localResToScreenRes)
            {
                if (pair.Value == largestHz.Value)
                {
                    return pair.Key;
                }
            }

            throw new UnityException($"Could not find corresponding key to res index: {largestHz.Value}");
        }

        int TargetFPSToDropdownIndex(int value)
        {
            return Array.IndexOf(fpsCaps, value);
        }
    }
}