using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Necromancer.Skills.EffectConditions;
using SideLoader;
using SideLoader.Helpers;
using UnityEngine;

namespace Necromancer.Skills.Effects
{
    public class DeathRitual : ShootBlast
    {
        protected override void ActivateLocally(Character _targetCharacter, object[] _infos)
        {
            if (SummonManager.FindWeakestSummon(_targetCharacter.UID) is Character summonChar
                && summonChar.isActiveAndEnabled)
            {

                // change blast position to the summon's position
                _infos[0] = summonChar.transform.position;
                base.ActivateLocally(_targetCharacter, _infos);

                // kill the summon
                summonChar.Stats.ReceiveDamage(999f);

                // fix for cooldown not working on this skill for some reason
                var skill = this.ParentItem as Skill;
                At.SetField(skill, "m_lastActivationTime", Time.time);
                At.SetField(skill, "m_lastReceivedCooldownProgress", -1);

                // plague aura interaction
                if (PlagueAuraProximityCondition.IsInsidePlagueAura(summonChar.transform.position))
                {
                    // if you're inside a plague aura, detonate resets your Summon cooldown.
                    if (this.OwnerCharacter?.Inventory?.SkillKnowledge?.GetItemFromItemID(8890103) is Skill summonSkill)
                    {
                        summonSkill.ResetCoolDown();
                    }
                }
            }
        }
    }
}
