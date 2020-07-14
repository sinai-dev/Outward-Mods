using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using HarmonyLib;
using SideLoader;

namespace ImbuedBows
{
    public class ManaBow : MonoBehaviour
    {
        public static Tag ManaBowTag;
        private const string ManaBowTagString = "ManaBow";

        public const float ManaBowCost = 5f;
        public const float ManaBowHoldCost = 0.1f;

        public const int ManaArrowID = 2800910;

        // ======= harmony patch fixes ==========

        [HarmonyPatch(typeof(ProjectileItem), "AssignVisual")]
        public class ProjectileItem_AssignVisual
        {
            [HarmonyPrefix]
            public static void Prefix(ref Projectile ___m_projectile, ProjectileItem __instance)
            {
                ___m_projectile = __instance.GetComponent<Projectile>();
            }
        }

        [HarmonyPatch(typeof(EffectSynchronizer), "RegisterEffectReference")]
        public class EffectSynchronizer_RegisterEffectReference
        {
            [HarmonyPrefix]
            public static void Prefix(EffectSynchronizer __instance, ref List<string>[] ___m_categoryEffects)
            {
                if (___m_categoryEffects == null)
                {
                    ___m_categoryEffects = new List<string>[EffectSynchronizer.EffectCategoriesCount];
                    for (int j = 0; j < ___m_categoryEffects.Length; j++)
                    {
                        ___m_categoryEffects[j] = new List<string>();
                    }
                }
            }
        }

        [HarmonyPatch(typeof(ProjectileWeapon), "UpdateProcessing")]
        public class ProjectileWeapon_UpdateProcessing
        {
            [HarmonyPostfix]
            public static void Postfix(ProjectileWeapon __instance, ref bool ___m_fullyBent)
            {
                if (IsManaBow(__instance) && ___m_fullyBent)
                {
                    var owner = __instance.OwnerCharacter;
                    var ammo = owner.Inventory.Equipment.GetEquippedItem(EquipmentSlot.EquipmentSlotIDs.Quiver);
                    if (!ammo)
                    {
                        ___m_fullyBent = false;
                        At.SetValue(false, typeof(Character), owner, "m_currentlyChargingAttack");
                        owner.BowRelease();

                        var charger = (WeaponCharger)__instance.GetExtension("WeaponCharger");
                        charger.ResetCharging();
                    }
                }
            }
        }


        // ============= methods ==================

        public static bool IsManaBow(Item item)
        {
            return item.HasTag(ManaBowTag);
        }

        internal void Awake()
        {
            SL.BeforePacksLoaded += SL_BeforePacksLoaded;
            SL.OnPacksLoaded += SL_OnPacksLoaded;
        }

        private void SL_BeforePacksLoaded()
        {
            ManaBowTag = CustomTags.CreateTag(ManaBowTagString);
        }

        //setup after sideloader init is done
        private void SL_OnPacksLoaded()
        {
            Debug.Log("Setting up mana arrow");

            // setup custom mana projectile
            var manaArrow = ResourcesPrefabManager.Instance.GetItemPrefab(ManaArrowID) as Ammunition;

            // set empty equipped visuals (hide quiver)
            var vLink = CustomItemVisuals.GetOrAddVisualLink(manaArrow);
            vLink.ItemSpecialVisuals = new GameObject("ManaQuiverDummy").transform;
            DontDestroyOnLoad(vLink.ItemSpecialVisuals.gameObject);

            // custom arrow ProjectileItem component (determines the ammunition behaviour as projectile)
            var origProjFX = manaArrow.ProjectileFXPrefab.gameObject;
            origProjFX.SetActive(false);
            manaArrow.ProjectileFXPrefab = Instantiate(origProjFX).transform;
            origProjFX.SetActive(true);

            DontDestroyOnLoad(manaArrow.ProjectileFXPrefab.gameObject);

            var projItem = manaArrow.ProjectileFXPrefab.GetComponent<ProjectileItem>();
            projItem.CollisionBehavior = ProjectileItem.CollisionBehaviorTypes.None;
            projItem.EphemeralProjectile = true;

            var raycast = manaArrow.ProjectileFXPrefab.GetComponent<RaycastProjectile>();
            raycast.ProjectileVisualsToDisable = projItem.gameObject;

            raycast.ImpactSoundMaterial = EquipmentSoundMaterials.Goo;
        }

        [HarmonyPatch(typeof(WeaponLoadout), "CanBeLoaded")]
        public class WeaponLoadout_CanBeLoaded
        {
            [HarmonyPrefix]
            public static bool Prefix(WeaponLoadout __instance, ref bool __result)
            {
                var item = __instance.Item;

                if (IsManaBow(item))
                {
                    float currentMana = item.OwnerCharacter.Stats.CurrentMana;
                    float manaCost = item.OwnerCharacter.Stats.GetFinalManaConsumption(null, ManaBowCost);
                    if (currentMana - manaCost >= 0)
                    {
                        __result = true;
                    }
                    else
                    {
                        item.OwnerCharacter.CharacterUI.ShowInfoNotificationLoc("Notification_Skill_NotEnoughtMana");
                        __result = false;
                    }
                    return false;
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(WeaponLoadout), "ReduceShotAmount")]
        public class WeaponLoadout_ReduceShotAmount
        {
            [HarmonyPrefix]
            public static bool Prefix(WeaponLoadout __instance, bool _destroyOnEmpty = false)
            {
                var item = __instance.Item;

                if (IsManaBow(item))
                {
                    float manaCost = item.OwnerCharacter.Stats.GetFinalManaConsumption(null, ManaBowCost);
                    item.OwnerCharacter.Stats.UseMana(null, manaCost);
                    return false;
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(CharacterEquipment), "GetEquippedAmmunition")]
        public class CharacterEquipment_GetEquippedAmmunition
        {
            [HarmonyPrefix]
            public static bool Prefix(CharacterEquipment __instance, ref Ammunition __result)
            {
                var self = __instance;

                if (self.GetEquippedItem(EquipmentSlot.EquipmentSlotIDs.RightHand) is Weapon weapon && IsManaBow(weapon))
                {
                    var character = At.GetValue(typeof(CharacterEquipment), self, "m_character") as Character;

                    if (!character.Inventory.HasEquipped(ManaArrowID))
                    {
                        Ammunition ammo;
                        if (!character.Inventory.OwnsItem(ManaArrowID))
                        {
                            ammo = ItemManager.Instance.GenerateItemNetwork(ManaArrowID) as Ammunition;
                        }
                        else
                        {
                            ammo = (Ammunition)character.Inventory.GetOwnedItems(ManaArrowID)[0];
                        }
                        ammo.ChangeParent(self.GetMatchingEquipmentSlotTransform(EquipmentSlot.EquipmentSlotIDs.Quiver));
                        __result = ammo;
                        return false;
                    }
                    else
                    {
                        __result = self.GetEquippedItem(EquipmentSlot.EquipmentSlotIDs.Quiver) as Ammunition;
                        return false;
                    }
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(AttackSkill), "OwnerHasAllRequiredItems")]
        public class AttackSkill_OwnerHasAllRequiredItems
        {
            [HarmonyPrefix]
            public static bool Prefix(AttackSkill __instance, bool _TryingToActivate, ref bool __result)
            {
                if (!(__instance is RangeAttackSkill))
                {
                    return true;
                }

                var self = __instance;

                if (self.OwnerCharacter && self.OwnerCharacter.CurrentWeapon is ProjectileWeapon bow && IsManaBow(bow))
                {
                    __result = true;
                    return false;
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(UseLoadoutAmunition), "TryTriggerConditions")]
        public class UseLoadoutAmmunition_TryTriggerConditions
        {
            [HarmonyPrefix]
            public static bool Prefix(UseLoadoutAmunition __instance, ref bool __result)
            {
                var self = __instance;

                if (self.MainHand && self.OwnerCharacter.CurrentWeapon is ProjectileWeapon bow && IsManaBow(bow))
                {
                    __result = true;
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        [HarmonyPatch(typeof(UseLoadoutAmunition), "ActivateLocally")]
        public class UseLoadoutAmmunition_ActivateLocally
        {
            [HarmonyPrefix]
            public static bool Prefix(UseLoadoutAmunition __instance, Character _affectedCharacter, object[] _infos)
            {
                var self = __instance;

                if (self.MainHand && _affectedCharacter.CurrentWeapon is ProjectileWeapon bow && IsManaBow(bow))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        [HarmonyPatch(typeof(CharacterInventory), "PlayEquipSFX")]
        public class CharacterInventory_PlayEquipSFX
        {
            [HarmonyPrefix]
            public static bool Prefix(CharacterInventory __instance, Equipment _equipment)
            {
                if (_equipment.ItemID == ManaArrowID)
                {
                    return false;
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(CharacterInventory), "PlayUnequipSFX")]
        public class CharacterInventory_PlayUnequipSFX
        {
            [HarmonyPrefix]
            public static bool Prefix(CharacterInventory __instance, Equipment _equipment)
            {
                if (_equipment.ItemID == ManaArrowID)
                {
                    return false;
                }

                return true;
            }
        }
    }
}

