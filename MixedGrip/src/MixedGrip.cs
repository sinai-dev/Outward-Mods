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
using SideLoader;
using BepInEx.Configuration;

namespace MixedGrip
{ 
    [BepInPlugin(GUID, NAME, VERSION)]
    public class MixedGrip : BaseUnityPlugin
    {
        public const string GUID = "com.sinai.mixedgrip";
        public const string NAME = "Mixed Grip";
        public const string VERSION = "3.3";

        public static MixedGrip Instance;
        // public static ModConfig config;

        public const string TOGGLE_KEY = "Toggle Weapon Grip";

        private static GameObject s_rpcObj;

        internal void Awake()
        {
            Instance = this;

            var harmony = new Harmony(GUID);
            harmony.PatchAll();

            // custom keybindings
            CustomKeybindings.AddAction(TOGGLE_KEY, KeybindingsCategory.CustomKeybindings, ControlType.Both);

            SetupConfig();

            s_rpcObj = new GameObject("MixedGripRPC");
            DontDestroyOnLoad(s_rpcObj);
            s_rpcObj.AddComponent<GripManager>();
        }

        internal const string CTG = "Grip Swap Settings";
        internal static ConfigEntry<bool> SwapAnimations;
        internal static ConfigEntry<bool> AutoSwapOnEquipChange;
        internal static ConfigEntry<bool> BalanceWeaponsOnSwap;

        private void SetupConfig()
        {
            SwapAnimations = Config.Bind(CTG, "Swap animations to alternate style?", true, "For swords, axes and maces, should animations be swapped to the alternate style (1h/2h)?");
            AutoSwapOnEquipChange = Config.Bind(CTG, "Automatically swap grip on equipment change?", true, "Should Mixed Grip automatically swap your grip for you when changing equipment?");
            BalanceWeaponsOnSwap = Config.Bind(CTG, "Balance weapons on grip-swap?", true, "Should weapon stats be balanced according to the amount the attack speed changed?");
        }
    }
}
