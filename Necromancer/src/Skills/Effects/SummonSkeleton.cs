using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.IO;
using SideLoader;
using Necromancer.Skills.EffectConditions;

namespace Necromancer.Skills.Effects
{
    public class SummonSkeleton : SpawnSLCharacter 
    {
        internal void Awake()
        {
            this.GenerateRandomUIDForSpawn = true;
            this.TryFollowCaster = true;
            this.SpawnOffset = new Vector3(1, 0.25f, 1);
        }

        // The game checks this before it activates the effect. Note that any mana/stamina/item costs will already be consumed.
        protected override bool TryTriggerConditions() 
        {
            float healthcost = NecromancerMod.settings.Summon_HealthCost * this.m_affectedCharacter.Stats.MaxHealth;
            // check player has enough HP
            if (this.m_affectedCharacter.Stats.CurrentHealth - healthcost <= 0)
            {
                this.m_affectedCharacter.CharacterUI.ShowInfoNotification("You do not have enough health to do that!");
                // refund the cooldown and costs
                if (this.ParentItem is Skill skill)
                {
                    skill.ResetCoolDown();
                    m_affectedCharacter.Stats.SetMana(m_affectedCharacter.Stats.CurrentMana + skill.ManaCost);
                }
                return false;
            }
            else // caster has enough HP
            {
                return true;
            }
        }

        protected override void ActivateLocally(Character _affectedCharacter, object[] _infos)
        {
            // SL.Log("summoning necromancer summon...");

            bool armyOfDeathLearned = _affectedCharacter.Inventory.SkillKnowledge.IsItemLearned(8890108);

            int maxSummons = armyOfDeathLearned 
                                ? NecromancerMod.settings.Summon_MaxSummons_WithArmyOfDeath 
                                : NecromancerMod.settings.Summon_MaxSummons_NoArmyOfDeath;

            if (SummonManager.SummonedCharacters.ContainsKey(_affectedCharacter.UID))
            {
                var list = SummonManager.SummonedCharacters[_affectedCharacter.UID];
                int toDestroy = list.Count - maxSummons;

                while (toDestroy >= 0)
                {
                    if (SummonManager.FindWeakestSummon(_affectedCharacter.UID) is Character summon)
                        SummonManager.OnSummonDeath(summon);

                    toDestroy--;
                }
            }

            // custom health cost for casting
            _affectedCharacter.Stats.UseBurntHealth = NecromancerMod.settings.Summon_BurnsHealth;
            float healthcost = NecromancerMod.settings.Summon_HealthCost * _affectedCharacter.Stats.MaxHealth;
            _affectedCharacter.Stats.ReceiveDamage(healthcost);
            _affectedCharacter.Stats.UseBurntHealth = true;

            // only host should do this
            if (!PhotonNetwork.isNonMasterClientInRoom)
            {
                bool insidePlagueAura = PlagueAuraProximityCondition.IsInsidePlagueAura(_affectedCharacter.transform.position);

                var template = insidePlagueAura ? SummonManager.Ghost : SummonManager.Skeleton;
                this.SLCharacter_UID = template.UID;

                CustomCharacters.Templates.TryGetValue(this.SLCharacter_UID, out m_charTemplate);

                this.ExtraRpcData = _affectedCharacter.UID.ToString();

                int amtToSummon = armyOfDeathLearned
                                    ? NecromancerMod.settings.Summon_MaxSummons_WithArmyOfDeath
                                    : NecromancerMod.settings.Summon_MaxSummons_NoArmyOfDeath;

                for (int i = 0; i < amtToSummon; i++)
                    base.ActivateLocally(_affectedCharacter, _infos);
            }
        }
    }
}
