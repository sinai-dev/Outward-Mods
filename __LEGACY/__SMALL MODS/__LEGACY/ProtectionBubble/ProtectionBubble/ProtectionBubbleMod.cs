using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BepInEx;
using HarmonyLib;
using SideLoader;
using UnityEngine;
using SharedModConfig;

namespace ProtectionBubble
{
    [BepInPlugin(GUID, NAME, VERSION)]
    public class ProtectionBubbleMod : BaseUnityPlugin
    {
        public const string GUID = "com.sinai.protectionbubble";
        public const string NAME = "Protection Bubble";
        public const string VERSION = "1.0.0";

        public static ModConfig config;

        internal void Awake()
        {
            config = SetupConfig();
            config.Register();

            var harmony = new Harmony(GUID);
            harmony.PatchAll();

            SL.OnPacksLoaded += SL_OnPacksLoaded;
        }

        private void SL_OnPacksLoaded()
        {
            ProtectionBubble.Setup();
        }

        private ModConfig SetupConfig()
        {
            return new ModConfig
            {
                ModName = NAME,
                SettingsVersion = 1.0,
                Settings = new List<BBSetting>
                {
                    new BoolSetting
                    {
                        SectionTitle = "Shield Behaviour",
                        Name = Settings.OrigProtectionApplied,
                        DefaultValue = false,
                        Description = "Original Protection Stat still applied to incoming damage"
                    },
                    new FloatSetting
                    {
                        Name = Settings.RegenDelay,
                        DefaultValue = 5.0f,
                        Description = "Delay after last received damage for Protection Bubble to continue regenerating",
                        MinValue = 0f,
                        MaxValue = 30f,
                        Increment = 0.1f,
                        RoundTo = 1
                    },
                    new FloatSetting
                    {
                        Name = Settings.RegenRate,
                        DefaultValue = 0.1f,
                        Description = "Amount of Protection Bubble restored per 0.1 seconds (flat value).",
                        MinValue = 0.01f,
                        MaxValue = 10f,
                        Increment = 0.05f,
                        RoundTo = 1
                    },
                    new FloatSetting
                    {
                        Name = Settings.ShieldRatio,
                        Description = "Protection Stat to Shield Health ratio. Default is 2:1.",
                        DefaultValue = 200f,
                        MinValue = 1f,
                        MaxValue = 1000f,
                        Increment = 1f,
                        ShowPercent = true
                    },
                    new BoolSetting
                    {
                        SectionTitle = "Visual Effects",
                        Name = Settings.ShowFX,
                        Description = "Show Visual FX on Character when Shield is active",
                        DefaultValue = true
                    }
                }
            };
        }
    }

    public class Settings
    {
        public const string OrigProtectionApplied = "OrigProtectionApplied";
        public const string RegenDelay = "RegenDelay";
        public const string RegenRate = "RegenRate";
        public const string ShieldRatio = "ShieldRatio";
        public const string ShowFX = "ShowVFX";
    }
}
