using System;
using System.Collections.Generic;
using SideLoader;
using UnityEngine;

namespace Juggernaut
{
	// Token: 0x02000021 RID: 33
	public class ParrySpell
	{
		// Token: 0x06000064 RID: 100 RVA: 0x00004548 File Offset: 0x00002748
		public static Skill Init()
		{
			SL_AttackSkill sl_AttackSkill = new SL_AttackSkill
			{
				Name = "Parry",
				EffectBehaviour = EffectBehaviours.OverrideEffects,
				Target_ItemID = 8100360,
				New_ItemID = 2502016,
				Description = "Completely block a physical attack.",
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
				Cooldown = 3f,
				StaminaCost = 4f,
				ManaCost = 0f
			};
			Skill skill = (Skill)CustomItems.CreateCustomItem(sl_AttackSkill.Target_ItemID, sl_AttackSkill.New_ItemID, sl_AttackSkill.Name, sl_AttackSkill);
			Transform transform = skill.gameObject.transform.FindInAllChildren("HitEffects");
			UnityEngine.Object.Destroy(transform.gameObject);
			return skill;
		}
	}
}
