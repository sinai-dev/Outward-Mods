using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using BepInEx;
using HarmonyLib;
using SideLoader;
using BepInEx.Configuration;

namespace CustomWeight
{
    [BepInPlugin(GUID, NAME, VERSION)]
    public class CustomWeight : BaseUnityPlugin
    {
        const string GUID = "com.sinai.customweight";
        const string NAME = "Custom Weight";
        const string VERSION = "2.3";

        public static CustomWeight Instance;

        public float m_timeOfLastUpdate;

        internal const string CTG_GENERAL = "Core Settings";
        public static ConfigEntry<bool> NO_CONTAINER_LIMIT;
        public static ConfigEntry<bool> DISABLE_ALL_BURDENS;

        internal const string CTG_BONUSES = "Extra Bonus Settings";
        public static ConfigEntry<float> EXTRA_POUCH_CAPACITY;
        public static ConfigEntry<float> EXTRA_BAG_CAPACITY_FLAT;
        public static ConfigEntry<float> EXTRA_BAG_CAPACITY_MULTIPLIER;

        // original capacities on bags (ID : Capacity)
        public Dictionary<int, float> OrigCapacities = new Dictionary<int, float>();

        internal void Start()
        {
            Instance = this;

            var harmony = new Harmony(GUID);
            harmony.PatchAll();

            NO_CONTAINER_LIMIT = Config.Bind(CTG_GENERAL, "No limits on all containers", false, "Disables capacity limits for Pouch and Backpacks");
            NO_CONTAINER_LIMIT.SettingChanged += SettingChanged;

            DISABLE_ALL_BURDENS = Config.Bind(CTG_GENERAL, "Disable all weight burdens", false, "Disables all burdens on the player related to Weight");
            DISABLE_ALL_BURDENS.SettingChanged += SettingChanged;

            EXTRA_POUCH_CAPACITY = Config.Bind(CTG_BONUSES, "Extra Pouch Capacity (flat)", 0f, 
                new ConfigDescription("Extra capacity (flat) added to player's Pouch", new AcceptableValueRange<float>(0f, 1000f)));
            EXTRA_POUCH_CAPACITY.SettingChanged += SettingChanged;

            EXTRA_BAG_CAPACITY_FLAT = Config.Bind(CTG_BONUSES, "Extra Bag Capacity (flat)", 0f,
                new ConfigDescription("Extra capacity (flat) added to player's Bag", new AcceptableValueRange<float>(0f, 1000f)));
            EXTRA_BAG_CAPACITY_FLAT.SettingChanged += SettingChanged;

            EXTRA_BAG_CAPACITY_MULTIPLIER = Config.Bind(CTG_BONUSES, "Bag Capacity multiplier", 1.0f,
                new ConfigDescription("Multiplier applied to player's Bag capacity", new AcceptableValueRange<float>(0f, 10f)));
            EXTRA_BAG_CAPACITY_MULTIPLIER.SettingChanged += SettingChanged;
        }

        private void SettingChanged(object _, EventArgs __)
        {
            UpdateAllPlayers();
        }

        internal void Update()
        {
            if (Time.time - m_timeOfLastUpdate > 2f)
            {
                m_timeOfLastUpdate = Time.time;

                if (Global.Lobby.PlayersInLobbyCount > 0 && !NetworkLevelLoader.Instance.IsGameplayPaused)
                    UpdateAllPlayers();
            }
        }

        private void UpdateAllPlayers()
        {
            foreach (PlayerSystem player in Global.Lobby.PlayersInLobby)
            {
                if (player.ControlledCharacter)
                {
                    if (!player) 
                        continue;

                    UpdatePlayer(player.ControlledCharacter);
                }
            }
        }

        private void UpdatePlayer(Character player)
        {
            float newValue = NO_CONTAINER_LIMIT.Value 
                                ? -1 
                                : 10.0f + EXTRA_POUCH_CAPACITY.Value;

            if ((float)At.GetField(player.Inventory.Pouch, "m_baseContainerCapacity") != newValue)
                At.SetField(player.Inventory.Pouch, "m_baseContainerCapacity", newValue);

            if (player.Inventory.EquippedBag)
                UpdateBag(player.Inventory.EquippedBag);
        }

        private void UpdateBag(Bag bag)
        {
            float cap;

            if (At.GetField(bag, "m_container") is ItemContainerStatic container)
            {
                if (OrigCapacities.ContainsKey(bag.ItemID))
                    cap = OrigCapacities[bag.ItemID];
                else
                {
                    cap = (float)At.GetField(container as ItemContainer, "m_baseContainerCapacity");
                    OrigCapacities.Add(bag.ItemID, cap);
                }

                // set new limit based on settings
                if (NO_CONTAINER_LIMIT.Value) 
                    cap = -1; 
                else
                {
                    cap *= EXTRA_BAG_CAPACITY_MULTIPLIER.Value;
                    cap += EXTRA_BAG_CAPACITY_FLAT.Value;
                }

                At.SetField(container as ItemContainer, "m_baseContainerCapacity", cap);
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

                var m_generalBurdenPenaltyActive = (bool)At.GetField(self, "m_generalBurdenPenaltyActive");
                var m_pouchBurdenPenaltyActive = (bool)At.GetField(self, "m_pouchBurdenPenaltyActive");
                var m_backBurdenPenaltyActive = (bool)At.GetField(self, "m_backBurdenPenaltyActive");

                var m_movementSpeed = (Stat)At.GetField(self, "m_movementSpeed");
                var m_staminaRegen = (Stat)At.GetField(self, "m_staminaRegen");
                var m_staminaUseModifiers = (Stat)At.GetField(self, "m_staminaUseModifiers");

                // get config
                var nolimits = NO_CONTAINER_LIMIT.Value;
                var removeAllBurden = DISABLE_ALL_BURDENS.Value || m_character.Cheats.NotAffectedByWeightPenalties;

                float totalWeight = removeAllBurden ? 0f : m_character.Inventory.TotalWeight;

                // update general burden
                if (totalWeight > 30f)
                {
                    At.SetField(self, "m_generalBurdenPenaltyActive", true);

                    float weightRatio = totalWeight / 30f;

                    float m_generalBurdenRatio = (float)At.GetField(self, "m_generalBurdenRatio");

                    if (weightRatio != m_generalBurdenRatio)
                    {
                        At.SetField(self, "m_generalBurdenRatio", weightRatio);

                        m_movementSpeed.AddMultiplierStack("Burden", weightRatio * -0.02f);
                        m_staminaRegen.AddMultiplierStack("Burden", weightRatio * -0.05f);
                        m_staminaUseModifiers.AddMultiplierStack("Burden_Dodge", weightRatio * 0.05f, TagSourceManager.Dodge);
                        m_staminaUseModifiers.AddMultiplierStack("Burden_Sprint", weightRatio * 0.05f, TagSourceManager.Sprint);
                    }
                }
                else if (m_generalBurdenPenaltyActive)
                {
                    At.SetField(self, "m_generalBurdenRatio", 1f);
                    At.SetField(self, "m_generalBurdenPenaltyActive", false);

                    m_movementSpeed.RemoveMultiplierStack("Burden");
                    m_staminaRegen.RemoveMultiplierStack("Burden");
                    m_staminaUseModifiers.RemoveMultiplierStack("Burden_Dodge");
                    m_staminaUseModifiers.RemoveMultiplierStack("Burden_Sprint");
                }

                // update pouch burden
                float pouchWeightCapacityRatio = (removeAllBurden || nolimits) 
                                                    ? -1f 
                                                    : m_character.Inventory.PouchWeightCapacityRatio;

                float m_pouchBurdenRatio = (float)At.GetField(self, "m_pouchBurdenRatio");

                if (pouchWeightCapacityRatio != m_pouchBurdenRatio)
                {
                    At.SetField(self, "m_pouchBurdenRatio", pouchWeightCapacityRatio);
                    m_pouchBurdenRatio = pouchWeightCapacityRatio;

                    var m_pouchBurdenThreshold = (StatThreshold)At.GetField(self, "m_pouchBurdenThreshold");

                    if (m_pouchBurdenThreshold)
                        m_pouchBurdenThreshold.UpdateThresholds(Mathf.Clamp01(pouchWeightCapacityRatio - 1f), 1f, true);

                    if (m_pouchBurdenRatio > 1f)
                    {
                        At.SetField(self, "m_pouchBurdenPenaltyActive", true);

                        var m_pouchBurdenPenaltyCurve = (AnimationCurve)At.GetField(self, "m_pouchBurdenPenaltyCurve");
                        float value = m_pouchBurdenPenaltyCurve.Evaluate(m_pouchBurdenRatio - 1f);

                        m_movementSpeed.AddMultiplierStack("PouchBurden", value);
                        
                        if (m_character.CharacterUI)
                            m_character.CharacterUI.ShowInfoNotification(LocalizationManager.Instance.GetLoc("Notification_Inventory_PouchOverweight"));
                    }
                    else if (m_pouchBurdenPenaltyActive)
                    {
                        At.SetField(self, "m_pouchBurdenPenaltyActive", false);
                        m_movementSpeed.RemoveMultiplierStack("PouchBurden");
                    }
                }

                // update bag burden
                float bagWeightCapacityRatio = (removeAllBurden || nolimits) 
                                                ? -1f 
                                                : m_character.Inventory.BagWeightCapacityRatio;

                float m_bagBurdenRatio = (float)At.GetField(self, "m_bagBurdenRatio");

                if (bagWeightCapacityRatio != m_bagBurdenRatio)
                {
                    m_bagBurdenRatio = bagWeightCapacityRatio;

                    At.SetField(self, "m_bagBurdenRatio", bagWeightCapacityRatio);

                    var m_bagBurdenThreshold = (StatThreshold)At.GetField(self, "m_bagBurdenThreshold");

                    if (m_bagBurdenThreshold)
                        m_bagBurdenThreshold.UpdateThresholds(Mathf.Clamp01(bagWeightCapacityRatio - 1f), 1f, true);
                    
                    if (m_bagBurdenRatio > 1f)
                    {
                        At.SetField(self, "m_backBurdenPenaltyActive", true);

                        var m_bagBurdenPenaltyCurve = (AnimationCurve)At.GetField(self, "m_bagBurdenPenaltyCurve");

                        float value2 = m_bagBurdenPenaltyCurve.Evaluate(m_bagBurdenRatio - 1f);
                        m_movementSpeed.AddMultiplierStack("BagBurden", value2);
                       
                        if (m_character.CharacterUI)
                            m_character.CharacterUI.ShowInfoNotification(LocalizationManager.Instance.GetLoc("Notification_Inventory_BagOverweight"));
                    }
                    else if (m_backBurdenPenaltyActive)
                    {
                        At.SetField(self, "m_backBurdenPenaltyActive", false);
                        m_movementSpeed.RemoveMultiplierStack("BagBurden");
                    }
                }

                //Instance.UpdatePlayer(m_character);

                return false;
            }
        }
    }
}