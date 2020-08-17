using System;
using System.Collections.Generic;
using BepInEx;
using SideLoader;
using TinyHelper;

namespace SkilledAtSitting
{
	// Token: 0x02000003 RID: 3
	[BepInPlugin("com.ehaugw.skilledatsitting", "Skilled at Sitting", "3.0.1")]
	public class SkilledAtSitting : BaseUnityPlugin
	{
		// Token: 0x06000003 RID: 3 RVA: 0x0000215F File Offset: 0x0000035F
		internal void Awake()
		{
			SL.OnPacksLoaded += this.OnPackLoaded;
			SL.OnSceneLoaded += this.OnSceneLoaded;
		}

		// Token: 0x06000004 RID: 4 RVA: 0x00002188 File Offset: 0x00000388
		private void OnSceneLoaded()
		{
			foreach (Character character in CharacterManager.Instance.Characters.Values)
			{
				bool flag = !character.IsAI && character.Inventory != null && !character.Inventory.LearnedSkill(this.sitInstance);
				if (flag)
				{
					character.Inventory.ReceiveSkillReward(this.sitInstance.ItemID);
				}
			}
		}

		// Token: 0x06000005 RID: 5 RVA: 0x0000222C File Offset: 0x0000042C
		private void OnPackLoaded()
		{
			this.MakeSittingPrefab();
			this.sitInstance = this.Sit();
		}

		// Token: 0x06000006 RID: 6 RVA: 0x00002244 File Offset: 0x00000444
		private Skill Sit()
		{
			SL_Skill sl_Skill = new SL_Skill
			{
				Name = "Sit",
				EffectBehaviour = EffectBehaviours.OverrideEffects,
				Target_ItemID = 8100120,
				New_ItemID = 2502011,
				SLPackName = "SkilledAtSitting",
				SubfolderName = "Sit",
				Description = "Sit down to regain health, burnt health and burnt stamina.",
				CastType = new Character.SpellCastType?(Character.SpellCastType.Sit),
				CastModifier = new Character.SpellCastModifier?(Character.SpellCastModifier.Immobilized),
				CastLocomotionEnabled = new bool?(true),
				MobileCastMovementMult = new float?(-1f),
				EffectTransforms = new SL_EffectTransform[]
				{
					new SL_EffectTransform
					{
						TransformName = "Effects",
						Effects = new SL_Effect[]
						{
							new SL_AddStatusEffect
							{
								StatusEffect = SkilledAtSitting.SITTING_EFFECT_NAME,
								ChanceToContract = 100,
								Delay = 2f
							}
						}
					}
				},
				Cooldown = new float?((float)1),
				StaminaCost = new float?(0f)
			};
			return (Skill)CustomItems.CreateCustomItem(sl_Skill.Target_ItemID, sl_Skill.New_ItemID, sl_Skill.Name, sl_Skill);
		}

		// Token: 0x06000007 RID: 7 RVA: 0x00002370 File Offset: 0x00000570
		private void MakeSittingPrefab()
		{
			StatusEffect statusEffect = TinyEffectManager.MakeStatusEffectPrefab(SkilledAtSitting.SITTING_EFFECT_NAME, SkilledAtSitting.SITTING_EFFECT_NAME, "Regain health, burnt health and stamina while sitting.", -1f, 1f, StatusEffectFamily.StackBehaviors.Override, "Bandage", false, null, null, "com.ehaugw.skilledatsitting");
			EffectSignature statusEffectSignature = statusEffect.StatusEffectSignature;
			Sitting sitting = TinyEffectManager.MakeFreshObject("Effects", true, true, statusEffectSignature.transform).AddComponent<Sitting>();
			sitting.UseOnce = false;
			statusEffectSignature.Effects = new List<Effect>
			{
				sitting
			};
			statusEffect.IsHidden = false;
			statusEffect.DisplayInHud = true;
		}

		// Token: 0x04000002 RID: 2
		public const string GUID = "com.ehaugw.skilledatsitting";

		// Token: 0x04000003 RID: 3
		public const string VERSION = "3.0.1";

		// Token: 0x04000004 RID: 4
		public const string NAME = "Skilled at Sitting";

		// Token: 0x04000005 RID: 5
		public Skill sitInstance;

		// Token: 0x04000006 RID: 6
		public static string SITTING_EFFECT_NAME = "Sitting";
	}
}
