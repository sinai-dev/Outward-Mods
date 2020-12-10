using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using HarmonyLib;

namespace Explorer
{
    public class WindowManager : MonoBehaviour
    {
        public static WindowManager Instance;

        public static List<UIWindow> Windows = new List<UIWindow>();
        public static int CurrentWindowID { get; set; } = 500000;
        private static Rect m_lastWindowRect;

        internal void Awake()
        {
            Instance = this;
        }

        [HarmonyPatch(typeof(UICursor), "StateForCursorButton")]
        public class UICursor_StateForCursorButton
        {
            [HarmonyPostfix]
            public static void Postfix(ref PointerEventData.FramePressState __result)
            {
                if (IsMouseInWindow)
                {
                    __result = PointerEventData.FramePressState.NotChanged;
                }
            }
        }

        // ========= Public Helpers =========

        public static bool IsMouseInWindow
        {
            get
            {
                if (!Explorer.ShowMenu)
                {
                    return false;
                }

                foreach (var window in Windows)
                {
                    if (RectContainsMouse(window.m_rect))
                    {
                        return true;
                    }
                }
                return RectContainsMouse(MainMenu.MainRect);
            }
        }

        private static bool RectContainsMouse(Rect rect)
        {
            return rect.Contains(new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y));
        }

        public static int NextWindowID()
        {
            return CurrentWindowID++;
        }

        public static Rect GetNewWindowRect()
        {
            return GetNewWindowRect(ref m_lastWindowRect);
        }

        public static Rect GetNewWindowRect(ref Rect lastRect)
        {
            Rect rect = new Rect(0, 0, 550, 700);

            var mainrect = MainMenu.MainRect;
            if (mainrect.x <= (Screen.width - mainrect.width - 100))
            {
                rect = new Rect(mainrect.x + mainrect.width + 20, mainrect.y, rect.width, rect.height);
            }

            if (lastRect.x == rect.x)
            {
                rect = new Rect(rect.x + 25, rect.y + 25, rect.width, rect.height);
            }

            lastRect = rect;

            return rect;
        }

        public static UIWindow InspectObject(object obj, out bool createdNew)
        {
            createdNew = false;

            foreach (var window in Windows)
            {
                if (obj == window.Target)
                {
                    GUI.BringWindowToFront(window.windowID);
                    GUI.FocusWindow(window.windowID);
                    return window;
                }
            }

            createdNew = true;
            if (obj is GameObject || obj is Transform)
            {
                return InspectGameObject(obj as GameObject ?? (obj as Transform).gameObject);
            }
            else
            {
                return InspectReflection(obj);
            }
        }

        private static UIWindow InspectGameObject(GameObject obj)
        {
            var new_window = UIWindow.CreateWindow<GameObjectWindow>(obj);
            GUI.FocusWindow(new_window.windowID);

            return new_window;
        }

        public static UIWindow InspectReflection(object obj)
        {
            var new_window = UIWindow.CreateWindow<ReflectionWindow>(obj);
            GUI.FocusWindow(new_window.windowID);

            return new_window;
        }


        // ============ GENERATED WINDOW HOLDER ============

        public abstract class UIWindow : MonoBehaviour
        {
            public abstract string Name { get; set; }

            public object Target;

            public int windowID;
            public Rect m_rect = new Rect(0, 0, 550, 700);

            public Vector2 scroll = Vector2.zero;

            public static UIWindow CreateWindow<T>(object target) where T: UIWindow
            {
                var component = (UIWindow)Instance.gameObject.AddComponent(typeof(T));

                component.Target = target;
                component.windowID = NextWindowID();
                component.m_rect = GetNewWindowRect();

                Windows.Add(component);

                component.Init();

                return component;
            }

            public void DestroyWindow()
            {
                Windows.Remove(this);
                Destroy(this);
            }

            public abstract void Init();
            public abstract void WindowFunction(int windowID);

            internal void OnGUI()
            {
                if (Explorer.ShowMenu)
                {
                    var origSkin = GUI.skin;

                    GUI.skin = UIStyles.WindowSkin;
                    m_rect = GUI.Window(windowID, m_rect, WindowFunction, Name);

                    GUI.skin = origSkin;
                }
            }

            public void Header()
            {
                GUI.DragWindow(new Rect(0, 0, m_rect.width - 90, 20));

                if (GUI.Button(new Rect(m_rect.width - 90, 2, 80, 20), "<color=red><b>X</b></color>"))
                {
                    DestroyWindow();
                    return;
                }
            }
        }

        // ============= Resize Window Helper ============

        static readonly GUIContent gcDrag = new GUIContent("<->", "drag to resize");

        private static bool isResizing = false;
        private static Rect m_currentResize;
        private static int m_currentWindow;

        public static Rect ResizeWindow(Rect _rect, int ID)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(_rect.width - 35);

            GUILayout.Button(gcDrag, GUI.skin.label, new GUILayoutOption[] { GUILayout.Width(25), GUILayout.Height(25) });

            var r = GUILayoutUtility.GetLastRect();

            Vector2 mouse = GUIUtility.ScreenToGUIPoint(new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y));

            if (r.Contains(mouse) && Input.GetMouseButtonDown(0))
            {
                isResizing = true;
                m_currentWindow = ID;
                m_currentResize = new Rect(mouse.x, mouse.y, _rect.width, _rect.height);
            }
            else if (!Input.GetMouseButton(0))
            {
                isResizing = false;
            }

            if (isResizing && ID == m_currentWindow)
            {
                _rect.width = Mathf.Max(100, m_currentResize.width + (mouse.x - m_currentResize.x));
                _rect.height = Mathf.Max(100, m_currentResize.height + (mouse.y - m_currentResize.y));
                _rect.xMax = Mathf.Min(Screen.width, _rect.xMax);  // modifying xMax affects width, not x
                _rect.yMax = Mathf.Min(Screen.height, _rect.yMax);  // modifying yMax affects height, not y
            }

            GUILayout.EndHorizontal();

            return _rect;
        }
    }
}
