using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Experimental.UIElements;

namespace PvP
{
    public class FriendlyFire
    {
        // Just a copy-paste of the original TargetingSystem.IsTargetable method.
        // For some reason, Harmony doesn't support a ReversePatch on this method.
        public static bool OrigIsTargetable(TargetingSystem instance, Character _character)
        {
            var _faction = _character.Faction;

            for (int i = instance.TargetableFactions.Length - 1; i >= 0; i--)
            {
                if (instance.TargetableFactions[i].CompareTo(_faction) == 0)
                {
                    return true;
                }
            }
            return false;
        }

        // this one is to fix an unintended bug
        [HarmonyPatch(typeof(Character), "IsEnemyClose")]
        public class Character_IsEnemyClose
        {
            [HarmonyPrefix]
            public static bool Prefix(ref bool __result, Character __instance, float _range)
            {
                __result = false;

                Collider[] array = Physics.OverlapSphere(__instance.CenterPosition, _range, Global.LockingPointsMask);
                for (int i = 0; i < array.Length; i++)
                {
                    LockingPoint component = array[i].GetComponent<LockingPoint>();
                    if (component && component.OwnerChar && component.OwnerChar.Alive && OrigIsTargetable(__instance.TargetingSystem, component.OwnerChar))
                    {
                        __result = true;
                    }
                }

                return false;
            }
        }

        // Actual needed patch on IsTargetable
        [HarmonyPatch(typeof(TargetingSystem), "IsTargetable", new Type[] { typeof(Character) })]
        public class TargetingSys_IsTargetable
        {
            [HarmonyPrefix]
            public static bool Prefix(ref bool __result, Character ___m_character, Character _char)
            {
                if (___m_character.IsAI || !PvP.Instance.FriendlyFireEnabled)
                {
                    return true;
                }

                if (___m_character.UID != _char.UID)
                {
                    __result = true;
                }
                else
                {
                    __result = false;
                }

                return false;
            }
        }

        // Actual needed patch on IsTargetable
        [HarmonyPatch(typeof(TargetingSystem), "IsTargetable", new Type[] { typeof(Character.Factions) })]
        public class TargetingSys_IsTargetable_2
        {
            [HarmonyPrefix]
            public static bool Prefix(ref bool __result, Character ___m_character)
            {
                if (___m_character.IsAI || !PvP.Instance.FriendlyFireEnabled)
                {
                    return true;
                }

                __result = true;

                return false;
            }
        }

        // Probably should be a transpiler, literally just changing one method call, everything else is copy paste.
        [HarmonyPatch(typeof(TargetingSystem), "AcquireTarget")]
        public class TargetingSys_AcquireTarget
        {
            [HarmonyPrefix]
            public static bool Prefix(TargetingSystem __instance, ref bool __result, Character ___m_character, ref LockingPoint ___m_currentLockingPoint,
                ref int ___m_remainingHelpLockCount)
            {
                if (!___m_character || ___m_character.IsAI || !PvP.Instance.FriendlyFireEnabled || (PvP.Instance.FriendlyFireEnabled && PvP.Instance.FriendlyTargetingEnabled))
                {
                    return true;
                }

                if (__instance.CameraRef != null)
                {
                    Collider[] array = Physics.OverlapSphere(___m_character.CenterPosition, __instance.TrueRange, Global.LockingPointsMask);
                    LockingPoint lockingPoint = null;
                    Vector3 vector = Vector2.zero;
                    Vector3 vector2;
                    Matrix4x4 worldToCameraMatrix = __instance.CameraRef.worldToCameraMatrix;
                    foreach (Collider collider in array)
                    {
                        vector2 = worldToCameraMatrix.MultiplyPoint(collider.transform.position);
                        if (vector2.z < 0f)
                        {
                            vector2.z *= 0.1f;
                            if (lockingPoint == null || vector2.magnitude < vector.magnitude)
                            {
                                LockingPoint component = collider.GetComponent<LockingPoint>();
                                if (component.OwnerChar == null
                                    // This is the only thing I'm changing, using the unmodified IsTargetable instead of the patched one.
                                    || (OrigIsTargetable(__instance, component.OwnerChar) // __instance.IsTargetable(component.OwnerChar) 
                                        && component.OwnerChar.Alive
                                        && !Physics.Linecast(___m_character.CenterPosition, collider.transform.position, Global.SightHideMask)))
                                {
                                    lockingPoint = component;
                                    vector = vector2;
                                }
                            }
                        }
                    }

                    if (lockingPoint != null)
                    {
                        ___m_currentLockingPoint = lockingPoint;

                        if (___m_remainingHelpLockCount > 0)
                        {
                            ___m_remainingHelpLockCount--;
                        }

                        __result = true;
                        return false;
                    }
                }

                __result = false;
                return false;
            }
        }

        // Again, another big copy+paste that should probably be a transpiler.
        // This one has a few more changes, just removed all the IsTargetable checks for players.
        [HarmonyPatch(typeof(Blast), "Hit")]
        public class Blast_Hit
        {
            [HarmonyPrefix]
            public static bool Prefix(Blast __instance, ref List<Hitbox> ___cachedHitBox)
            {
                if (!__instance.OwnerCharacter || __instance.OwnerCharacter.IsAI || !PvP.Instance.FriendlyFireEnabled)
                {
                    return true;
                }

                Collider[] array = Physics.OverlapSphere(__instance.transform.position, __instance.Radius, Global.WeaponHittingMask);

                List<Character> list = new List<Character>();
                ___cachedHitBox.Clear();

                for (int i = 0; i < array.Length; i++)
                {
                    Hitbox component = array[i].GetComponent<Hitbox>();
                    if (component != null
                        && component.OwnerChar != null
                        && component.OwnerChar.UID != __instance.OwnerCharacter.UID
                        && !component.BlockBox
                        && !list.Contains(component.OwnerChar))
                        //&& __instance.m_targetableFactions.Contains(component.OwnerChar.Faction) 
                        //&& (!__instance.IgnoreShooter || component.OwnerChar != __instance.OwnerCharacter))
                    {
                        ___cachedHitBox.Add(component);
                        list.Add(component.OwnerChar);
                    }
                }

                //__instance.AffectHit(___cachedHitBox);
                At.Call(typeof(Blast), __instance, "AffectHit", null, ___cachedHitBox);

                return false;
            }
        }

        // And the third big copy+paste that could be a transpiler.
        // Same as the Blast.Hit changes.
        [HarmonyPatch(typeof(Projectile), "Explode", new Type[] { typeof(Collider), typeof(Vector3), typeof(Vector3) })]
        public class Projectile_Explode
        {
            [HarmonyPrefix]
            public static bool Prefix(Projectile __instance, Collider _collider, Vector3 _hitPoint, Vector3 _hitDir,
                ref float ___m_lastHitTimer, ref float ___m_targetLightIntensity, float ___m_lightStartIntensity, ref float ___m_lightIntensityFadeSpeed,
                SoundPlayer ___m_shootSound, SoundPlayer ___m_travelSound,
                ParticleSystem ___m_explosionFX, ref object[] ___m_explodeInfos)
            {
                if (!__instance.OwnerCharacter || __instance.OwnerCharacter.IsAI || !PvP.Instance.FriendlyFireEnabled)
                {
                    Debug.Log("projectile hit, returning orig");
                    return true;
                }

                ___m_lastHitTimer = 5f;
                ___m_targetLightIntensity = ___m_lightStartIntensity * 1.8f;
                ___m_lightIntensityFadeSpeed = __instance.LightIntensityFade.x * 0.5f;

                if (___m_shootSound)
                {
                    ___m_shootSound.Stop(false);
                }
                if (___m_travelSound)
                {
                    ___m_travelSound.Stop(false);
                }

                Character character = null;
                if (_collider != null)
                {
                    if (___m_explosionFX)
                    {
                        ___m_explosionFX.Play();
                    }
                    character = _collider.GetCharacterOwner();
                }

                bool blocked = false;
                if (character && __instance.OwnerCharacter != character)
                {
                    if (!__instance.Unblockable && character.ShieldEquipped)
                    {
                        Hitbox component = _collider.GetComponent<Hitbox>();
                        if (component != null && component.BlockBox && character.Blocking && !character.Countering)
                        {
                            blocked = true;
                        }
                    }
                }
                else
                {
                    character = null;
                }

                Debug.Log(character?.Name ?? "Null projectile hit");

                // __instance.OnProjectileHit(character, _hitPoint, _hitDir, blocked);
                At.Call(typeof(Projectile), __instance, "OnProjectileHit", null, new object[] { character, _hitPoint, _hitDir, blocked });

                ___m_explodeInfos[0] = character;
                ___m_explodeInfos[1] = _hitPoint;
                ___m_explodeInfos[2] = _hitDir;
                ___m_explodeInfos[3] = (_collider != null);

                __instance.SendMessage("OnExplodeDone", ___m_explodeInfos, SendMessageOptions.DontRequireReceiver);

                if (__instance.EndMode == Projectile.EndLifeMode.Normal
                    || (__instance.EndMode == Projectile.EndLifeMode.EnvironmentOnly
                        && (_collider == null || Global.FullEnvironmentMask == (Global.FullEnvironmentMask | 1 << _collider.gameObject.layer))))
                {
                    // __instance.EndLife();
                    At.Call(typeof(Projectile), __instance, "EndLife", null);
                }

                return false;
            }
        }

        [HarmonyPatch(typeof(RaycastProjectile), "CheckCollision")]
        public class RaycastProjectile_CheckCollision
        {
            [HarmonyPrefix]
            public static bool Prefix(ref bool __result, RaycastProjectile __instance, Vector3 _startRaycast, float dist, ref RaycastHit _hit,
                ref Vector3 ___m_shootDir, float ___m_radiusAdd, float ___m_capsuleAdd, LockingPoint ___m_homingTarget,
                List<Character> ___m_hitCharList)
            {
                var self = __instance;

                if (!PvP.Instance.FriendlyFireEnabled || self.OwnerCharacter || self.OwnerCharacter.IsAI)
                {
                    return true;
                }

                _hit = default;
                RaycastHit[] array;

                int hitMask = RaycastHitLayerMask(self);

                if (self.Radius == 0f)
                {
                    array = Physics.RaycastAll(_startRaycast, ___m_shootDir, dist, hitMask);
                }
                else if (self.Capsule != 0f)
                {
                    array = Physics.SphereCastAll(_startRaycast, self.Radius + ___m_radiusAdd, ___m_shootDir, dist, hitMask);
                }
                else
                {
                    array = Physics.CapsuleCastAll(
                        _startRaycast - self.transform.right * (self.Capsule + ___m_capsuleAdd), 
                        _startRaycast + self.transform.right * (self.Capsule + ___m_capsuleAdd), 
                        self.Radius + ___m_radiusAdd, 
                        ___m_shootDir, 
                        dist, 
                        hitMask
                    );
                }
                if (array.Length != 0)
                {
                    var ignoredChar = (Character)At.GetValue(typeof(Projectile), __instance, "m_ignoredCharacter");

                    int num = -1;
                    for (int i = 0; i < array.Length; i++)
                    {
                        Character hitChar = array[i].collider.GetCharacterOwner();

                        // Added: check that we are not hitting ourselves
                        if (self.OwnerCharacter.UID != hitChar.UID)
                        {
                            // This is the same check that the game does, just broken up so it's actually readable.
                            bool valid = hitChar == null && !self.HitTargetOnly;

                            if (!valid)
                            {
                                valid = !self.HitTargetOnly || (___m_homingTarget && hitChar == ___m_homingTarget.OwnerChar);
                                valid &= hitChar != ignoredChar;
                                valid &= !___m_hitCharList.Contains(hitChar) || (!self.MultiTarget && self.EndMode == Projectile.EndLifeMode.Normal);
                                // removed: targetable check
                            }

                            if (valid)
                            {
                                if (array[i].point == Vector3.zero)
                                {
                                    array[i].point = self.transform.position;
                                }
                                if (num == -1 || (_startRaycast - array[i].point).sqrMagnitude < (_startRaycast - array[num].point).sqrMagnitude)
                                {
                                    if (hitChar)
                                    {
                                        ___m_hitCharList.Add(hitChar);
                                    }
                                    num = i;
                                }
                            }
                        }
                    }
                    if (num != -1)
                    {
                        _hit = array[num];
                        __result = true;
                        return false;
                    }
                }

                __result = false;
                return false;
            }

            private static int RaycastHitLayerMask(RaycastProjectile __instance)
            {
                if (__instance.OnlyExplodeOnLayers)
                {
                    return __instance.ExplodeOnContactWithLayers;
                }
                if (__instance.HitLayer == RaycastProjectile.HitLayers.Both)
                {
                    return Global.ProjectileHitAllMask;
                }
                if (__instance.HitLayer == RaycastProjectile.HitLayers.MeleeHitBox)
                {
                    return Global.ProjectileHitMeleeMask;
                }
                return Global.ProjectileHitRangedMask;
            }
        }
    }
}
