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

    [HarmonyPatch(typeof(Weapon), "HasHit")]
    public class Weapon_HasHit
    {
        [HarmonyPrefix]
        public static bool Prefix(Weapon __instance, RaycastHit _hit, Vector3 _dir)
        {
            Hitbox hitbox = _hit.collider.GetComponent<Hitbox>();
            var owner = __instance.OwnerCharacter;
            var target = hitbox.OwnerChar;
            var m_alreadyHitChars = At.GetValue(typeof(Weapon), __instance, "m_alreadyHitChars") as List<Character>;

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
                    var getID = At.GetValue(typeof(Weapon), __instance, "m_attackID");
                    if (getID is int attackID && attackID >= 0)
                    {
                        DamageList damages = __instance.GetDamage(attackID).Clone();

                        target.Stats.GetMitigatedDamage(null, ref damages);

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
            Character selfChar = At.GetValue(typeof(Item), __instance as Item, "m_ownerCharacter") as Character;

            if (At.GetValue(typeof(Weapon), __instance as Weapon, "m_alreadyHitChars") is List<Character> alreadyhit)
            {
                bool eligible = _hitCharacter && (_hitCharacter != selfChar) && (__instance.CanHitEveryoneButOwner || selfChar.TargetingSystem.IsTargetable(_hitCharacter));

                if (eligible && !alreadyhit.Contains((Character)_hitCharacter))
                {
                    if (!_blocked)
                    {
                        DamageList damages = __instance.GetDamage(0);
                        _hitCharacter.Stats.GetMitigatedDamage(null, ref damages);

                        DamageLabels.AddDamageLabel(damages, _hitPos, _hitCharacter);
                    }
                    else
                    {
                        // Attack was blocked.
                    }
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
                PunctualDamage punctualSelf = __instance as PunctualDamage;

                if (At.GetValue(typeof(PunctualDamage), punctualSelf, "m_tempList") is DamageList damagelist)
                {
                    DamageList damages = damagelist.Clone();
                    _targetCharacter.Stats.GetMitigatedDamage(null, ref damages);

                    DamageLabels.AddDamageLabel(damages, _targetCharacter.CenterPosition, _targetCharacter);
                }
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
                if (At.GetValue(typeof(PunctualDamage), __instance, "m_tempList") is DamageList damagelist)
                {
                    DamageList damages = damagelist.Clone();
                    _targetCharacter.Stats.GetMitigatedDamage(null, ref damages);

                    DamageLabels.AddDamageLabel(damages, _targetCharacter.CenterPosition, _targetCharacter);
                }
            }
        }
    }
}
