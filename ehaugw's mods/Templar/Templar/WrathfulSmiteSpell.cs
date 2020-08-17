using System;
using System.Collections.Generic;
using System.Linq;
using CustomWeaponBehaviour;
using SideLoader;
using TinyHelper;
using UnityEngine;

namespace Templar
{
	// Token: 0x0200000A RID: 10
	public class WrathfulSmiteSpell
	{
		// Token: 0x06000011 RID: 17 RVA: 0x000029EC File Offset: 0x00000BEC
		public static Skill Init()
		{
			SL_AttackSkill sl_AttackSkill = new SL_AttackSkill
			{
				Name = "Wrathful Smite",
				EffectBehaviour = EffectBehaviours.OverrideEffects,
				Target_ItemID = 8100350,
				New_ItemID = 2502008,
				SLPackName = "Templar",
				SubfolderName = "Wrathful Smite",
				Description = "A leaping attack that deals more damage to enemies that are knocked down.\n\nWrathful Smite instantly becomes available for another use if it kills its target.",
				CastType = Character.SpellCastType.AxeLeap,
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
				ManaCost = 7f
			};
			Skill skill = (Skill)CustomItems.CreateCustomItem(sl_AttackSkill.Target_ItemID, sl_AttackSkill.New_ItemID, sl_AttackSkill.Name, sl_AttackSkill);
			skill.transform.Find("ActivationEffects").gameObject.AddComponent<EnableHitDetection>().Delay = 0.5f;
			UnityEngine.Object.Destroy(skill.gameObject.GetComponentInChildren<WeaponDamage>());
			ExecutionWeaponDamage executionWeaponDamage = skill.transform.Find("HitEffects").gameObject.AddComponent<ExecutionWeaponDamage>();
			executionWeaponDamage.SetCooldown = 1f;
			executionWeaponDamage.WeaponDamageMult = 1f;
			executionWeaponDamage.WeaponDamageMultKDown = 2f;
			executionWeaponDamage.WeaponDurabilityLossPercent = 0f;
			executionWeaponDamage.WeaponDurabilityLoss = 1f;
			executionWeaponDamage.OverrideDType = DamageType.Types.Count;
			executionWeaponDamage.Damages = new DamageType[]
			{
				new DamageType(DamageType.Types.Electric, 30f)
			};
			executionWeaponDamage.WeaponKnockbackMult = 2f;
			UnityEngine.Object.Destroy(skill.gameObject.GetComponentInChildren<HasStatusEffectEffectCondition>());
			UnityEngine.Object.Destroy(skill.gameObject.GetComponentInChildren<AddStatusEffect>());
			foreach (PlaySoundEffect obj in from x in skill.gameObject.GetComponentsInChildren<PlaySoundEffect>()
			where x.Sounds.Contains(GlobalAudioManager.Sounds.CS_Golem_HeavyAttackFence_Whoosh1)
			select x)
			{
				UnityEngine.Object.Destroy(obj);
			}
			return skill;
		}
	}
}
