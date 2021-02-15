using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using UnityEngine.SceneManagement;
using BepInEx;
using HarmonyLib;
using BepInEx.Configuration;

namespace SpeedrunTimer
{
    [BepInPlugin(GUID, NAME, VERSION)]
    public class SpeedrunTimer : BaseUnityPlugin
    {
        const string GUID = "com.sinai.speedruntimer";
        const string VERSION = "2.0";
        const string NAME = "Speedrun Timer";

        public static SpeedrunTimer Instance;

        private const string CTG_KEYBINDINGS = "Keybindings";
        public ConfigEntry<KeyCode> StartKey;
        public ConfigEntry<KeyCode> StopKey;
        public ConfigEntry<KeyCode> ConditionKey;

        private static readonly string defaultTimeString = "0:00.000";
        public float m_Time = 0.0f;
        public string timeString = defaultTimeString;
        public bool timerRunning = false;
        private bool runCompleted = false;

        private static string CANNOT_START_REASON;

        public Dictionary<string, List<int>> StopConditions = new Dictionary<string, List<int>>
        {
            {  // "Peacemaker" quests (main quest)
                "Well-Earned Rest", 
                new List<int> 
                { 
                    7011104, 
                    7011204, 
                    7011304 
                } 
            },
            {   // Call to Adventure
                "Blood Price", 
                new List<int> 
                { 
                    7011001 
                } 
            }, 
        };
        private int m_currentStopCondition = 0;

        public static string configPath = @"BepInEx\config\SpeedrunTimer.json";

        internal void Awake()
        {
            Instance = this;

            new Harmony(GUID).PatchAll();

            StartKey = Config.Bind(CTG_KEYBINDINGS, "Start key", KeyCode.F8);
            StopKey = Config.Bind(CTG_KEYBINDINGS, "Stop key", KeyCode.F9);
            ConditionKey = Config.Bind(CTG_KEYBINDINGS, "Cycle auto-stop condition key", KeyCode.F10);

            m_currentStopCondition = 0;

            //StartKey = (KeyCode)Enum.Parse(typeof(KeyCode), settings.StartKey);
            //StopKey = (KeyCode)Enum.Parse(typeof(KeyCode), settings.StopKey);
            //ConditionKey = (KeyCode)Enum.Parse(typeof(KeyCode), settings.ConditionKey);
        }

        private static bool s_doneInitCheck;

        [HarmonyPatch(typeof(ResourcesPrefabManager), "Load")]
        public class RPM_Load
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                Instance.StartCoroutine(DelayedSetupCoroutine());
            }
        }

        private static IEnumerator DelayedSetupCoroutine()
        {
            while (ResourcesPrefabManager.Instance == null || !ResourcesPrefabManager.Instance.Loaded)
                yield return new WaitForSeconds(1f);

            s_doneInitCheck = true;

            if (Global.CheatsEnabled)
            {
                Debug.LogError("[SPEEDRUNTIMER] Cheats enabled! Not starting.");
                CANNOT_START_REASON = "Debug mode is enabled. Delete 'DEBUG.txt' file in Outward_Data.";
                yield break;
            }
            else
            {
                Debug.LogWarning("[SPEEDRUNTIMER] Cheats not enabled...");
                var plugins = Instance.gameObject.GetComponents<BaseUnityPlugin>().Where(it => it.Info.Metadata.GUID != "io.mefino.configurationmanager").ToArray();
                if (plugins.Length > 1)
                {
                    Debug.LogError("[SPEEDRUNTIMER] Plugins length: " + plugins.Length);
                    CANNOT_START_REASON = "Mods detected. Please disable all other mods (other than Configuration Manager) and restart.";
                    yield break;
                }

                Debug.LogWarning("[SPEEDRUNTIMER] No other plugins enabled...");
            }
        }

        internal void Update()
        {
            if (Input.GetKeyDown(StartKey.Value))
            {
                timerRunning = true;
                m_Time = 0;
                timeString = defaultTimeString;
                runCompleted = false;
            }
            else if (Input.GetKeyDown(StopKey.Value))
            {
                timerRunning = false;
                runCompleted = false;
            }
            else if (Input.GetKeyDown(ConditionKey.Value))
            {
                if (StopConditions.Count() - 1 > m_currentStopCondition)
                    m_currentStopCondition++;
                else
                    m_currentStopCondition = 0;
            }

            if (IsGameplayRunning() && timerRunning)
            {
                m_Time += Time.deltaTime;

                TimeSpan time = TimeSpan.FromSeconds(m_Time);
                timeString = (time.Hours > 0 ? (time.Hours + ":") : "") + time.Minutes + ":" + time.Seconds.ToString("00") + "." + time.Milliseconds.ToString("000");

                // todo check stop condition
                var c = CharacterManager.Instance.GetFirstLocalCharacter();
                foreach (int id in StopConditions.ElementAt(m_currentStopCondition).Value)
                {
                    var quest = c.Inventory.QuestKnowledge.GetItemFromItemID(id);
                    if (quest && (quest as Quest).IsCompleted)
                    {
                        timerRunning = false;
                        runCompleted = true;
                    }
                }
            }
        }

        private bool IsGameplayRunning()
        {
            return Global.Lobby.PlayersInLobbyCount > 0 && !NetworkLevelLoader.Instance.IsGameplayPaused;
        }

        // ============= GUI ==============

        internal void OnGUI()
        {
            if (!s_doneInitCheck)
                return;

            if (CANNOT_START_REASON != null)
            {
                var middle = new Rect(Screen.width / 2 - 200, Screen.height / 2 - 50, 400, 100);
                GUILayout.BeginArea(middle, GUI.skin.box);

                GUILayout.Label($"You need to restart to begin a speedrun.\n\nReason: {CANNOT_START_REASON}");

                GUILayout.EndArea();
                return;
            }

            int origFontsize = GUI.skin.label.fontSize;
            GUI.BeginGroup(new Rect(5, 5, 350, 250));

            // Category
            GUI.skin.label.fontSize = 14;
            GUI.Label(new Rect(3, 3, 350, 25), "[" + ConditionKey.Value.ToString() + "] Category: " + StopConditions.ElementAt(m_currentStopCondition).Key);

            // Timer
            GUI.skin.label.fontSize = 27;
            // shadowtext
            GUI.color = Color.black;
            GUI.Label(new Rect(4, 31, 349, 79), timeString);
            // main text
            if (!timerRunning || !IsGameplayRunning())
                if (runCompleted)
                    GUI.color = Color.green;
                else
                    GUI.color = Color.yellow;
            else
                GUI.color = Color.white;
            GUI.Label(new Rect(3, 30, 350, 35), timeString);

            // [StartKey] to start...
            if (!timerRunning)
            {
                GUI.skin.label.fontSize = 13;
                GUI.Label(new Rect(3, 70, 350, 30), StartKey.Value.ToString() + " to start...");
            }

            GUI.EndGroup();
            GUI.skin.label.fontSize = origFontsize;
            GUI.color = Color.white;
        }
    }

    public class Settings
    {
        public string StartKey = "F8";
        public string StopKey = "F9";
        public string ConditionKey = "F10";
    }
}
