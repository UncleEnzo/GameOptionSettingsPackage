using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace Nevelson.GameSettingOptions
{
    public class AudioSettings : SettingsBase
    {
        const string AUDIO_LOGGING = "GameSettingsAudio: ";
        const string MASTER_VOLUME_KEY = "MASTERVolume";
        const string MUSIC_VOLUME_KEY = "MUSICVolume";
        const string SFX_VOLUME_KEY = "SFXVolume";
        const string UI_VOLUME_KEY = "UIVolume";

        [SerializeField] AudioMixer audioMixer;
        [SerializeField] Slider masterVolumeSlider;
        [SerializeField] Slider musicVolumeSlider;
        [SerializeField] Slider sfxVolumeSlider;
        [SerializeField] Slider uiVolumeSlider;

        [SerializeField, Range(0, 1f)] float defaultMasterVolume = 1f;
        [SerializeField, Range(0, 1f)] float defaultMusicVolume = .5f;
        [SerializeField, Range(0, 1f)] float defaultSFXVolume = .5f;
        [SerializeField, Range(0, 1f)] float defaultUIVolume = .5f;

        public float MasterVolume
        {
            get => PlayerPrefs.GetFloat(MASTER_VOLUME_KEY, defaultMasterVolume);
            set => PlayerPrefs.SetFloat(MASTER_VOLUME_KEY, value);
        }

        public float MusicVolume
        {
            get => PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, defaultMusicVolume);
            set => PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY, value);
        }

        public float SFXVolume
        {
            get => PlayerPrefs.GetFloat(SFX_VOLUME_KEY, defaultSFXVolume);
            set => PlayerPrefs.SetFloat(SFX_VOLUME_KEY, value);
        }

        public float UIVolume
        {
            get => PlayerPrefs.GetFloat(UI_VOLUME_KEY, defaultUIVolume);
            set => PlayerPrefs.SetFloat(UI_VOLUME_KEY, value);
        }

        protected override void Awake()
        {
            base.Awake();

        }

        protected override void Start()
        {
            base.Start();
            if (!PlayerPrefs.HasKey(MASTER_VOLUME_KEY) ||
                !PlayerPrefs.HasKey(MUSIC_VOLUME_KEY) ||
                !PlayerPrefs.HasKey(SFX_VOLUME_KEY) ||
                !PlayerPrefs.HasKey(UI_VOLUME_KEY))
            {
                //default value supplied in the getters.  doing this just for logging purposes to verify
                Debug.Log($"{AUDIO_LOGGING}Could not find audio player prefs. Initializing \n" +
                    $"Master volume {MasterVolume}\n" +
                    $"Music volume {MusicVolume}\n" +
                    $"SFX volume {SFXVolume}\n" +
                    $"UI volume {UIVolume}\n");
            }
            else
            {
                Debug.Log($"{AUDIO_LOGGING} found audio player prefs:\n" +
                    $"Master volume {MasterVolume}\n" +
                    $"Music volume {MusicVolume}\n" +
                    $"SFX volume {SFXVolume}\n" +
                    $"UI volume {UIVolume}");
            }

            //I call this here once because if the values don't change from UI below they don't get set on init
            if (masterVolumeSlider) SetMasterMixerValue(MasterVolume);
            if (musicVolumeSlider) SetMusicMixerValue(MusicVolume);
            if (sfxVolumeSlider) SetSFXMixerValue(SFXVolume);
            if (uiVolumeSlider) SetUIMixerValue(UIVolume);

            if (masterVolumeSlider) SetVolumeSlider(MasterVolume, masterVolumeSlider);
            if (musicVolumeSlider) SetVolumeSlider(MusicVolume, musicVolumeSlider);
            if (sfxVolumeSlider) SetVolumeSlider(SFXVolume, sfxVolumeSlider);
            if (uiVolumeSlider) SetVolumeSlider(UIVolume, uiVolumeSlider);
        }

        protected override void OnDisable() { base.OnDisable(); }

        protected override void OnDestroy() { base.OnDestroy(); }

        /// <summary>
        /// Sets the volume of the master mixer.  
        /// Called ONLY by the on change even for slider values
        /// </summary>
        /// <param name="volume"></param>
        public void SetMasterMixerValue(float volume)
        {
            Debug.Log($"{AUDIO_LOGGING}Setting Master volume to: {volume}");
            audioMixer.SetFloat("MasterVolume", ConvertVolumeToLogarithmic(volume));
            MasterVolume = volume;
        }

        /// <summary>
        /// Sets the volume of the music mixer.  
        /// Called ONLY by the on change even for slider values
        /// </summary>
        /// <param name="volume"></param>
        public void SetMusicMixerValue(float volume)
        {
            Debug.Log($"{AUDIO_LOGGING}Setting Music volume to: {volume}");
            audioMixer.SetFloat("MusicVolume", ConvertVolumeToLogarithmic(volume));
            MusicVolume = volume;
        }

        /// <summary>
        /// Sets the volume of the SFX mixer.  
        /// Called ONLY by the on change even for slider values
        /// </summary>
        /// <param name="volume"></param>
        public void SetSFXMixerValue(float volume)
        {
            Debug.Log($"{AUDIO_LOGGING}Setting SFX volume to: {volume}");
            audioMixer.SetFloat("SFXVolume", ConvertVolumeToLogarithmic(volume));
            SFXVolume = volume;
        }

        /// <summary>
        /// Sets the volume of the UI mixer.  
        /// Called ONLY by the on change even for slider values
        /// </summary>
        /// <param name="volume"></param>
        public void SetUIMixerValue(float volume)
        {
            Debug.Log($"{AUDIO_LOGGING}Setting UI volume to: {volume}");
            audioMixer.SetFloat("UIVolume", ConvertVolumeToLogarithmic(volume));
            UIVolume = volume;
        }

        /// <summary>
        /// Saves the current settings to player prefs. 
        /// This is done automatically when this component is disabled or destroyed.
        /// </summary>
        public override void SaveAllData()
        {
            PlayerPrefs.Save();
        }

        void SetVolumeSlider(float volume, Slider slider)
        {
            slider.value = volume;
        }

        float ConvertVolumeToLogarithmic(float volume)
        {
            return 20f * Mathf.Log10(volume);
        }
    }
}