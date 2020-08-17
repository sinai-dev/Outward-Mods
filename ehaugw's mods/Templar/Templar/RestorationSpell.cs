using System;
using System.Collections.Generic;
using SideLoader;

namespace Templar
{
	// Token: 0x02000006 RID: 6
	public class RestorationSpell
	{
		// Token: 0x06000009 RID: 9 RVA: 0x00002460 File Offset: 0x00000660
		public static Skill Init()
		{
			SL_Skill sl_Skill = new SL_Skill
			{
				Name = "Restoration",
				EffectBehaviour = EffectBehaviours.OverrideEffects,
				Target_ItemID = 8200180,
				New_ItemID = 2502003,
				SLPackName = "Templar",
				SubfolderName = "Restoration",
				Description = "Cures you from all hexes, bleeding and slowing effects.",
				CastType = Character.SpellCastType.CallElements,
				CastModifier = Character.SpellCastModifier.Immobilized,
				CastLocomotionEnabled = false,
				MobileCastMovementMult = 0f,
				EffectTransforms = new SL_EffectTransform[]
				{
					new SL_EffectTransform
					{
						TransformName = "Effects",
						Effects = new SL_Effect[]
						{
							new SL_RemoveStatusEffect
							{
								Status_Tag = "Hex",
								CleanseType = RemoveStatusEffect.RemoveTypes.StatusType
							},
							new SL_RemoveStatusEffect
							{
								Status_Name = "Slow Down",
								CleanseType = RemoveStatusEffect.RemoveTypes.StatusSpecific
							},
							new SL_RemoveStatusEffect
							{
								Status_Name = "Cripple",
								CleanseType = RemoveStatusEffect.RemoveTypes.StatusSpecific
							},
							new SL_RemoveStatusEffect
							{
								Status_Tag = "Bleeding",
								CleanseType = RemoveStatusEffect.RemoveTypes.StatusType
							}
						}
					}
				},
				Cooldown = 30f,
				StaminaCost = 0f,
				ManaCost = 14f,
			};
			return (Skill)CustomItems.CreateCustomItem(sl_Skill.Target_ItemID, sl_Skill.New_ItemID, sl_Skill.Name, sl_Skill);
		}
	}
}
