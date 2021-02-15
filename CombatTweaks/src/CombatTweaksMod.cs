using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using BepInEx;
using HarmonyLib;
using BepInEx.Configuration;

namespace CombatAndDodgeOverhaul
{
    [BepInPlugin(GUID, NAME, VERSION)]
    public class CombatTweaksMod : BaseUnityPlugin
    {
        const string GUID = "com.sinai.combattweaks";
        const string NAME = "Combat Tweaks";
        const string VERSION = "3.0";

        public static CombatTweaksMod Instance;

        internal void Awake()
        {
            Instance = this;

            SetupConfig();

            var harmony = new Harmony(GUID);
            harmony.PatchAll();
        }

        // ============ settings ============== //

        internal const string CTG_HITSTOP = "Hit-Stop Tweaks";
        internal static ConfigEntry<float> HitStop_Modifier;

        internal const string CTG_DODGE = "Dodge Tweaks";
        internal static ConfigEntry<bool> Dodge_Cancelling;
        internal static ConfigEntry<float> Dodge_DelayAfterStagger;
        internal static ConfigEntry<float> Dodge_DelayAfterKnockdown;
        internal static ConfigEntry<float> Dodge_DelayAfterPlayerHits;

        internal const string CTG_BLOCK = "Blocking Tweaks";
        internal static ConfigEntry<bool> Blocking_CancelledByAttack;

        private void SetupConfig()
        {
            HitStop_Modifier = Config.Bind(CTG_HITSTOP, "Hit-Stop effect modifier", 0.3f, 
                new ConfigDescription("Controls the severity of the 'hit-stop' effect when an attack collides with a target.\n\n1.0 = no change.", new AcceptableValueRange<float>(0f, 10f)));

            Dodge_Cancelling = Config.Bind(CTG_DODGE, "Dodge cancelling allowed?", true, "Allows you to interrupt actions with a dodge.");
            Dodge_DelayAfterStagger = Config.Bind(CTG_DODGE, "Dodge delay after stagger", 0.8f, 
                new ConfigDescription("Delay after being staggered before you are allowed to dodge again.", new AcceptableValueRange<float>(0f, 2f)));
            Dodge_DelayAfterKnockdown = Config.Bind(CTG_DODGE, "Dodge delay after knockdown", 2.0f, 
                new ConfigDescription("Delay after being staggered before you are allowed to dodge again.", new AcceptableValueRange<float>(0f, 5f)));
            Dodge_DelayAfterPlayerHits = Config.Bind(CTG_DODGE, "Dodge delay after you hit someone", 0.35f,
                new ConfigDescription("Delay after hitting an enemy with your weapon before you can dodge again.", new AcceptableValueRange<float>(0f, 2f)));

            Blocking_CancelledByAttack = Config.Bind(CTG_BLOCK, "Blocking cancelled by attack input?", true, "Allows you to interrupt blocking by attacking.");
        }
    }

    // ============ LEGACY ==============

    //public class Settings
    //{
    //    // player settings
    //    public static string Dodge_Cancelling = "Dodge_Cancelling";
    //    public static string Dodge_DelayAfterStagger = "Dodge_Delay_After_Stagger";
    //    public static string Dodge_DelayAfterKD = "Dodge_Delay_After_KD";
    //    public static string Dodge_DelayAfterHit = "Dodge_DelayAfterHit";

    //    public static string Attack_Cancels_Blocking = "Attack_Cancels_Blocking";
    //    public static string Blocking_Staggers_Attacker = "Blocking_Staggers_Attacker";
    //    public static string No_Stability_Regen_When_Blocking = "No_Stability_Regen_When_Blocking";

    //    // animation collision slowdown
    //    public static string SlowDown_Modifier = "SlowDown_Modifier";

    //    // stability mods
    //    public static string Stagger_Threshold = "Stagger_Threshold";
    //    public static string Stagger_Immunity_Period = "Stagger_Immunity_Period";
    //    public static string Knockdown_Threshold = "Knockdown_Threshold";

    // ~~~~~~~ deprecated ~~~~~~~

    //    public static string Stamina_Regen_Delay = "Stamina_Regen_Delay";
    //    public static string Extra_Stamina_Regen = "Extra_Stamina_Regen";

    //    //public static string Stamina_Cost_Stat = "Stamina_Cost_Stat";
    //    //public static string Custom_Dodge_Cost = "Custom_Dodge_Cost";
    //    //public static string Custom_Bag_Burden = "Custom_Bag_Burden";
    //    //public static string min_burden_weight = "min_burden_weight";
    //    //public static string min_slow_effect = "min_slow_effect";
    //    //public static string max_slow_effect = "max_slow_effect";

    //    //// enemy mods
    //    //public static string All_Enemies_Allied = "All_Enemies_Allied";
    //    //public static string Enemy_Balancing = "Enemy_Balancing";
    //    //public static string Enemy_Health = "Enemy_Health"; // multiplier
    //    //public static string Enemy_Damages = "Enemy_Damages"; // flat to damage bonus
    //    //public static string Enemy_ImpactDmg = "Enemy_ImpactDmg";
    //    //public static string Enemy_Resistances = "Enemy_Resistances"; // flat (scaled)
    //    //public static string Enemy_ImpactRes = "Enemy_ImpactRes";

    //    // public static string Stability_Regen_Speed = "Stability_Regen_Speed";
    //    // public static string Stability_Regen_Delay = "Stability_Regen_Delay";
    //    //public static string Enemy_AutoKD_Count = "Enemy_AutoKD_Count";
    //    //public static string Enemy_AutoKD_Reset_Time = "Enemy_AutoKD_Reset_Time";
    //}
}

