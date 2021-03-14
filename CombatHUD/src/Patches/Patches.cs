using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using UnityEngine;
using SideLoader;

namespace CombatHUD
{
    public class HookUtil
    {
        public static bool IsElligable(Weapon weapon, Character owner, Character target)
        {
            return target && target != owner && (weapon.CanHitEveryoneButOwner || owner.TargetingSystem.IsTargetable(target));
        }
    }

    // todo AffectHealth, maybe falling

    [HarmonyPatch(typeof(Weapon), "HasHit")]
    public class Weapon_HasHit
    {
        [HarmonyPrefix]
        public static bool Prefix(Weapon __instance, RaycastHit _hit, Vector3 _dir)
        {
            Hitbox hitbox = _hit.collider?.GetComponent<Hitbox>();
            if (!hitbox)
                return true;

            var owner = __instance.OwnerCharacter;
            var target = hitbox.OwnerChar;

            if (!target || !owner)
                return true;

            var m_alreadyHitChars = (List<Character>)At.GetField(__instance, "m_alreadyHitChars");

            if (!m_alreadyHitChars.Contains(target) && HookUtil.IsElligable(__instance, owner, target))
            {
                bool blocked = false;
                float num = Vector3.Angle(hitbox.OwnerChar.transform.forward, owner.transform.position - hitbox.OwnerChar.transform.position);

                if (!__instance.Unblockable && hitbox.OwnerChar.Blocking && num < (float)(hitbox.OwnerChar.ShieldEquipped ? Weapon.SHIELD_BLOCK_ANGLE : Weapon.BLOCK_ANGLE))
                {
                    blocked = true;
                }
                if (!blocked)
                {
                    var attackID = (int)At.GetField(__instance, "m_attackID");
                    if (attackID >= 0)
                    {
                        DamageList damages = __instance.GetDamage(attackID).Clone();

                        //target.Stats.GetMitigatedDamage(null, ref damages, false);
                        At.Invoke(target, "ProcessDamageReduction", new object[] { __instance, damages, false });

                        DamageLabels.AddDamageLabel(damages, _hit.point, target);
                    }
                }
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(ProjectileWeapon), "HasHit")]
    public class ProjectileWeapon_HasHit
    {
        [HarmonyPrefix]
        public static bool Prefix(Weapon __instance, Character _hitCharacter, Vector3 _hitPos, Vector3 _dir, bool _blocked)
        {
            var selfChar = At.GetField(__instance, "m_ownerCharacter") as Character;
            var alreadyhit = At.GetField(__instance, "m_alreadyHitChars") as List<Character>;

            bool eligible = _hitCharacter && (_hitCharacter != selfChar) && (__instance.CanHitEveryoneButOwner || selfChar.TargetingSystem.IsTargetable(_hitCharacter));

            if (eligible && !alreadyhit.Contains(_hitCharacter))
            {
                if (!_blocked)
                {
                    DamageList damages = __instance.GetDamage(0);
                    //_hitCharacter.Stats.GetMitigatedDamage(null, ref damages, false);
                    At.Invoke(_hitCharacter, "ProcessDamageReduction", new object[] { __instance, damages, false });

                    DamageLabels.AddDamageLabel(damages, _hitPos, _hitCharacter);
                }
                else
                {
                    // Attack was blocked.
                }
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(PunctualDamage), "DealHit")]
    public class PunctualDamage_DealHit
    {
        [HarmonyPostfix]
        public static void Postfix(PunctualDamage __instance, Character _targetCharacter)
        {
            if (_targetCharacter.Alive)
            {
                bool ignoreBarrier = false;
                if (__instance.ParentSynchronizer is StatusEffect status)
                    ignoreBarrier = status.IgnoreBarrier;
                
                DamageList damages = (At.GetField(__instance, "m_tempList") as DamageList).Clone();

                At.Invoke(_targetCharacter, "ProcessDamageReduction", new object[] { __instance.ParentSynchronizer, damages, ignoreBarrier });

                // _targetCharacter.Stats.GetMitigatedDamage(null, ref damages, ignoreBarrier);

                DamageLabels.AddDamageLabel(damages, _targetCharacter.CenterPosition, _targetCharacter);
            }
        }
    }

    [HarmonyPatch(typeof(WeaponDamage), "DealHit")]
    public class WeaponDamage_DealHit
    {
        [HarmonyPostfix]
        public static void Postfix(PunctualDamage __instance, Character _targetCharacter)
        {
            if (_targetCharacter.Alive)
            {
                DamageList damages = (At.GetField(__instance, "m_tempList") as DamageList).Clone();

                Weapon weapon = At.GetField(__instance, "m_weapon") as Weapon;

                At.Invoke(_targetCharacter, "ProcessDamageReduction", new object[] { weapon, damages, false });

                // _targetCharacter.Stats.GetMitigatedDamage(null, ref damages, false);

                DamageLabels.AddDamageLabel(damages, _targetCharacter.CenterPosition, _targetCharacter);
            }
        }
    }
}
