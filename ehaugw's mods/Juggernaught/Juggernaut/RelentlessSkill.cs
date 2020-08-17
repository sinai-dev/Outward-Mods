using System;
using SideLoader;

namespace Juggernaut
{
	// Token: 0x0200001F RID: 31
	public class RelentlessSkill
	{
		// Token: 0x06000060 RID: 96 RVA: 0x000041F4 File Offset: 0x000023F4
		public static Skill Init()
		{
			SL_Skill sl_Skill = new SL_Skill
			{
				Name = "Relentless",
				EffectBehaviour = EffectBehaviours.DestroyEffects,
				Target_ItemID = 8205030,
				New_ItemID = 2502018,
				Description = "Reduces armor movement penalties and gives bonus physical resistance equal to your armor protection.",
				IsUsable = false,
				CastType = Character.SpellCastType.NONE,
				CastModifier = Character.SpellCastModifier.Immobilized,
				CastLocomotionEnabled = false,
				MobileCastMovementMult = -1f,
			};
			return (Skill)CustomItems.CreateCustomItem(sl_Skill.Target_ItemID, sl_Skill.New_ItemID, sl_Skill.Name, sl_Skill);
		}
	}
}
