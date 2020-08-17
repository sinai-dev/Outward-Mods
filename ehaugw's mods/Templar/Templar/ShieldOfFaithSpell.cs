using System;
using SideLoader;
using TinyHelper;
using UnityEngine;

namespace Templar
{
	// Token: 0x0200000B RID: 11
	public class ShieldOfFaithSpell
	{
		// Token: 0x06000013 RID: 19 RVA: 0x00002C78 File Offset: 0x00000E78
		public static Skill Init()
		{
			SL_Skill sl_Skill = new SL_Skill
			{
				Name = "Shield of Faith",
				EffectBehaviour = 0,
				Target_ItemID = 8100360,
				New_ItemID = 2502009,
				SLPackName = "Templar",
				SubfolderName = "Shield of Faith",
				Description = "Blocks an attack, restores your stability and protects you against damage for a brief moment.",
				CastType = new Character.SpellCastType?(Character.SpellCastType.Brace),
				CastModifier = new Character.SpellCastModifier?(Character.SpellCastModifier.Immobilized),
				CastLocomotionEnabled = new bool?(false),
				MobileCastMovementMult = new float?((float)-1),
				CastSheatheRequired = new int?(0),
				Cooldown = new float?((float)15),
				StaminaCost = new float?(0f),
				ManaCost = new float?((float)7)
			};
			Skill skill = (Skill)CustomItems.CreateCustomItem(sl_Skill.Target_ItemID, sl_Skill.New_ItemID, sl_Skill.Name, sl_Skill);
			UnityEngine.Object.Destroy(skill.gameObject.GetComponentInChildren<AddBoonEffect>());
			UnityEngine.Object.Destroy(skill.gameObject.GetComponentInChildren<AutoKnock>());
			GameObject gameObject = skill.transform.Find("HitEffects").gameObject;
			gameObject.AddComponent<AddStatusEffect>().Status = ResourcesPrefabManager.Instance.GetStatusEffectPrefab("Force Bubble");
			GameObject gameObject2 = skill.transform.Find("BlockEffects").gameObject;
			gameObject2.AddComponent<CasualStagger>();
			return skill;
		}
	}
}
