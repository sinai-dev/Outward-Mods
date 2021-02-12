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
        const string VERSION = "2.3";

        public static Settings settings = new Settings();
        private const string SAVE_PATH = @"Mods\BlacksmithsToolbox.json";

        public const int TOOLBOX_ID = 5850750;
        public Item ToolboxPrefab;

        public static readonly string DROPTABLE_UID = GUID + "_droptable";

        internal void Awake()
        {
            LoadSettings();

            SL.OnPacksLoaded += OnPacksLoadedSetup;

            var table = new SL_DropTable
            {
                UID = DROPTABLE_UID,
                GuaranteedDrops = new List<SL_ItemDrop>
                {
                    new SL_ItemDrop { DroppedItemID = TOOLBOX_ID }
                }
            };
            table.ApplyTemplate();

            var source = new SL_DropTableAddition
            {
                SelectorTargets = new List<string>
                {
                    "3Rx_R0XDLUmYaNWm66SVCQ", // Default blacksmiths
                    "Zdg-qTDRa0-qOzzNSbtvzg", // Howard Brock
                    "EYhBqM653UGhDd5kcdMFXg", // Master-Smith Tokuga (2)
                    "QyJwLKySB0epxpEG3EBICQ_3-Finished", // Sal-Dumas
                },
                DropTableUIDsToAdd = new List<string>
                {
                    DROPTABLE_UID
                },
            };
            source.ApplyTemplate();
        }

        private void LoadSettings()
        {
            if (File.Exists(SAVE_PATH))
            {
                JsonUtility.FromJsonOverwrite(File.ReadAllText(SAVE_PATH), settings);
            }
            else
            {
                File.WriteAllText(SAVE_PATH, JsonUtility.ToJson(settings, true));
            }
        }

        private void OnPacksLoadedSetup()
        {
            SetupToolboxItem();
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
    }

    public class Settings
    {
        public int Iron_Scrap_Cost = 5;
        public int Toolbox_Cost = 300;
        public float Durability_Cost_Per_Use = 5.0f;
    }
}
