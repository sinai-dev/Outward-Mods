using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.IO;
using SideLoader;
using BepInEx;
using HarmonyLib;
using SharedModConfig;
using System.Collections;

namespace SlowerTime
{
    [BepInPlugin(GUID, NAME, VERSION)]
    public class CustomTimeSpeed : BaseUnityPlugin
    {
        public enum Settings
        {
            Time_Multiplier,
        }

        const string GUID = "com.sinai.customtimespeed";
        const string NAME = "Custom Time Speed";
        const string VERSION = "2.0";

        public static ModConfig config;

        internal void Awake()
        {
            config = SetupConfig();
            config.Register();

            new Harmony(GUID).PatchAll();
        }

        [HarmonyPatch(typeof(TOD_Time), nameof(TOD_Time.AddSeconds))]
        public class TOD_Time_AddSeconds
        {
            [HarmonyPrefix]
            public static void Prefix(ref float seconds)
            {
                seconds *= (float)config.GetValue(Settings.Time_Multiplier.ToString());
            }
        }

        private ModConfig SetupConfig()
        {
            var newConfig = new ModConfig
            {
                ModName = "Custom Time Speed",
                SettingsVersion = 1.0,
                Settings = new List<BBSetting>
                {
                    new FloatSetting
                    {
                        Name = Settings.Time_Multiplier.ToString(),
                        Description = "Time Multiplier (1.0x = normal time, 0.5x = half speed, 2.0x = double)",
                        DefaultValue = 1.0f,
                        MinValue = 0f,
                        MaxValue = 20f,
                        RoundTo = 2,
                        ShowPercent = false
                    }
                }
            };

            return newConfig;
        }
    }
}
