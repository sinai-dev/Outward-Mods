using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using BepInEx;
using System.IO;
using System.Reflection;
using HarmonyLib;
using Object = UnityEngine.Object;
using SideLoader;

namespace Explorer
{
    [BepInPlugin(ID, NAME, VERSION)]
    public class Explorer : BaseUnityPlugin
    {
        const string ID = "com.sinai.explorer";
        const string NAME = "Explorer";
        const string VERSION = "2.0";

        public static Explorer Instance;

        public static bool ShowMenu { get; set; } = false;

        public static bool ShowMouse { get; set; }
        private static bool m_mouseShowing;

        public static bool QuestDebugging { get; set; } = true;

        public static int ArrayLimit { get; set; } = 100;

        public bool MouseInspect { get; set; } = false;

        private string m_objUnderMouseName = "";

        public Camera MainCamera
        {
            get
            {
                if (m_main == null)
                {
                    m_main = Camera.main;
                }
                return m_main;
            }
        }
        private Camera m_main;

        internal void Awake()
        {
            Instance = this;

            LoadMCS();

            this.gameObject.AddComponent<WindowManager>();
            this.gameObject.AddComponent<MainMenu>();

            // log to game window
            Application.logMessageReceived += Application_logMessageReceived;

            // init debugging hooks
            var harmony = new Harmony(ID);
            harmony.PatchAll();
        }

        private void LoadMCS()
        {
            var mcsPath = Path.GetDirectoryName(Info.Location) + @"\mcs.dll";
            if (File.Exists(mcsPath))
            {
                Assembly.Load(File.ReadAllBytes(mcsPath));
                Logger.LogMessage("Loaded mcs.dll");
            }
            else
            {
                Logger.LogError("Could not find mcs.dll!");
            }
        }

        internal void Start()
        {
            // create logger
            var m_logger = new Vector2(600, 260);
            OLogger.CreateLog(new Rect(Screen.width - m_logger.x - 5, Screen.height - m_logger.y - 5, m_logger.x, m_logger.y));            

            // done init
            ShowMenu = true;
            OLogger.Log("Initialised Explorer. Unity version: " + Application.unityVersion.ToString());
        }

        internal void Update()
        {
            if (Input.GetKeyDown(KeyCode.F7))
            {
                ShowMenu = !ShowMenu;
            }

            if (ShowMenu)
            {
                if (Input.GetKeyDown(KeyCode.LeftAlt))
                    ShowMouse = !ShowMouse;

                if (Input.GetKeyDown(KeyCode.Escape))
                    ShowMouse = false;

                if (Input.GetKey(KeyCode.LeftShift) && Input.GetMouseButtonDown(1))
                {
                    MouseInspect = !MouseInspect;
                    
                    if (MouseInspect)
                    {
                        ShowMouse = true;
                    }
                }

                if (MouseInspect)
                {
                    InspectUnderMouse();
                }
            }
            else if (MouseInspect)
            {
                MouseInspect = false;
            }

            MouseFix();
        }

        private void InspectUnderMouse()
        {
            Ray ray = MainCamera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, 1000f))
            {
                var obj = hit.transform.gameObject;

                m_objUnderMouseName = obj.transform.GetGameObjectPath();

                if (Input.GetMouseButtonDown(0))
                {
                    MouseInspect = false;
                    m_objUnderMouseName = "";

                    WindowManager.InspectObject(obj, out _);
                }
            }
            else
            {
                m_objUnderMouseName = "";
            }
        }

        // draw Object Under Mouse text
        internal void OnGUI()
        {
            if (MouseInspect)
            {
                if (m_objUnderMouseName != "")
                {
                    var pos = Input.mousePosition;
                    var rect = new Rect(
                        pos.x - (Screen.width / 2), // x
                        Screen.height - pos.y - 50, // y
                        Screen.width,               // w
                        50                          // h
                    );

                    var origAlign = GUI.skin.label.alignment;
                    GUI.skin.label.alignment = TextAnchor.MiddleCenter;

                    //shadow text
                    GUI.Label(rect, $"<color=black>{m_objUnderMouseName}</color>");
                    //white text
                    GUI.Label(new Rect(rect.x -1, rect.y + 1, rect.width, rect.height), m_objUnderMouseName);

                    GUI.skin.label.alignment = origAlign;
                }
            }
        }

        public static void MouseFix()
        {
            var cha = CharacterManager.Instance.GetFirstLocalCharacter();

            if (!cha)
            {
                return;
            }

            if (ShowMenu && ShowMouse)
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

        // ************** public helpers **************

        public static Type GetType(string _type)
        {
            if (TryGetType(_type, "Assembly-CSharp") is Type gameType)
            {
                return gameType;
            }
            else if (TryGetType(_type, "UnityEngine") is Type unityType)
            {
                return unityType;
            }
            else
            {
                try
                {
                    return Type.GetType(_type);
                }
                catch
                {
                    return null;
                }
            }
        }

        private static Type TryGetType(string _type, string _assembly)
        {
            try
            {
                var type = Type.GetType($"{_type}, {_assembly}, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
                if (type == null)
                {
                    throw new Exception();
                }
                else
                {
                    return type;
                }
            }
            catch
            {
                return null;
            }
        }

        // ***************** LOG DEBUGGING ******************** //

        // logs i want to ignore
        private static readonly string[] blacklist = new string[]
        {
            "Internal: JobTempAlloc",
            "GUI Error:",
            "BoxColliders does not support negative scale or size",
            "is registered with more than one LODGroup",
            "only 0 controls when doing Repaint",
            "it is not close enough to the NavMesh",
            "Start Node:",
            "TLA_DEBUG_STACK_LEAK",
        };

        // Log Debug messages to OLogger window
        private void Application_logMessageReceived(string message, string stackTrace, LogType type)
        {
            foreach (string s in blacklist)
            {
                if (message.ToLower().Contains(s.ToLower()))
                {
                    return;
                }
            }

            if (type == LogType.Exception)
            {
                OLogger.Error(message + "\r\nStack Trace: " + stackTrace);
            }
            else if (type == LogType.Warning)
            {
                OLogger.Warning(message);
            }
            else
            {
                OLogger.Log(message);
            }
        }
    }
}
