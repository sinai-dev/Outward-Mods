using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Necromancer.Skills.EffectConditions;
using SideLoader;
using UnityEngine;

namespace Necromancer.Skills.Effects
{
    public class LifeRitual : Effect
    {
        protected override void ActivateLocally(Character _affectedCharacter, object[] _infos)
        {
            if (SummonManager.FindWeakestSummon(_affectedCharacter.UID) is Character summonChar)
            {
                bool insideSigil = PlagueAuraProximityCondition.IsInsidePlagueAura(_affectedCharacter.transform.position);

                float healSummon = insideSigil ? 0.66f : 0.33f;

                // restores HP to the summon
                summonChar.Stats.AffectHealth(summonChar.ActiveMaxHealth * healSummon);

                // add status effects
                summonChar.StatusEffectMngr.AddStatusEffect(ResourcesPrefabManager.Instance.GetStatusEffectPrefab("Rage"), null);
                summonChar.StatusEffectMngr.AddStatusEffect(ResourcesPrefabManager.Instance.GetStatusEffectPrefab("Possessed"), null);
                summonChar.StatusEffectMngr.AddStatusEffect(ResourcesPrefabManager.Instance.GetStatusEffectPrefab("Speed Up"), null);

                if (insideSigil)
                {
                    // add decay imbue
                    summonChar.CurrentWeapon.AddImbueEffect(ResourcesPrefabManager.Instance.GetEffectPreset(211) as ImbueEffectPreset, 180f);
                }

            }
        }
    }
}
