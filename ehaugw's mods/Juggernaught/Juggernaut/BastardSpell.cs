using System;
using SideLoader;

namespace Juggernaut
{
	// Token: 0x02000017 RID: 23
	public class BastardSpell
	{
		// Token: 0x06000050 RID: 80 RVA: 0x0000396C File Offset: 0x00001B6C
		public static Skill Init()
		{
			SL_Skill sl_Skill = new SL_Skill
			{
				Name = "Bastard",
				EffectBehaviour = EffectBehaviours.DestroyEffects,
				Target_ItemID = 8205030,
				New_ItemID = 2502015,
				SLPackName = "Juggernaut",
				SubfolderName = "Bastard",
				Description = "Increases the speed and damage bonuses from two-handing a bastard sword.",
				IsUsable = false,
				CastType = Character.SpellCastType.NONE,
				CastModifier = Character.SpellCastModifier.Immobilized,
				CastLocomotionEnabled = false,
				MobileCastMovementMult = -1f
			};
			return (Skill)CustomItems.CreateCustomItem(sl_Skill.Target_ItemID, sl_Skill.New_ItemID, sl_Skill.Name, sl_Skill);
		}
	}
}
