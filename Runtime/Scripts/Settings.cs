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


    //other things I want:
    //Graphical settings


    //LATER: GRAPHICS SETTINGS

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
        [SerializeField] bool fullScreenResolutionChanging;
        [SerializeField] DesiredResolution[] desiredResolutions;

        Dictionary<int, int> localResToScreenRes = new Dictionary<int, int>();
        SettingsSaveData settingsData;
        Resolution currentResolution;
        Resolution[] _resolutions;

        float masterVolume;
        float musicVolume;
        float sfxVolume;
        bool isVSync;
        bool isFullScreen;
        int currentTargetFPS;
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
                OnAwake_SetUIDropdownToSaved(ResolutionToDropdownIndex(Screen.currentResolution), resolutionDropdown);
            }
            //Sets to full screen with current resolutions
            else if (fullScreenResolutionChanging && isFullScreen)
            {
                Debug.Log($"CALLING THIS: {currentResolution}");
                OnAwake_SetUIDropdownToSaved(ResolutionToDropdownIndex(currentResolution), resolutionDropdown);
            }
            //sets to previously saved resolution (display size could be one of them)
            else if (!fullScreenResolutionChanging && !isFullScreen)
            {
                Debug.Log($"CALLING THIS: {currentResolution} 1");
                OnAwake_SetUIDropdownToSaved(ResolutionToDropdownIndex(currentResolution.width, currentResolution.height), resolutionDropdown);
            }
            //Sets to previously saved resolution
            else if (fullScreenResolutionChanging && !isFullScreen)
            {
                Debug.Log($"CALLING THIS: {currentResolution} 2");
                OnAwake_SetUIDropdownToSaved(ResolutionToDropdownIndex(currentResolution.width, currentResolution.height), resolutionDropdown);
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
            Debug.Log("SCREEN RES INDEX IS" + screenResIndex);
            Resolution resolution = ScreenResolutions[screenResIndex];
            Screen.SetResolution(resolution.width, resolution.height, isFullScreen);
            currentResolution = resolution;
            Debug.Log($"Setting resolution to: {currentResolution}");

            Debug.LogError($"Setting resolution to: {currentResolution}");

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
            currentTargetFPS = targetFPS;
            SetUIElementInteractable(isVSync, targetFPSDropdown);
            Debug.Log($"Setting Target FPS Limit to: {Application.targetFrameRate}");
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
            Debug.Log($"Saving resolution to: {currentResolution}");
            settingsData.Resolution = currentResolution;
            Debug.Log($"Saving Target FPS to: {currentTargetFPS}");
            settingsData.TargetFPS = currentTargetFPS;
        }

        void Awake()
        {
            settingsData = new SettingsSaveData();
            RefreshResolutionDropdownOptions();
            PopulateAvailableTargetFPS();
        }

        void Start()
        {
            OnAwake_SetUIVolumeSliderToSaved(settingsData.MasterVolume, masterSlider);
            OnAwake_SetUIVolumeSliderToSaved(settingsData.MusicVolume, musicSlider);
            OnAwake_SetUIVolumeSliderToSaved(settingsData.SFXVolume, sfxSlider);
            OnAwake_SetUIToggleToSaved(settingsData.VSync, vsyncToggle);
            //this needs to be called before full screen toggle to populate the currentResolution
            OnAwake_SetUIDropdownToSaved(ResolutionToDropdownIndex(settingsData.Resolution), resolutionDropdown);
            OnAwake_SetUIToggleToSaved(settingsData.FullScreen, fullScreenToggle);
            OnAwake_SetUIDropdownToSaved(TargetFPSToDropdownIndex(settingsData.TargetFPS), targetFPSDropdown);
        }

        //private void FixedUpdate()
        //{
        //    RefreshResolutionDropdownOptions();
        //}

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                Debug.Log(localResToScreenRes.Count);

                foreach (var lasdasd in localResToScreenRes)
                {
                    Debug.Log("KEY " + lasdasd.Key + " VALUE " + lasdasd.Value);
                }
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

        void OnAwake_SetUIVolumeSliderToSaved(float volume, Slider slider)
        {
            slider.value = volume;
        }

        void OnAwake_SetUIToggleToSaved(bool value, Toggle toggle)
        {
            toggle.isOn = value;
        }

        void OnAwake_SetUIDropdownToSaved(int value, TMP_Dropdown dropdown)
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
            Debug.LogError("SCREEN RES LENGTH IS: " + ScreenResolutions.Length);
            Debug.LogError($"WIDTH x HEIGHT is {width} {height}");
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