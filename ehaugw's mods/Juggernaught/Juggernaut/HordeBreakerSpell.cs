using System;
using System.Collections.Generic;
using CustomWeaponBehaviour;
using SideLoader;
using UnityEngine;

namespace Juggernaut
{
	// Token: 0x0200001A RID: 26
	public class HordeBreakerSpell
	{
		// Token: 0x06000056 RID: 86 RVA: 0x00003C78 File Offset: 0x00001E78
		public static Skill Init()
		{
			SL_AttackSkill sl_AttackSkill = new SL_AttackSkill
			{
				Name = "Horde Breaker",
				EffectBehaviour = EffectBehaviours.OverrideEffects,
				Target_ItemID = 8100320,
				New_ItemID = 2502024,
				SLPackName = "Juggernaut",
				SubfolderName = "HordeBreaker",
				Description = string.Format("Does two attacks in wide archs that stagger on hit.\n\n{0}: Confused enemies are knocked down.\n\n{1}: Enemies in pain are slowed down.", "Unyielding", "Vengeful"),
				CastType = new Character.SpellCastType?(Character.SpellCastType.WeaponSkill1),
				CastModifier = new Character.SpellCastModifier?(Character.SpellCastModifier.Attack),
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
				EffectTransforms = new SL_EffectTransform[]
				{
					new SL_EffectTransform
					{
						TransformName = "Effects",
						Effects = new SL_Effect[0],
					}
				},
				Cooldown = 60f,
				StaminaCost = 16f,
				ManaCost = 0f
			};
			Skill skill = (Skill)CustomItems.CreateCustomItem(sl_AttackSkill.Target_ItemID, sl_AttackSkill.New_ItemID, sl_AttackSkill.Name, sl_AttackSkill);
			UnityEngine.Object.Destroy(skill.transform.Find("HitEffects_Rage").gameObject);
			UnityEngine.Object.Destroy(skill.transform.Find("HitEffects_Discipline").gameObject);
			WeaponSkillAnimationSelector.SetCustomAttackAnimation(skill, Weapon.WeaponType.Halberd_2H);
			Transform transform = skill.transform.Find("HitEffects");
			transform.gameObject.AddComponent<HordeBreakerEffect>();
			WeaponDamage componentInChildren = skill.gameObject.GetComponentInChildren<WeaponDamage>();
			componentInChildren.WeaponDamageMult = 1f;
			componentInChildren.WeaponDurabilityLossPercent = 0f;
			componentInChildren.WeaponDurabilityLoss = 1f;
			componentInChildren.Damages = new DamageType[0];
			componentInChildren.WeaponKnockbackMult = 1f;
			UnityEngine.Object.Destroy(skill.gameObject.GetComponentInChildren<HasStatusEffectEffectCondition>());
			UnityEngine.Object.Destroy(skill.gameObject.GetComponentInChildren<AddStatusEffect>());
			return skill;
		}
	}
}
