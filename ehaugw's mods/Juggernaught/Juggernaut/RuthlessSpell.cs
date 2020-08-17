using System;
using SideLoader;

namespace Juggernaut
{
	// Token: 0x0200001B RID: 27
	public class RuthlessSpell
	{
		// Token: 0x06000058 RID: 88 RVA: 0x00003EC0 File Offset: 0x000020C0
		public static Skill Init()
		{
			SL_Skill sl_Skill = new SL_Skill
			{
				Name = "Ruthless",
				EffectBehaviour = EffectBehaviours.OverrideEffects,
				Target_ItemID = 8205030,
				New_ItemID = 2502022,
				SLPackName = "Juggernaut",
				SubfolderName = "Ruthless",
				Description = string.Format("{0}: Armor stamina and movement penalties are reduced. Damaging enemies causes confusion among their allies, and may even cause them to stagger in fear.\n\n{1}: While enraged, weapon damage is increased and the attack stamina cost is reduced, but you can't be affected by boons other than Rage.", "Unyielding", "Vengeful"),
				CastType = Character.SpellCastType.Rage,
				CastModifier = Character.SpellCastModifier.Immobilized,
				CastLocomotionEnabled = false,
				MobileCastMovementMult = 0f
			};
			return (Skill)CustomItems.CreateCustomItem(sl_Skill.Target_ItemID, sl_Skill.New_ItemID, sl_Skill.Name, sl_Skill);
		}

		// Token: 0x0400001C RID: 28
		public const float Range = 7f;
	}
}
