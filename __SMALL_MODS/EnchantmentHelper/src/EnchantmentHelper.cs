using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BepInEx;
using HarmonyLib;
using SideLoader;
using UnityEngine;

namespace EnchantmentHelper
{
    [BepInPlugin(GUID, NAME, VERSION)]
    public class EnchantmentHelper : BaseUnityPlugin
    {
        const string GUID = "com.sinai.enchantmenthelper";
        const string NAME = "Enchantment Helper";
        const string VERSION = "1.0.0";

        // Enchantment cache
        private static Dictionary<string, Enchantment> m_cachedEnchantments;

        // GUI
        private const int WINDOW_ID = 8273;
        private Rect m_rect = new Rect(5, 5, 350, 450);
        private Vector2 scroll = Vector2.zero;

        public static bool ShowMenu
        {
            get => m_showMenu;
            set
            {
                m_showMenu = value;
                if (m_showMenu)
                    SideLoader.Helpers.ForceUnlockCursor.AddUnlockSource();
                else
                    SideLoader.Helpers.ForceUnlockCursor.RemoveUnlockSource();
            }
        }
        private static bool m_showMenu;

        public string SearchText = "";
        public Enchantment SelectedEnchant;
        private EquipmentSlot.EquipmentSlotIDs SelectedSlot = EquipmentSlot.EquipmentSlotIDs.RightHand;

        internal const string MENU_TOGGLE_NAME = "Enchantment Helper Menu";

        // Setup
        internal void Awake()
        {
            var harmony = new Harmony(GUID);
            harmony.PatchAll();

            CustomKeybindings.AddAction(MENU_TOGGLE_NAME, KeybindingsCategory.CustomKeybindings);
        }

        // Patch to get cached dictionary of Enchantments
        [HarmonyPatch(typeof(ResourcesPrefabManager), "Load")]
        public class RPM_Load
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                var dict = (Dictionary<int, Enchantment>)At.GetField(ResourcesPrefabManager.Instance, "ENCHANTMENT_PREFABS");
                m_cachedEnchantments = new Dictionary<string, Enchantment>();
                foreach (var entry in dict)
                {
                    m_cachedEnchantments.Add($"{entry.Value.Name} ({entry.Key})", entry.Value);
                }
            }
        }

        internal void Update()
        {
            if (CustomKeybindings.GetKeyDown(MENU_TOGGLE_NAME))
            {
                ShowMenu = !ShowMenu;
                SL.LogWarning("Show Enchantment Menu: " + ShowMenu);
            }
        }

        internal void OnGUI()
        {
            if (ShowMenu)
            {
                //var orig = GUI.skin;
                //GUI.skin = SideLoader.UI.UIStyles.WindowSkin;
                m_rect = GUI.Window(WINDOW_ID, m_rect, WindowFunction, "Enchantment Menu (Ctrl+Alt+E Toggle)");
                //GUI.skin = orig;
            }
        }

        private void WindowFunction(int id)
        {
            GUI.DragWindow(new Rect(0, 0, m_rect.width - 35, 23));
            if (GUI.Button(new Rect(m_rect.width - 30, 2, 30, 20), "X"))
            {
                ShowMenu = false;
                return;
            }

            GUILayout.BeginArea(new Rect(5, 20, m_rect.width - 10, m_rect.height - 15));

            GUILayout.Space(20);

            if (SelectedEnchant != null)
            {
                if (GUILayout.Button("< Back to Enchantments"))
                {
                    SelectedEnchant = null;
                }
                GUILayout.Label("Selected Enchant: <b><color=orange>" + SelectedEnchant.Name + "</color></b>");
            }

            GUILayout.BeginHorizontal();
            GUILayout.Label("Selected Slot: " + SelectedSlot.ToString(), GUILayout.Width(m_rect.width - 70));
            if (GUILayout.Button("<", GUILayout.Width(25)))
            {
                SetEnum(ref SelectedSlot, -1);
            }
            if (GUILayout.Button(">", GUILayout.Width(25)))
            {
                SetEnum(ref SelectedSlot, 1);
            }
            GUILayout.EndHorizontal();

            var character = CharacterManager.Instance.GetFirstLocalCharacter();

            if (character)
            {
                var equipment = (Equipment)character.Inventory.GetEquippedItem(SelectedSlot);

                if (equipment)
                {
                    GUILayout.Label("<b>Equipped:</b> <color=orange>" + equipment.Name + "</color>");

                    if (equipment.IsEnchanted)
                    {
                        GUI.color = Color.red;
                        var enchant = equipment.GetComponentInChildren<Enchantment>();
                        if (GUILayout.Button("Remove Enchantment (" + enchant.Name + ")"))
                        {
                            enchant.UnapplyEnchantment();
                            var ids = (List<int>)At.GetField(equipment, "m_enchantmentIDs");
                            ids.Clear();
                            At.Invoke(equipment, "RefreshEnchantmentModifiers");
                        }
                    }
                    else if (SelectedEnchant != null)
                    {
                        GUI.color = Color.green;
                        if (GUILayout.Button("Apply Selected Enchant"))
                        {
                            equipment.AddEnchantment(SelectedEnchant.PresetID, false);
                        }
                    }
                    GUI.color = Color.white;
                }
                else
                {
                    GUI.color = Color.red;
                    GUILayout.Label("No item equipped in " + SelectedSlot.ToString() + " slot!");
                }
            }
            GUI.color = Color.white;

            if (SelectedEnchant == null)
            {
                GUILayout.Label("Enter an Enchantment ID to search for...");
                SearchText = GUILayout.TextField(SearchText);

                GUILayout.BeginVertical(GUI.skin.box);
                scroll = GUILayout.BeginScrollView(scroll);
                var search = SearchText.ToLower();
                foreach (var entry in m_cachedEnchantments)
                {
                    if (search == "" || entry.Key.ToLower().Contains(search))
                    {
                        if (GUILayout.Button(entry.Key))
                        {
                            SelectedEnchant = entry.Value;
                        }
                    }
                }
                GUILayout.EndScrollView();
                GUILayout.EndVertical();
            }

            GUILayout.EndArea();
        }

        public static void SetEnum(ref EquipmentSlot.EquipmentSlotIDs value, int change)
        {
            int newindex = (int)value + change;

            if (newindex == 2 || newindex == 4)
            {
                newindex += change;
            }

            if (newindex >= 0 && newindex < 9)
            {
                value = (EquipmentSlot.EquipmentSlotIDs)newindex;
            }
        }
    }
}
