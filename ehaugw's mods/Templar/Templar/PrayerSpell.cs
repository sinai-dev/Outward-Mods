using System;
using System.Collections.Generic;
using SideLoader;
using TinyHelper;

namespace Templar
{
	// Token: 0x0200001C RID: 28
	public class PrayerSpell
	{
		// Token: 0x06000048 RID: 72 RVA: 0x00004BE4 File Offset: 0x00002DE4
		public static Skill Init()
		{
			SL_Skill sl_Skill = new SL_Skill
			{
				Name = "Prayer",
				EffectBehaviour = EffectBehaviours.DestroyEffects,
				Target_ItemID = 8100120,
				New_ItemID = 2502012,
				SLPackName = "Templar",
				SubfolderName = "Prayer",
				Description = "Pray to Elatt.\n\nDo not expect him to reply though!",
				CastType = Character.SpellCastType.EnterInnBed,
				CastModifier = Character.SpellCastModifier.Immobilized,
				CastLocomotionEnabled = false,
				MobileCastMovementMult = -1f,
				CastSheatheRequired = 1,
				EffectTransforms = new SL_EffectTransform[]
				{
					new SL_EffectTransform
					{
						TransformName = "ActivationEffects",
						Effects = new SL_Effect[]
						{
							new SL_AddStatusEffectBuildUp
							{
								StatusEffect = "Prayer",
								Buildup = 100f,
								Delay = 0f
							}
						}
					}
				},
				Cooldown = 1f,
				StaminaCost = 0f,
				ManaCost = 0f
			};
			Skill skill = (Skill)CustomItems.CreateCustomItem(sl_Skill.Target_ItemID, sl_Skill.New_ItemID, sl_Skill.Name, sl_Skill);
			EmptyOffHandCondition.AddToSkill(skill, true, true);
			return skill;
		}

		// Token: 0x0400000E RID: 14
		public const string PRAYER_EFFECT_NAME = "Prayer";

		// Token: 0x0400000F RID: 15
		public const string PRAYER_COOLDOWN_EFFECT_NAME = "PrayOnCooldownEffect";
	}
}
