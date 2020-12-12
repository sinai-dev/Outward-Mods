using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using SharedModConfig;
using BepInEx;
using HarmonyLib;

namespace SharedCoopRewards
{
    public class Settings
    {
        public static readonly string Shared_Quest_Rewards = "Shared_Quest_Rewards";
        public static readonly string Shared_ALL_Quest_Rewards = "Share ALL Quest Rewards";
        public static readonly string Shared_World_Drops = "Shared_World_Drops";
    }

    [BepInPlugin(GUID, NAME, VERSION)]
    [BepInDependency("com.sinai.SharedModConfig", BepInDependency.DependencyFlags.HardDependency)]
    public class SharedCoopRewards : BaseUnityPlugin
    {
        const string GUID = "com.sinai.sharedcooprewards";
        const string NAME = "Shared Coop Rewards";
        const string VERSION = "2.1";

        public static ModConfig config;

        internal void Awake()
        {
            // LoadSettings();
            config = SetupConfig();
            config.Register();

            var harmony = new Harmony(GUID);
            harmony.PatchAll();
        }

        private ModConfig SetupConfig()
        {
            var newConfig = new ModConfig
            {
                ModName = "SharedCoopRewards",
                SettingsVersion = 1.1,
                Settings = new List<BBSetting>
                {
                    new BoolSetting
                    {
                        Name = Settings.Shared_Quest_Rewards,
                        Description = "Share Items and Skills from Quest Rewards (safer, may not cover everything)",
                        DefaultValue = true
                    },
                    new BoolSetting
                    {
                        Name = Settings.Shared_ALL_Quest_Rewards,
                        Description = "Share ALL Quest Rewards (may occasionally lead to unexpected things being shared)",
                        DefaultValue = false
                    },
                    new BoolSetting
                    {
                        Name = Settings.Shared_World_Drops,
                        Description = "Generate extra loot from Enemies and Loot Containers for each player",
                        DefaultValue = true
                    }
                }
            };

            return newConfig;
        }
    }
}
