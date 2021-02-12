using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using SideLoader;
using UnityEngine;

namespace ProtectionBubble
{
    public class MitigationOverride
    {
        public static void Calculate(DamageList _damage, Character character, bool countBubble = true)
        {
            var stats = character.Stats;

            // ========== Original method (GetMitigatedDamage) ==========
            for (int i = 0; i < _damage.Count; i++)
            {
                float dmg = _damage[i].Damage;

                if ((bool)ProtectionBubbleMod.config.GetValue(Settings.OrigProtectionApplied))
                {
                    dmg -= stats.GetDamageProtection((DamageType.Types)i);
                }

                var res = 1f - stats.GetDamageResistance((DamageType.Types)i);

                if (dmg > 0f)
                {
                    if (res < 0f)
                    {
                        res = 0f;
                    }

                    dmg *= res;
                }
                else
                {
                    dmg = 0f;
                }

                _damage[i].Damage = dmg;
            }

            // ========== Custom Protection Bubble ==========
            var protStatus = character.StatusEffectMngr.GetStatusEffectOfName(ProtectionBubble.IDENTIFIER);

            if (protStatus)
            {
                protStatus.GetComponentInChildren<ProtectionBubble>().MitigateDamage(ref _damage, countBubble);
            }
        }
    }

    [HarmonyPatch(typeof(Character), "ProcessDamageReduction")]
    public class Character_ProcessDamageReduction
    {
        [HarmonyPrefix]
        public static bool Prefix(ref DamageList _damage, Character __instance)
        {
            if (__instance.IsAI)
            {
                return true;
            }

            MitigationOverride.Calculate(_damage, __instance, true);

            return false;
        }
    }

    [HarmonyPatch(typeof(CharacterStats), "GetMitigatedDamage")]
    public class CharacterStats_GetMitigatedDamage
    {
        [HarmonyPrefix]
        public static bool Prefix(Character ___m_character, ref DamageList _damages)
        {
            if (___m_character.IsAI)
            {
                return true;
            }

            MitigationOverride.Calculate(_damages, ___m_character, false);

            return false;
        }
    }

    [HarmonyPatch(typeof(CharacterStats), "UpdateEquipmentStats")]
    public class CharacterStats_UpdateEquipmentStats
    {
        [HarmonyPostfix]
        public static void Postfix(Character ___m_character, float [] ___m_totalDamageProtection)
        {
            if (___m_character.IsAI)
            {
                return;
            }

            var mngr = ___m_character.StatusEffectMngr;
            if (mngr)
            {
                var status = mngr.GetStatusEffectOfName(ProtectionBubble.IDENTIFIER);

                if (___m_totalDamageProtection[0] > 0f)
                {
                    if (!status)
                    {
                        var prefab = ResourcesPrefabManager.Instance.GetStatusEffectPrefab(ProtectionBubble.IDENTIFIER);
                        mngr.AddStatusEffect(prefab, null);
                    }
                    
                }
                else
                {
                    if (status)
                    {
                        mngr.RemoveStatusWithIdentifierName(ProtectionBubble.IDENTIFIER);
                    }
                }
            }
        }
    }
}
