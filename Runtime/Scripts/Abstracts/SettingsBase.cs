using Nevelson.GameSettingOptions;
using UnityEngine;

public abstract class SettingsBase : MonoBehaviour
{
    protected SettingsSaveData settingsData;

    public abstract void SaveAllData();

    protected virtual void Awake() { settingsData = new SettingsSaveData(); }

    protected virtual void Start() { }

    protected virtual void OnDisable() { SaveAllData(); }

    protected virtual void OnDestroy() { SaveAllData(); }
}
