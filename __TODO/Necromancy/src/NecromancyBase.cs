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
using SideLoader;
using System.Xml.Serialization;

namespace NecromancerSkills
{
    [BepInPlugin(GUID, NAME, VERSION)]
    [BepInDependency(SideLoader.SLPlugin.GUID, BepInDependency.DependencyFlags.HardDependency)]
    public class NecromancerBase : BaseUnityPlugin
    {
        public static NecromancerBase Instance;
        public const string GUID = "com.sinai.Necromancer";
        public const string NAME = "Necromancer Skills";
        public const string VERSION = "3.0";

        public static Settings settings;
        private const string SAVE_PATH = @"Mods\NecromancerSkills.xml";

        private static GameObject behaviourObj;

        internal void Awake()
        {
            LoadSettings();

            behaviourObj = new GameObject("NecromancerBehaviour");
            GameObject.DontDestroyOnLoad(behaviourObj);
            behaviourObj.AddComponent<TrainerManager>();
            behaviourObj.AddComponent<SkillManager>();
            behaviourObj.AddComponent<SummonManager>();

            var harmony = new Harmony(GUID);
            harmony.PatchAll();
        }

        private void LoadSettings()
        {
            bool _new = true;

            if (File.Exists(SAVE_PATH))
            {
                var serializer = new XmlSerializer(typeof(Settings));

                using (var file = File.OpenRead(SAVE_PATH))
                {
                    settings = serializer.Deserialize(file) as Settings;
                    _new = false;
                }
            }
            if (_new)
            {
                settings = new Settings();
                SaveSettings();
            }
        }

        private void SaveSettings()
        {
            if (File.Exists(SAVE_PATH))
                File.Delete(SAVE_PATH);

            using (var file = File.Create(SAVE_PATH))
            {
                var serializer = new XmlSerializer(typeof(Settings));
                serializer.Serialize(file, settings);
            }
        }
    }
}
