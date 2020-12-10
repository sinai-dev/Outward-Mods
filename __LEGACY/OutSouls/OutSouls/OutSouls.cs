using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using BepInEx;
using HarmonyLib;
using SharedModConfig;

namespace OutSoulsMod
{
    [BepInPlugin(GUID, NAME, VERSION)]
    [BepInDependency(SharedModConfig.SharedModConfig.GUID, BepInDependency.DependencyFlags.HardDependency)]    
    public class OutSouls : BaseUnityPlugin
    {
        public const string GUID = "com.sinai.outsouls";
        public const string VERSION = "2.2";
        public const string NAME = "OutSouls";

        public static OutSouls Instance;

        public static ModConfig config;

        internal void Awake()
        {
            Instance = this;

            this.gameObject.AddComponent<BonfireManager>();
            this.gameObject.AddComponent<BonfireGUI>();
            this.gameObject.AddComponent<RPCManager>();

            var harmony = new Harmony(GUID);
            harmony.PatchAll();
        }

        internal void Start()
        {
            config = SetupConfig();
            config.Register();
        }

        private ModConfig SetupConfig()
        {
            var newConfig = new ModConfig
            {
                ModName = "OutSouls",
                SettingsVersion = 1.0,
                Settings = new List<BBSetting>
                {
                    new BoolSetting
                    {
                        Name = Settings.Enable_Bonfire_System,
                        DefaultValue = true,
                        Description = "Enable Bonfires (requires scene reload)",
                    },
                    new BoolSetting
                    {
                        Name = Settings.Bonfires_Heal_Enemies,
                        DefaultValue = false,
                        Description = "Bonfires heal and resurrect enemies",
                    },
                    new BoolSetting
                    {
                        Name = Settings.Disable_Bonfire_Costs,
                        DefaultValue = false,
                        Description = "Disable all Bonfire costs",
                    },
                    new BoolSetting
                    {
                        Name = Settings.Cant_Use_Bonfires_In_Combat,
                        DefaultValue = true,
                        Description = "Can't use Bonfires in combat",
                    },
                }
            };

            return newConfig;
        }

        public bool CanInteract(Character c)
        {
            return (bool)At.Call(c, "CanInteract", null);
        }
        
    }

    public class Settings
    {
        public static string Enable_Bonfire_System = "Enable_Bonfire_System";
        public static string Bonfires_Heal_Enemies = "Bonfires_Heal_Enemies";
        public static string Disable_Bonfire_Costs = "Disable_Bonfire_Costs";
        public static string Cant_Use_Bonfires_In_Combat = "Cant_Use_Bonfires_In_Combat";
        //public static string Disable_Scaling = "Disable_Scaling";
    }
}
