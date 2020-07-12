using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using BepInEx;
using HarmonyLib;

namespace NecromancySkills
{
    [BepInPlugin(GUID, NAME, VERSION)]
    [BepInDependency("com.sinai.SideLoader", BepInDependency.DependencyFlags.HardDependency)]
    public class NecromancyBase : BaseUnityPlugin
    {
        public static NecromancyBase Instance;
        public const string GUID = "com.sinai.necromancy";
        public const string NAME = "Necromancy Skills";
        public const string VERSION = "2.8";

        public static Settings settings;
        private static readonly string savePath = @"Mods\NecromancySkills.json";

        private static GameObject rpcObj;

        internal void Awake()
        {
            LoadSettings();

            var go = this.gameObject;
            go.AddComponent<TrainerManager>();
            go.AddComponent<SkillManager>();
            go.AddComponent<SummonManager>();

            rpcObj = new GameObject("NecromancyRPC");
            DontDestroyOnLoad(rpcObj);
            rpcObj.AddComponent<RPCManager>();

            var harmony = new Harmony(GUID);
            harmony.PatchAll();
        }

        private void LoadSettings()
        {
            bool _new = true;

            if (File.Exists(savePath))
            {
                try
                {
                    settings = JsonUtility.FromJson<Settings>(File.ReadAllText(savePath));
                    _new = false;
                }
                catch { }
            }
            if (_new)
            {
                settings = new Settings();
                SaveSettings();
            }
        }

        private void SaveSettings()
        {
            if (File.Exists(savePath))
            {
                File.Delete(savePath);
            }
            File.WriteAllText(savePath, JsonUtility.ToJson(settings, true));
        }
    }
}
