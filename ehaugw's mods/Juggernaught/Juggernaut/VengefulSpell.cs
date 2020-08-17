using System;
using SideLoader;

namespace Juggernaut
{
	// Token: 0x0200001D RID: 29
	public class VengefulSpell
	{
		// Token: 0x0600005C RID: 92 RVA: 0x00004064 File Offset: 0x00002264
		public static Skill Init()
		{
			SL_Skill sl_Skill = new SL_Skill
			{
				Name = "Vengeful",
				EffectBehaviour = EffectBehaviours.DestroyEffects,
				Target_ItemID = 8205030,
				New_ItemID = 2502020,
				SLPackName = "Juggernaut",
				SubfolderName = "Vengeful",
				Description = "Being damaged causes rage to build up.\n\nBe aware that learning this skill has impact on most other Juggernaut skills!",
				IsUsable = false,
				CastType = Character.SpellCastType.NONE,
				CastModifier = Character.SpellCastModifier.Immobilized,
				CastLocomotionEnabled = false,
				MobileCastMovementMult = -1f
			};
			return (Skill)CustomItems.CreateCustomItem(sl_Skill.Target_ItemID, sl_Skill.New_ItemID, sl_Skill.Name, sl_Skill);
		}

		// Token: 0x0400001D RID: 29
		public const string NAME = "Vengeful";
	}
}
