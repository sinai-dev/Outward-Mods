using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BepInEx;
using HarmonyLib;
using static CharacterEditor.CustomKeybindings;

namespace CharacterEditor
{
    [BepInPlugin(GUID, NAME, VERSION)]
    public class CharacterEditor : BaseUnityPlugin
    {
        public const string GUID = "com.sinai.charactereditor";
        public const string NAME = "Character Editor";
        public const string VERSION = "1.0.0";

        public static CharacterEditor Instance;

        public const string MenuKey = "Character Editor Menu";

        public bool ShowMenu { get; private set; }

        internal void Awake()
        {
            Instance = this;

            // setup custom keybinding
            AddAction(MenuKey, KeybindingsCategory.Actions, ControlType.Both, 5);

            // setup harmony
            var harmony = new Harmony(GUID);
            harmony.PatchAll();
        }

        private bool GameplayRunning()
        {
            return Global.Lobby.PlayersInLobbyCount > 0 && !MenuManager.Instance.IsInMainMenuScene && !NetworkLevelLoader.Instance.InLoading;
        }
        
        internal void Update()
        {

            for (int i = 0; i < m_playerInputManager.Count; i++)
            {
                if (m_playerInputManager[i].GetButtonDown(MenuKey))
                {
                    ShowMenu = !ShowMenu;
                }
            }
        }

        internal void OnGUI()
        {
            if (Global.Lobby.PlayersInLobbyCount < 1)
            {
                return;
            }


        }

        private void WindowFunction(int id)
        {

        }

        public static void MouseFix()
        {
            var cha = CharacterManager.Instance.GetFirstLocalCharacter();

            if (!cha)
            {
                return;
            }

            //if (ModGUI.ShowMenu)
            //{
            //    if (!m_mouseShowing)
            //    {
            //        m_mouseShowing = true;
            //        ToggleDummyPanel(cha, true);
            //    }
            //}
            //else if (m_mouseShowing)
            //{
            //    m_mouseShowing = false;
            //    ToggleDummyPanel(cha, false);
            //}
        }
    }
}
