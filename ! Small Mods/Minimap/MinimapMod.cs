using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using UnityEngine;
using BepInEx;
using HarmonyLib;

namespace Minimap
{
    [BepInPlugin(GUID, NAME, VERSION)]
    public class MinimapMod : BaseUnityPlugin
    {
        public const string GUID = "com.sinai.outward.minimap";
        public const string NAME = "Outward Minimap";
        public const string VERSION = "1.0.2";

        private static readonly FieldInfo currentAreaHasMap = typeof(MapDisplay).GetField("m_currentAreaHasMap", BindingFlags.NonPublic | BindingFlags.Instance);

        private const string TOGGLE_KEY = "Toggle Minimap";

        internal void Awake()
        {
            CustomKeybindings.AddAction(TOGGLE_KEY, CustomKeybindings.KeybindingsCategory.Menus, CustomKeybindings.ControlType.Both, 5, CustomKeybindings.InputActionType.Button);

            new Harmony(GUID).PatchAll();
        }

        internal void Update()
        {
            foreach (var player in SplitScreenManager.Instance.LocalPlayers)
            {
                int id = player.RewiredID;
                if (CustomKeybindings.m_playerInputManager[id].GetButtonDown(TOGGLE_KEY))
                {
                    if (id == 0)
                    {
                        MinimapScript.P1Instance.ToggleEnable();
                    }
                    else
                    {
                        MinimapScript.P2Instance.ToggleEnable();
                    }
                }
            }
        }

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
                        minimap.ShowBigMap();
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
                    minimap.HideBigMap();
                }
            }
        }

        [HarmonyPatch(typeof(SplitScreenManager), nameof(SplitScreenManager.AddLocalPlayer))]
        public class SplitScreenManager_AddLocalPlayer
        {
            [HarmonyPrefix]
            public static void Prefix(SplitScreenManager __instance)
            {
                if (__instance.LocalPlayerCount == 1)
                {
                    // Adding Player 2
                    var pos = MinimapScript.P1Instance.CanvasImage.rectTransform.localPosition;
                    pos.y -= MinimapScript.P1_ADJUST_SPLIT;
                    MinimapScript.P1Instance.CanvasImage.rectTransform.localPosition = pos;
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
                    // Removing Player 2
                    var pos = MinimapScript.P1Instance.CanvasImage.rectTransform.localPosition;
                    pos.y += MinimapScript.P1_ADJUST_SPLIT;
                    MinimapScript.P1Instance.CanvasImage.rectTransform.localPosition = pos;
                }
            }
        }
    }
}
