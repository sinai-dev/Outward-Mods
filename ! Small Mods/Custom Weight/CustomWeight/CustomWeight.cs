using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using SharedModConfig;
using BepInEx;
using HarmonyLib;

namespace CustomWeight
{
    [BepInPlugin(GUID, NAME, VERSION)]
    [BepInDependency("com.sinai.SharedModConfig", BepInDependency.DependencyFlags.HardDependency)]
    public class CustomWeight : BaseUnityPlugin
    {
        const string GUID = "com.sinai.customweight";
        const string NAME = "Custom Weight";
        const string VERSION = "2.2";

        public static CustomWeight Instance;
        public ModConfig config;

        public float m_timeOfLastUpdate;

        // original capacities on bags (ID : Capacity)
        public Dictionary<int, float> OrigCapacities = new Dictionary<int, float>();

        internal void Start()
        {
            Instance = this;

            var harmony = new Harmony(GUID);
            harmony.PatchAll();

            // set up and load settings
            config = SetupConfig();
            config.Register();

            config.OnSettingsSaved += Config_OnSettingsSaved;
        }

        // hook for updating container weights on settings save
        private void Config_OnSettingsSaved()
        {
            UpdateAllPlayers();
        }

        internal void Update()
        {
            if (Time.time - m_timeOfLastUpdate > 2f)
            {
                m_timeOfLastUpdate = Time.time;

                if (Global.Lobby.PlayersInLobbyCount > 0 && !NetworkLevelLoader.Instance.IsGameplayPaused)
                {
                    UpdateAllPlayers();
                }
            }
        }

        private void UpdateAllPlayers()
        {
            foreach (PlayerSystem player in Global.Lobby.PlayersInLobby)
            {
                if (player.ControlledCharacter)
                {
                    if (!player) { continue; }

                    UpdatePlayer(player.ControlledCharacter);
                }
            }
        }

        private void UpdatePlayer(Character player)
        {
            float newValue = (bool)config.GetValue(Settings.NoContainerLimit) ? -1 : 10.0f + (float)config.GetValue(Settings.PouchBonus);

            if ((float)At.GetValue(typeof(ItemContainer), player.Inventory.Pouch, "m_baseContainerCapacity") != newValue)
            {
                At.SetValue(newValue, typeof(ItemContainer), player.Inventory.Pouch, "m_baseContainerCapacity");
            }

            if (player.Inventory.EquippedBag)
            {
                UpdateBag(player.Inventory.EquippedBag);
            }

            //foreach (var container in player.GetComponentsInChildren<ItemContainer>())
            //{
            //    container.UpdateVersion();
            //}
        }

        private void UpdateBag(Bag bag)
        {
            float cap;

            if (At.GetValue(typeof(Bag), bag, "m_container") is ItemContainerStatic container)
            {
                if (OrigCapacities.ContainsKey(bag.ItemID))
                {
                    cap = OrigCapacities[bag.ItemID];
                }
                else
                {
                    cap = (float)At.GetValue(typeof(ItemContainer), container, "m_baseContainerCapacity");
                    OrigCapacities.Add(bag.ItemID, cap);
                }

                // set new limit based on settings
                if ((bool)config.GetValue(Settings.NoContainerLimit)) 
                { 
                    cap = -1; 
                }
                else
                {
                    cap *= (float)config.GetValue(Settings.BagBonusMulti);
                    cap += (float)config.GetValue(Settings.BagBonusFlat);
                }

                At.SetValue(cap, typeof(ItemContainer), container, "m_baseContainerCapacity");
            }
        }

        [HarmonyPatch(typeof(PlayerCharacterStats), "UpdateWeight")]
        public class PlayerCharacterStats_UpdateWeight
        {
            [HarmonyPrefix]
            public static bool Prefix(PlayerCharacterStats __instance)
            {
                var self = __instance;

                // get private fields
                var m_character = self.GetComponent<Character>();

                var m_generalBurdenPenaltyActive = (bool)At.GetValue(typeof(PlayerCharacterStats), self, "m_generalBurdenPenaltyActive");
                var m_pouchBurdenPenaltyActive = (bool)At.GetValue(typeof(PlayerCharacterStats), self, "m_pouchBurdenPenaltyActive");
                var m_backBurdenPenaltyActive = (bool)At.GetValue(typeof(PlayerCharacterStats), self, "m_backBurdenPenaltyActive");

                var m_movementSpeed = (Stat)At.GetValue(typeof(CharacterStats), self as CharacterStats, "m_movementSpeed");
                var m_staminaRegen = (Stat)At.GetValue(typeof(CharacterStats), self as CharacterStats, "m_staminaRegen");
                var m_staminaUseModifiers = (Stat)At.GetValue(typeof(CharacterStats), self as CharacterStats, "m_staminaUseModifiers");

                // get config
                var nolimits = (bool)Instance.config.GetValue(Settings.NoContainerLimit);
                var removeAllBurden = (bool)Instance.config.GetValue(Settings.DisableAllBurdens) || m_character.Cheats.NotAffectedByWeightPenalties;

                float totalWeight = removeAllBurden ? 0f : m_character.Inventory.TotalWeight;

                // update general burden
                if (totalWeight > 30f)
                {
                    At.SetValue(true, typeof(PlayerCharacterStats), self, "m_generalBurdenPenaltyActive");

                    float num = totalWeight / 30f;

                    float m_generalBurdenRatio = (float)At.GetValue(typeof(PlayerCharacterStats), self, "m_generalBurdenRatio");

                    if (num != m_generalBurdenRatio)
                    {
                        At.SetValue(num, typeof(PlayerCharacterStats), self, "m_generalBurdenRatio");

                        m_movementSpeed.AddMultiplierStack("Burden", num * -0.02f);
                        m_staminaRegen.AddMultiplierStack("Burden", num * -0.05f);
                        m_staminaUseModifiers.AddMultiplierStack("Burden_Dodge", num * 0.05f, TagSourceManager.Dodge);
                        m_staminaUseModifiers.AddMultiplierStack("Burden_Sprint", num * 0.05f, TagSourceManager.Sprint);
                    }
                }
                else if (m_generalBurdenPenaltyActive)
                {
                    At.SetValue(1f, typeof(PlayerCharacterStats), self, "m_generalBurdenRatio");
                    At.SetValue(false, typeof(PlayerCharacterStats), self, "m_generalBurdenPenaltyActive");

                    m_movementSpeed.RemoveMultiplierStack("Burden");
                    m_staminaRegen.RemoveMultiplierStack("Burden");
                    m_staminaUseModifiers.RemoveMultiplierStack("Burden_Dodge");
                    m_staminaUseModifiers.RemoveMultiplierStack("Burden_Sprint");
                }

                // update pouch burden
                float pouchWeightCapacityRatio = (removeAllBurden || nolimits) ? -1f : m_character.Inventory.PouchWeightCapacityRatio;
                float m_pouchBurdenRatio = (float)At.GetValue(typeof(PlayerCharacterStats), self, "m_pouchBurdenRatio");

                if (pouchWeightCapacityRatio != m_pouchBurdenRatio)
                {
                    At.SetValue(pouchWeightCapacityRatio, typeof(PlayerCharacterStats), self, "m_pouchBurdenRatio");
                    m_pouchBurdenRatio = pouchWeightCapacityRatio;

                    var m_pouchBurdenThreshold = (StatThreshold)At.GetValue(typeof(PlayerCharacterStats), self, "m_pouchBurdenThreshold");

                    if (m_pouchBurdenThreshold)
                    {
                        m_pouchBurdenThreshold.UpdateThresholds(Mathf.Clamp01(pouchWeightCapacityRatio - 1f), 1f, true);
                    }

                    if (m_pouchBurdenRatio > 1f)
                    {
                        At.SetValue(true, typeof(PlayerCharacterStats), self, "m_pouchBurdenPenaltyActive");

                        var m_pouchBurdenPenaltyCurve = (AnimationCurve)At.GetValue(typeof(PlayerCharacterStats), self, "m_pouchBurdenPenaltyCurve");
                        float value = m_pouchBurdenPenaltyCurve.Evaluate(m_pouchBurdenRatio - 1f);

                        m_movementSpeed.AddMultiplierStack("PouchBurden", value);
                        if (m_character.CharacterUI)
                        {
                            m_character.CharacterUI.ShowInfoNotification(LocalizationManager.Instance.GetLoc("Notification_Inventory_PouchOverweight"));
                        }
                    }
                    else if (m_pouchBurdenPenaltyActive)
                    {
                        At.SetValue(false, typeof(PlayerCharacterStats), self, "m_pouchBurdenPenaltyActive");
                        m_movementSpeed.RemoveMultiplierStack("PouchBurden");
                    }
                }

                // update bag burden
                float bagWeightCapacityRatio = (removeAllBurden || nolimits) ? -1f : m_character.Inventory.BagWeightCapacityRatio;
                float m_bagBurdenRatio = (float)At.GetValue(typeof(PlayerCharacterStats), self, "m_bagBurdenRatio");

                if (bagWeightCapacityRatio != m_bagBurdenRatio)
                {
                    m_bagBurdenRatio = bagWeightCapacityRatio;

                    At.SetValue(bagWeightCapacityRatio, typeof(PlayerCharacterStats), self, "m_bagBurdenRatio");

                    var m_bagBurdenThreshold = (StatThreshold)At.GetValue(typeof(PlayerCharacterStats), self, "m_bagBurdenThreshold");

                    if (m_bagBurdenThreshold)
                    {
                        m_bagBurdenThreshold.UpdateThresholds(Mathf.Clamp01(bagWeightCapacityRatio - 1f), 1f, true);
                    }
                    if (m_bagBurdenRatio > 1f)
                    {
                        At.SetValue(true, typeof(PlayerCharacterStats), self, "m_backBurdenPenaltyActive");

                        var m_bagBurdenPenaltyCurve = (AnimationCurve)At.GetValue(typeof(PlayerCharacterStats), self, "m_bagBurdenPenaltyCurve");

                        float value2 = m_bagBurdenPenaltyCurve.Evaluate(m_bagBurdenRatio - 1f);
                        m_movementSpeed.AddMultiplierStack("BagBurden", value2);
                        if (m_character.CharacterUI)
                        {
                            m_character.CharacterUI.ShowInfoNotification(LocalizationManager.Instance.GetLoc("Notification_Inventory_BagOverweight"));
                        }
                    }
                    else if (m_backBurdenPenaltyActive)
                    {
                        At.SetValue(false, typeof(PlayerCharacterStats), self, "m_backBurdenPenaltyActive");
                        m_movementSpeed.RemoveMultiplierStack("BagBurden");
                    }
                }

                //Instance.UpdatePlayer(m_character);

                return false;
            }
        }

        private ModConfig SetupConfig()
        {
            var newConfig = new ModConfig
            {
                ModName = "Custom Weight",
                SettingsVersion = 1.0,
                Settings = new List<BBSetting>
                {
                    new BoolSetting
                    {
                        Name = Settings.NoContainerLimit,
                        Description = "Disable limits on all containers",
                        DefaultValue = false,
                    },
                    new BoolSetting
                    {
                        Name = Settings.DisableAllBurdens,
                        Description = "Disable all burdens from weight",
                        DefaultValue = false,
                    },
                    new FloatSetting
                    {
                        Name = Settings.PouchBonus,
                        Description = "Extra Pouch capacity",
                        MinValue = 0f,
                        MaxValue = 1000f,
                        DefaultValue = 0f,
                        RoundTo = 0,
                        ShowPercent = false,
                    },
                    new FloatSetting
                    {
                        Name = Settings.BagBonusFlat,
                        Description = "Extra Bag capacity (flat bonus)",
                        MinValue = 0f,
                        MaxValue = 1000f,
                        DefaultValue = 0f,
                        RoundTo = 0,
                        ShowPercent = false,
                    },
                    new FloatSetting
                    {
                        Name = Settings.BagBonusMulti,
                        Description = "Extra Bag capacity (multiplier)",
                        MinValue = 0f,
                        MaxValue = 10f,
                        DefaultValue = 1.0f,
                        RoundTo = 2,
                        ShowPercent = false,
                    },
                }
            };

            return newConfig;
        }
    }

    public static class Settings
    {
        public static string NoContainerLimit = "NoContainerLimit";
        public static string DisableAllBurdens = "DisableAllBurdens";
        public static string PouchBonus = "PouchBonus";
        public static string BagBonusFlat = "BagBonusFlat";
        public static string BagBonusMulti = "BagBonusMulti";
    }
}