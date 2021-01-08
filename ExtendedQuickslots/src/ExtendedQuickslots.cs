using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.IO;
using BepInEx;
using HarmonyLib;
using SharedModConfig;
using SideLoader;
using UnityEngine.UI;

// ORIGINAL MOD BY ASHNAL AND STIMMEDCOW
// CUSTOM KEYBINDINGS BY STIAN

// Fixed by Sinai
// Addition of limited controller support by HardLess

namespace ExtendedQuickslots
{
    public static class SettingNames
    {
        public const string QUICKSLOTS_TO_ADD = "QuickSlotsToAdd";
        public const string CENTER_QUICKSLOTS = "CenterQuickslots";
    }

    [BepInPlugin(GUID, NAME, VERSION)]
    public class ExtendedQuickslots : BaseUnityPlugin
    {
        public const string GUID = "com.sinai.ExtendedQuickslots";
        public const string NAME = "Extended Quickslots";
        public const string VERSION = "3.0";

        internal static ModConfig Settings;
        internal static int SlotsToAdd;
        internal static bool CenterSlots;

        private static bool fixedDictionary;
        private static readonly bool[] fixedPositions = new bool[2] { false, false };

        internal void Awake()
        {
            Settings = SetupConfig();

            Settings.OnSettingsLoaded += Setup;
            Settings.Register();

            SetupLocalization();

            var harmony = new Harmony(GUID);
            harmony.PatchAll();
        }

        private void SetupLocalization()
        {
            var genLoc = References.GENERAL_LOCALIZATION;

            for (int i = 0; i < SlotsToAdd; i++)
            {
                var key = "InputAction_QS_Instant" + (i + 12);
                var loc = "QuickSlot " + (i + 9);

                if (genLoc.ContainsKey(key))
                    genLoc[key] = loc;
                else
                    genLoc.Add(key, loc);

                //SL.Log("Set QuickSlot localization. Key: '" + key + "', Val: '" + loc + "'");
            }
        }

        private void Setup()
        {
            SlotsToAdd = (int)(float)Settings.GetValue(SettingNames.QUICKSLOTS_TO_ADD);
            CenterSlots = (bool)Settings.GetValue(SettingNames.CENTER_QUICKSLOTS);

            // Add CustomKeybindings
            for (int i = 0; i < SlotsToAdd; i++)
                CustomKeybindings.AddAction($"QS_Instant{i + 12}", KeybindingsCategory.QuickSlot, ControlType.Both);
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

            foreach (SplitPlayer player in SplitScreenManager.Instance.LocalPlayers)
            {
                var character = player.AssignedCharacter;
                int id = character.OwnerPlayerSys.PlayerID;
                if (QuickSlotInstant9(id))
                {
                    character.QuickSlotMngr.QuickSlotInput(12);
                }
                if (QuickSlotInstant10(id))
                {
                    character.QuickSlotMngr.QuickSlotInput(13);
                }
                if (QuickSlotInstant11(id))
                {
                    character.QuickSlotMngr.QuickSlotInput(14);
                }
                if (QuickSlotInstant12(id))
                {
                    character.QuickSlotMngr.QuickSlotInput(15);
                }
            }

            if (!fixedDictionary && !LocalizationManager.Instance.IsLoading && LocalizationManager.Instance.Loaded)
            {
                fixedDictionary = true;

                var genLoc = SideLoader.References.GENERAL_LOCALIZATION;

                for (int i = 0; i < SlotsToAdd; i++)
                    genLoc[$"InputAction_QS_Instant{i + 12}"] = $"Quick Slot {i + 9}";
            }
        }

        internal bool QuickSlotInstant9(int _playerID)
        {
            return ControlsInput.QuickSlotToggle2(_playerID) && ControlsInput.QuickSlotToggle1(_playerID) && ControlsInput.QuickSlot1(_playerID);
        }

        internal bool QuickSlotInstant10(int _playerID)
        {
            return ControlsInput.QuickSlotToggle2(_playerID) && ControlsInput.QuickSlotToggle1(_playerID) && ControlsInput.QuickSlot2(_playerID);
        }

        internal bool QuickSlotInstant11(int _playerID)
        {
            return ControlsInput.QuickSlotToggle2(_playerID) && ControlsInput.QuickSlotToggle1(_playerID) && ControlsInput.QuickSlot3(_playerID);
        }

        internal bool QuickSlotInstant12(int _playerID)
        {
            return ControlsInput.QuickSlotToggle2(_playerID) && ControlsInput.QuickSlotToggle1(_playerID) && ControlsInput.QuickSlot4(_playerID);
        }


        // ============== HOOKS ==============

        [HarmonyPatch(typeof(SplitScreenManager), "Awake")]
        public class SplitScreenManager_Awake
        {

            [HarmonyPostfix]
            public static void Postfix(CharacterUI ___m_charUIPrefab)
            {

                if (SlotsToAdd < 4)
                    return;


                GameObject.DontDestroyOnLoad(___m_charUIPrefab.gameObject);

                GameObject controllerQuickSlotMenu= ___m_charUIPrefab.transform
                    .Find("Canvas/GameplayPanels/Menus/CharacterMenus/MainPanel/Content/MiddlePanel/QuickSlotPanel/PanelSwitcher/Controller")
                    .gameObject;
                Transform LT_RT = controllerQuickSlotMenu.transform.Find("LT-RT");
                LT_RT.Find("LeftDecoration").gameObject.SetActive(false);
                LT_RT.Find("RightDecoration").gameObject.SetActive(false);
                Transform LT = LT_RT.Find("LT");
                Transform RT = LT_RT.Find("RT");
                LT.localPosition = new Vector3(-300f,0f,0f);
                RT.localPosition = new Vector3(300f, 0f, 0f);
                EditorQuickSlotDisplayPlacer[] quickSlotsLT = LT.GetComponentsInChildren<EditorQuickSlotDisplayPlacer>();
                foreach (EditorQuickSlotDisplayPlacer displayPlacer in quickSlotsLT)
                    displayPlacer.IsTemplate = true;

                GameObject newTab = GameObject.Instantiate<GameObject>(LT.gameObject);
                newTab.transform.Find("imgLT").localPosition = new Vector3(-25f,22.5f,0f);
                GameObject imgRT = GameObject.Instantiate<GameObject>(RT.Find("imgRT").gameObject);
                imgRT.transform.parent = newTab.transform;
                imgRT.transform.localPosition = new Vector3(25f, 22.5f, 0f);
                newTab.transform.parent = LT_RT;
                newTab.transform.position = LT.transform.position;
                LT.localPosition = new Vector3(0f, 0f, 0f);

                EditorQuickSlotDisplayPlacer[] quickSlotsNewTab = newTab.GetComponentsInChildren<EditorQuickSlotDisplayPlacer>();
                for (int i = 0; i < quickSlotsNewTab.Length; i++)
                {
                    quickSlotsNewTab[i].RefSlotID = 12 + i;
                    quickSlotsNewTab[i].IsTemplate = false;
                }
                QuickSlotDisplay[] quickSlotsDisplayNewTab = newTab.GetComponentsInChildren<QuickSlotDisplay>();
                for (int i = 0; i < quickSlotsDisplayNewTab.Length; i++)
                {
                    quickSlotsDisplayNewTab[i].RefSlotID = 12 + i;
                }

                foreach (EditorQuickSlotDisplayPlacer displayPlacer in quickSlotsLT)
                    displayPlacer.IsTemplate = false;
            }
        }

        // Quickslot update hook, just for custom initialization

        [HarmonyPatch(typeof(QuickSlotPanel), "Update")]
        public class QuickSlotPanel_Update
        {
            [HarmonyPrefix]
            public static bool Prefix(QuickSlotPanel __instance, ref bool ___m_hideWanted, ref Character ___m_lastCharacter,
                ref bool ___m_initialized, QuickSlotDisplay[] ___m_quickSlotDisplays, bool ___m_active)
            {
                var self = __instance;

                if (___m_hideWanted && self.IsDisplayed)
                    At.Invoke(self, "OnHide");

                // check init
                if ((self.LocalCharacter == null || ___m_lastCharacter != self.LocalCharacter) && ___m_initialized)
                    At.SetField(self, "m_initialized", false);

                // normal update when initialized
                if (___m_initialized)
                {
                    if (self.UpdateInputVisibility)
                    {
                        for (int i = 0; i < ___m_quickSlotDisplays.Count(); i++)
                            At.Invoke(___m_quickSlotDisplays[i], "SetInputTargetAlpha", new object[] { (!___m_active) ? 0f : 1f });
                    }
                }

                // custom initialize setup
                else if (self.LocalCharacter != null)
                {
                    ___m_lastCharacter = self.LocalCharacter;
                    ___m_initialized = true;

                    // set quickslot display refs (orig function)
                    for (int j = 0; j < ___m_quickSlotDisplays.Length; j++)
                    {
                        int refSlotID = ___m_quickSlotDisplays[j].RefSlotID;
                        ___m_quickSlotDisplays[j].SetQuickSlot(self.LocalCharacter.QuickSlotMngr.GetQuickSlot(refSlotID));
                    }

                    // if its a keyboard quickslot, set up the custom display stuff
                    if (self.name == "Keyboard" && self.transform.parent.name == "QuickSlot")
                        SetupKeyboardQuickslotDisplay(self, ___m_quickSlotDisplays);
                }

                return false;
            }
        }

        private static void SetupKeyboardQuickslotDisplay(UIElement slot, QuickSlotDisplay[] m_quickSlotDisplays)
        {
            if (fixedPositions[slot.PlayerID] == false)
            {
                var stabilityDisplay = Resources.FindObjectsOfTypeAll<StabilityDisplay_Simple>()
                    .ToList()
                    .Find(it => it.LocalCharacter == slot.LocalCharacter);

                // Drop the stability bar to 1/3 of its original height
                stabilityDisplay.transform.position = new Vector3(
                    stabilityDisplay.transform.position.x,
                    stabilityDisplay.transform.position.y / 3f,
                    stabilityDisplay.transform.position.z
                );

                // Get stability bar rect bounds
                Vector3[] stabilityRect = new Vector3[4];
                stabilityDisplay.RectTransform.GetWorldCorners(stabilityRect);

                // Set new quickslot bar height
                float newY = stabilityRect[1].y + stabilityRect[0].y;
                slot.transform.parent.position = new Vector3(
                    slot.transform.parent.position.x,
                    newY,
                    slot.transform.parent.position.z
                );

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
                Array.Resize(ref self.DisplayOrder, length);

                // then add custom ones too
                int s = 12;
                for (int i = SlotsToAdd; i >= 1; i--)
                    self.DisplayOrder[length - i] = (QuickSlot.QuickSlotIDs)s++;
            }
        }

        // Controller quickslot initialize hook. Add our custom slots.

        [HarmonyPatch(typeof(QuickSlotPanelSwitcher), "StartInit")]
        public class QSPanelSwitcher_Init
        {
            [HarmonyPrefix]
            public static void Prefix(QuickSlotPanelSwitcher __instance, ref QuickSlotPanel[]  ___m_quickSlotPanels)
            {
                // For simplicity we need at least 4 more slots
                if (SlotsToAdd < 4)
                    return;

                var self = __instance;

                var length = ___m_quickSlotPanels.Length + 1;
                Array.Resize(ref ___m_quickSlotPanels, length);

                // Add the new tab

                Transform LT = ___m_quickSlotPanels[0].transform;
                EditorQuickSlotDisplayPlacer[] quickSlotsLT = LT.GetComponentsInChildren<EditorQuickSlotDisplayPlacer>();
                foreach (EditorQuickSlotDisplayPlacer displayPlacer in quickSlotsLT)
                    displayPlacer.IsTemplate = true;
                GameObject newTab = GameObject.Instantiate<GameObject>(LT.gameObject);
                newTab.transform.parent = LT.parent;
                newTab.transform.position = LT.transform.position;

                EditorQuickSlotDisplayPlacer[] quickSlotsNewTab = newTab.GetComponentsInChildren<EditorQuickSlotDisplayPlacer>();
                for (int i = 0; i < quickSlotsNewTab.Length; i++)
                {
                    quickSlotsNewTab[i].RefSlotID = 12 + i;
                    quickSlotsNewTab[i].IsTemplate = false;
                }
                QuickSlotDisplay[] quickSlotsDisplayNewTab = newTab.GetComponentsInChildren<QuickSlotDisplay>();
                for (int i = 0; i < quickSlotsDisplayNewTab.Length; i++)
                {
                    quickSlotsDisplayNewTab[i].RefSlotID = 12 + i;
                }

                foreach (EditorQuickSlotDisplayPlacer displayPlacer in quickSlotsLT)
                    displayPlacer.IsTemplate = false;


                ___m_quickSlotPanels[length - 1] = newTab.GetComponent<QuickSlotPanel>();
            }
        }

        // Controller quickslot custom update.

        [HarmonyPatch(typeof(QuickSlotPanelSwitcher), "Update")]
        public class QSPanelSwitcher_Update
        {
            [HarmonyPrefix]
            public static bool Prefix(QuickSlotPanelSwitcher __instance, ref QuickSlotPanel[] ___m_quickSlotPanels, ref int ___m_currentlyDisplayedPanel, ref Image[] ___m_panelIndicators)
            {
                // For simplicity we need at least 4 more slots
                if (SlotsToAdd < 4)
                    return true;

                var self = __instance;

                if (self.LocalCharacter != null)
                {
                    int previouslyDisplayedPanel = ___m_currentlyDisplayedPanel;
                    bool flag = false;
                    if (self.LocalCharacter.QuickSlotMngr.ShowQuickSlotSection1 && !self.LocalCharacter.QuickSlotMngr.ShowQuickSlotSection2)
                    {
                        ___m_currentlyDisplayedPanel = 0;
                        flag = true;
                    }
                    else if (!self.LocalCharacter.QuickSlotMngr.ShowQuickSlotSection1 && self.LocalCharacter.QuickSlotMngr.ShowQuickSlotSection2)
                    {
                        ___m_currentlyDisplayedPanel = 1;
                        flag = true;
                    }
                    else if (self.LocalCharacter.QuickSlotMngr.ShowQuickSlotSection1 && self.LocalCharacter.QuickSlotMngr.ShowQuickSlotSection2)
                    {
                        flag = true;
                        if (SlotsToAdd >= 4)
                        {
                            ___m_currentlyDisplayedPanel = 2;
                        }
                        else
                        {
                            if (self.LocalCharacter.QuickSlotMngr.ShowQuickSlotSection1LastTime > self.LocalCharacter.QuickSlotMngr.ShowQuickSlotSection2LastTime)
                            {
                                ___m_currentlyDisplayedPanel = 0;
                            }
                            else if (self.LocalCharacter.QuickSlotMngr.ShowQuickSlotSection1LastTime < self.LocalCharacter.QuickSlotMngr.ShowQuickSlotSection2LastTime)
                            {
                                ___m_currentlyDisplayedPanel = 1;
                            }
                        }
                    }
                    Color color = Color.white;
                    for (int i = 0; i < ___m_quickSlotPanels.Length; i++)
                    {
                        color = Color.white;
                        if (i == ___m_currentlyDisplayedPanel)
                        {
                            if (flag)
                            {
                                color = self.ActiveColor;
                            }
                            if (___m_quickSlotPanels[i])
                            {
                                if (___m_quickSlotPanels[i].gameObject.activeSelf)
                                {
                                    ___m_quickSlotPanels[i].SetActive(true);
                                }
                                if (previouslyDisplayedPanel != ___m_currentlyDisplayedPanel)
                                {
                                    ___m_quickSlotPanels[i].ShowFront(false);
                                }
                            }
                        }
                        else
                        {
                            color = self.DisabledColor;
                            if (___m_quickSlotPanels[i])
                            {
                                if (___m_quickSlotPanels[i].gameObject.activeSelf)
                                {
                                    ___m_quickSlotPanels[i].SetActive(false);
                                }
                                if (i == previouslyDisplayedPanel)
                                {
                                    ___m_quickSlotPanels[i].PutBack(false);
                                }
                            }
                        }
                    }
                    if (___m_currentlyDisplayedPanel == 2)
                    {
                        if (flag == true)
                            color = self.ActiveColor;
                        else
                            color = self.DisabledColor;
                        for (int i = 0; i < ___m_quickSlotPanels.Length - 1; i++)
                            if (___m_panelIndicators[i] && ___m_panelIndicators[i].color != color)
                                ___m_panelIndicators[i].color = Vector4.MoveTowards(___m_panelIndicators[i].color, color, 5f * Time.deltaTime);
                    }
                    else if (___m_currentlyDisplayedPanel < 2)
                    {
                        for (int i = 0; i < ___m_quickSlotPanels.Length - 1; i++)
                        {
                            if (i == ___m_currentlyDisplayedPanel && flag == true)
                                color = self.ActiveColor;
                            else if (i != ___m_currentlyDisplayedPanel)
                                color = self.DisabledColor;

                            if (___m_panelIndicators[i] && ___m_panelIndicators[i].color != color)
                                ___m_panelIndicators[i].color = Vector4.MoveTowards(___m_panelIndicators[i].color, color, 5f * Time.deltaTime);
                        }
                    }
                }


                return false;
            }
        }


        [HarmonyPatch(typeof(ControlsInput), "QuickSlotInstant1")]
        public class ControlsInput_QuickSlotInstant1
        {
            [HarmonyPrefix]
            public static bool Prefix(int _playerID, ref bool __result, Dictionary<int, RewiredInputs>  ___m_playerInputManager)
            {
                __result = ___m_playerInputManager[_playerID].GetButtonDown("QS_Instant1") || (ControlsInput.QuickSlotToggle2(_playerID) && !ControlsInput.QuickSlotToggle1(_playerID) && ControlsInput.QuickSlot1(_playerID));
                return false;
            }
        }

        [HarmonyPatch(typeof(ControlsInput), "QuickSlotInstant2")]
        public class ControlsInput_QuickSlotInstant2
        {
            [HarmonyPrefix]
            public static bool Prefix(int _playerID, ref bool __result, Dictionary<int, RewiredInputs> ___m_playerInputManager)
            {
                __result = ___m_playerInputManager[_playerID].GetButtonDown("QS_Instant2") || (ControlsInput.QuickSlotToggle2(_playerID) && !ControlsInput.QuickSlotToggle1(_playerID) && ControlsInput.QuickSlot2(_playerID));
                return false;
            }
        }

        [HarmonyPatch(typeof(ControlsInput), "QuickSlotInstant3")]
        public class ControlsInput_QuickSlotInstant3
        {
            [HarmonyPrefix]
            public static bool Prefix(int _playerID, ref bool __result, Dictionary<int, RewiredInputs> ___m_playerInputManager)
            {
                __result = ___m_playerInputManager[_playerID].GetButtonDown("QS_Instant3") || (ControlsInput.QuickSlotToggle2(_playerID) && !ControlsInput.QuickSlotToggle1(_playerID) && ControlsInput.QuickSlot3(_playerID));
                return false;
            }
        }

        [HarmonyPatch(typeof(ControlsInput), "QuickSlotInstant4")]
        public class ControlsInput_QuickSlotInstant4
        {
            [HarmonyPrefix]
            public static bool Prefix(int _playerID, ref bool __result, Dictionary<int, RewiredInputs> ___m_playerInputManager)
            {
                __result = ___m_playerInputManager[_playerID].GetButtonDown("QS_Instant4") || (ControlsInput.QuickSlotToggle2(_playerID) && !ControlsInput.QuickSlotToggle1(_playerID) && ControlsInput.QuickSlot4(_playerID));
                return false;
            }
        }

        [HarmonyPatch(typeof(ControlsInput), "QuickSlotInstant5")]
        public class ControlsInput_QuickSlotInstant5
        {
            [HarmonyPrefix]
            public static bool Prefix(int _playerID, ref bool __result, Dictionary<int, RewiredInputs> ___m_playerInputManager)
            {
                __result = ___m_playerInputManager[_playerID].GetButtonDown("QS_Instant1") || (!ControlsInput.QuickSlotToggle2(_playerID) && ControlsInput.QuickSlotToggle1(_playerID) && ControlsInput.QuickSlot1(_playerID));
                return false;
            }
        }

        [HarmonyPatch(typeof(ControlsInput), "QuickSlotInstant6")]
        public class ControlsInput_QuickSlotInstant6
        {
            [HarmonyPrefix]
            public static bool Prefix(int _playerID, ref bool __result, Dictionary<int, RewiredInputs> ___m_playerInputManager)
            {
                __result = ___m_playerInputManager[_playerID].GetButtonDown("QS_Instant2") || (!ControlsInput.QuickSlotToggle2(_playerID) && ControlsInput.QuickSlotToggle1(_playerID) && ControlsInput.QuickSlot2(_playerID));
                return false;
            }
        }

        [HarmonyPatch(typeof(ControlsInput), "QuickSlotInstant7")]
        public class ControlsInput_QuickSlotInstant7
        {
            [HarmonyPrefix]
            public static bool Prefix(int _playerID, ref bool __result, Dictionary<int, RewiredInputs> ___m_playerInputManager)
            {
                __result = ___m_playerInputManager[_playerID].GetButtonDown("QS_Instant3") || (!ControlsInput.QuickSlotToggle2(_playerID) && ControlsInput.QuickSlotToggle1(_playerID) && ControlsInput.QuickSlot3(_playerID));
                return false;
            }
        }

        [HarmonyPatch(typeof(ControlsInput), "QuickSlotInstant8")]
        public class ControlsInput_QuickSlotInstant8
        {
            [HarmonyPrefix]
            public static bool Prefix(int _playerID, ref bool __result, Dictionary<int, RewiredInputs> ___m_playerInputManager)
            {
                __result = ___m_playerInputManager[_playerID].GetButtonDown("QS_Instant4") || (!ControlsInput.QuickSlotToggle2(_playerID) && ControlsInput.QuickSlotToggle1(_playerID) && ControlsInput.QuickSlot4(_playerID));
                return false;
            }
        }

        // character quickslot manager awake hook. Add our custom slots first.
        [HarmonyPatch(typeof(CharacterQuickSlotManager), "Awake")]
        public class CharacterQSMgr_Awake
        {
            public static void Prefix(CharacterQuickSlotManager __instance, ref Transform ___m_quickslotTrans)
            {
                var self = __instance;

                var trans = self.transform.Find("QuickSlots");
                ___m_quickslotTrans = trans;
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
