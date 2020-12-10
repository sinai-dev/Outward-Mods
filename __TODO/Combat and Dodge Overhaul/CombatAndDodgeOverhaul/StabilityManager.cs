using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Reflection;
using HarmonyLib;

namespace CombatAndDodgeOverhaul
{
    public class StabilityManager : MonoBehaviour
    {
        public static StabilityManager Instance;

        public Dictionary<string, float> LastStaggerTimes = new Dictionary<string, float>();

        internal void Awake()
        {
            Instance = this;
        }

        // This is the main Stability Hit function, called whenever a Character receives impact damage.
        // It is mostly a copy+paste of the original, I've just had to use reflection for all the private fields / methods, and patched in our custom values.
        // The only real thing I added was the 'stagger immunity period', which is essentially a timeout period to prevent stun locking after getting staggered.

        [HarmonyPatch(typeof(Character), "StabilityHit")]
        public class Character_StabilityHit
        {
            public static bool Prefix(Character __instance, float _knockValue, float _angle, bool _block, Character _dealerChar)
            {
                var self = __instance;
                var _base = self as Photon.MonoBehaviour;

                if (At.GetValue(typeof(Character), self, "m_impactImmune") is bool m_impactImmune
                    && At.GetValue(typeof(Character), self, "m_shieldStability") is float m_shieldStability
                    && At.GetValue(typeof(Character), self, "m_stability") is float m_stability
                    && At.GetValue(typeof(Character), self, "m_knockbackCount") is float m_knockbackCount
                    && At.GetValue(typeof(Character), self, "m_knockHurtAllowed") is bool m_knockHurtAllowed
                    && At.GetValue(typeof(Character), self, "m_currentlyChargingAttack") is bool m_currentlyChargingAttack
                    && At.GetValue(typeof(Character), self, "m_animator") is Animator m_animator)
                {
                    // Begin actual stability hit function
                    var hit = _knockValue;
                    if (hit < 0)
                        hit = 0;

                    if (!m_impactImmune && hit > 0f)
                    {
                        //Debug.Log("--------- " + self.Name + " ---------");

                        // check stagger immunity dictionary (custom)
                        float lastStagger = -1;
                        if (Instance.LastStaggerTimes.ContainsKey(self.UID))
                        {
                            lastStagger = Instance.LastStaggerTimes[self.UID];
                        }

                        // if you run out of stamina and get hit, you will always get staggered. (unchanged, except to reflect custom stagger threshold)
                        if (self.Stats.CurrentStamina < 1f)
                        {
                            float hitToStagger = m_shieldStability + m_stability - (100 - (float)CombatOverhaul.config.GetValue(Settings.Stagger_Threshold));
                            if (hit < hitToStagger)
                            {
                                hit = hitToStagger;
                            }
                            //Debug.LogError("Stamina autostagger called! hitToStagger: " + hitToStagger + ", hit: " + hit);
                        }

                        At.SetValue(Time.time, typeof(Character), self, "m_timeOfLastStabilityHit");
                        // Debug.Log("Set " + Time.time + " as character's last stability hit");

                        if (self.CharacterCamera != null && hit > 0f)
                        {
                            self.CharacterCamera.Hit(hit * 6f);
                        }

                        // check shield stability if blocking (unchanged)
                        if (_block && m_shieldStability > 0f)
                        {
                            if (hit > m_shieldStability)
                            {
                                var num2 = m_stability - (hit - m_shieldStability);
                                At.SetValue(num2, typeof(Character), self, "m_stability");
                                m_stability = num2;
                            }
                            var num3 = Mathf.Clamp(m_shieldStability - hit, 0f, 50f);
                            At.SetValue(num3, typeof(Character), self, "m_shieldStability");
                            m_shieldStability = num3;
                        }
                        // check non-blocking stability (unchanged)
                        else
                        {
                            var num2 = Mathf.Clamp(m_stability - hit, 0f, 100f);
                            At.SetValue(num2, typeof(Character), self, "m_stability");
                            m_stability = num2;
                        }
                        // if hit takes us below knockdown threshold, or if AI auto-knockdown stagger count was reached...
                        if (m_stability <= (float)CombatOverhaul.config.GetValue(Settings.Knockdown_Threshold)
                            || (self.IsAI && m_knockbackCount >= (float)CombatOverhaul.config.GetValue(Settings.Enemy_AutoKD_Count)))
                        {
                            //Debug.LogError("Knockdown! Hit Value: " + _knockValue + ", current stability: " + m_stability);

                            if ((!self.IsAI && _base.photonView.isMine) || (self.IsAI && (_dealerChar == null || _dealerChar.photonView.isMine)))
                            {
                                _base.photonView.RPC("SendKnock", PhotonTargets.All, new object[]
                                {
                                true,
                                m_stability
                                });
                            }
                            else
                            {
                                At.Call(self, "Knock", new object[]
                                {
                                true
                                });
                            }
                            At.SetValue(0f, typeof(Character), self, "m_stability");
                            m_stability = 0f;
                            if (self.IsPhotonPlayerLocal)
                            {
                                self.BlockInput(false);
                            }
                        }
                        // else if hit is a stagger...
                        else if (m_stability <= (float)CombatOverhaul.config.GetValue(Settings.Stagger_Threshold) && (Time.time - lastStagger > (float)CombatOverhaul.config.GetValue(Settings.Stagger_Immunity_Period)))
                        {
                            // Debug.LogWarning("Stagger! Hit Value: " + _knockValue + ", current stability: " + m_stability);

                            // update Stagger Immunity dictionary
                            if (!Instance.LastStaggerTimes.ContainsKey(self.UID))
                            {
                                Instance.LastStaggerTimes.Add(self.UID, Time.time);
                            }
                            else
                            {
                                Instance.LastStaggerTimes[self.UID] = Time.time;
                            }

                            if ((!self.IsAI && _base.photonView.isMine) || (self.IsAI && (_dealerChar == null || _dealerChar.photonView.isMine)))
                            {
                                _base.photonView.RPC("SendKnock", PhotonTargets.All, new object[]
                                {
                                false,
                                m_stability
                                });
                            }
                            else
                            {
                                At.Call(self, "Knock", new object[]
                                {
                                false
                                });
                            }
                            if (self.IsPhotonPlayerLocal && _block)
                            {
                                self.BlockInput(false);
                            }
                        }
                        // else if we are not blocking...
                        else if (!_block)
                        {
                            // Debug.Log("Value: " + _knockValue + ", new stability: " + m_stability);
                            if (m_knockHurtAllowed)
                            {
                                At.SetValue(Character.HurtType.Hurt, typeof(Character), self, "m_hurtType");

                                if (m_currentlyChargingAttack)
                                {
                                    self.CancelCharging();
                                }

                                m_animator.SetTrigger("Knockhurt");
                                _base.StopCoroutine("KnockhurtRoutine");

                                MethodInfo _knockhurtRoutine = self.GetType().GetMethod("KnockhurtRoutine", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                                IEnumerator _knockEnum = (IEnumerator)_knockhurtRoutine.Invoke(self, new object[] { hit });
                                _base.StartCoroutine(_knockEnum);
                            }

                            if (m_stability <= (float)CombatOverhaul.config.GetValue(Settings.Stagger_Immunity_Period))
                            {
                                // Debug.LogError(self.Name + " would have staggered. Current delta: " + (Time.time - lastStagger));
                            }
                        }
                        else // hit was blocked and no stagger
                        {
                            Instance.StaggerAttacker(self, m_animator, _dealerChar);
                        }
                        m_animator.SetInteger("KnockAngle", (int)_angle);
                        self.StabilityHitCall?.Invoke();
                    }
                    else if (!m_impactImmune && _block) // hit dealt 0 impact and was blocked
                    {
                        Instance.StaggerAttacker(self, m_animator, _dealerChar);
                    }
                }

                return false;
            }
        }

        private void StaggerAttacker(Character self, Animator m_animator, Character _dealerChar)
        {
            // Debug.Log(self.Name + " blocked the attack. Shield stability: " + m_shieldStability);
            At.SetValue(Character.HurtType.NONE, typeof(Character), self, "m_hurtType");
            if (self.InLocomotion)
            {
                m_animator.SetTrigger("BlockHit");
            }

            if ((bool)CombatOverhaul.config.GetValue(Settings.Blocking_Staggers_Attacker))
            {
                // Debug.Log("autoknocking " + _dealerChar.Name);
                if (_dealerChar.CurrentWeapon.Type != Weapon.WeaponType.Bow)
                {
                    _dealerChar.AutoKnock(false, new Vector3(0, 0, 0));
                }
            }
        }

        [HarmonyPatch(typeof(Character), "UpdateStability")]
        public class Character_UpdateStability
        {
            [HarmonyPrefix]
            public static bool Prefix(Character __instance)
            {
                var self = __instance;

                if (At.GetValue(typeof(Character), self, "m_stability") is float m_stability
                && At.GetValue(typeof(Character), self, "m_timeOfLastStabilityHit") is float m_timeOfLastStabilityHit
                && At.GetValue(typeof(Character), self, "m_shieldStability") is float m_shieldStability
                && At.GetValue(typeof(Character), self, "m_knockbackCount") is float m_knockbackCount)
                {
                    // ----------- original method, unchanged other than to reflect custom values -------------
                    if ((bool)CombatOverhaul.config.GetValue(Settings.No_Stability_Regen_When_Blocking) && self.Blocking) // no stability regen while blocking! otherwise too op
                        return false;

                    float num = Time.time - m_timeOfLastStabilityHit;
                    if (num > (float)CombatOverhaul.config.GetValue(Settings.Stability_Regen_Delay))
                    {
                        if (m_stability < 100f)
                        {
                            var num2 = Mathf.Clamp(m_stability + (self.StabilityRegen * (float)CombatOverhaul.config.GetValue(Settings.Stability_Regen_Speed)) * Time.deltaTime, 0f, 100f);
                            At.SetValue(num2, typeof(Character), self, "m_stability");

                        }
                        else if (m_shieldStability < 50f)
                        {
                            var num2 = Mathf.Clamp(m_shieldStability + self.StabilityRegen * Time.deltaTime, 0f, 50f);
                            At.SetValue(num2, typeof(Character), self, "m_shieldStability");
                        }
                        if (num > (float)CombatOverhaul.config.GetValue(Settings.Enemy_AutoKD_Reset_Time))
                        {
                            bool flag = m_knockbackCount > 0;
                            var num2 = Mathf.Clamp(m_knockbackCount - Time.deltaTime, 0f, 4f);
                            At.SetValue(num2, typeof(Character), self, "m_knockbackCount");
                            if (flag && num2 <= 0)
                            {
                                // Debug.Log("Resetting AI stagger count for " + self.Name);
                            }
                        }
                    }
                }

                return false;
            }
        }

        [HarmonyPatch(typeof(Character), "SlowDown")]
        public class Character_SlowDown
        {
            [HarmonyPrefix]
            public static bool Prefix(Character __instance, ref float _slowVal, ref float _timeTo, ref float _timeStay, ref float _timeFrom)
            {
                var num = (float)CombatOverhaul.config.GetValue(Settings.SlowDown_Modifier);

                _slowVal = Mathf.Clamp(num * num, 0.1f, 2) * _slowVal;  // apply the custom modifier MORE to the "slowVal" value, seems to get better results. Clamp above 0.1 otherwise the attack can last like 30+ seconds
                _timeTo = ((4 + num) / 5) * _timeTo; // apply MUCH LESS to "timeTo", for same reason.
                _timeStay = num * _timeStay; // apply raw modifier to these two
                _timeFrom = num * _timeFrom;

                return true;
            }
        }

        [HarmonyPatch(typeof(Character), "AutoKnock")]
        public class Character_AutoKnock
        {
            [HarmonyPrefix]
            public static bool Prefix(Character __instance, bool _down, Vector3 _dir)
            {
                var self = __instance;

                var _base = self as Photon.MonoBehaviour;
                if (At.GetValue(typeof(Character), self, "m_stability") is float m_stability)
                {
                    //OLogger.Error("Autoknock, m_stability: " + m_stability);

                    At.Call(self, "StabilityHit", new object[] {
                    (!_down) ? Mathf.Clamp(m_stability - (float)CombatOverhaul.config.GetValue(Settings.Stagger_Threshold), 1f, 100 - (float)CombatOverhaul.config.GetValue(Settings.Stagger_Threshold)) : m_stability,
                    Vector3.Angle(_base.transform.forward, -_dir),
                    _down,
                    null
                });
                }

                return false;
            }
        }
    }
}
