using System;
using System.Collections.Generic;
using SideLoader;
using TinyHelper;
using UnityEngine;

namespace Templar
{
	// Token: 0x0200000C RID: 12
	public class ChannelDivinitySpell
	{
		// Token: 0x06000015 RID: 21 RVA: 0x00002DDC File Offset: 0x00000FDC
		public static Skill Init()
		{
			SL_Skill sl_Skill = new SL_Skill
			{
				Name = "Channel Divinity",
				EffectBehaviour = EffectBehaviours.OverrideEffects,
				Target_ItemID = 8200180,
				New_ItemID = 2502010,
				SLPackName = "Templar",
				SubfolderName = "Channel Divinity",
				Description = "Heals you and your allies, or produces combo effects when casted in combination with a Rune spell.",
				CastType = Character.SpellCastType.CallElements,
				CastModifier = Character.SpellCastModifier.Immobilized,
				CastLocomotionEnabled = false,
				MobileCastMovementMult = -1f,
				CastSheatheRequired = 1,
				EffectTransforms = new SL_EffectTransform[]
				{
					new SL_EffectTransform
					{
						TransformName = "Effects",
						Effects = new SL_Effect[0],
					}
				},
				Cooldown = 300f,
				ManaCost = 14f,
				StaminaCost = 0f
			};
			Skill skill = (Skill)CustomItems.CreateCustomItem(sl_Skill.Target_ItemID, sl_Skill.New_ItemID, sl_Skill.Name, sl_Skill);
			EmptyOffHandCondition.AddToSkill(skill, true, true);
			Transform transform = skill.transform.Find("Effects");
			transform.gameObject.AddComponent<ChannelDivinity>();
			UnityEngine.Object.Destroy(skill.gameObject.GetComponentInChildren<AddStatusEffect>());
			return skill;
		}
	}
}
