using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using UnityEngine;
using BepInEx;
using HarmonyLib;
using UnityEngine.SceneManagement;
using SideLoader;

namespace Minimap
{
    [BepInPlugin(GUID, NAME, VERSION)]
    [BepInDependency(SharedModConfig.SharedModConfig.GUID, BepInDependency.DependencyFlags.HardDependency)]
    public class MinimapMod : BaseUnityPlugin
    {
        public const string GUID = "com.sinai.outward.minimap";
        public const string NAME = "Outward Minimap";
        public const string VERSION = "1.2.0";

        private static readonly FieldInfo currentAreaHasMap 
            = typeof(MapDisplay).GetField("m_currentAreaHasMap", BindingFlags.NonPublic | BindingFlags.Instance);

        public const string ASSETBUNDLE_PATH = @"BepInEx\plugins\Minimap\shaderbundle";
        public static Material AlwaysOnTopMaterial;

        private const string TOGGLE_KEY = "Toggle Minimap";

        internal void Awake()
        {
            // setup keybinding
            CustomKeybindings.AddAction(TOGGLE_KEY, 
                KeybindingsCategory.Menus,
                ControlType.Both);

            // setup harmony
            new Harmony(GUID).PatchAll();

            // setup config
            Settings.SetupConfig();

            // global scene change event
            SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;

            // Load bundle
            var bundle = AssetBundle.LoadFromFile(ASSETBUNDLE_PATH);
            AlwaysOnTopMaterial = bundle.LoadAsset<Material>("UI_AlwaysOnTop");
        }

        private void SceneManager_activeSceneChanged(Scene arg0, Scene arg1)
        {
            StartCoroutine(SetupSceneCoroutine());
        }

        private IEnumerator SetupSceneCoroutine()
        {
            while (!NetworkLevelLoader.Instance.AllPlayerDoneLoading || !NetworkLevelLoader.Instance.IsOverallLoadingDone)
            {
                yield return null;
            }

            // enemies
            foreach (var character in CharacterManager.Instance.Characters.Values.Where(x => x.IsAI))
            {
                MarkerScript.AddMarker(character.gameObject, MarkerScript.Types.Enemy);
            }

            // gatherables
            foreach (var loot in Resources.FindObjectsOfTypeAll<SelfFilledItemContainer>())
            {
                MarkerScript.AddMarker(loot.gameObject, MarkerScript.Types.Loot);
            }

            foreach (var npc in Resources.FindObjectsOfTypeAll<SNPC>())
            {
                MarkerScript.AddMarker(npc.gameObject, MarkerScript.Types.NPC);
            }
        }

        // custom keybinding input

        internal void Update()
        {
            if (CustomKeybindings.GetKeyDown(TOGGLE_KEY, out int playerID))
                MinimapScript.Instances[playerID].ToggleEnable();
        }

        // Add markers to friends
        [HarmonyPatch(typeof(Character), "Awake")]
        public class Character_Awake
        {
            [HarmonyPostfix]
            public static void Postfix(Character __instance)
            {
                if (__instance.GetComponent<CharacterAI>())
                {
                    return;
                }

                MarkerScript.AddMarker(__instance.gameObject, MarkerScript.Types.Player);
            }
        }

        [HarmonyPatch(typeof(CharacterCamera), "LinkCamera")]
        public class CharacterCamera_Awake
        {
            [HarmonyPostfix]
            public static void Postfix(Camera _camera)
            {
                // XOR-out the marker layer from the mask
                _camera.cullingMask ^= 1 << MarkerScript.MARKER_LAYER;
            }
        }

        // Setup the MinimapScript when player is created

        [HarmonyPatch(typeof(SplitPlayer), nameof(SplitPlayer.SetCharacter))]
        public class SplitPlayer_SetCharacter
        {
            [HarmonyPostfix]
            public static void Postfix(Character _character)
            {
                _character.gameObject.AddComponent<MinimapScript>()
                                     .Init(_character);
            }
        }

        // For scenes with no map, show a big version of our manual minimap instead.

        [HarmonyPatch(typeof(MapDisplay), nameof(MapDisplay.Show), new Type[0])]
        public class MapDisplay_Show
        {
            [HarmonyPostfix]
            public static void Postfix(MapDisplay __instance)
            {
                if (!(bool)currentAreaHasMap.GetValue(__instance))
                {
                    var minimap = __instance.LocalCharacter?.GetComponentInChildren<MinimapScript>();
                    if (minimap)
                    {
                        minimap.OnShowBigMap();
                    }
                }
            }
        }

        [HarmonyPatch(typeof(MapDisplay), "OnHide")]
        public class MapDisplay_OnHide
        {
            [HarmonyPrefix]
            public static void Prefix(MapDisplay __instance)
            {
                var minimap = __instance.LocalCharacter?.GetComponentInChildren<MinimapScript>();
                if (minimap)
                {
                    minimap.OnHideBigMap();
                }
            }
        }

        // Fix Player 1's minimap position when split begins and ends

        [HarmonyPatch(typeof(SplitScreenManager), nameof(SplitScreenManager.AddLocalPlayer))]
        public class SplitScreenManager_AddLocalPlayer
        {
            [HarmonyPrefix]
            public static void Prefix(SplitScreenManager __instance)
            {
                if (__instance.LocalPlayerCount == 1)
                {
                    MinimapScript.P1_OnSplitBegin();
                }
            }
        }

        [HarmonyPatch(typeof(SplitScreenManager), "RemoveLocalPlayer", new Type[] { typeof(SplitPlayer), typeof(string) })]
        public class SplitScreenManager_RemoveLocalPlayer
        {
            [HarmonyPrefix]
            public static void Prefix(SplitScreenManager __instance)
            {
                if (__instance.LocalPlayerCount == 2)
                {
                    MinimapScript.P1_OnSplitEnd();
                }
            }
        }
    }
}
