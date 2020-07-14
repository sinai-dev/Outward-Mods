using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using SideLoader;
using BepInEx;
using HarmonyLib;

namespace ImbuedBows
{
    [BepInPlugin(GUID, NAME, VERSION)]
    [BepInDependency("com.sinai.SideLoader", BepInDependency.DependencyFlags.HardDependency)]
    public class ImbuedBows : BaseUnityPlugin
    {
        public const string GUID = "com.sinai.imbuedbows";
        public const string NAME = "Imbued Bows & Mana Bow";
        public const string VERSION = "1.5";

        internal void Awake()
        {
            SL.OnPacksLoaded += Setup;

            this.gameObject.AddComponent<ManaBow>();

            var harmony = new Harmony(GUID);
            harmony.PatchAll();
        }

        private void Setup()
        {
            // yield the OnPacksLoaded call (and wait a second just in case) so we can be sure other mods have loaded.
            StartCoroutine(LateFix());
        }

        private IEnumerator LateFix()
        {
            yield return new WaitForSeconds(1f);

            // =============== setup infuse skills and discharge ===============
            var list = new List<int>
            {
                8200100, // infuse light
                8200101, // infuse wind
                8200102, // infuse frost
                8200103, // infuse fire
                2502001, // infuse burst of light (templar)

                8200310, // elemental discharge
            };

            foreach (int id in list)
            {
                if (ResourcesPrefabManager.Instance.GetItemPrefab(id) is AttackSkill skill)
                {
                    skill.RequiredWeaponTypes.Add(Weapon.WeaponType.Bow);
                }
            }
        }

        [HarmonyPatch(typeof(InfuseConsumable), "Use")]
        public class InfuseConsumable_Use
        {
            [HarmonyPostfix]
            public static void Prefix(InfuseConsumable __instance, Character _character, ref bool __result)
            {
                __result = false;

                var self = __instance;

                if (_character != null)
                {
                    if (_character.CurrentWeapon != null) // && _character.CurrentWeapon.Type != Weapon.WeaponType.Bow)
                    {
                        if (self.m_UseSound)
                        {
                            self.m_UseSound.Play(false);
                        }

                        //self.m_characterUsing = _character;
                        At.SetValue(_character, typeof(Item), self as Item, "m_characterUsing");

                        if (self.ActivateEffectAnimType == Character.SpellCastType.NONE)
                        {
                            _character.Inventory.OnUseItem(self.UID);
                        }
                        else
                        {
                            //self.StartEffectsCast(_character);
                            At.Call(self as Item, "StartEffectsCast", new object[] { _character });
                        }
                        __result = true;
                    }
                    if (_character.CharacterUI)
                    {
                        _character.CharacterUI.ShowInfoNotificationLoc("Notification_Item_InfuseNotCompatible");
                    }
                }
            }
        }

        [HarmonyPatch(typeof(ItemVisual), "AddImbueFX")]
        public class ItemVisual_AddImbueFX
        {
            [HarmonyPrefix]
            public static bool Prefix(ItemVisual __instance, ImbueStack newStack, Weapon _linkedWeapon)
            {   
                var self = __instance;

                // if its not a bow, just do orig(self) and return
                if (_linkedWeapon.Type != Weapon.WeaponType.Bow)
                {
                    return true;
                }

                //newStack.ImbueFX = ItemManager.Instance.GetImbuedFX(newStack.ImbuedEffectPrefab);
                //if (!newStack.ImbueFX.gameObject.activeSelf)
                //{
                //    newStack.ImbueFX.gameObject.SetActive(true);
                //}
                //newStack.ParticleSystems = newStack.ImbueFX.GetComponentsInChildren<ParticleSystem>();

                //if (self.GetComponentInChildren<SkinnedMeshRenderer>() is SkinnedMeshRenderer skinnedMesh)
                //{
                //    for (int j = 0; j < newStack.ParticleSystems.Length; j++)
                //    {
                //        Debug.Log("Setting shape type and skinned mesh");
                //        var shape = newStack.ParticleSystems[j].shape;
                //        shape.shapeType = ParticleSystemShapeType.SkinnedMeshRenderer;
                //        shape.skinnedMeshRenderer = skinnedMesh;
                //        newStack.ParticleSystems[j].Play();
                //    }
                //}

                //newStack.ImbueFX.SetParent(self.transform);
                //newStack.ImbueFX.ResetLocal(true);
                //if (At.GetValue(typeof(ItemVisual), self, "m_linkedImbueFX") is List<ImbueStack> m_linkedImbues)
                //{
                //    m_linkedImbues.Add(newStack);
                //    At.SetValue(m_linkedImbues, typeof(ItemVisual), self, "m_linkedImbueFX");
                //}

                return false;
            }
        }

    }
}
