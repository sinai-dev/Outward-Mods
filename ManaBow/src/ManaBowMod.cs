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
    public class ManaBowMod : BaseUnityPlugin
    {
        public const string GUID = "com.sinai.manabow";
        public const string NAME = "Mana Bow";
        public const string VERSION = "2.0";

        internal void Awake()
        {
            SL.OnPacksLoaded += SetupSkills;

            SL.BeforePacksLoaded += ManaBowManager.SetupTag;
            SL.OnPacksLoaded += ManaBowManager.SetupManaArrow;

            var harmony = new Harmony(GUID);
            harmony.PatchAll();
        }

        // this patch fixes a bug with the game
        [HarmonyPatch(typeof(PlayerCharacterStats), "OnUpdateStats")]
        public class PlayerStats_Update
        {
            [HarmonyFinalizer]
            public static Exception Finalizer(PlayerCharacterStats __instance)
            {
                var character = __instance.GetComponent<Character>();

                if (!character)
                {
                    return null;
                }

                var charging = (bool)At.GetField(character, "m_currentlyChargingAttack");
                var bowDrawn = (bool)At.GetField(character, "m_bowDrawn");

                if (!charging || !bowDrawn)
                {
                    return null;
                }

                if (character.CurrentWeapon && character.CurrentWeapon.Type == Weapon.WeaponType.Bow)
                {
                    var charger = (WeaponCharger)character.CurrentWeapon.GetExtension(nameof(WeaponCharger));
                    if (!charger.ChargeStarted && charger.StartChargeTime < 0)
                    {
                        At.SetField(character, "m_currentlyChargingAttack", false);
                        At.SetField(character, "m_bowDrawn", false);
                    }
                }

                return null;
            }
        }

        private void SetupSkills()
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
    }
}
