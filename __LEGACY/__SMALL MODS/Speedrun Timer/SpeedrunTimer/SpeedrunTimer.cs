using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using UnityEngine.SceneManagement;
using BepInEx;
using HarmonyLib;

namespace SpeedrunTimer
{
    [BepInPlugin(GUID, NAME, VERSION)]
    public class SpeedrunTimer : BaseUnityPlugin
    {
        const string GUID = "com.sinai.speedruntimer";
        const string VERSION = "2.0";
        const string NAME = "Speedrun Timer";

        public static SpeedrunTimer Instance;

        public Settings settings = new Settings();

        public KeyCode StartKey;
        public KeyCode StopKey;
        public KeyCode ConditionKey;

        private static readonly string defaultTimeString = "0:00.000";
        public float m_Time = 0.0f;
        public string timeString = defaultTimeString;
        public bool timerRunning = false;
        private bool runCompleted = false;

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

        public static string configPath = "Mods/SpeedrunTimer.json";

        internal void Awake()
        {
            Instance = this;

            var harmony = new Harmony(GUID);
            harmony.PatchAll();

            LoadSettings();

            m_currentStopCondition = 0;

            StartKey = (KeyCode)Enum.Parse(typeof(KeyCode), settings.StartKey);
            StopKey = (KeyCode)Enum.Parse(typeof(KeyCode), settings.StopKey);
            ConditionKey = (KeyCode)Enum.Parse(typeof(KeyCode), settings.ConditionKey);
        }

        internal void Update()
        {
            if (Input.GetKeyDown(StartKey))
            {
                timerRunning = true;
                m_Time = 0;
                timeString = defaultTimeString;
                runCompleted = false;
            }

            if (Input.GetKeyDown(StopKey))
            {
                timerRunning = false;
                runCompleted = false;
            }

            if (Input.GetKeyDown(ConditionKey))
            {
                if (StopConditions.Count() - 1 > m_currentStopCondition)
                {
                    m_currentStopCondition++;
                }
                else
                {
                    m_currentStopCondition = 0;
                }
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

        private void LoadSettings()
        {
            bool flag = true;
            if (File.Exists(configPath))
            {
                string json = File.ReadAllText(configPath);
                var tempSettings = JsonUtility.FromJson<Settings>(json);
                if (tempSettings != null)
                {
                    if (Enum.IsDefined(typeof(KeyCode), tempSettings.StartKey)
                        && Enum.IsDefined(typeof(KeyCode), tempSettings.StopKey)
                        && Enum.IsDefined(typeof(KeyCode), tempSettings.ConditionKey))
                    {
                        settings = tempSettings;
                        flag = false;
                    }
                    else
                    {
                        Debug.LogError("[SpeedrunTimer] Could not parse KeyCodes! Please make sure they are valid KeyCodes");
                    }
                }
            }
            if (flag)
            {
                File.WriteAllText(configPath, JsonUtility.ToJson(settings, true));
            }
        }

        // ============= GUI ==============

        internal void OnGUI()
        {
            int origFontsize = GUI.skin.label.fontSize;
            GUI.BeginGroup(new Rect(5, 5, 350, 250));

            // Category
            GUI.skin.label.fontSize = 14;
            GUI.Label(new Rect(3, 3, 350, 25), "[" + settings.ConditionKey + "] Category: " + StopConditions.ElementAt(m_currentStopCondition).Key);

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
                GUI.Label(new Rect(3, 70, 350, 30), StartKey.ToString() + " to start...");
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
