using System;
using System.Collections.Generic;
using SideLoader;
using UnityEngine;

namespace Templar
{
	// Token: 0x02000003 RID: 3
	public class CelestialSurgeSpell
	{
		// Token: 0x06000003 RID: 3 RVA: 0x00002068 File Offset: 0x00000268
		public static Skill Init()
		{
			SL_Skill sl_Skill = new SL_Skill
			{
				Name = "Celestial Surge",
				EffectBehaviour = EffectBehaviours.OverrideEffects,
				Target_ItemID = 8200180,
				New_ItemID = 2502013,
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
			};
			Skill skill = (Skill)CustomItems.CreateCustomItem(sl_Skill.Target_ItemID, sl_Skill.New_ItemID, sl_Skill.Name, sl_Skill);
			Transform transform = skill.transform.Find("Effects");
			transform.gameObject.AddComponent<CelestialSurge>();
			UnityEngine.Object.Destroy(skill.gameObject.GetComponentInChildren<AddStatusEffect>());
			return skill;
		}

		// Token: 0x04000001 RID: 1
		public const float DAMAGE = 40f;

		// Token: 0x04000002 RID: 2
		public const float RANGE = 35f;
	}
}
