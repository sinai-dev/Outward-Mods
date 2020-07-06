using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using UnityEngine;


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

        // Actual needed patch on IsTargetable
        [HarmonyPatch(typeof(TargetingSystem), "IsTargetable", new Type[] { typeof(Character) })]
        public class TargetingSys_IsTargetable
        {
            public static bool Prefix(ref bool __result, Character ___m_character, Character _char)
            {
                if (___m_character.IsAI || !PvP.Instance.FriendlyFireEnabled)
                {
                    return true;
                }

                // just to avoid confusion
                var targetter = ___m_character;

                if (targetter.UID == _char.UID)
                {
                    __result = false;
                }
                else if (!targetter.IsAI)
                {
                    __result = true;
                }

                return false;
            }
        }

        // Actual needed patch on IsTargetable
        [HarmonyPatch(typeof(TargetingSystem), "IsTargetable", new Type[] { typeof(Character.Factions) })]
        public class TargetingSys_IsTargetable_2
        {
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
                        //&& __instance.m_targetableFactions.Contains(component.OwnerChar.Faction) 
                        && !component.BlockBox
                        && !list.Contains(component.OwnerChar)
                        && (!__instance.IgnoreShooter || component.OwnerChar != __instance.OwnerCharacter))
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
                if (character != null)
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
    }
}
