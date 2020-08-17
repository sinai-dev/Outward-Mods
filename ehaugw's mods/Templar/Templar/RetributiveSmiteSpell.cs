using System;
using System.Collections.Generic;
using SideLoader;

namespace Templar
{
	// Token: 0x02000009 RID: 9
	public class RetributiveSmiteSpell
	{
		// Token: 0x0600000F RID: 15 RVA: 0x00002850 File Offset: 0x00000A50
		public static Skill Init()
		{
			SL_AttackSkill sl_AttackSkill = new SL_AttackSkill
			{
				Name = "Retributive Smite",
				EffectBehaviour = EffectBehaviours.NONE,
				Target_ItemID = 8100260,
				New_ItemID = 2502006,
				SLPackName = "Templar",
				SubfolderName = "Retributive Smite",
				Description = "Completely block a physical attack, striking the attacker and dealing additional lightning damage.",
				CastType = Character.SpellCastType.Counter,
				CastModifier = Character.SpellCastModifier.Attack,
				CastLocomotionEnabled = false,
				MobileCastMovementMult = -1f,
				CastSheatheRequired = 2,
				RequiredWeaponTypes = new Weapon.WeaponType[]
				{
					Weapon.WeaponType.Axe_1H,
					Weapon.WeaponType.Axe_2H,
					Weapon.WeaponType.Sword_1H,
					Weapon.WeaponType.Sword_2H,
					Weapon.WeaponType.Mace_1H,
					Weapon.WeaponType.Mace_2H,
					Weapon.WeaponType.Halberd_2H,
					Weapon.WeaponType.Spear_2H
				},
				Cooldown = 40f,
				StaminaCost = 8f,
				ManaCost = 7f,
			};
			Skill skill = (Skill)CustomItems.CreateCustomItem(sl_AttackSkill.Target_ItemID, sl_AttackSkill.New_ItemID, sl_AttackSkill.Name, sl_AttackSkill);
			WeaponDamage componentInChildren = skill.gameObject.GetComponentInChildren<WeaponDamage>();
			componentInChildren.WeaponDamageMult = 1.5f;
			componentInChildren.WeaponDamageMultKDown = -1f;
			componentInChildren.WeaponDurabilityLossPercent = 0f;
			componentInChildren.WeaponDurabilityLoss = 1f;
			componentInChildren.OverrideDType = DamageType.Types.Count;
			componentInChildren.Damages = new DamageType[]
			{
				new DamageType(DamageType.Types.Electric, 20f)
			};
			return skill;
		}
	}
}
