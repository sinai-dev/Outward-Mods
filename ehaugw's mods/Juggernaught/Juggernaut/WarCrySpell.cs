using System;
using System.Collections.Generic;
using SideLoader;

namespace Juggernaut
{
	// Token: 0x02000018 RID: 24
	public class WarCrySpell
	{
		// Token: 0x06000052 RID: 82 RVA: 0x00003A34 File Offset: 0x00001C34
		public static Skill Init()
		{
			SL_Skill sl_Skill = new SL_Skill
			{
				Name = "War Cry",
				EffectBehaviour = EffectBehaviours.OverrideEffects,
				Target_ItemID = 8200110,
				New_ItemID = 2502025,
				SLPackName = "Juggernaut",
				SubfolderName = "WarCry",
				Description = string.Format("Unleash a terrifying roar that staggers nearby enemies.\n\n{0}: Applies confusion to affected enemies.\n\n{1}: Applies pain to affected enemies.", "Unyielding", "Vengeful"),
				CastType = Character.SpellCastType.Rage,
				CastModifier = Character.SpellCastModifier.Immobilized,
				CastLocomotionEnabled = false,
				MobileCastMovementMult = 0f,
				EffectTransforms = new SL_EffectTransform[]
				{
					new SL_EffectTransform
					{
						TransformName = "Effects",
						Effects = new SL_Effect[0],
					}
				},
				Cooldown = 100f,
				StaminaCost = 0f,
				ManaCost = 0f
			};
			Skill skill = (Skill)CustomItems.CreateCustomItem(sl_Skill.Target_ItemID, sl_Skill.New_ItemID, sl_Skill.Name, sl_Skill);
			skill.transform.Find("Effects").gameObject.AddComponent<WarCryEffect>();
			return skill;
		}
	}
}
