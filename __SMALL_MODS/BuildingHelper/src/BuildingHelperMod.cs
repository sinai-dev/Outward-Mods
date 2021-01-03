using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx;
using HarmonyLib;
using SideLoader;
using UnityEngine;

namespace BuildingHelper
{
    public class Settings
    {
        public bool AutoFinishBuildings = false;
        public bool ForceNoRequirements = false;
        public int SpecialBuildingCap = 0;
    }

    [BepInPlugin(GUID, NAME, VERSION)]
    public class BuildingHelperMod : BaseUnityPlugin
    {
        const string GUID = "com.sinai.buildinghelper";
        const string NAME = "Building Helper";
        const string VERSION = "1.0.3";

        public static BuildingHelperMod Instance;

        public Settings settings;

        internal bool DoneSetup;

        internal const string MENU_KEY = "Building Helper Menu";

        internal const string SETTINGS_PATH = @"Mods\BuildingHelper.json";

        internal void Awake()
        {
            Instance = this;

            CustomKeybindings.AddAction(MENU_KEY, KeybindingsCategory.CustomKeybindings, ControlType.Keyboard);

            SL.OnPacksLoaded += SL_OnPacksLoaded;
            SL.OnSceneLoaded += SL_OnSceneLoaded;

            LoadSettings();

            new Harmony(GUID).PatchAll();
        }

        private void SL_OnSceneLoaded()
        {
            BuildingMenu.OnSceneChange();
        }

        private void SL_OnPacksLoaded()
        {
            DoneSetup = true;

            BuildingMenu.Init();
        }

        internal void Update()
        {
            if (CustomKeybindings.GetKeyDown(MENU_KEY))
                BuildingMenu.Show = !BuildingMenu.Show;
        }

        internal void OnGUI()
        {
            if (DoneSetup)
                BuildingMenu.OnGUI();
        }

        internal void OnApplicationQuit()
        {
            SaveSettings();
        }

        private void LoadSettings()
        {
            if (File.Exists(SETTINGS_PATH))
            {
                try
                {
                    var settings = JsonUtility.FromJson<Settings>(File.ReadAllText(SETTINGS_PATH));
                    if (settings != null)
                        this.settings = settings;
                    else
                        throw new Exception();
                }
                catch
                {
                    File.Delete(SETTINGS_PATH);
                    settings = new Settings();
                }
            }
            else
            {
                settings = new Settings();
            }
        }

        public void SaveSettings()
        {
            if (!Directory.Exists(@"Mods"))
                Directory.CreateDirectory(@"Mods");
            else if (File.Exists(SETTINGS_PATH))
                File.Delete(SETTINGS_PATH);

            File.WriteAllText(SETTINGS_PATH, JsonUtility.ToJson(settings, true));
        }

    }
}
