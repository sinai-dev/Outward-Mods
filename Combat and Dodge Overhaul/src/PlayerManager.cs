using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Reflection;
using HarmonyLib;
using SideLoader;

namespace CombatAndDodgeOverhaul
{
    public class PlayerManager : MonoBehaviour
    {
        public static PlayerManager Instance;

        private Dictionary<string, float> PlayerLastHitTimes = new Dictionary<string, float>();

        internal void Awake()
        {
            Instance = this;
        }

        [HarmonyPatch(typeof(Character), "HasHit")]
        public class Character_HasHit
        {
            [HarmonyPrefix]
            public static bool Prefix(Character __instance, Weapon _weapon, float _damage, Vector3 _hitDir, Vector3 _hitPoint, float _angle, bool _blocked, Character _target, float _knockback, int _attackID = -999)
            {
                var self = __instance;

                if (Instance.PlayerLastHitTimes.ContainsKey(self.UID))
                {
                    Instance.PlayerLastHitTimes[self.UID] = Time.time;
                }
                else
                {
                    Instance.PlayerLastHitTimes.Add(self.UID, Time.time);
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(Character), "DodgeInput", new Type[] { typeof(Vector3) })]
        public class Character_DodgeInput
        {
            [HarmonyPrefix]
            public static bool Prefix(Character __instance, Vector3 _direction)
            {
                var self = __instance;

                // only use this hook for local players. return orig everything else
                if (self.IsAI || !self.IsPhotonPlayerLocal)
                {
                    return true;
                }

                float staminaCost = (float)CombatOverhaul.config.GetValue(Settings.Custom_Dodge_Cost);

                // if dodge cancelling is NOT enabled, just do a normal dodge check.
                if (!(bool)CombatOverhaul.config.GetValue(Settings.Dodge_Cancelling))
                {
                    if (At.GetField(self, "m_currentlyChargingAttack") is bool m_currentlyChargingAttack
                       && At.GetField(self, "m_preparingToSleep") is bool m_preparingToSleep
                       && At.GetField(self, "m_nextIsLocomotion") is bool m_nextIsLocomotion
                       && At.GetField(self, "m_dodgeAllowedInAction") is int m_dodgeAllowedInAction)
                    {
                        if (self.Stats.MovementSpeed > 0f
                            && !m_preparingToSleep
                            && (!self.LocomotionAction || m_currentlyChargingAttack)
                            && (m_nextIsLocomotion || m_dodgeAllowedInAction > 0))
                        {
                            if (!self.Dodging)
                            {
                                Instance.SendDodge(self, staminaCost, _direction);
                            }
                            return false;
                        }
                    }
                }
                else // cancelling enabled. check if we should allow the dodge
                {
                    if (Instance.PlayerLastHitTimes.ContainsKey(self.UID) 
                        && Time.time - Instance.PlayerLastHitTimes[self.UID] < (float)CombatOverhaul.config.GetValue(Settings.Dodge_DelayAfterHit))
                    {
                        //  Debug.Log("Player has hit within the last few seconds. Dodge not allowed!");
                        return false;
                    }

                    Character.HurtType hurtType = (Character.HurtType)At.GetField(self, "m_hurtType");

                    // manual fix (game sometimes does not reset HurtType to NONE when animation ends.
                    float timeout = (float)CombatOverhaul.config.GetValue(Settings.Dodge_DelayAfterStagger);
                    if (hurtType == Character.HurtType.Knockdown)
                    {
                        timeout = (float)CombatOverhaul.config.GetValue(Settings.Dodge_DelayAfterKD);
                    }

                    if ((float)At.GetField(self, "m_timeOfLastStabilityHit") is float lasthit && Time.time - lasthit > timeout)
                    {
                        hurtType = Character.HurtType.NONE;
                        At.SetField(self, "m_hurtType", hurtType);
                    }

                    // if we're not currently dodging or staggered, force an animation cancel dodge (provided we have enough stamina).
                    if (!self.Dodging && hurtType == Character.HurtType.NONE)
                    {
                        Instance.SendDodge(self, staminaCost, _direction);
                    }

                    // send a fix to force m_dodging to false after a short delay.
                    // this is a fix for if the player dodges while airborne, the game wont reset their m_dodging to true when they land.
                    Instance.StartCoroutine(Instance.DodgeLateFix(self));
                }

                return false;
            }
        }

        private IEnumerator DodgeLateFix(Character character)
        {
            yield return new WaitForSeconds(0.25f);

            while (!character.NextIsLocomotion)
            {
                yield return null;
            }

            At.SetField(character, "m_dodging", false);
        }

        private void SendDodge(Character self, float staminaCost, Vector3 _direction)
        {
            float f = (float)At.GetField(self.Stats, "m_stamina");

            if (f >= staminaCost)
            {
                //At.SetValue(f - staminaCost, typeof(CharacterStats), self.Stats, "m_stamina");
                self.Stats.UseStamina(TagSourceManager.Dodge, staminaCost);

                At.SetField(self, "m_dodgeAllowedInAction", 0);

                if (self.CharacterCamera && self.CharacterCamera.InZoomMode)
                {
                    self.SetZoomMode(false);
                }

                self.ForceCancel(false, true);
                self.ResetCastType();

                self.photonView.RPC("SendDodgeTriggerTrivial", PhotonTargets.All, new object[] { _direction });

                At.Invoke(self, "ActionPerformed", false);

                self.Invoke("ResetDodgeTrigger", 0.5f);
            }
        }


        [HarmonyPatch(typeof(Character), "SendDodgeTriggerTrivial")]
        public class Character_SendDodgeTriggerTrivial
        {
            [HarmonyPrefix]
            public static bool Prefix(Character __instance, Vector3 _direction)
            {
                var self = __instance;

                if (!(bool)CombatOverhaul.config.GetValue(Settings.Custom_Bag_Burden))
                {
                    return true;
                }

                if (self.CurrentWeapon)

                if (self.HasDodgeDirection)
                {
                    self.Animator.SetFloat("DodgeBlend", !self.DodgeRestricted ? 0.0f : Instance.GetDodgeRestriction(self));
                }

                self.Animator.SetTrigger("Dodge");

                if (self.CurrentlyChargingAttack)
                {
                    //self.SendCancelCharging();
                    At.Invoke(self, "SendCancelCharging");
                }

                // get sound player with null coalescing operator
                (At.GetField(self, "m_dodgeSoundPlayer") as SoundPlayer)?.Play(false);

                //self.m_dodging = true;
                At.SetField(self, "m_dodging", true);

                //self.StopBlocking();
                At.Invoke(self, "StopBlocking");

                // null coalescing OnDodgeEvent invoke
                self.OnDodgeEvent?.Invoke();

                if (At.GetField(self, "m_characterSoundManager") is CharacterSoundManager charSounds)
                {
                    Global.AudioManager.PlaySoundAtPosition(charSounds.GetDodgeSound(), self.transform, 0f, 1f, 1f, 1f, 1f);
                }

                self.SendMessage("DodgeTrigger", _direction, SendMessageOptions.DontRequireReceiver);


                return false;
            }
        }

        // dodge burden helper
        private float GetDodgeRestriction(Character self)
        {
            // Handle if our bag doesn't restrict us anyway
            if (!self.DodgeRestricted)
                return 0.0f;

            // Find the currently equipped bag, it should exist
            Bag bag = self.Inventory.EquippedBag;
            if (bag == null) // This shouldn't happen but who knows
                return 0.0f;

            float weight = bag.Weight * 100;
            float ratio = (weight / bag.BagCapacity) * 0.01f;

            if (ratio < ((float)CombatOverhaul.config.GetValue(Settings.min_burden_weight) * 0.01f))
            {
                return (float)CombatOverhaul.config.GetValue(Settings.min_slow_effect) * 0.01f;
            }
            else
            {
                return Mathf.Clamp(ratio * 100f, (float)CombatOverhaul.config.GetValue(Settings.min_slow_effect), (float)CombatOverhaul.config.GetValue(Settings.max_slow_effect)) * 0.01f;
            }
        }

        [HarmonyPatch(typeof(Character), "AttackInput")]
        public class Character_AttackInput
        {
            public static bool Prefix(Character __instance, int _type, int _id = 0)
            {
                var self = __instance;

                if (self.IsLocalPlayer && (bool)CombatOverhaul.config.GetValue(Settings.Attack_Cancels_Blocking) && !self.IsAI && self.Blocking)
                {
                    Instance.StartCoroutine(Instance.StopBlockingCoroutine(self));
                    At.Invoke(self, "StopBlocking");
                    At.SetField(self, "m_blockDesired", false);
                }


                return true;
            }
        }

        private IEnumerator StopBlockingCoroutine(Character character)
        {
            yield return new WaitForSeconds(0.05f); // 50ms wait (1 or 2 frames)

            At.Invoke(character, "StopBlocking");
            At.SetField(character, "m_blockDesired", false);
        }

        [HarmonyPatch(typeof(CharacterStats), "UpdateVitalStats")]
        public class CharacterStats_UpdateVitalStats
        {
            public static bool Prefix(CharacterStats __instance)
            {
                var self = __instance;

                if (At.GetField(self, "m_timeOfLastStamUse") is float timeOfLast && Time.time - timeOfLast > (float)CombatOverhaul.config.GetValue(Settings.Stamina_Regen_Delay)
                && At.GetField(self, "m_stamina") is float m_stamina
                && At.GetField(self, "m_character") is Character character
                && !character.Blocking)
                {
                    float regen = (float)CombatOverhaul.config.GetValue(Settings.Extra_Stamina_Regen) * Time.deltaTime;
                    float newStamina = Mathf.Clamp(m_stamina + regen, 0, self.ActiveMaxStamina);
                    At.SetField(self, "m_stamina", newStamina);
                }

                return true;
            }
        }
    }
}
