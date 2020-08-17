using System;
using System.Collections.Generic;
using SideLoader;
using UnityEngine;

namespace Templar
{
	// Token: 0x0200000D RID: 13
	public class PrayerOfHealingSpell
	{
		// Token: 0x06000017 RID: 23 RVA: 0x00002F38 File Offset: 0x00001138
		public static Skill Init()
		{
			SL_Skill sl_Skill = new SL_Skill
			{
				Name = "Prayer of Healing",
				EffectBehaviour = EffectBehaviours.OverrideEffects,
				Target_ItemID = 8200180,
				New_ItemID = 2502014,
				Description = "WIP",
				CastType = Character.SpellCastType.CallElements,
				CastModifier = Character.SpellCastModifier.Immobilized,
				CastLocomotionEnabled = false,
				MobileCastMovementMult = -1f,
				CastSheatheRequired = 2,
				EffectTransforms = new SL_EffectTransform[]
				{
					new SL_EffectTransform
					{
						TransformName = "ActivationEffects",
						Effects = new SL_Effect[0]
					},
					new SL_EffectTransform
					{
						TransformName = "Effects",
						Effects = new SL_Effect[0]
					}
				},
				Cooldown = 30f,
				ManaCost = 600f,
				StaminaCost = 0f
			};
			Skill skill = (Skill)CustomItems.CreateCustomItem(sl_Skill.Target_ItemID, sl_Skill.New_ItemID, sl_Skill.Name, sl_Skill);
			Transform transform = skill.transform.Find("Effects");
			HealingAoE healingAoE = transform.gameObject.AddComponent<HealingAoE>();
			healingAoE.RestoredHealth = 30f;
			healingAoE.Range = 30f;
			healingAoE.CanRevive = true;
			healingAoE.AmplificationType = DamageType.Types.Electric;
			UnityEngine.Object.Destroy(skill.gameObject.GetComponentInChildren<AddStatusEffect>());
			return skill;
		}
	}
}
