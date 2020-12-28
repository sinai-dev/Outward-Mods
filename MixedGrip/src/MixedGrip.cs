using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using SharedModConfig;
using SideLoader;

namespace MixedGrip
{ 
    [BepInPlugin(GUID, NAME, VERSION)]
    [BepInDependency(SharedModConfig.SharedModConfig.GUID, BepInDependency.DependencyFlags.HardDependency)]
    public class MixedGrip : BaseUnityPlugin
    {
        public const string GUID = "com.sinai.mixedgrip";
        public const string NAME = "Mixed Grip";
        public const string VERSION = "3.2";

        public static MixedGrip Instance;
        public static ModConfig config;

        public const string TOGGLE_KEY = "Toggle Weapon Grip";

        private static GameObject s_rpcObj;

        internal void Awake()
        {
            Instance = this;

            var harmony = new Harmony(GUID);
            harmony.PatchAll();

            // custom keybindings
            CustomKeybindings.AddAction(TOGGLE_KEY, KeybindingsCategory.CustomKeybindings, ControlType.Both);

            config = SetupConfig();
            config.Register();

            s_rpcObj = new GameObject("MixedGripRPC");
            DontDestroyOnLoad(s_rpcObj);
            s_rpcObj.AddComponent<GripManager>();
        }

        private ModConfig SetupConfig()
        {
            var newConfig = new ModConfig
            {
                ModName = "Mixed Grip",
                SettingsVersion = 1.0,
                Settings = new List<BBSetting>
                {
                    new BoolSetting
                    {
                        Name = Settings.Swap_Animations,
                        Description = "Swap animations when swapping grip (for Swords, Axes and Maces)",
                        DefaultValue = true
                    },
                    new BoolSetting
                    {
                        Name = Settings.Swap_On_Equip_And_Unequip,
                        Description = "Automatically adjust grip when equipping or un-equipping gear",
                        DefaultValue = true
                    },
                    new BoolSetting
                    {
                        Name = Settings.Balance_Weapons,
                        Description = "Balance weapons when swapping grip (nerf if becoming 1H, buff if becoming 2H)",
                        DefaultValue = true
                    },
                }
            };

            return newConfig;
        }
    }

    public class Settings
    {
        public static string Swap_Animations = "Swap_Animations";
        public static string Swap_On_Equip_And_Unequip = "Swap_On_Equip_And_Unequip";
        public static string Balance_Weapons = "Balance_Weapons";
    }
}
