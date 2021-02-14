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
    public class ManaBowManager
    {
        public static Tag ManaBowTag;
        private const string ManaBowTagString = "ManaBow";

        public const float ManaBowCost = 5f;
        public const float ManaBowHoldCost = 0.1f;

        public const int ManaArrowID = 2800910;

        // ======= harmony patch fixes ==========

        // fix a small null ref error
        [HarmonyPatch(typeof(ProjectileItem), "AssignVisual")]
        public class ProjectileItem_AssignVisual
        {
            [HarmonyPrefix]
            public static void Prefix(ref Projectile ___m_projectile, ProjectileItem __instance)
            {
                ___m_projectile = __instance.GetComponent<Projectile>();
            }
        }

        // fix a null dictionary on init
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

        // fix a bug with drawing the mana bow
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
                        At.SetField(owner, "m_currentlyChargingAttack", false);
                        owner.BowRelease();

                        var charger = (WeaponCharger)__instance.GetExtension("WeaponCharger");
                        charger.ResetCharging();
                    }
                }
            }
        }

        // fix changing scenes while an arrow is mid-flight
        [HarmonyPatch(typeof(ProjectileItem), "ForceReturn")]
        public class ProjectileItem_ForceReturn
        {
            [HarmonyFinalizer]
            public static Exception Finalizer()
            {
                return null;
            }
        }

        // not working yet

        //// fix effects not registering from mana arrows
        //[HarmonyPatch(typeof(WeaponLoadoutItem), "ShootItemFromLoadout")]
        //public class WeaponLoadoutItem_ShootItemFromLoadout
        //{
        //    [HarmonyPostfix]
        //    public static void Postfix(WeaponLoadoutItem __instance, ProjectileItem _projItem)
        //    {
        //        var ammo = (Ammunition)At.GetValue(typeof(WeaponLoadoutItem), __instance, "m_lastLoadedAmmunition");

        //        if (ammo.ItemID != ManaArrowID)
        //        {
        //            return;
        //        }

        //        var raycast = _projItem.GetComponent<RaycastProjectile>();
        //        var imbued = (List<Effect>)At.GetValue(typeof(ProjectileItem), _projItem, "m_imbuedEffects");

        //        if (raycast && imbued != null)
        //        {
        //            var dict = (IDictionary)At.GetValue(typeof(EffectSynchronizer), raycast, "m_effects");
        //            dict.Clear();

        //            foreach (var effect in imbued)
        //            {
        //                if ((bool)At.Call(typeof(EffectSynchronizer), raycast, "IsEffectAlreadyRegistered", null, new object[] { effect }))
        //                {
        //                    Debug.LogWarning("Effect is already registered");
        //                }
        //                else
        //                {
        //                    var data = raycast.RegisterEffect(effect);

        //                    if (data == null)
        //                    {
        //                        Debug.LogWarning("null data!");
        //                    }
        //                    else
        //                    {
        //                        Debug.Log("Registered effect of type " + effect.GetType().Name);
        //                    }
        //                }
        //            }
        //        }

        //        Debug.LogWarning("-- registered custom effects to raycast on mana arrow --");
        //    }
        //}

        // ============= methods ==================

        public static bool IsManaBow(Item item)
        {
            return item.HasTag(ManaBowTag);
        }

        public static void SetupTag()
        {
            ManaBowTag = CustomTags.CreateTag(ManaBowTagString);
        }

        public static void SetupManaArrow()
        {
            Debug.Log("Setting up mana arrow");

            // setup custom mana projectile
            var manaArrow = ResourcesPrefabManager.Instance.GetItemPrefab(ManaArrowID) as Ammunition;

            // set empty equipped visuals (hide quiver)
            var vLink = CustomItemVisuals.GetOrAddVisualLink(manaArrow);
            vLink.ItemSpecialVisuals = new GameObject("ManaQuiverDummy").transform;
            GameObject.DontDestroyOnLoad(vLink.ItemSpecialVisuals.gameObject);

            // custom arrow ProjectileItem component (determines the ammunition behaviour as projectile)
            var origProjFX = manaArrow.ProjectileFXPrefab.gameObject;
            origProjFX.SetActive(false);
            manaArrow.ProjectileFXPrefab = GameObject.Instantiate(origProjFX).transform;
            origProjFX.SetActive(true);

            GameObject.DontDestroyOnLoad(manaArrow.ProjectileFXPrefab.gameObject);

            var projItem = manaArrow.ProjectileFXPrefab.GetComponent<ProjectileItem>();
            projItem.CollisionBehavior = ProjectileItem.CollisionBehaviorTypes.Destroyed;
            projItem.EphemeralProjectile = true;

            At.SetField(projItem, "m_itemID", ManaArrowID);
            At.SetField(projItem, "m_item", ResourcesPrefabManager.Instance.GetItemPrefab(ManaArrowID));

            var raycast = projItem.GetComponent<RaycastProjectile>();
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
                    __result = CanBeLoaded(item);
                    return false;
                }

                return true;
            }

            public static bool CanBeLoaded(Item item)
            {
                var owner = item.OwnerCharacter;

                if (!owner)
                {
                    Debug.Log("Mana bow has no owner!");
                    return false;
                }

                float currentMana = owner.Stats.CurrentMana;
                float manaCost = owner.Stats.GetFinalManaConsumption(null, ManaBowCost);
                if (currentMana - manaCost >= 0)
                {
                    if (!owner.Inventory.HasEquipped(ManaArrowID))
                    {
                        Ammunition ammo;
                        if (!owner.Inventory.OwnsItem(ManaArrowID))
                        {
                            // need to generate one
                            ammo = ItemManager.Instance.GenerateItemNetwork(ManaArrowID) as Ammunition;
                        }
                        else
                        {
                            // we own one, but not equipped
                            ammo = (Ammunition)owner.Inventory.GetOwnedItems(ManaArrowID)[0];
                        }

                        // make sure the arrow is in the right slot
                        ammo.ChangeParent(owner.Inventory.Equipment.GetMatchingEquipmentSlotTransform(EquipmentSlot.EquipmentSlotIDs.Quiver));
                    }

                    return true;
                }
                else
                {
                    owner.CharacterUI.ShowInfoNotificationLoc("Notification_Skill_NotEnoughtMana");
                    return false;
                }
            }
        }

        [HarmonyPatch(typeof(WeaponLoadout), "ReduceShotAmount")]
        public class WeaponLoadout_ReduceShotAmount
        {
            [HarmonyPrefix]
            public static bool Prefix(WeaponLoadout __instance)
            {
                var item = __instance.Item;

                if (IsManaBow(item))
                {
                    // custom mana cost
                    float manaCost = item.OwnerCharacter.Stats.GetFinalManaConsumption(null, ManaBowCost);
                    item.OwnerCharacter.Stats.UseMana(null, manaCost);

                    // (vanilla) update weapon loadout remaining shots
                    int remaining = (int)At.GetField(__instance, "m_remainingShots");
                    remaining--;
                    At.SetField(__instance, "m_remainingShots", remaining);

                    if (remaining < 0)
                    {
                        remaining = 0;
                    }
                    if (remaining == 0)
                    {
                        __instance.Unload();
                    }

                    // unequip the arrow
                    var ammo = __instance.Item.OwnerCharacter.Inventory.Equipment.GetEquippedItem(EquipmentSlot.EquipmentSlotIDs.Quiver) as Ammunition;
                    __instance.Item.OwnerCharacter.Inventory.Equipment.UnequipItem(ammo, false, null);
                    ammo.transform.SetParent(null);
                    ammo.transform.ResetLocal();

                    // vanilla method
                    At.Invoke(__instance, "SetHasChanged");

                    return false;
                }

                return true;
            }
        }

        //[HarmonyPatch(typeof(CharacterEquipment), "GetEquippedAmmunition")]
        //public class CharacterEquipment_GetEquippedAmmunition
        //{
        //    [HarmonyPrefix]
        //    public static bool Prefix(CharacterEquipment __instance, ref Ammunition __result)
        //    {
        //        //var self = __instance;

        //        //if (self.GetEquippedItem(EquipmentSlot.EquipmentSlotIDs.RightHand) is Weapon weapon && IsManaBow(weapon))
        //        //{
        //        //    var character = At.GetField(self, "m_character") as Character;

        //        //    Ammunition ammo;

        //        //    if (character.Inventory.HasEquipped(ManaArrowID))
        //        //    {
        //        //        // we all good
        //        //        SL.LogWarning("Mana arrow already equipped in quiver");
        //        //        __result = self.GetEquippedItem(EquipmentSlot.EquipmentSlotIDs.Quiver) as Ammunition;
        //        //        return false;
        //        //    }
        //        //    else // we dont have mana arrow equipped
        //        //    {
        //        //        if (!character.Inventory.OwnsItem(ManaArrowID))
        //        //        {
        //        //            SL.LogWarning("Instantiating new arrow item");
        //        //            // need to generate one
        //        //            ammo = ItemManager.Instance.GenerateItemNetwork(ManaArrowID) as Ammunition;
        //        //        }
        //        //        else
        //        //        {
        //        //            SL.LogWarning("Owned but not equipped..");
        //        //            // we own one, but not equipped
        //        //            ammo = (Ammunition)character.Inventory.GetOwnedItems(ManaArrowID)[0];
        //        //        }
        //        //    }

        //        //    SL.LogWarning("Changing parent to quiver");
        //        //    // make sure the arrow is in the right slot
        //        //    ammo.ChangeParent(self.GetMatchingEquipmentSlotTransform(EquipmentSlot.EquipmentSlotIDs.Quiver));

        //        //    __result = ammo;
        //        //    return false;
        //        //}

        //        // don't have mana bow equipped
        //        return true;
        //    }
        //}

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
                    __result = WeaponLoadout_CanBeLoaded.CanBeLoaded(bow);
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
                    WeaponLoadout_ReduceShotAmount.Prefix(bow.GetComponent<WeaponLoadout>());
                    return false;
                }

                return true;
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

