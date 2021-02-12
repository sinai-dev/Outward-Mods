using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Xml.Serialization;

namespace SharedModConfig
{
    public class ModConfig
    {
        // Public, Serializable
        public string ModName;
        public double SettingsVersion;
        public List<BBSetting> Settings = new List<BBSetting>();
        
        /// <summary> Callback for when your Settings are loaded by the game</summary>
        public event Action OnSettingsLoaded;
        /// <summary> Callback for when your Settings are saved by the user</summary>
        public event Action OnSettingsSaved;
        /// <summary> Callback for when your Settings are opened by the user in the Mod Config Menu </summary>
        public event Action OnSettingsOpened;

        // internal use only
        [XmlIgnore] internal readonly Dictionary<string, BBSetting> m_Settings = new Dictionary<string, BBSetting>();
        [XmlIgnore] internal GameObject m_linkedPanel;

        public void Register()
        {
            foreach (var setting in Settings)
            {
                m_Settings.Add(setting.Name, setting);
            }

            ConfigManager.RegisterSettings(this);
        }

        public void INTERNAL_OnSettingsLoaded()
        {
            if (OnSettingsLoaded != null)
            {
                Debug.Log("OnSettingsLoaded Callback for " + this.ModName);
                OnSettingsLoaded.Invoke();
            }
        }

        public void INTERNAL_OnSettingsSaved()
        {
            foreach (var setting in Settings)
            {
                setting.UpdateValue(false);
            }

            if (OnSettingsSaved != null)
            {
                //Debug.Log("OnSettingsSaved Callback for " + this.ModName);
                OnSettingsSaved.Invoke();
            }
        }

        public void INTERNAL_OnSettingsOpened()
        {
            OnSettingsOpened?.Invoke();
        }

        public object GetValue(string SettingName)
        {
            if (m_Settings.ContainsKey(SettingName))
            {
                return m_Settings[SettingName].GetValue();
            }
            else
            {
                Debug.LogError("[SharedModConfig] A mod requested the value of '" + SettingName + "' on Config '" + this.ModName + "', but such a setting was not found!");
                return null;
            }
        }

        public void SetValue(string SettingName, object value)
        {
            if (m_Settings.ContainsKey(SettingName))
            {
                m_Settings[SettingName].SetValue(value);
            }
            else
            {
                Debug.LogError("[SharedModConfig] A mod tried to set the value of '" + SettingName + "' on Config '" + this.ModName + "', but such a setting was not found!");
            }
        }
    }
}
