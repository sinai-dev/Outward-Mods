using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.IO;
using SideLoader;

namespace NecromancySkills
{
    public class SummonSkeleton : Effect // Inherits from the game's "Effect" class, so it works with those systems automatically.
    {
        // Setup (called from SkillManager init)
        #region Summon Skill Setup
        public static void SetupSummon()
        {
            var summon = ResourcesPrefabManager.Instance.GetItemPrefab(8890103) as Skill;

            // destroy the existing skills, but keep the rest (VFX / Sound).
            DestroyImmediate(summon.transform.Find("Lightning").gameObject);
            DestroyImmediate(summon.transform.Find("SummonSoul").gameObject);

            var effects = new GameObject("Effects");
            effects.transform.parent = summon.transform;
            effects.AddComponent<SummonSkeleton>();

            // setup custom blade visuals
            try
            {
                var blade = ResourcesPrefabManager.Instance.GetItemPrefab(2598500) as Weapon;
                var bladeVisuals = CustomItemVisuals.GetOrAddVisualLink(blade).ItemVisuals;
                if (bladeVisuals.transform.Find("Weapon3DVisual").GetComponent<MeshRenderer>() is MeshRenderer mesh)
                {
                    mesh.material.color = new Color(-0.5f, 1.5f, -0.5f);
                }
            }
            catch { }

            // make sure the config is applied from the save
            SummonManager.Skeleton.Health = NecromancyBase.settings.Summon_MaxHealth;
            SummonManager.Skeleton.HealthRegen = NecromancyBase.settings.Summon_HealthLoss;
            SummonManager.Ghost.Health = NecromancyBase.settings.StrongSummon_MaxHealth;
            SummonManager.Ghost.HealthRegen = NecromancyBase.settings.StrongSummon_HealthLoss;
        }
        #endregion


        protected override bool TryTriggerConditions() // The game checks this before it activates the effect. Note that any mana/stamina/item costs will already be consumed.
        {
            float healthcost = NecromancyBase.settings.Summon_HealthCost * this.m_affectedCharacter.Stats.MaxHealth;
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
            if (SummonManager.Instance == null) { return; }

            bool armyOfDeathLearned = _affectedCharacter.Inventory.SkillKnowledge.IsItemLearned(8890108);

            int MaxSummons = armyOfDeathLearned ? NecromancyBase.settings.Summon_MaxSummons_WithArmyOfDeath : NecromancyBase.settings.Summon_MaxSummons_NoArmyOfDeath;

            if (SummonManager.Instance.SummonedCharacters.ContainsKey(_affectedCharacter.UID))
            {
                var list = SummonManager.Instance.SummonedCharacters[_affectedCharacter.UID];
                int diff = MaxSummons - list.Count;

                if (armyOfDeathLearned)
                {
                    while (diff < NecromancyBase.settings.Summon_Summoned_Per_Cast_withArmyOfDeath)
                    {
                        if (SummonManager.Instance.FindWeakestSummon(_affectedCharacter.UID) is Character summon)
                        {
                            SummonManager.DestroySummon(summon);
                        }

                        diff++;
                    }
                }
                else
                {
                    if (list.Count == MaxSummons && SummonManager.Instance.FindWeakestSummon(_affectedCharacter.UID) is Character summon)
                    {
                        SummonManager.DestroySummon(summon);
                    }
                }
            }

            // custom health cost for casting
            _affectedCharacter.Stats.UseBurntHealth = NecromancyBase.settings.Summon_BurnsHealth;
            float healthcost = NecromancyBase.settings.Summon_HealthCost * _affectedCharacter.Stats.MaxHealth;
            _affectedCharacter.Stats.ReceiveDamage(healthcost);
            _affectedCharacter.Stats.UseBurntHealth = true;

            // only host should do this
            if (!PhotonNetwork.isNonMasterClientInRoom)
            {

                bool insidePlagueAura = PlagueAuraProximityCondition.IsInsidePlagueAura(_affectedCharacter.transform.position);

                if (armyOfDeathLearned)
                {
                    for (int i = 0; i < NecromancyBase.settings.Summon_Summoned_Per_Cast_withArmyOfDeath; i++)
                    {
                        var uid = UID.Generate().ToString();
                        SummonManager.Instance.SummonSpawn(_affectedCharacter, uid, insidePlagueAura);
                    }
                }
                else
                {
                    var uid = UID.Generate().ToString();
                    SummonManager.Instance.SummonSpawn(_affectedCharacter, uid, insidePlagueAura);
                }
            }
        }
    }
}
