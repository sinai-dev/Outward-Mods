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
        private static Rect m_rect = new Rect(5, 5, 350, 450);
        private const int GuiWindowID = 8273;
        private static Vector2 scroll = Vector2.zero;

        public static bool ShowMenu = false;
        public static string SearchText = "";
        public static Enchantment SelectedEnchant;
        private EquipmentSlot.EquipmentSlotIDs SelectedSlot = EquipmentSlot.EquipmentSlotIDs.RightHand;

        // Setup
        internal void Awake()
        {
            var harmony = new Harmony(GUID);
            harmony.PatchAll();
        }

        // Patch to get cached dictionary of Enchantments
        [HarmonyPatch(typeof(ResourcesPrefabManager), "Load")]
        public class RPM_Load
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                var dict = (Dictionary<int, Enchantment>)At.GetValue(typeof(ResourcesPrefabManager), ResourcesPrefabManager.Instance, "ENCHANTMENT_PREFABS");
                m_cachedEnchantments = new Dictionary<string, Enchantment>();
                foreach (var entry in dict)
                {
                    m_cachedEnchantments.Add($"{entry.Value.Name} ({entry.Key})", entry.Value);
                }
            }
        }

        // Keyboard shortcut: Ctrl+Alt+E
        internal void Update()
        {
            if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
                && (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)))
            {
                if (Input.GetKeyDown(KeyCode.E))
                {
                    ShowMenu = !ShowMenu;
                }
            }

            MouseFix();
        }

        internal void OnGUI()
        {
            if (ShowMenu)
            {
                var orig = GUI.skin;
                GUI.skin = SideLoader.UI.UIStyles.WindowSkin;
                m_rect = GUI.Window(GuiWindowID, m_rect, WindowFunction, "Enchantment Menu (Ctrl+Alt+E Toggle)");
                GUI.skin = orig;
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
                            var ids = (List<int>)At.GetValue(typeof(Equipment), equipment, "m_enchantmentIDs");
                            ids.Clear();
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

        // =============== mouse fix ===============

        private static bool m_mouseShowing = false;

        public static void MouseFix()
        {
            var cha = CharacterManager.Instance.GetFirstLocalCharacter();

            if (!cha)
            {
                return;
            }

            if (ShowMenu)
            {
                if (!m_mouseShowing)
                {
                    m_mouseShowing = true;
                    ToggleDummyPanel(cha, true);
                }
            }
            else if (m_mouseShowing)
            {
                m_mouseShowing = false;
                ToggleDummyPanel(cha, false);
            }
        }

        private static void ToggleDummyPanel(Character cha, bool show)
        {
            if (cha.CharacterUI.PendingDemoCharSelectionScreen is Panel panel)
            {
                if (show)
                    panel.Show();
                else
                    panel.Hide();
            }
            else if (show)
            {
                GameObject obj = new GameObject();
                obj.transform.parent = cha.transform;
                obj.SetActive(true);

                Panel newPanel = obj.AddComponent<Panel>();
                At.SetValue(newPanel, typeof(CharacterUI), cha.CharacterUI, "PendingDemoCharSelectionScreen");
                newPanel.Show();
            }
        }
    }
}
