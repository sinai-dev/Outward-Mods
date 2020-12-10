using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.IO;

namespace OutSoulsMod
{
    public class BonfireGUI : MonoBehaviour
    {
        public static BonfireGUI Instance;

        public Texture2D bonfireTex;
        public Font PhilosopherFont;

        public Vector2 guiSize = new Vector2(250, 194);
        public Vector2 scroll = Vector2.zero;

        public int cur_Main_Page = 0;
        public int cur_Tele_Page = 0;

        internal void Awake()
        {
            Instance = this;
        }

        internal void Update()
        {
            MenuMouseFix();
        }

        internal void OnGUI()
        {
            if (NetworkLevelLoader.Instance.IsGameplayPaused || Global.Lobby.PlayersInLobbyCount < 1)
            {
                return;
            }

            if (BonfireManager.Instance.IsBonfireInteracting && Time.time - BonfireManager.Instance.lastInteractionTime > 1) // wait 1 second to show the bonfire menu
            {
                BonfireWindow();
            }
        }

        public void BonfireWindow()
        {
            float x = (Screen.width / 2) - (guiSize.x / 2);
            float y = (Screen.height / 2) - (guiSize.y / 2);

            if (bonfireTex == null)
            {
                string path = @"Mods\OutSouls\gui.png";

                if (File.Exists(path))
                {
                    bonfireTex = LoadPNG(path);
                    GUI.BeginGroup(new Rect(x, y, guiSize.x, guiSize.y), bonfireTex);
                }
                else
                {
                    Debug.LogError(@"[OutSouls] Bonfire Menu image not found! Ensure the .png file exists at Mods\OutSouls\gui.png");
                    GUI.BeginGroup(new Rect(x, y, guiSize.x, guiSize.y), GUI.skin.box);
                }
            }
            else
            {
                GUI.BeginGroup(new Rect(x, y, guiSize.x, guiSize.y), bonfireTex);
            }
            
            GUILayout.BeginArea(new Rect(8, 8, guiSize.x - 16, guiSize.y - 16));
            GUILayout.BeginVertical();

            int origSize = GUI.skin.button.fontSize;
            if (PhilosopherFont == null)
            {
                var fonts = Resources.FindObjectsOfTypeAll<Font>();
                foreach (Font f in fonts)
                {
                    if (f.name.ToLower().Contains("philosopher-regular"))
                    {
                        PhilosopherFont = f;
                        break;
                    }
                }
            }
            else 
            {
                GUI.skin.label.font = PhilosopherFont;
                GUI.skin.button.font = PhilosopherFont;
                GUI.skin.button.fontSize = 15;
            }

            if (cur_Main_Page == 0)
            {
                BonfireMenu();
            }
            else if (cur_Main_Page == 1)
            {
                TeleportsPage();
            }
            //else if (cur_Main_Page == 2)
            //{
            //    SettingsPage();
            //}
            else { cur_Main_Page = 0; }

            GUI.skin.button.fontSize = origSize;
            GUI.skin.label.font = null;
            GUI.skin.button.font = null;

            GUILayout.EndVertical();
            GUILayout.EndArea();
            GUI.EndGroup();
        }

        public void BonfireMenu()
        {
            GUI.skin.label.alignment = TextAnchor.UpperCenter;
            BoldHeader("OutSouls Bonfire Menu");
            GUI.skin.label.alignment = TextAnchor.UpperLeft;
            GUI.color = Color.white;

            string cost = "";
            if (!(bool)OutSouls.config.GetValue(Settings.Disable_Bonfire_Costs)) { cost = " (1 Fire Stone)"; }
            GUILayout.Space(5);
            if (GUILayout.Button("Teleport" + cost))
            {
                cur_Main_Page = 1;
            }

            SilverPriceButton("Repair Items", 50, "All Items Repaired!", RepairItems(), true);

            SilverPriceButton("Reset Area", 100, "Resetting Area...", RPCManager.Instance.BonfireReset(), false);

            //GUILayout.Space(5);
            //if (GUILayout.Button("Settings"))
            //{
            //    cur_Main_Page = 2;
            //}

            GUILayout.Space(5);
            GUI.color = Color.red;
            if (GUILayout.Button("Leave"))
            {
                BonfireManager.Instance.IsBonfireInteracting = false;
            }
        }

        private IEnumerator RepairItems()
        {
            foreach (string uid in CharacterManager.Instance.PlayerCharacters.Values)
            {
                if (CharacterManager.Instance.GetCharacter(uid) is Character c)
                {
                    c.Inventory.RepairEverything();
                }
            }

            yield return null;
        }

        public void SilverPriceButton(string label, int cost, string successMessage, IEnumerator successAction, bool everyonePays)
        {
            string costString = "";
            if (!(bool)OutSouls.config.GetValue(Settings.Disable_Bonfire_Costs)) { costString += " (" + cost + ") Silver"; }

            if (GUILayout.Button(label + costString))
            {
                bool costFlag = false;
                string costNames = "";
                List<Character> toPay = new List<Character>();

                if (!(bool)OutSouls.config.GetValue(Settings.Disable_Bonfire_Costs))
                {
                    if (everyonePays)
                    {
                        foreach (string uid in CharacterManager.Instance.PlayerCharacters.Values)
                        {
                            if (CharacterManager.Instance.GetCharacter(uid) is Character c)
                            {
                                CheckCost(c, cost, ref toPay, ref costNames, ref costFlag);
                            }
                        }
                    }
                    else
                    {
                        if (CharacterManager.Instance.GetWorldHostCharacter() is Character c)
                        {
                            CheckCost(c, cost, ref toPay, ref costNames, ref costFlag);
                        }
                    }
                }
                else { costFlag = false; }

                if (costFlag)
                {
                    if (everyonePays)
                    {
                        foreach (SplitPlayer player in SplitScreenManager.Instance.LocalPlayers)
                        {
                            player.CharUI.ShowInfoNotification(costNames + " does not have enough silver to " + label + " (" + cost + ")");
                        }
                    }
                    else
                    {
                        foreach (SplitPlayer player in SplitScreenManager.Instance.LocalPlayers)
                        {
                            player.CharUI.ShowInfoNotification("You do not have enough silver to " + label + " (" + cost + ")");
                        }
                    }
                }
                else
                {
                    if (!(bool)OutSouls.config.GetValue(Settings.Disable_Bonfire_Costs))
                    {
                        foreach (Character c in toPay)
                        {
                            RemoveSilverBetter(cost, c);
                        }
                    }

                    StartCoroutine(successAction);
                    foreach (SplitPlayer player in SplitScreenManager.Instance.LocalPlayers)
                    {
                        player.CharUI.ShowInfoNotification(successMessage);
                    }
                }
            }
        }

        private void CheckCost(Character c, int cost, ref List<Character> toPay, ref string costNames, ref bool costFlag)
        {
            if (c.Inventory.ContainedSilver < cost)
            {
                costFlag = true;
                if (costNames != "") { costNames += ", "; }
                costNames += c.Name;
            }
            else { toPay.Add(c); }
        }

        private void RemoveSilverBetter(int amount, Character c)
        {
            if (c.Inventory.Pouch.ContainedSilver < amount)
            {
                c.Inventory.EquippedBag.Container.RemoveSilver(amount - c.Inventory.Pouch.ContainedSilver);
                c.Inventory.Pouch.RemoveAllSilver();
            }
            else
            {
                c.Inventory.Pouch.RemoveSilver(amount);
            }
        }

        public void TeleportsPage()
        {
            if (cur_Tele_Page == 0)
            {
                if (GUILayout.Button("< Main Menu"))
                {
                    cur_Main_Page = 0;
                }

                GUILayout.Space(5);

                BoldHeader("Regions");

                if (GUILayout.Button("Chersonese"))
                {
                    cur_Tele_Page = 1;
                }
                if (GUILayout.Button("Enmerkar Forest"))
                {
                    cur_Tele_Page = 2;
                }
                if (GUILayout.Button("Abrassar"))
                {
                    cur_Tele_Page = 3;
                }
                if (GUILayout.Button("Hallowed Marsh"))
                {
                    cur_Tele_Page = 4;
                }
            }
            else
            {
                if (GUILayout.Button("< Regions"))
                {
                    cur_Tele_Page = 0;
                }

                scroll = GUILayout.BeginScrollView(scroll);

                if (cur_Tele_Page == 1)
                {
                    BoldHeader("Chersonese Outdoors");

                    BonfireManager.Instance.TeleportButton("Cierzo", "ChersoneseNewTerrain", 0);
                    BonfireManager.Instance.TeleportButton("Vendavel Fortress", "ChersoneseNewTerrain", 1);
                    BonfireManager.Instance.TeleportButton("Bandits' Prison", "ChersoneseNewTerrain", 2);
                    BonfireManager.Instance.TeleportButton("Ghost Pass", "ChersoneseNewTerrain", 3);
                    BonfireManager.Instance.TeleportButton("Conflux Mountain", "ChersoneseNewTerrain", 4);
                    BonfireManager.Instance.TeleportButton("Beach (Cierzo Storage)", "ChersoneseNewTerrain", 5);

                    BoldHeader("Dungeons");

                    BonfireManager.Instance.TeleportButton("Blister Burrow", "Chersonese_Dungeon2", 0);
                    BonfireManager.Instance.TeleportButton("Corrupted Tombs", "Chersonese_Dungeon6", 0);
                    BonfireManager.Instance.TeleportButton("Voltaic Hatchery", "Chersonese_Dungeon5", 0);
                }
                else if (cur_Tele_Page == 2)
                {
                    BoldHeader("Enmerkar Outdoors");

                    BonfireManager.Instance.TeleportButton("Berg", "Emercar", 0);
                    BonfireManager.Instance.TeleportButton("Ruined Settlement", "Emercar", 1);
                    BonfireManager.Instance.TeleportButton("Cabal of Wind Temple", "Emercar", 2);
                    BonfireManager.Instance.TeleportButton("Old Hunter's Cabin", "Emercar", 3);
                    BonfireManager.Instance.TeleportButton("Tree Husk (Center)", "Emercar", 4);

                    BoldHeader("Dungeons");

                    BonfireManager.Instance.TeleportButton("Royal Manticore's Lair", "Emercar_Dungeon1", 0);
                    BonfireManager.Instance.TeleportButton("Forest Hives", "Emercar_Dungeon2", 0);
                    BonfireManager.Instance.TeleportButton("Face of the Ancients", "Emercar_Dungeon4", 0);
                }
                else if (cur_Tele_Page == 3)
                {
                    BoldHeader("Abrassar Outdoors");

                    BonfireManager.Instance.TeleportButton("Levant", "Abrassar", 0);
                    BonfireManager.Instance.TeleportButton("Stone Titan Caves", "Abrassar", 1);
                    BonfireManager.Instance.TeleportButton("The Walled Garden", "Abrassar", 2);
                    BonfireManager.Instance.TeleportButton("Abandoned Docks", "Abrassar", 3);
                    BonfireManager.Instance.TeleportButton("Ancient Hive", "Abrassar", 4);

                    BoldHeader("Dungeons");

                    BonfireManager.Instance.TeleportButton("Undercity Passage", "Abrassar_Dungeon1", 0);
                    BonfireManager.Instance.TeleportButton("Electric Lab", "Abrassar_Dungeon2", 0);
                    BonfireManager.Instance.TeleportButton("The Slide", "Abrassar_Dungeon3", 0);
                    BonfireManager.Instance.TeleportButton("Sand Rose Cave", "Abrassar_Dungeon6", 0);
                }
                else if (cur_Tele_Page == 4)
                {
                    BoldHeader("Marsh Outdoors");

                    BonfireManager.Instance.TeleportButton("Monsoon", "HallowedMarshNewTerrain", 0);
                    BonfireManager.Instance.TeleportButton("Under Island", "HallowedMarshNewTerrain", 1);
                    BonfireManager.Instance.TeleportButton("Western Flatlands", "HallowedMarshNewTerrain", 2);
                    BonfireManager.Instance.TeleportButton("Cabal of Wind Altar", "HallowedMarshNewTerrain", 3);
                    BonfireManager.Instance.TeleportButton("Spire of Light", "HallowedMarshNewTerrain", 4);

                    BoldHeader("Dungeons");

                    BonfireManager.Instance.TeleportButton("Jade Quarry", "Hallowed_Dungeon1", 0);
                    BonfireManager.Instance.TeleportButton("Reptilian Lair", "Hallowed_Dungeon3", 0);
                    BonfireManager.Instance.TeleportButton("Dark Ziggurat", "Hallowed_Dungeon4_Interior", 0);
                    BonfireManager.Instance.TeleportButton("Dead Roots", "Hallowed_Dungeon7", 0);
                }

                GUILayout.EndScrollView();
            }
        }

        private bool m_lastMenuFix;

        private void MenuMouseFix()
        {
            var flag = BonfireManager.Instance.IsBonfireInteracting;
            if (m_lastMenuFix != flag)
            {
                m_lastMenuFix = flag;

                Character c = CharacterManager.Instance.GetFirstLocalCharacter();

                if (c.CharacterUI.PendingDemoCharSelectionScreen is Panel panel)
                {
                    if (m_lastMenuFix)
                        panel.Show();
                    else
                        panel.Hide();
                }
                else if (m_lastMenuFix)
                {
                    GameObject obj = new GameObject();
                    obj.transform.parent = c.transform;
                    obj.SetActive(true);

                    Panel newPanel = obj.AddComponent<Panel>();
                    At.SetValue(newPanel, typeof(CharacterUI), c.CharacterUI, "PendingDemoCharSelectionScreen");
                    newPanel.Show();
                }
            }
        }

        //public void SettingsPage()
        //{
        //    if (GUILayout.Button("< Main Menu"))
        //    {
        //        cur_Main_Page = 0;
        //    }
        //    GUILayout.Space(15);

        //    OutSouls.settings.Bonfires_Heal_Enemies = GUILayout.Toggle(OutSouls.settings.Bonfires_Heal_Enemies, "Bonfires Resurrect Enemies");
        //    OutSouls.settings.Disable_Bonfire_Costs = GUILayout.Toggle(OutSouls.settings.Disable_Bonfire_Costs, "Disable Bonfire Costs");
        //    OutSouls.settings.Cant_Use_Bonfires_In_Combat = GUILayout.Toggle(OutSouls.settings.Cant_Use_Bonfires_In_Combat, "Can't Use Bonfires In Combat");

        //    GUILayout.Space(20);

        //    GUILayout.Label("These settings can also be accessed from the main OutSouls menu");
        //}

        // ===== misc gui =====

        public void BoldHeader(string label)
        {
            GUI.color = new Color { r = 1, g = 0.45f, b = 0, a = 1 };
            GUI.skin.label.fontStyle = FontStyle.Bold;
            GUI.skin.label.fontSize = 17;
            GUI.skin.label.alignment = TextAnchor.MiddleCenter;
            GUILayout.Label(label);
            GUI.skin.label.fontStyle = FontStyle.Normal;
            GUI.skin.label.fontSize = 13;
            GUI.skin.label.alignment = TextAnchor.UpperLeft;
            GUI.color = Color.white;
        }

        public static Texture2D LoadPNG(string filePath)
        {
            Texture2D tex = null;
            byte[] fileData;

            if (File.Exists(filePath))
            {
                fileData = File.ReadAllBytes(filePath);
                tex = new Texture2D(2, 2);
                tex.LoadImage(fileData); //..this will auto-resize the texture dimensions.
            }
            return tex;
        }
    }
}
