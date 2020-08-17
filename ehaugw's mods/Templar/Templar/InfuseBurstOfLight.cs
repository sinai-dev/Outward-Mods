using System;
using System.Collections.Generic;
using SideLoader;
using UnityEngine;

namespace Templar
{
	// Token: 0x02000005 RID: 5
	public class InfuseBurstOfLight
	{
		// Token: 0x06000007 RID: 7 RVA: 0x000022B4 File Offset: 0x000004B4
		public static Skill Init()
		{
			SL_AttackSkill sl_AttackSkill = new SL_AttackSkill
			{
				Name = "Infuse Burst of Light",
				EffectBehaviour = EffectBehaviours.OverrideEffects,
				Target_ItemID = 8200100,
				New_ItemID = 2502001,
				SLPackName = "Templar",
				SubfolderName = "Infuse Burst of Light",
				Description = "Temporarly infuse your weapon with light for, without consuming your Blessed boon.",
				IsUsable = true,
				CastType = Character.SpellCastType.SpellBindLight,
				CastModifier = Character.SpellCastModifier.Immobilized,
				CastLocomotionEnabled = false,
				MobileCastMovementMult = -1f,
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
						Effects = new SL_Effect[]
						{
							new SL_ImbueWeapon
							{
								Lifespan = 60f,
								ImbueEffect_Preset_ID = 219,
								Imbue_Slot = Weapon.WeaponSlot.MainHand
							}
						}
					}
				},
				Cooldown = new float?((float)2),
				StaminaCost = new float?(0f),
				ManaCost = new float?((float)7)
			};
			Skill skill = (Skill)CustomItems.CreateCustomItem(sl_AttackSkill.Target_ItemID, sl_AttackSkill.New_ItemID, sl_AttackSkill.Name, sl_AttackSkill);
			UnityEngine.Object.Destroy(skill.gameObject.GetComponentInChildren<HasStatusEffectEffectCondition>());
			return skill;
		}
	}
}
