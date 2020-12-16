using System;
using System.IO;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SharedModConfig;
using HarmonyLib;
using SideLoader;
using BepInEx;
using SideLoader.Helpers;
using UnityEngine.UI;
using UnityEngine;

namespace CustomMultiplayerLimit
{
    public class Settings
    {
        public static string PlayerLimit = "PlayerLimit";
    }

    [BepInPlugin(GUID, NAME, VERSION)]
    [BepInDependency("com.sinai.SharedModConfig", BepInDependency.DependencyFlags.HardDependency)]
    public class CustomMultiplayerLimit : BaseUnityPlugin
    {
        public const string GUID = "com.sinai.CustomMultiplayerLimit";
        public const string NAME = "Custom Multiplayer Limit";
        public const string VERSION = "3.0";

        public static ModConfig config;

        internal static int s_limitInt;

        static void UpdateLimit() 
        {
            s_limitInt = (int)(float)config.GetValue(Settings.PlayerLimit); 
        }

        internal void Awake()
        {
            var harmony = new Harmony(GUID);
            harmony.PatchAll();

            config = SetupConfig();
            config.Register();

            UpdateLimit();
            config.OnSettingsSaved += UpdateLimit;
        }

        internal void Update()
        {
            if (Global.Lobby.PlayersInLobbyCount < 1 || NetworkLevelLoader.Instance.IsGameplayPaused)
                return;

            // handle custom multiplayer limit 
            if (PhotonNetwork.inRoom && PhotonNetwork.isMasterClient)
            {
                // if the room limit is not set to our custom value, do that.
                if (PhotonNetwork.room.MaxPlayers != s_limitInt)
                    PhotonNetwork.room.MaxPlayers = s_limitInt;

                // not sure if this is necessary
                if (!PhotonNetwork.room.IsOpen && PhotonNetwork.room.PlayerCount < s_limitInt)
                    PhotonNetwork.room.IsOpen = true;
            }
        }

        // =========== settings =============

        private ModConfig SetupConfig()
        {
            var newConfig = new ModConfig
            {
                ModName = "Custom Multiplayer Limit",
                SettingsVersion = 1.0,
                Settings = new List<BBSetting>
                {
                    new FloatSetting
                    {
                        Name = Settings.PlayerLimit,
                        Description = "Max number of Players in room (when you are host)",
                        DefaultValue = 4f,
                        RoundTo = 0,
                        MinValue = 1f,
                        MaxValue = 20f,
                        ShowPercent = false
                    }
                }
            };

            return newConfig;
        }

        // pause menu hook credit to Ashnal and Faedar

        [HarmonyPatch(typeof(PauseMenu), "Show")]
        public class PauseMenu_Show
        {
            [HarmonyPostfix]
            public static void PostFix(PauseMenu __instance)
            {
                var self = __instance;

                //Due to spawning bugs, only allow disconnect if you are the master, or if you are a client with no splitscreen, force splitscreen to quit before disconnect
                if (PhotonNetwork.isMasterClient || SplitScreenManager.Instance.LocalPlayerCount == 1)
                    //self.m_btnToggleNetwork.interactable = true;
                    ((Button)At.GetField(self, "m_btnToggleNetwork")).interactable = true;

                SetSplitButtonInteractable(self);

                //If this is used with a second splitscreen player both players load in missing inventory. Very BAD. Disabled for now.
                //Button findMatchButton = typeof(PauseMenu).GetField("m_btnFindMatch", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(instance) as Button;
                //findMatchButton.interactable = PhotonNetwork.offlineMode;
            }
        }

        // fix pause menu 2
        //for some reason the update function also forces the split button interactable, so we have to override it here too
        [HarmonyPatch(typeof(PauseMenu), "Update")]
        public class PauseMenu_Update
        {
            [HarmonyPostfix]
            public static void PostFix(PauseMenu __instance)
            {
                var self = __instance;

                SetSplitButtonInteractable(self);
            }
        }

        public static void SetSplitButtonInteractable(PauseMenu instance)
        {
            if (!PhotonNetwork.isMasterClient || !PhotonNetwork.isNonMasterClientInRoom)
                //instance.m_btnSplit.interactable = true;
                ((Button)At.GetField(instance, "m_btnSplit")).interactable = true;
        }

        // resting panel fix
        [HarmonyPatch(typeof(RestingMenu), "UpdatePanel")]
        public class RestingMenu_UpdatePanel
        {
            [HarmonyPrefix]
            public static bool Prefix(RestingMenu __instance, List<UID> ___m_otherPlayerUIDs, Slider[] ___m_sldOtherPlayerCursors,
                ref CanvasGroup ___m_restingCanvasGroup, ref Transform ___m_waitingForOthers, ref Text ___m_waitingText,
                RestingActivityDisplay[] ___m_restingActivityDisplays, RestingActivity.ActivityTypes[] ___ActiveActivities,
                Slider ___m_sldLocalPlayerCursor, ref int ___m_lastTotalRestTime, bool ___m_tryRest)
            {
                var self = __instance;

                At.Invoke(self, "RefreshSkylinePosition");
                int num = 0;
                bool flag = true;
                bool flag2 = true;
                if (Global.Lobby.PlayersInLobby.Count - 1 != ___m_otherPlayerUIDs.Count)
                {
                    self.InitPlayerCursors();
                    flag = false;
                    flag2 = false;
                }
                else
                {
                    try
                    {
                        for (int i = 0; i < ___m_otherPlayerUIDs.Count; i++)
                        {
                            Character characterFromPlayer = CharacterManager.Instance.GetCharacterFromPlayer(___m_otherPlayerUIDs[i]);
                            if (characterFromPlayer != null)
                            {
                                if (CharacterManager.Instance.RestingPlayerUIDs.Contains(characterFromPlayer.UID))
                                    flag2 &= characterFromPlayer.CharacterResting.DonePreparingRest;
                                else
                                    flag = false;
                                ___m_sldOtherPlayerCursors[i].value = (float)characterFromPlayer.CharacterResting.TotalRestTime;
                            }
                            else
                                flag = false;
                        }
                    }
                    catch { }
                }

                for (int j = 0; j < SplitScreenManager.Instance.LocalPlayerCount; j++)
                {
                    try
                    {
                        flag &= (SplitScreenManager.Instance.LocalPlayers[j].AssignedCharacter != null);
                    }
                    catch { }
                }

                flag2 = (flag2 && flag);

                ___m_restingCanvasGroup.interactable = (flag && !self.LocalCharacter.CharacterResting.DonePreparingRest);

                if (___m_waitingForOthers)
                {
                    if (___m_waitingForOthers.gameObject.activeSelf == ___m_restingCanvasGroup.interactable)
                        ___m_waitingForOthers.gameObject.SetActive(!___m_restingCanvasGroup.interactable);

                    if (___m_waitingText && ___m_waitingForOthers.gameObject.activeSelf)
                        ___m_waitingText.text = LocalizationManager.Instance.GetLoc(flag2 ? "Rest_Title_Resting" : "Sleep_Title_Waiting");
                }

                for (int k = 0; k < ___m_restingActivityDisplays.Length; k++)
                {
                    try
                    {
                        num += ___m_restingActivityDisplays[k].AssignedTime;
                    }
                    catch { }
                }

                for (int l = 0; l < ___m_restingActivityDisplays.Length; l++)
                {
                    try
                    {
                        if (___ActiveActivities[l] != RestingActivity.ActivityTypes.Guard || CharacterManager.Instance.BaseAmbushProbability > 0)
                            ___m_restingActivityDisplays[l].MaxValue = 24 - (num - ___m_restingActivityDisplays[l].AssignedTime);
                        else
                            ___m_restingActivityDisplays[l].MaxValue = 0;
                    }
                    catch { }
                }

                if (___m_sldLocalPlayerCursor)
                    ___m_sldLocalPlayerCursor.value = (float)num;

                bool flag3 = false;
                if (___m_lastTotalRestTime != num)
                {
                    flag3 = true;
                    ___m_lastTotalRestTime = num;
                }

                At.Invoke(self, "RefreshOverviews", new object[] { flag3 && !___m_tryRest });

                return false;
            }
        }
    }
}
