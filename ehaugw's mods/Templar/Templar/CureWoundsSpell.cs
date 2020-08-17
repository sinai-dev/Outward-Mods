using System;
using System.Collections.Generic;
using HarmonyLib;
using SideLoader;
using UnityEngine;

namespace Templar
{
	// Token: 0x02000007 RID: 7
	public class CureWoundsSpell
	{
		// Token: 0x0600000B RID: 11 RVA: 0x000025F0 File Offset: 0x000007F0
		public static Skill Init()
		{
			SL_Skill sl_Skill = new SL_Skill
			{
				Name = "Cure Wounds",
				EffectBehaviour = EffectBehaviours.OverrideEffects,
				Target_ItemID = 8200180,
				New_ItemID = 2502000,
				SLPackName = "Templar",
				SubfolderName = "Cure Wounds",
				Description = "Restores some health for you and your nearby allies.",
				CastType = Character.SpellCastType.Fast,
				CastModifier = Character.SpellCastModifier.Mobile,
				CastLocomotionEnabled = true,
				MobileCastMovementMult = 0.5f,
				EffectTransforms = new SL_EffectTransform[]
				{
					new SL_EffectTransform
					{
						TransformName = "Effects",
						Effects = new SL_Effect[0],
					}
				},
				Cooldown = 5f,
				StaminaCost = 0f,
				ManaCost = 14f,
			};
			Skill skill = (Skill)CustomItems.CreateCustomItem(sl_Skill.Target_ItemID, sl_Skill.New_ItemID, sl_Skill.Name, sl_Skill);
			HealingAoE healingAoE = skill.transform.Find("Effects").gameObject.AddComponent<HealingAoE>();
			healingAoE.Range = 30f;
			healingAoE.RestoredHealth = 7f;
			healingAoE.AmplificationType = DamageType.Types.Electric;
			healingAoE.CanRevive = false;
			return skill;
		}

		// Token: 0x0200001E RID: 30
		[HarmonyPatch(typeof(Character), "SpellCastAnim")]
		public class Character_SpellCastAnim
		{
			// Token: 0x06000050 RID: 80 RVA: 0x000051D9 File Offset: 0x000033D9
			[HarmonyPrefix]
			public static void Prefix(Character __instance, ref Character.SpellCastType _type, ref Character.SpellCastModifier _modifier, Animator ___m_animator, int _sheatheRequired = 1)
			{
			}
		}
	}
}
