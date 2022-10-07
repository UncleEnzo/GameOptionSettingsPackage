using System;
using System.Collections.Generic;
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
    //ability to choose between predetermined resolutions and all resos (for pixel art games)
    //Graphical settings

    //different defaults with the fullscreen/resolution relationship || Allow setting it during 


    //Edge cases>
    //Game gets moved to a different monitor and the list of options changes (need to refresh every time resolution changing becomes available
    //


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

        SettingsSaveData settingsData;
        Resolution[] resolutions;
        Resolution currentResolution;
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


        //issues:
        //need to hammer out resolution changing when in full screen and not
        //CURRENTRESOLUTION is the screen's current resolution not your actual value
        //Need to lower amount of resolutions based on hertz
        //need to have a resolution preset option



        //TESTING:
        //REFRESHES THE RESOLUTIONS AVAILABLE BASED ON DISPLAY
        //LATER: GRAPHICS SETTINGS


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
                OnAwake_SetUIDropdownToSaved(ResolutionToDropdownIndex(currentResolution), resolutionDropdown);
            }
            //sets to previously saved resolution (display size could be one of them)
            else if (!fullScreenResolutionChanging && !isFullScreen)
            {
                OnAwake_SetUIDropdownToSaved(ResolutionToDropdownIndex(currentResolution.width, currentResolution.height), resolutionDropdown);
            }
            //Sets to previously saved resolution
            else if (fullScreenResolutionChanging && !isFullScreen)
            {
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

        public void SetResolutionValue(int indexValue)
        {
            Resolution resolution = resolutions[indexValue];
            Screen.SetResolution(resolution.width, resolution.height, isFullScreen);
            currentResolution = resolution;
            Debug.Log($"Setting resolution to: {currentResolution}");
        }

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
            //TODO NEED TO FIGURE OUT HZ AS WELL , currenlty it just takes first of any resolution
            //couple of options > Collect the actual screen's resolution and filter any options that DON'T HAVE THE SAME HZ
            PopulateAvailableResolutionsDropdown();
            //CollectApprovedResolutions(); // this is just the list you allow (For pixel art game)
            PopulateAvailableTargetFPS();

            OnAwake_SetUIVolumeSliderToSaved(settingsData.MasterVolume, masterSlider);
            OnAwake_SetUIVolumeSliderToSaved(settingsData.MusicVolume, musicSlider);
            OnAwake_SetUIVolumeSliderToSaved(settingsData.SFXVolume, sfxSlider);
            OnAwake_SetUIToggleToSaved(settingsData.VSync, vsyncToggle);
            OnAwake_SetUIToggleToSaved(settingsData.FullScreen, fullScreenToggle);
            OnAwake_SetUIDropdownToSaved(ResolutionToDropdownIndex(settingsData.Resolution), resolutionDropdown);
            OnAwake_SetUIDropdownToSaved(TargetFPSToDropdownIndex(settingsData.TargetFPS), targetFPSDropdown);
        }

        void Start()
        {
            SetMasterValue(settingsData.MasterVolume);
            SetMusicValue(settingsData.MusicVolume);
            SetSFXValue(settingsData.SFXVolume);
            SetResolutionValue(ResolutionToDropdownIndex(settingsData.Resolution));
            SetFullScreenValue(settingsData.FullScreen);
            SetTargetFrameRateValue(TargetFPSToDropdownIndex(settingsData.TargetFPS));
            SetVsyncValue(settingsData.VSync);
        }

        void OnDisable()
        {
            SaveAllData();
        }

        void OnDestroy()
        {
            SaveAllData();
        }

        void PopulateAvailableResolutionsDropdown()
        {
            resolutions = Screen.resolutions;
            resolutionDropdown.ClearOptions();
            List<string> resolutionsList = new List<string>();
            for (int i = 0; i < resolutions.Length; i++)
            {
                Resolution resolution = resolutions[i];
                string res = $"{resolution.width} x {resolution.height}";
                resolutionsList.Add(res);
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
                string fps = fpsCap == -1 ? "UnLimited" : $"{fpsCap}";
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
            int currentResolutionIndex = 0;
            for (int i = 0; i < resolutions.Length; i++)
            {
                Resolution resolution = resolutions[i];
                if (resolution.width == value.width &&
                    resolution.height == value.height)
                {
                    currentResolutionIndex = i;
                }
            }
            return currentResolutionIndex;
        }

        int ResolutionToDropdownIndex(int width, int height)
        {
            int currentResolutionIndex = 0;
            for (int i = 0; i < resolutions.Length; i++)
            {
                Resolution resolution = resolutions[i];
                if (resolution.width == width &&
                    resolution.height == height)
                {
                    currentResolutionIndex = i;
                }
            }
            return currentResolutionIndex;
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