using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SideLoader;
using UnityEngine;

namespace Necromancer.Skills.Effects
{
    public class ManaPointAffectStat : AffectStat
    {
        public CharacterStats.StatType Stat;

        private float lastUpdateTime = -1f;

        internal void Update() // limited to 2s update since its low priority
        {
            if (OwnerCharacter != null && Time.time - lastUpdateTime > 2f)
            {
                lastUpdateTime = Time.time;

                if (Value != OwnerCharacter.Stats.ManaPoint * NecromancerMod.settings.Transcendence_DamageBonus)
                {
                    Value = OwnerCharacter.Stats.ManaPoint * NecromancerMod.settings.Transcendence_DamageBonus;
                    
                    if (IsRegistered)
                    {
                        OwnerCharacter.Stats.RemoveStatStack(this.AffectedStat, this.SourceID, this.IsModifier);

                        var m_statStack = OwnerCharacter.Stats.AddStatStack(
                            this.AffectedStat,
                            new StatStack(
                                this.m_sourceID,
                                this.Lifespan,
                                this.Value * ((!this.IsModifier) ? 1f : 0.01f),
                                null),
                            this.IsModifier
                        );

                        At.SetField(this as AffectStat, "m_statStack", m_statStack);
                    }
                }
            }
        }

        protected override void ActivateLocally(Character _affectedCharacter, object[] _infos)
        {
            if (AffectedStat == null || AffectedStat.Tag == null || AffectedStat.Tag == Tag.None)
            {
                base.AffectedStat = new TagSourceSelector(CustomTags.GetTag(Stat.ToString()));
            }

            Update(); // refresh mana point value

            base.ActivateLocally(_affectedCharacter, _infos);
        }
    }
}
