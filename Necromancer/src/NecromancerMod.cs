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

namespace Necromancer
{
    [BepInPlugin(GUID, NAME, VERSION)]
    [BepInDependency(SL.GUID, BepInDependency.DependencyFlags.HardDependency)]
    public class NecromancerMod : BaseUnityPlugin
    {
        public static NecromancerMod Instance;
        public const string GUID = "com.sinai.Necromancer";
        public const string NAME = "Necromancer Skills";
        public const string VERSION = "3.0";

        public static Settings settings;

        private const string SAVE_PATH = @"BepInEx\config\NecromancerSkills.xml";

        // private static GameObject behaviourObj;

        internal void Awake()
        {
            LoadSettings();

            SL.OnPacksLoaded += SL_OnPacksLoaded;

            var harmony = new Harmony(GUID);
            harmony.PatchAll();
        }

        private void SL_OnPacksLoaded()
        {
            SummonManager.Init();

            TrainerManager.Init();

            SkillManager.Init();
        }

        private void LoadSettings()
        {
            // legacy settings check
            string legacySettingsPath = @"Mods\NecromancerSkills.xml";
            if (File.Exists(legacySettingsPath))
            {
                if (File.Exists(SAVE_PATH))
                {
                    File.Delete(legacySettingsPath);
                    SL.Log($"[Necromancer] Deleted legacy settings because new settings exist. Deleted: '{legacySettingsPath}'");
                }
                else
                {
                    File.Move(legacySettingsPath, SAVE_PATH);
                    SL.Log($"[Necromancer] Moved Necromancer settings from '{legacySettingsPath}' to '{SAVE_PATH}'");
                }
            }

            bool makeNew = true;

            try
            {
                if (File.Exists(SAVE_PATH))
                {
                    var serializer = new XmlSerializer(typeof(Settings));

                    using (var file = File.OpenRead(SAVE_PATH))
                    {
                        settings = serializer.Deserialize(file) as Settings;
                        makeNew = false;
                    }
                }
            }
            catch (Exception ex)
            {
                makeNew = true;
                Debug.Log("Exception loading Necromancer settings!");
                Debug.Log(ex);
            }

            if (makeNew)
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
