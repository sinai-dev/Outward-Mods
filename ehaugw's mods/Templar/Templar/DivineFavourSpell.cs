using System;
using SideLoader;
using UnityEngine;

namespace Templar
{
	// Token: 0x02000008 RID: 8
	public class DivineFavourSpell
	{
		// Token: 0x0600000D RID: 13 RVA: 0x00002744 File Offset: 0x00000944
		public static Skill Init()
		{
			SL_Skill sl_Skill = new SL_Skill
			{
				Name = "Divine Favour",
				EffectBehaviour = EffectBehaviours.OverrideEffects,
				Target_ItemID = 8205030,
				New_ItemID = 2502026,
				SLPackName = "Templar",
				SubfolderName = "Divine Favour",
				Description = "Augments Divine Light Imbue to cause a Radiating effect, which damages the victim and those in its proximity over time.",
				IsUsable = false,
				CastType = Character.SpellCastType.NONE,
				CastModifier = Character.SpellCastModifier.Immobilized,
				CastLocomotionEnabled = false,
				MobileCastMovementMult = -1f,
				Cooldown = 0f,
				StaminaCost = 0f,
				ManaCost = 0f,
			};
			Skill skill = (Skill)CustomItems.CreateCustomItem(sl_Skill.Target_ItemID, sl_Skill.New_ItemID, sl_Skill.Name, sl_Skill);
			UnityEngine.Object.Destroy(skill.gameObject.GetComponentInChildren<HasStatusEffectEffectCondition>());
			return skill;
		}
	}
}
