using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using Slider = UnityEngine.UI.Slider;
using Toggle = UnityEngine.UI.Toggle;

namespace Nevelson.GameSettingOptions
{
    //How To:
    //Create an audio mixer named MasterVolume (EXPLAIN BETTER)
    //Create two audio mixer groups and name them: MusicVolume and SFXVolume (EXPLAIN BETTER)
    //Expose their volume channels (EXPLAIN BETTER)
    //Reference the audiomixer in this script
    //Make a UI container for your settings elements and add this script to it
    //Create all ui elements and reference them in this script
    //In the ui elements, add the correct script methods to the On Value changed events (EXPLAIN BETTER)

    //Functionality:
    //On awake sets all UI elements to the saved values
    //On start sets all option settings to their saved values
    //Master volume controls both music and sfx
    //Clicking full screen disables resolution options and sets to current screen's resolution
    //Disabling full screen lets you set different resolution options

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

    public class Settings : MonoBehaviour
    {
        [SerializeField] AudioMixer audioMixer;
        [SerializeField] Slider masterSlider;
        [SerializeField] Slider musicSlider;
        [SerializeField] Slider sfxSlider;
        [SerializeField] Toggle vsyncToggle;
        [SerializeField] Toggle fullScreenToggle;
        [SerializeField] TMP_Dropdown resolutionDropdown;
        [SerializeField] TMP_Dropdown targetFPSDropdown;
        [SerializeField] TMP_Dropdown graphicsDropdown;
        [SerializeField] bool fullScreenResolutionChanging;
        [SerializeField] DesiredResolution[] desiredResolutions;

        Dictionary<int, int> localResToScreenRes = new Dictionary<int, int>();
        SettingsSaveData settingsData;
        Resolution[] _resolutions;
        Resolution resolution;
        string[] graphicsSettingNames;
        float masterVolume;
        float musicVolume;
        float sfxVolume;
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
        /// Sets the volume of the master mixer.  
        /// Called ONLY by the on change even for slider values
        /// </summary>
        /// <param name="volume"></param>
        public void SetMasterValue(float volume)
        {
            Debug.Log($"Setting Master volume to: {volume}");
            audioMixer.SetFloat("MasterVolume", ConvertVolumeToLogarithmic(volume));
            masterVolume = volume;
        }

        /// <summary>
        /// Sets the volume of the master mixer.  
        /// Called ONLY by the on change even for slider values
        /// </summary>
        /// <param name="volume"></param>
        public void SetMusicValue(float volume)
        {
            Debug.Log($"Setting Music volume to: {volume}");
            audioMixer.SetFloat("MusicVolume", ConvertVolumeToLogarithmic(volume));
            musicVolume = volume;
        }

        /// <summary>
        /// Sets the volume of the master mixer.  
        /// Called ONLY by the on change even for slider values
        /// </summary>
        /// <param name="volume"></param>
        public void SetSFXValue(float volume)
        {
            Debug.Log($"Setting SFX volume to: {volume}");
            audioMixer.SetFloat("SFXVolume", ConvertVolumeToLogarithmic(volume));
            sfxVolume = volume;
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
        public void SaveAllData()
        {
            Debug.Log($"Saving Master Volume to: {masterVolume}");
            settingsData.MasterVolume = masterVolume;
            Debug.Log($"Saving Music Volume to: {musicVolume}");
            settingsData.MusicVolume = musicVolume;
            Debug.Log($"Saving SFX Volume to: {sfxVolume}");
            settingsData.SFXVolume = sfxVolume;
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

        void Awake()
        {
            settingsData = new SettingsSaveData();
            RefreshResolutionDropdownOptions();
            PopulateAvailableTargetFPS();
            PopulateAvailableGraphicalLevels();
            SetCorrectGraphicalDefault();
        }

        void Start()
        {
            SetUIVolumeSlider(settingsData.MasterVolume, masterSlider);
            SetUIVolumeSlider(settingsData.MusicVolume, musicSlider);
            SetUIVolumeSlider(settingsData.SFXVolume, sfxSlider);
            SetUIToggle(settingsData.VSync, vsyncToggle);
            SetUIDropdown(TargetFPSToDropdownIndex(settingsData.TargetFPS), targetFPSDropdown);
            SetUIDropdown(settingsData.Graphics, graphicsDropdown);
            //this needs to be called before full screen toggle to populate the currentResolution
            SetUIDropdown(ResolutionToDropdownIndex(settingsData.Resolution), resolutionDropdown);
            SetUIToggle(settingsData.FullScreen, fullScreenToggle);
        }

        private void FixedUpdate()
        {
            //removes resolutions unsupported by monitor when moving monitors
            if (_resolutions == null || Screen.resolutions.Length != _resolutions.Length)
            {
                RefreshResolutionDropdownOptions();
                _resolutions = Screen.resolutions;
                SetUIDropdown(0, resolutionDropdown);
            }
        }

        void OnDisable()
        {
            SaveAllData();
        }

        void OnDestroy()
        {
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
            List<DesiredResolution> dRes = desiredResolutions.ToList();
            bool match = dRes.Exists(
                x => x.width == Screen.currentResolution.width &&
                x.height == Screen.currentResolution.height
                );
            if (!match)
            {
                dRes.Add(new DesiredResolution(Screen.currentResolution.width,
                    Screen.currentResolution.height));
            }
            desiredResolutions = dRes.ToArray();

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
            graphicsSettingNames = QualitySettings.names;
            List<string> graphicsList = new List<string>();
            for (int i = 0; i < graphicsSettingNames.Length; i++)
            {
                string name = graphicsSettingNames[i];
                graphicsList.Add(name);
            }
            graphicsDropdown.AddOptions(graphicsList);
        }

        void SetUIVolumeSlider(float volume, Slider slider)
        {
            slider.value = volume;
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

        float ConvertVolumeToLogarithmic(float volume)
        {
            return Mathf.Log10(volume) * 20;
        }
    }
}