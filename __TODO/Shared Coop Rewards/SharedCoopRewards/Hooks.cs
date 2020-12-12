using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using NodeCanvas.Framework;
using NodeCanvas.Tasks;
using NodeCanvas.Tasks.Actions;
using System.IO;
using System.Reflection;
using HarmonyLib;

namespace SharedCoopRewards
{
    [HarmonyPatch(typeof(GiveReward), "OnExecute")]
    public class GiveReward_OnExecute
    {
        [HarmonyPrefix]
        public static bool Prefix(GiveReward __instance)
        {
            if ((bool)SharedCoopRewards.config.GetValue(Settings.Shared_ALL_Quest_Rewards))
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
        public static bool Prefix(ActionList __instance)
        {
            try
            {
                // this will cause an InvalidOperationException if there is no GiveReward.
                __instance.actions.First(x => x is GiveReward);

                if ((bool)SharedCoopRewards.config.GetValue(Settings.Shared_Quest_Rewards))
                {
                    bool HasSilverCost = false;

                    // check for "RemoveItem" tasks
                    foreach (var task in __instance.actions.Where(x => x is RemoveItem))
                    {
                        // check if the Items list contains Silver
                        if ((task as RemoveItem).Items.Where(x => x.value.ItemID == 9000010).Count() > 0)
                        {
                            // we are spending silver to get this reward. dont share.
                            HasSilverCost = true;
                            Debug.Log("Silver cost found! Not sharing, if there are rewards.");
                            break;
                        }
                    }

                    if (!HasSilverCost)
                    {
                        Debug.Log("Reward does not cost silver. Sharing.");
                        foreach (var task in __instance.actions.Where(x => x is GiveReward))
                        {
                            (task as GiveReward).RewardReceiver = GiveReward.Receiver.Everyone;
                        }
                    }
                }

            }
            catch (InvalidOperationException)
            {
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(Dropable), "GenerateContents", new Type[] { typeof(ItemContainer) })]
    public class Dropable_GenerateContents
    {
        [HarmonyPrefix]
        public static bool Prefix(Dropable __instance, ItemContainer _container)
        {
            if (_container)
            {
                int count = (bool)SharedCoopRewards.config.GetValue(Settings.Shared_World_Drops) ? Global.Lobby.PlayersInLobbyCount : 1;

                for (int i = 0; i < count; i++)
                {
                    GenerateContents(__instance, _container);
                }
            }

            return false;
        }

        public static void GenerateContents(Dropable self, ItemContainer container)
        {
            var allGuaranteed = At.GetValue(typeof(Dropable), self, "m_allGuaranteedDrops") as List<GuaranteedDrop>;
            var mainDropTables = At.GetValue(typeof(Dropable), self, "m_mainDropTables") as List<DropTable>;

            for (int i = 0; i < allGuaranteed.Count; i++)
            {
                if (allGuaranteed[i])
                {
                    allGuaranteed[i].GenerateDrop(container);
                }
            }
            for (int j = 0; j < mainDropTables.Count; j++)
            {
                if (mainDropTables[j])
                {
                    mainDropTables[j].GenerateDrop(container);
                }
            }
        }
    }
}
