using System;
using System.Collections.Generic;
using SideLoader;
using UnityEngine;

namespace Juggernaut
{
	// Token: 0x02000019 RID: 25
	public class JuggernaughtySpell
	{
		// Token: 0x06000054 RID: 84 RVA: 0x00003B78 File Offset: 0x00001D78
		public static Skill Init()
		{
			SL_Skill sl_Skill = new SL_Skill
			{
				Name = "Juggernaughty",
				EffectBehaviour = EffectBehaviours.DestroyEffects,
				Target_ItemID = 8100120,
				New_ItemID = 2502023,
				Description = "Trainer Hax",
				CastType = Character.SpellCastType.Sit,
				CastModifier = Character.SpellCastModifier.Immobilized,
				CastLocomotionEnabled = true,
				MobileCastMovementMult = -1f,
				EffectTransforms = new SL_EffectTransform[]
				{
					new SL_EffectTransform
					{
						TransformName = "Effects",
						Effects = new SL_Effect[0],
					}
				},
				Cooldown = 1f,
			};
			Skill skill = (Skill)CustomItems.CreateCustomItem(sl_Skill.Target_ItemID, sl_Skill.New_ItemID, sl_Skill.Name, sl_Skill);
			Transform transform = skill.transform.Find("Effects");
			transform.gameObject.AddComponent<JuggernaughtyEffect>();
			return skill;
		}
	}
}
