using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace Nevelson.GameSettingOptions
{
    public class AudioSettings : SettingsBase
    {
        [SerializeField] AudioMixer audioMixer;
        [SerializeField] Slider masterSlider;
        [SerializeField] Slider musicSlider;
        [SerializeField] Slider sfxSlider;

        float masterVolume;
        float musicVolume;
        float sfxVolume;

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
        /// Saves the current settings to player prefs. 
        /// This is done automatically when this component is disabled or destroyed.
        /// </summary>
        public override void SaveAllData()
        {
            Debug.Log($"Saving Master Volume to: {masterVolume}");
            settingsData.MasterVolume = masterVolume;
            Debug.Log($"Saving Music Volume to: {musicVolume}");
            settingsData.MusicVolume = musicVolume;
            Debug.Log($"Saving SFX Volume to: {sfxVolume}");
            settingsData.SFXVolume = sfxVolume;
        }

        protected override void Awake() { base.Awake(); }

        protected override void Start()
        {
            base.Start();
            if (masterSlider) SetUIVolumeSlider(settingsData.MasterVolume, masterSlider);
            if (musicSlider) SetUIVolumeSlider(settingsData.MusicVolume, musicSlider);
            if (sfxSlider) SetUIVolumeSlider(settingsData.SFXVolume, sfxSlider);
        }

        protected override void OnDisable() { base.OnDisable(); }

        protected override void OnDestroy() { base.OnDestroy(); }

        void SetUIVolumeSlider(float volume, Slider slider)
        {
            slider.value = volume;
        }

        float ConvertVolumeToLogarithmic(float volume)
        {
            return Mathf.Log10(volume) * 20;
        }
    }
}