using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using BepInEx;
using HarmonyLib;
using SideLoader;

namespace Combat_Dummy
{
    [BepInPlugin("com.sinai.combatdummy", "Combat Dummy", "1.0.0")]
    public class CombatDummyMod : BaseUnityPlugin
    {
        public static CombatDummyMod Instance;

        public static List<DummyCharacter> ActiveDummies = new List<DummyCharacter>();

        public const string MenuKey = "Combat Dummy Menu";

        private static bool m_mouseShowing = false;

        internal void Awake()
        {
            Instance = this;

            var obj = new GameObject("CombatDummyGUI");
            DontDestroyOnLoad(obj);
            obj.AddComponent<ModGUI>();

            var harmony = new Harmony("com.sinai.combatdummy");
            harmony.PatchAll();

            CustomKeybindings.AddAction(MenuKey, CustomKeybindings.KeybindingsCategory.Menus, CustomKeybindings.ControlType.Both, 5, CustomKeybindings.InputActionType.Button);
        }

        internal void Update()
        {
            if (CustomKeybindings.m_playerInputManager[0].GetButtonDown(MenuKey))
            {
                ModGUI.ShowMenu = !ModGUI.ShowMenu;
            }

            MouseFix();
        }

        public static DummyCharacter AddDummy(string name)
        {
            var dummy = new DummyCharacter
            {
                Name = name,
                Config = new DummyConfig()
            };

            ActiveDummies.Add(dummy);

            dummy.SpawnOrReset();

            return dummy;
        }

        public static void DestroyDummy(DummyCharacter dummy)
        {
            if (ActiveDummies.Contains(dummy))
            {
                ActiveDummies.Remove(dummy);
            }
            if (dummy.CharacterExists)
            {
                dummy.DestroyCharacter();
            }
        }

        public static void MouseFix()
        {
            var cha = CharacterManager.Instance.GetFirstLocalCharacter();

            if (!cha)
            {
                return;
            }

            if (ModGUI.ShowMenu)
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
