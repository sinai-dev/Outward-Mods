using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Partiality.Modloader;
using UnityEngine;
using UnityEngine.UI;
//using SinAPI;

namespace MertonsChallenge
{
    public class ChallengeGUI : MonoBehaviour
    {
        public ChallengeGlobal global;

        private Rect m_windowRect = Rect.zero;
        private Vector2 scroll = Vector2.zero;
        private string currentMessage = "";
        public Font Philosopher_Font = null;

        internal void Update()
        {
            // get gui font
            if (Philosopher_Font == null)
            {
                Font[] fonts = Resources.FindObjectsOfTypeAll<Font>();
                if (fonts != null && fonts.Count() > 0)
                {
                    foreach (Font font in fonts)
                    {
                        if (font.name == "Philosopher-Regular")
                        {
                            Philosopher_Font = font;
                        }
                    }
                }
            }
        }

        internal void OnGUI()
        {
            //if (m_windowRect == Rect.zero) {
            //    m_windowRect = new Rect(5, 5, 370, 300);
            //}
            //else {
            //    m_windowRect = GUI.Window(250, m_windowRect, DrawMenu, "Horde manager GUI");
            //}

            if (currentMessage != "") {
                DrawMessage();
            }

            if (global.IsGameplayStarted && global.CurrentTime > 0) {
                DrawCurrentTime();
            }
        }

        public void SetMessage(string s)
        {
            currentMessage = s;
        }

        public IEnumerator SetMessage(string s, float time)
        {
            currentMessage = s;
            yield return new WaitForSeconds(time);
            if (currentMessage == s) { currentMessage = ""; }
        }

        private void DrawMessage()
        {
            if (MenuManager.Instance.IsReturningToMainMenu || MenuManager.Instance.IsInMainMenuScene) { return; }

            GUI.skin.label.font = Philosopher_Font;
            GUI.skin.label.fontSize = 25;
            GUI.skin.label.fontStyle = FontStyle.Bold;
            GUI.skin.label.alignment = TextAnchor.UpperCenter;

            GUI.color = Color.black;
            Rect rect = new Rect(1, Screen.height / 13 + 1, Screen.width, 300);
            GUI.Label(rect, currentMessage);

            GUI.color = new Color(r: 1, b: 0.4f, g: 0.4f, a: 1);
            rect = new Rect(0, Screen.height / 13, Screen.width, 300);
            GUI.Label(rect, currentMessage);

            ResetGUI();
        }

        private void DrawCurrentTime()
        {
            GUI.skin.label.font = Philosopher_Font;
            GUI.skin.label.fontSize = 20;
            GUI.skin.label.fontStyle = FontStyle.Bold;
            GUI.skin.label.alignment = TextAnchor.UpperLeft;

            string timestring = GetTimeString();
            float wave = global.BossActive ? global.BossesSpawned : global.BossesSpawned + 1;
            string label = "Time: " + timestring + "\r\nWave: " + wave + "\r\nKills: " + global.EnemiesKilled;

            GUI.color = Color.black;
            Rect rect = new Rect(31, Screen.height / 2 + 1, Screen.width, 300);
            GUI.Label(rect, label);

            GUI.color = new Color(r: 1, b: 0.4f, g: 0.4f, a: 1);
            rect = new Rect(30, Screen.height / 2, Screen.width, 300);
            GUI.Label(rect, label);

            ResetGUI();
        }

        public string GetTimeString()
        {
            string timestring = "";
            TimeSpan t = TimeSpan.FromSeconds(global.CurrentTime);
            if (t.Hours > 0) { timestring += t.Hours + "h "; }
            if (t.Minutes > 0) { timestring += t.Minutes + "m "; }
            if (t.Seconds > 0) { timestring += t.Seconds + "s "; }

            return timestring;
        }

        private void ResetGUI()
        {
            GUI.color = Color.white;
            GUI.skin.label.fontStyle = FontStyle.Normal;
            GUI.skin.label.alignment = TextAnchor.UpperLeft;
            GUI.skin.label.fontSize = 13;
            GUI.skin.label.font = null;
        }

        //private void DrawMenu(int id)
        //{
        //    ResetGUI();
        //    GUI.DragWindow(new Rect(0, 0, m_windowRect.width, 20));

        //    GUILayout.BeginArea(new Rect(3, 25, m_windowRect.width - 5, m_windowRect.height - 35));
        //    GUILayout.BeginVertical(GUI.skin.box);
        //    scroll = GUILayout.BeginScrollView(scroll);

        //    try
        //    {
        //        if (global.CurrentTemplate == null) { GUI.color = Color.red; } else { GUI.color = Color.green; }
        //        GUILayout.Label("Current Template Name: " + (global.CurrentTemplate == null ? "null" : global.CurrentTemplate.ArenaName));
        //        GUI.color = Color.white;
        //        GUILayout.Space(15);

        //        GUILayout.Label("Boss Ready: " + global.ShouldSpawnBoss());
        //        GUILayout.Label("Enemy spawn target: " + global.EnemySpawnTarget());
        //        GUILayout.Label("Total Enemies: " + global.TotalEnemiesInPlay() + " (" + global.EnemiesInQueue + " in queue)");
        //        foreach (Character c in global.enemyMgr.ActiveMinions.Where(x => x.Health > 0))
        //        {
        //            if (c.isActiveAndEnabled)
        //            {
        //                GUI.color = Color.green;
        //            }
        //            else
        //            {
        //                GUI.color = Color.red;
        //            }

        //            GUILayout.Label(c.Name);
        //        }
        //        GUI.color = Color.white;
        //    }
        //    catch { }

        //    GUILayout.EndScrollView();
        //    GUILayout.EndVertical();
        //    GUILayout.EndArea();
        //}
    }
}
