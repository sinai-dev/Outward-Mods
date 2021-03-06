using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using BepInEx;
using HarmonyLib;
using BepInEx.Configuration;
using NodeCanvas.Tasks.Actions;
using NodeCanvas.Framework;
using SideLoader;

namespace SharedQuestRewards
{
    [BepInPlugin(GUID, NAME, VERSION)]
    public class SharedQuestRewards : BaseUnityPlugin
    {
        const string GUID = "com.sinai.sharedquestrewards";
        const string NAME = "Shared Quest Rewards";
        const string VERSION = "3.0";

        internal const string CTG_NAME = "Shared Rewards Settings";
        public static ConfigEntry<bool> cfg_AggressiveSharing;

        internal void Awake()
        {
            cfg_AggressiveSharing = Config.Bind(CTG_NAME, "Aggressive sharing (Experimental)", false, 
                "More aggressive rewards sharing, may share things which you wouldn't expect (eg. buying a skill from a trainer)");

            var harmony = new Harmony(GUID);
            harmony.PatchAll();
        }

        [HarmonyPatch(typeof(GiveReward), "OnExecute")]
        public class GiveReward_OnExecute
        {
            [HarmonyPrefix]
            public static bool Prefix(GiveReward __instance)
            {
                if (cfg_AggressiveSharing.Value)
                {
                    __instance.RewardReceiver = GiveReward.Receiver.Everyone;
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(ActionList), "OnExecute")]
        public class ActionList_OnExecute
        {
            [HarmonyPrefix]
            public static void Prefix(ActionList __instance)
            {
                try
                {
                    var giveRewards = __instance.actions.Where(it => it is GiveReward);
                    
                    if (!giveRewards.Any())
                        return;

                    bool silverCost = false;

                    // check for "RemoveItem" tasks
                    foreach (var task in __instance.actions.Where(it => it is RemoveItem))
                    {
                        // check if the Items list contains Silver
                        if ((task as RemoveItem).Items.Any(it => it.value.ItemID == 9000010))
                        {
                            // we are spending silver to get this reward. dont share.
                            silverCost = true;
                            Debug.Log("Silver cost found! Not sharing, if there are rewards.");
                            break;
                        }
                    }

                    if (!silverCost)
                    {
                        Debug.Log("Reward does not cost silver. Sharing.");
                        foreach (GiveReward task in giveRewards)
                        {
                            task.RewardReceiver = GiveReward.Receiver.Everyone;
                        }
                    }

                }
                catch (Exception ex)
                {
                    Debug.Log("Excepting trying to share rewards: " + ex);
                }
            }
        }

        //[HarmonyPatch(typeof(Dropable), "GenerateContents", new Type[] { typeof(ItemContainer) })]
        //public class Dropable_GenerateContents
        //{
        //    [HarmonyPrefix]
        //    public static bool Prefix(Dropable __instance, ItemContainer _container)
        //    {
        //        if (__instance.GetComponentInParent<Merchant>())
        //            return true;

        //        if (_container)
        //        {
        //            int count = (bool)SharedCoopRewards.config.GetValue(Settings.Shared_World_Drops) ? Global.Lobby.PlayersInLobbyCount : 1;

        //            for (int i = 0; i < count; i++)
        //            {
        //                GenerateContents(__instance, _container);
        //            }
        //        }

        //        return false;
        //    }

        //    public static void GenerateContents(Dropable self, ItemContainer container)
        //    {
        //        var allGuaranteed = At.GetField(self, "m_allGuaranteedDrops") as List<GuaranteedDrop>;
        //        var mainDropTables = At.GetField(self, "m_mainDropTables") as List<DropTable>;

        //        for (int i = 0; i < allGuaranteed.Count; i++)
        //        {
        //            if (allGuaranteed[i])
        //            {
        //                allGuaranteed[i].GenerateDrop(container);
        //            }
        //        }
        //        for (int j = 0; j < mainDropTables.Count; j++)
        //        {
        //            if (mainDropTables[j])
        //            {
        //                mainDropTables[j].GenerateDrop(container);
        //            }
        //        }
        //    }
        //}
    }
}
