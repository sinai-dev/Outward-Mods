using System;
using SideLoader;

namespace Juggernaut
{
	// Token: 0x0200001E RID: 30
	public class UnyieldingSpell
	{
		// Token: 0x0600005E RID: 94 RVA: 0x0000412C File Offset: 0x0000232C
		public static Skill Init()
		{
			SL_Skill sl_Skill = new SL_Skill
			{
				Name = "Unyielding",
				EffectBehaviour = EffectBehaviours.DestroyEffects,
				Target_ItemID = 8205030,
				New_ItemID = 2502019,
				SLPackName = "Juggernaut",
				SubfolderName = "Unyielding",
				Description = "Enables you to block incoming attacks sooner after attacking.\n\nBe aware that learning this skill has impact on most other Juggernaut skills!",
				IsUsable = false,
				CastType = Character.SpellCastType.NONE,
				CastModifier = Character.SpellCastModifier.Immobilized,
				CastLocomotionEnabled = false,
				MobileCastMovementMult = -1f,
			};
			return (Skill)CustomItems.CreateCustomItem(sl_Skill.Target_ItemID, sl_Skill.New_ItemID, sl_Skill.Name, sl_Skill);
		}

		// Token: 0x0400001E RID: 30
		public const string NAME = "Unyielding";
	}
}
