using UnityEngine;

namespace Nevelson.GameSettingOptions
{
    //todo, need to do away with this entirely at some point
    //Trying to separte Audio / Video

    //WANT TO KEEP THE SAVE DATA ON DISABLE LOGIC
    public abstract class SettingsBase : MonoBehaviour
    {
        [SerializeField] bool saveDataOnDisable = true;
        [SerializeField] bool saveDataOnDestroy = true;

        public abstract void SaveAllData();

        protected virtual void Awake() { }

        protected virtual void Start() { }

        protected virtual void OnDisable() { if (saveDataOnDisable) SaveAllData(); }

        protected virtual void OnDestroy() { if (saveDataOnDestroy) SaveAllData(); }
    }
}