using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.IO;
using SideLoader;
using BepInEx;
using HarmonyLib;

namespace BlacksmithsToolbox
{
    [BepInPlugin(GUID, NAME, VERSION)]
    [BepInDependency("com.sinai.SideLoader", BepInDependency.DependencyFlags.HardDependency)]
    public class BlacksmithsToolbox : BaseUnityPlugin
    {
        const string GUID = "com.sinai.blacksmithstoolbox";
        const string NAME = "Blacksmith's Toolbox";
        const string VERSION = "2.1";

        public static Settings settings = new Settings();
        private static readonly string savePath = @"Mods\BlacksmithsToolbox.json";

        public const int TOOLBOX_ID = 5850750;
        public Item ToolboxPrefab;

        internal void Awake()
        {
            LoadSettings();

            SL.OnPacksLoaded += Setup;
            SL.OnSceneLoaded += OnSceneChange;
        }

        private void LoadSettings()
        {
            if (File.Exists(savePath))
            {
                JsonUtility.FromJsonOverwrite(File.ReadAllText(savePath), settings);
            }
            else
            {
                File.WriteAllText(savePath, JsonUtility.ToJson(settings, true));
            }
        }

        private void Setup()
        {
            SetupToolboxItem();
        }

        private void OnSceneChange()
        {
            SetupBlacksmith();
        }

        // Set up the Toolbox item prefab

        private void SetupToolboxItem()
        {
            var item = ResourcesPrefabManager.Instance.GetItemPrefab(TOOLBOX_ID);

            var desc = item.Description;
            desc = desc.Replace("%COST%", settings.Iron_Scrap_Cost.ToString());
            CustomItems.SetDescription(item, desc);

            var stats = new SL_ItemStats()
            {
                BaseValue = settings.Toolbox_Cost,
                MaxDurability = 100,
                RawWeight = 5.0f,
            };
            stats.ApplyToItem(item.GetComponent<ItemStats>());

            // add our custom effect
            var effects = new GameObject("Effects");
            effects.transform.parent = item.transform;
            effects.AddComponent<ToolboxEffect>();
        }

        // Function for setting up Blacksmith NPCs whenever a scene is loaded. Just adds our item to their pouch, if they dont already have it.

        private void SetupBlacksmith()
        {
            List<GameObject> list = Resources.FindObjectsOfTypeAll<GameObject>().Where(x => x.name == "HumanSNPC_Blacksmith").ToList();

            foreach (GameObject obj in list)
            {
                if (obj.GetComponentInChildren<MerchantPouch>(true) is MerchantPouch pouch
                    && !pouch.ContainsOfSameID(TOOLBOX_ID))
                {
                    Item item = ItemManager.Instance.GenerateItemNetwork(TOOLBOX_ID);
                    item.transform.parent = pouch.transform;
                }
            }
        }
    }

    public class Settings
    {
        public int Iron_Scrap_Cost = 5;
        public int Toolbox_Cost = 300;
        public float Durability_Cost_Per_Use = 5.0f;
    }
}
