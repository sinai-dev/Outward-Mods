using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Explorer
{
    public class MainMenu : MonoBehaviour
    {
        public static MainMenu Instance;

        public const int MainWindowID = 10;
        public static Rect MainRect = new Rect(5, 5, 550, 700);
        private static readonly List<WindowPage> Pages = new List<WindowPage>();
        private static int m_currentPage = 0;

        internal void Awake()
        {
            Instance = this;

            Pages.Add(new ScenePage());
            Pages.Add(new SearchPage());
            Pages.Add(new ConsolePage());

            foreach (var page in Pages)
            {
                page.Init();
            }
        }

        public static void SetCurrentPage(int index)
        {
            if (index < 0 || Pages.Count <= index)
            {
                Debug.Log("cannot set page " + index);
                return;
            }
            m_currentPage = index;
            GUI.BringWindowToFront(MainWindowID);
            GUI.FocusWindow(MainWindowID);
        }

        internal void Update()
        {
            Pages[m_currentPage].Update();
        }

        internal void OnGUI()
        {
            if (Explorer.ShowMenu)
            {
                var origSkin = GUI.skin;
                GUI.skin = UIStyles.WindowSkin;

                MainRect = GUI.Window(MainWindowID, MainRect, MainWindow, "Outward Explorer");

                GUI.skin = origSkin;
            }
        }

        private void MainWindow(int id = MainWindowID)
        {
            GUI.DragWindow(new Rect(0, 0, MainRect.width - 90, 20));

            if (GUI.Button(new Rect(MainRect.width - 90, 2, 80, 20), "Hide (F7)"))
            {
                Explorer.ShowMenu = false;
                return;
            }

            GUILayout.BeginArea(new Rect(5, 25, MainRect.width - 10, MainRect.height - 35), GUI.skin.box);

            MainHeader();

            var page = Pages[m_currentPage];
            page.scroll = GUILayout.BeginScrollView(page.scroll);
            page.DrawWindow();
            GUILayout.EndScrollView();

            MainRect = WindowManager.ResizeWindow(MainRect, MainWindowID);

            GUILayout.EndArea();
        }

        private void MainHeader()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("<b>Options:</b>", GUILayout.Width(70));
            Explorer.QuestDebugging = GUILayout.Toggle(Explorer.QuestDebugging, "Debug Quest Events", GUILayout.Width(160));
            Explorer.ShowMouse = GUILayout.Toggle(Explorer.ShowMouse, "Lock Mouse (Alt)", GUILayout.Width(140));
            GUI.skin.label.alignment = TextAnchor.MiddleRight;
            GUILayout.Label("Array Limit:", GUILayout.Width(70));
            GUI.skin.label.alignment = TextAnchor.UpperLeft;
            var _input = GUILayout.TextField(Explorer.ArrayLimit.ToString(), GUILayout.Width(60));
            if (int.TryParse(_input, out int _lim))
            {
                Explorer.ArrayLimit = _lim;
            }
            GUILayout.EndHorizontal();

            Explorer.Instance.MouseInspect = GUILayout.Toggle(Explorer.Instance.MouseInspect, "Inspect Under Mouse (Shift + RMB)");

            GUILayout.BeginHorizontal();
            for (int i = 0; i < Pages.Count; i++)
            {
                if (m_currentPage == i)
                    GUI.color = Color.green;
                else
                    GUI.color = Color.white;

                if (GUILayout.Button(Pages[i].Name))
                {
                    m_currentPage = i;
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(10);
            GUI.color = Color.white;
        }

        public abstract class WindowPage
        {
            public virtual string Name { get; set; }

            public Vector2 scroll = Vector2.zero;

            public abstract void Init();

            public abstract void DrawWindow();

            public abstract void Update();
        }
    }
}
