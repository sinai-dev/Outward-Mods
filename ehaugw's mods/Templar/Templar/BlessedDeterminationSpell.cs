using System;
using SideLoader;

namespace Templar
{
	// Token: 0x02000004 RID: 4
	public class BlessedDeterminationSpell
	{
		// Token: 0x06000005 RID: 5 RVA: 0x000021B8 File Offset: 0x000003B8
		public static Skill Init()
		{
			SL_Skill sl_Skill = new SL_Skill
			{
				Name = "Blessed Determination",
				EffectBehaviour = EffectBehaviours.OverrideEffects,
				Target_ItemID = 8205030,
				New_ItemID = 2502002,
				SLPackName = "Templar",
				SubfolderName = "Blessed Determination",
				Description = "While under the effect of the Blessed boon, all spent mana is regained as stamina, and spending stamina builds up a Burst of Divinity effect that reduces the mana cost of the next spell you cast.",
				IsUsable = false,
				CastType = Character.SpellCastType.NONE,
				CastModifier = Character.SpellCastModifier.Immobilized,
				CastLocomotionEnabled = false,
				MobileCastMovementMult = -1f,
				Cooldown = 0f,
				StaminaCost = 0f,
				ManaCost = 0f
			};
			return (Skill)CustomItems.CreateCustomItem(sl_Skill.Target_ItemID, sl_Skill.New_ItemID, sl_Skill.Name, sl_Skill);
		}
	}
}
