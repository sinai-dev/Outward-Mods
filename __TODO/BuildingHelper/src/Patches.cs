using System;
using HarmonyLib;
using SideLoader;
using UnityEngine;

namespace BuildingHelper
{
    [HarmonyPatch(typeof(BuildingResourcesManager), "GetSpecializedBuildingCap")]
    public class BuildingResourcesManager_GetSpecializedBuildingCap
    {
        [HarmonyPostfix]
        public static void Postfix(ref int __result)
        {
            __result += Mathf.Min(BuildingHelperMod.Instance.settings.SpecialBuildingCap, 0);
        }
    }

    [HarmonyPatch(typeof(BuildingResourcesManager), "GetCanDeployNewBuilding")]
    public class BuildingResourcesManager_GetCanDeployNewBuilding
    {
        [HarmonyPrefix]
        public static bool Prefix(ref bool __result)
        {
            if (!StoreManager.Instance.IsDlcInstalled(OTWStoreAPI.DLCs.DLC2)
                || !BuildingHelperMod.Instance.settings.ForceNoRequirements 
                || PhotonNetwork.isNonMasterClientInRoom)
                return true;

            __result = true;
            return false;
        }
    }

    [HarmonyPatch(typeof(Building), "ActivateLevelVisuals", new Type[0])]
    public class Building_ActivateLevelVisuals
    {
        [HarmonyPrefix]
        public static void Prefix(Building __instance)
        {
            if (!StoreManager.Instance.IsDlcInstalled(OTWStoreAPI.DLCs.DLC2)
                || !BuildingHelperMod.Instance.settings.AutoFinishBuildings 
                || PhotonNetwork.isNonMasterClientInRoom)
                return;

            int sanity = 5;
            while (sanity >= 0 && __instance.CurrentPhase.ConstructionType == Building.ConstructionPhase.Type.WIP)
            {
                int cur = (int)At.GetField(__instance, "m_currentBasicPhaseIndex");
                if (cur < 0)
                    cur = 0;
                else
                    cur++;
                At.SetField(__instance, "m_currentBasicPhaseIndex", cur);

                sanity--;
            }

            if (sanity < 0)
                SL.LogWarning("Did more than 6 loops trying to auto-finish building, something went wrong!");

            At.SetField(__instance, "m_remainingConstructionTime", 0f);
        }
    }

    [HarmonyPatch(typeof(BuildingUpgrade), "CanSnapTo")]
    public class BuildingUpgrade_CanSnapTo
    {
        [HarmonyPrefix]
        public static bool CanSnapTo(BuildingUpgrade __instance, ref bool __result, Item _itemToSnapTo)
        {
            if (!StoreManager.Instance.IsDlcInstalled(OTWStoreAPI.DLCs.DLC2) 
                || !BuildingHelperMod.Instance.settings.ForceNoRequirements 
                || PhotonNetwork.isNonMasterClientInRoom)
                return true;

            __result = _itemToSnapTo
                        && _itemToSnapTo.ItemID == __instance.CompatibleItem.ItemID
                        && ((Building)_itemToSnapTo).IsInFinishedState;
            //&& ((Building)_itemToSnapTo).CurrentUpradePhaseIndex == __instance.UpgradeFromIndex 
            //&& ((Building)_itemToSnapTo).PendingUpgradePhaseIndex == -1;

            return false;
        }
    }

    [HarmonyPatch(typeof(Blueprint), "Use")]
    public class Blueprint_Use
    {
        [HarmonyPrefix]
        public static bool Prefix(Blueprint __instance, Character _character, ref bool __result)
        {
            if (!StoreManager.Instance.IsDlcInstalled(OTWStoreAPI.DLCs.DLC2) 
                || !BuildingHelperMod.Instance.settings.ForceNoRequirements 
                || PhotonNetwork.isNonMasterClientInRoom)
                return true;

            __result = true;
            _character.Inventory.OnUseItem(__instance.UID);
            return false;
        }
    }

    [HarmonyPatch(typeof(Building), "StartUpgrade")]
    public class Building_StartUpgrade
    {
        [HarmonyPostfix]
        public static void Postfix(Building __instance)
        {
            if (!StoreManager.Instance.IsDlcInstalled(OTWStoreAPI.DLCs.DLC2) 
                || !BuildingHelperMod.Instance.settings.AutoFinishBuildings 
                || PhotonNetwork.isNonMasterClientInRoom)
                return;

            At.SetField(__instance, "m_remainingConstructionTime", 0f);

            var pending = (int)At.GetField(__instance, "m_pendingUpgradePhaseIndex");
            At.SetField(__instance, "m_currentUpgradePhaseIndex", pending);
            At.SetField(__instance, "m_pendingUpgradePhaseIndex", -1);
            __instance.UpdateConstruction(0f);
        }
    }

    [HarmonyPatch(typeof(BuildingUpgrade), "GetCanDeploy")]
    public class BuildingUpgrade_GetCanDeploy
    {
        [HarmonyPrefix]
        public static bool Prefix(ref bool __result)
        {
            if (!StoreManager.Instance.IsDlcInstalled(OTWStoreAPI.DLCs.DLC2) 
                || !BuildingHelperMod.Instance.settings.ForceNoRequirements 
                || PhotonNetwork.isNonMasterClientInRoom)
                return true;

            __result = true;
            return false;
        }
    }
}
