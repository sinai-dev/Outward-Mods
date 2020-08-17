using System;
using SideLoader;

namespace Juggernaut
{
	// Token: 0x0200001C RID: 28
	public class FortifiedSpell
	{
		// Token: 0x0600005A RID: 90 RVA: 0x00003F90 File Offset: 0x00002190
		public static Skill Init()
		{
			SL_Skill sl_Skill = new SL_Skill
			{
				Name = "Fortified",
				EffectBehaviour = EffectBehaviours.DestroyEffects,
				Target_ItemID = 8205030,
				New_ItemID = 2502021,
				SLPackName = "Juggernaut",
				SubfolderName = "Fortified",
				Description = "Gives resistance bonuses equal to your armor protection.",
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
