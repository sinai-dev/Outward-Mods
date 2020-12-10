using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.IO;
using BepInEx;
using HarmonyLib;
using BepInEx.IL2CPP;
using SharedModConfig;
using SideLoader;
using UnhollowerBaseLib;

// ORIGINAL MOD BY ASHNAL AND STIMMEDCOW
// CUSTOM KEYBINDINGS BY STIAN

// Fixed by Sinai

namespace ExtendedQuickslots
{
    public static class SettingNames
    {
        public const string QUICKSLOTS_TO_ADD = "QuickSlotsToAdd";
        public const string CENTER_QUICKSLOTS = "CenterQuickslots";
    }

    [BepInPlugin(GUID, NAME, VERSION)]
    public class ExtendedQuickslots : BasePlugin
    {
        public const string GUID = "com.sinai.ExtendedQuickslots";
        public const string NAME = "Extended Quickslots";
        public const string VERSION = "3.0";

        internal static ModConfig Settings;
        internal static int SlotsToAdd;
        internal static bool CenterSlots;

        private static bool fixedDictionary;
        private static readonly bool[] fixedPositions = new bool[2] { false, false };

        public override void Load()
        {
            Settings = SetupConfig();
            Settings.Register();

            Setup();

            SLPlugin.OnUpdate += Update;

            var harmony = new Harmony(GUID);
            harmony.PatchAll();
        }

        private void Setup()
        {
            SlotsToAdd = (int)(float)Settings.GetValue(SettingNames.QUICKSLOTS_TO_ADD);
            CenterSlots = (bool)Settings.GetValue(SettingNames.CENTER_QUICKSLOTS);

            // Add CustomKeybindings
            for (int i = 0; i < SlotsToAdd; i++)
                CustomKeybindings.AddAction($"QS_Instant{i + 12}", ControllerType.Both, InputType.Button, true);
        }

        internal void Update()
        {
            for (int i = 0; i < SlotsToAdd; i++)
            {
                if (CustomKeybindings.GetKeyDown($"QS_Instant{i + 12}", out int playerID))
                {
                    var character = SplitScreenManager.Instance.LocalPlayers[playerID].AssignedCharacter;
                    character.QuickSlotMngr.QuickSlotInput(i + 11);
                    break;
                }
            }

            if (!fixedDictionary && !LocalizationManager.Instance.IsLoading && LocalizationManager.Instance.Loaded)
            {
                fixedDictionary = true;

                var genLoc = SideLoader.Helpers.References.GENERAL_LOCALIZATION;

                for (int i = 0; i < SlotsToAdd; i++)
                    genLoc[$"InputAction_QS_Instant{i + 12}"] = $"Quick Slot {i + 9}";
            }
        }

        // ============== HOOKS ==============

        // Quickslot update hook, just for custom initialization

        [HarmonyPatch(typeof(QuickSlotPanel), "Update")]
        public class QuickSlotPanel_Update
        {
            [HarmonyPrefix]
            public static bool Prefix(QuickSlotPanel __instance)
            {
                var self = __instance;

                if (self.m_hideWanted && self.IsDisplayed)
                    self.OnHide();

                // check init
                if ((self.LocalCharacter == null || self.m_lastCharacter != self.LocalCharacter) && self.m_initialized)
                    self.m_initialized = false;

                // normal update when initialized
                if (self.m_initialized)
                {
                    if (self.UpdateInputVisibility)
                    {
                        for (int i = 0; i < self.m_quickSlotDisplays.Count(); i++)
                            self.m_quickSlotDisplays[i].SetInputTargetAlpha((!self.m_active) ? 0f : 1f);
                    }
                }
                // custom initialize setup
                else if (self.LocalCharacter != null)
                {
                    self.m_lastCharacter = self.LocalCharacter;
                    self.m_initialized = true;

                    // set quickslot display refs (orig function)
                    for (int j = 0; j < self.m_quickSlotDisplays.Length; j++)
                    {
                        int refSlotID = self.m_quickSlotDisplays[j].RefSlotID;
                        self.m_quickSlotDisplays[j].SetQuickSlot(self.LocalCharacter.QuickSlotMngr.GetQuickSlot(refSlotID));
                    }

                    // if its a keyboard quickslot, set up the custom display stuff
                    if (self.name == "Keyboard" && self.transform.parent.name == "QuickSlot")
                        SetupKeyboardQuickslotDisplay(self, self.m_quickSlotDisplays);

                }

                return false;
            }
        }

        private static void SetupKeyboardQuickslotDisplay(UIElement slot, QuickSlotDisplay[] m_quickSlotDisplays)
        {
            if (fixedPositions[slot.PlayerID] == false)
            {
                //var stabilityDisplay = SideLoader.IL2CPP.Unstrip.FindObjectsOfTypeAll<StabilityDisplay_Simple>()
                //    .Find(it => it.LocalCharacter == slot.LocalCharacter);

                //// Drop the stability bar to 1/3 of its original height
                //stabilityDisplay.transform.position = new Vector3(
                //    stabilityDisplay.transform.position.x,
                //    stabilityDisplay.transform.position.y / 3f,
                //    stabilityDisplay.transform.position.z
                //);

                //// Get stability bar rect bounds
                //Vector3[] stabilityRect = new Vector3[4];
                //stabilityDisplay.RectTransform.GetWorldCorners(stabilityRect);

                //// Set new quickslot bar height
                //float newY = stabilityRect[1].y + stabilityRect[0].y;
                //slot.transform.parent.position = new Vector3(
                //    slot.transform.parent.position.x,
                //    newY,
                //    slot.transform.parent.position.z
                //);

                if (CenterSlots)
                {
                    // Get first two quickslots to calculate margins.
                    List<Vector3[]> matrix = new List<Vector3[]> { new Vector3[4], new Vector3[4] };
                    for (int i = 0; i < 2; i++) { m_quickSlotDisplays[i].RectTransform.GetWorldCorners(matrix[i]); }

                    // do some math
                    var iconW = matrix[0][2].x - matrix[0][1].x;             // The width of each icon
                    var margin = matrix[1][0].x - matrix[0][2].x;            // The margin between each icon
                    var elemWidth = iconW + margin;                          // Total space per icon+margin pair
                    var totalWidth = elemWidth * m_quickSlotDisplays.Length; // How long our bar really is

                    // Re-center it based on actual content
                    slot.transform.parent.position = new Vector3(
                        totalWidth / 2.0f + elemWidth / 2.0f,
                        slot.transform.parent.position.y,
                        slot.transform.parent.position.z
                    );
                }

                fixedPositions[slot.PlayerID] = true;
            }
        }

        // Keyboard quickslot initialize hook. Add our custom slots.

        [HarmonyPatch(typeof(KeyboardQuickSlotPanel), "InitializeQuickSlotDisplays")]
        public class KeyboardQSPanel_Init
        {
            [HarmonyPrefix]
            public static void Prefix(KeyboardQuickSlotPanel __instance)
            {
                var self = __instance;

                var length = self.DisplayOrder.Length + SlotsToAdd;
                var newArray = new Il2CppStructArray<QuickSlot.QuickSlotIDs>(length);

                // first set original values
                for (int i = 0; i < self.DisplayOrder.Length; i++)
                    newArray[i] = self.DisplayOrder[i];

                // then add custom ones too
                int s = 12;
                for (int i = SlotsToAdd; i >= 1; i--)
                    newArray[length - i] = (QuickSlot.QuickSlotIDs)s++;

                self.DisplayOrder = newArray;
            }
        }

        // character quickslot manager awake hook. Add our custom slots first.
        [HarmonyPatch(typeof(CharacterQuickSlotManager), "Awake")]
        public class CharacterQSMgr_Awake
        {
            public static void Prefix(CharacterQuickSlotManager __instance)
            {
                var self = __instance;

                var trans = self.transform.Find("QuickSlots");
                self.m_quickslotTrans = trans;
                for (int i = 0; i < SlotsToAdd; i++)
                {
                    GameObject gameObject = new GameObject($"EQS_{i}");
                    QuickSlot qs = gameObject.AddComponent<QuickSlot>();
                    qs.name = "" + (i + 12);
                    gameObject.transform.SetParent(trans);
                }
            }
        }

        // =============== Setup Config =================

        private ModConfig SetupConfig()
        {
            return new ModConfig
            {
                ModName = "ExtendedQuickSlots",
                SettingsVersion = 1.0,
                Settings = new List<BBSetting>
                {
                    new FloatSetting
                    {
                        SectionTitle = "Restart Required if you change settings!",
                        Name = SettingNames.QUICKSLOTS_TO_ADD,
                        DefaultValue = 8f,
                        Description = "Number of quickslots to add",
                        Increment = 1,
                        RoundTo = 0,
                        MaxValue = 24,
                        MinValue = 0,
                        ShowPercent = false,
                    },
                    new BoolSetting
                    {
                        Name = SettingNames.CENTER_QUICKSLOTS,
                        Description = "Align quickslots to center?",
                        DefaultValue = false,
                    }
                },
            };
        }
    }
}
