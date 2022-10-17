using Nevelson.GameSettingOptions;
using UnityEngine;

public abstract class SettingsBase : MonoBehaviour
{
    [SerializeField] bool saveDataOnDisable = true;
    [SerializeField] bool saveDataOnDestroy = true;
    protected SettingsSaveData settingsData;

    public abstract void SaveAllData();

    protected virtual void Awake() { settingsData = new SettingsSaveData(); }

    protected virtual void Start() { }

    protected virtual void OnDisable() { if (saveDataOnDisable) SaveAllData(); }

    protected virtual void OnDestroy() { if (saveDataOnDestroy) SaveAllData(); }
}
