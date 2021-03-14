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
    public static class PlayerManager
    {
        private static readonly Dictionary<string, float> PlayerLastHitTimes = new Dictionary<string, float>();

        [HarmonyPatch(typeof(Character), "HasHit")]
        public class Character_HasHit
        {
            [HarmonyPrefix]
            public static bool Prefix(Character __instance)
            {
                var self = __instance;

                if (PlayerLastHitTimes.ContainsKey(self.UID))
                    PlayerLastHitTimes[self.UID] = Time.time;
                else
                    PlayerLastHitTimes.Add(self.UID, Time.time);

                return true;
            }
        }

        [HarmonyPatch(typeof(Character), "DodgeInput", new Type[] { typeof(Vector3) })]
        public class Character_DodgeInput
        {
            [HarmonyPostfix]
            public static void Postfix(Character __instance, Vector3 _direction, bool ___m_pendingDeath, ref int ___m_dodgeAllowedInAction)
            {
                if (!CombatTweaksMod.Dodge_Cancelling.Value)
                    return;

                if (!__instance.IsPhotonPlayerLocal || __instance.IsAI || __instance.Dodging)
                    return;

                if (___m_pendingDeath)
                    return;

                // check player has enough stamina
                if (!(bool)At.Invoke(__instance, "HasEnoughStamina", (float)__instance.DodgeStamCost))
                    return;

                if (PlayerLastHitTimes.ContainsKey(__instance.UID)
                    && Time.time - PlayerLastHitTimes[__instance.UID] < CombatTweaksMod.Dodge_DelayAfterPlayerHits.Value)
                {
                    //  Debug.Log("Player has hit within the last few seconds. Dodge not allowed!");
                    return;
                }

                Character.HurtType hurtType = (Character.HurtType)At.GetField(__instance, "m_hurtType");

                // manual fix (game sometimes does not reset HurtType to NONE when animation ends.
                float timeout;
                if (hurtType == Character.HurtType.Knockdown)
                    timeout = CombatTweaksMod.Dodge_DelayAfterKnockdown.Value;
                else
                    timeout = CombatTweaksMod.Dodge_DelayAfterStagger.Value;

                if ((float)At.GetField(__instance, "m_timeOfLastStabilityHit") is float lasthit 
                    && Time.time - lasthit > timeout)
                {
                    hurtType = Character.HurtType.NONE;
                    At.SetField(__instance, "m_hurtType", hurtType);
                }

                // if we're not currently staggered, force an animation cancel dodge (provided we have enough stamina).
                if (hurtType == Character.HurtType.NONE)
                {
                    //SendDodge(__instance, __instance.DodgeStamCost, _direction);

                    __instance.Stats.UseStamina(TagSourceManager.Dodge, __instance.DodgeStamCost);

                    ___m_dodgeAllowedInAction = 0;

                    if (__instance.CharacterCamera && __instance.CharacterCamera.InZoomMode)
                        __instance.SetZoomMode(false);

                    __instance.ForceCancel(false, true);
                    __instance.ResetCastType();

                    __instance.photonView.RPC("SendDodgeTriggerTrivial", PhotonTargets.All, new object[]
                    {
                        _direction
                    });

                    At.Invoke(__instance, "ActionPerformed", true);


                    __instance.Invoke("ResetDodgeTrigger", 0.5f);
                }

                // send a fix to force m_dodging to false after a short delay.
                // this is a fix for if the player dodges while airborne, the game wont reset their m_dodging to true when they land.
                CombatTweaksMod.Instance.StartCoroutine(DodgeLateFix(__instance));
            }
        }

        private static IEnumerator DodgeLateFix(Character character)
        {
            yield return new WaitForSeconds(0.25f);

            while (!character.NextIsLocomotion)
                yield return null;

            At.SetField(character, "m_dodging", false);
        }

        [HarmonyPatch(typeof(Character), "HitStarted")]
        public class Character_HitStarted
        {
            [HarmonyPrefix]
            public static bool Prefix(Character __instance)
            {
                if (__instance.Dodging)
                    return false;

                return true;
            }
        }

        // ========== Cancel blocking by attacking ==========

        [HarmonyPatch(typeof(Character), "AttackInput")]
        public class Character_AttackInput
        {
            public static bool Prefix(Character __instance, int _type, int _id = 0)
            {
                var self = __instance;

                if (self.IsLocalPlayer && CombatTweaksMod.Blocking_CancelledByAttack.Value && !self.IsAI && self.Blocking)
                {
                    CombatTweaksMod.Instance.StartCoroutine(StopBlockingCoroutine(self));
                    At.Invoke(self, "StopBlocking");
                    At.SetField(self, "m_blockDesired", false);
                }


                return true;
            }
        }

        private static IEnumerator StopBlockingCoroutine(Character character)
        {
            yield return new WaitForSeconds(0.05f); // 50ms wait (1 or 2 frames)

            At.Invoke(character, "StopBlocking");
            At.SetField(character, "m_blockDesired", false);
        }
    }
}
