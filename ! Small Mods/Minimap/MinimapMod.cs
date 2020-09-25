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

namespace Minimap
{
    [BepInPlugin(GUID, NAME, VERSION)]
    public class MinimapMod : BaseUnityPlugin
    {
        public const string GUID = "com.sinai.outward.minimap";
        public const string NAME = "Outward Minimap";
        public const string VERSION = "1.1.0";

        private static readonly FieldInfo currentAreaHasMap 
            = typeof(MapDisplay).GetField("m_currentAreaHasMap", BindingFlags.NonPublic | BindingFlags.Instance);

        private const string TOGGLE_KEY = "Toggle Minimap";

        private const string MARKER_NAME = "MarkerSphere";
        private const int MARKER_LAYER = 14;
        private static readonly Color CYAN = new Color(52f/255f, 229f/255f, 235f/255f);

        internal void Awake()
        {
            // setup keybinding
            CustomKeybindings.AddAction(TOGGLE_KEY, 
                CustomKeybindings.KeybindingsCategory.Menus,
                CustomKeybindings.ControlType.Both, 
                5, 
                CustomKeybindings.InputActionType.Button);

            // setup harmony
            new Harmony(GUID).PatchAll();

            // setup config
            Settings.SetupConfig();

            // global scene change event
            SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
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
                AddMarker(character.gameObject, Color.red, MarkerScript.Types.Enemy);
            }

            // gatherables
            foreach (var loot in Resources.FindObjectsOfTypeAll<SelfFilledItemContainer>())
            {
                AddMarker(loot.gameObject, Color.yellow, MarkerScript.Types.Loot);
            }

            foreach (var npc in Resources.FindObjectsOfTypeAll<SNPC>())
            {
                AddMarker(npc.gameObject, CYAN, MarkerScript.Types.NPC);
            }
        }

        private static void AddMarker(GameObject obj, Color color, MarkerScript.Types type)
        {
            if (obj.transform.Find(MARKER_NAME))
                return;

            var marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);

            marker.name = MARKER_NAME;

            marker.layer = MARKER_LAYER;

            marker.transform.parent = obj.transform;
            marker.transform.ResetLocal();
            marker.transform.localPosition += Vector3.up * 2.5f;

            marker.transform.localScale *= 2;

            var mesh = marker.GetComponent<MeshRenderer>();
            mesh.material.color = color;

            var script = marker.AddComponent<MarkerScript>();
            script.Init(type);

            //// make bright
            //var light = marker.AddComponent<Light>();
            //light.color = color;
            //light.intensity = 15f;
            //light.range = 5f;
            //light.cullingMask = 1 << MARKER_LAYER;
        }

        // custom keybinding input

        internal void Update()
        {
            foreach (var player in SplitScreenManager.Instance.LocalPlayers)
            {
                int id = player.RewiredID;
                if (CustomKeybindings.m_playerInputManager[id].GetButtonDown(TOGGLE_KEY))
                {
                    MinimapScript.Instances[id].ToggleEnable();
                }
            }
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

                AddMarker(__instance.gameObject, Color.green, MarkerScript.Types.Player);
            }
        }

        [HarmonyPatch(typeof(CharacterCamera), "LinkCamera")]
        public class CharacterCamera_Awake
        {
            [HarmonyPostfix]
            public static void Postfix(Camera _camera)
            {
                // XOR-out the marker layer from the mask
                _camera.cullingMask ^= 1 << MARKER_LAYER;
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
