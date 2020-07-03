using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using HarmonyLib;

namespace ImbuedBows
{
    public class ManaBow : MonoBehaviour
    {
        public static readonly int ManaBowID = 2800900;
        public static readonly int ManaArrowID = 2800901;
        public static readonly float ManaBowCost = 5f;

        internal void Awake()
        {
            SideLoader.SL.OnPacksLoaded += Setup;
        }

        //setup after sideloader init is done
        private void Setup()
        {
            Debug.Log("Setting up mana bow");

            SkinnedMeshRenderer skinnedMesh = null;

            // setup bow
            var bow = ResourcesPrefabManager.Instance.GetItemPrefab(ManaBowID) as ProjectileWeapon;

            if (bow != null && bow.GetItemVisual() is Transform bowVisuals)
            {
                skinnedMesh = bowVisuals.GetComponentInChildren<SkinnedMeshRenderer>();
                if (skinnedMesh)
                {
                    skinnedMesh.material.color = new Color(0.5f, 0.8f, 2.0f);
                }

                var light = bowVisuals.gameObject.AddComponent<Light>();
                light.color = new Color(0.3f, 0.7f, 0.9f);
                light.intensity = 1.5f;
                light.range = 1.3f;
            }

            var etherealImbue = ResourcesPrefabManager.Instance.GetEffectPreset(208);

            var fx = etherealImbue.GetComponent<ImbueEffectPreset>().ImbueFX;

            var newFX = Instantiate(fx.gameObject);
            DontDestroyOnLoad(newFX.gameObject);
            newFX.transform.parent = bow.GetItemVisual();

            foreach (var ps in newFX.GetComponentsInChildren<ParticleSystem>())
            {
                var shape = ps.shape;
                shape.shapeType = ParticleSystemShapeType.SkinnedMeshRenderer;
                shape.skinnedMeshRenderer = skinnedMesh;

                var main = ps.main;
                main.startColor = new Color(0.1f, 0.4f, 0.95f);
            }

            // setup custom mana projectile
            var manaArrow = ResourcesPrefabManager.Instance.GetItemPrefab(ManaArrowID) as Ammunition;

            // manaArrow.IsPickable = false;

            // custom arrow ProjectileItem component (determines the ammunition behaviour as projectile)
            var origObj = manaArrow.ProjectileFXPrefab.gameObject;
            origObj.SetActive(false);
            var newObj = Instantiate(origObj);
            origObj.SetActive(true);
            DontDestroyOnLoad(newObj);
            var projBehaviour = newObj.GetComponent<ProjectileItem>();
            projBehaviour.CollisionBehavior = ProjectileItem.CollisionBehaviorTypes.Destroyed;

            // custom arrow visuals
            var arrowVisuals = manaArrow.GetItemVisual();
            foreach (MeshRenderer mesh in arrowVisuals.GetComponentsInChildren<MeshRenderer>())
            {
                if (mesh.GetComponent<BoxCollider>())
                {
                    mesh.material.color = new Color(0.3f, 0.8f, 1.2f);

                    var light = mesh.gameObject.AddComponent<Light>();
                    light.color = new Color(0.3f, 0.7f, 0.9f);
                    light.intensity = 1.2f;
                    light.range = 0.5f;

                    break;
                }
            }
        }

        [HarmonyPatch(typeof(WeaponLoadout), "CanBeLoaded")]
        public class WeaponLoadout_CanBeLoaded
        {
            [HarmonyPrefix]
            public static bool Prefix(WeaponLoadout __instance, ref bool __result)
            {
                var self = __instance;
                var item = self.Item;

                if (item.ItemID == ManaBowID)
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
                var self = __instance;

                if (self.Item.ItemID == ManaBowID)
                {
                    float manaCost = self.Item.OwnerCharacter.Stats.GetFinalManaConsumption(null, ManaBowCost);
                    self.Item.OwnerCharacter.Stats.UseMana(null, manaCost);
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

                if (self.GetEquippedItem(EquipmentSlot.EquipmentSlotIDs.RightHand) is Weapon weapon && weapon.ItemID == ManaBowID)
                {
                    var character = At.GetValue(typeof(CharacterEquipment), self, "m_character") as Character;

                    if (!character.Inventory.HasEquipped(ManaArrowID))
                    {
                        if (!character.Inventory.OwnsItem(ManaArrowID))
                        {
                            var newAmmo = ItemManager.Instance.GenerateItemNetwork(ManaArrowID) as Ammunition;
                            newAmmo.ChangeParent(self.GetMatchingEquipmentSlotTransform(EquipmentSlot.EquipmentSlotIDs.Quiver));
                            __result = newAmmo;
                            return false;
                        }
                        else
                        {
                            var ammoFromID = character.Inventory.GetOwnedItems(ManaArrowID);
                            ammoFromID[0].ChangeParent(self.GetMatchingEquipmentSlotTransform(EquipmentSlot.EquipmentSlotIDs.Quiver));
                            __result = ammoFromID[0] as Ammunition;
                            return false;
                        }
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
                var self = __instance;

                if (self.OwnerCharacter && self.OwnerCharacter.CurrentWeapon is ProjectileWeapon bow && bow.ItemID == ManaBowID)
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

        [HarmonyPatch(typeof(UseLoadoutAmunition), "TryTriggerConditions")]
        public class UseLoadoutAmmunition_TryTriggerConditions
        {
            [HarmonyPrefix]
            public static bool Prefix(UseLoadoutAmunition __instance, ref bool __result)
            {
                var self = __instance;

                var affectedChar = At.GetValue(typeof(Effect), self as Effect, "m_affectedCharacter") as Character;

                if (self.MainHand && affectedChar.CurrentWeapon is ProjectileWeapon bow && bow.ItemID == ManaBowID)
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

                if (self.MainHand && _affectedCharacter.CurrentWeapon is ProjectileWeapon bow && bow.ItemID == ManaBowID)
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

