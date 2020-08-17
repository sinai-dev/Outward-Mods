using System;
using System.Collections.Generic;
using BepInEx;
using HarmonyLib;
using SideLoader;
using UnityEngine;

namespace Templar
{
	// Token: 0x0200001D RID: 29
	[BepInPlugin("com.ehaugw.templarclass", "Templar Class", "3.0.1")]
	[BepInDependency("com.sinai.SideLoader", BepInDependency.DependencyFlags.HardDependency)]
	[BepInDependency("com.ehaugw.tinyhelper", "1.0.0")]
	[BepInDependency("com.ehaugw.customweaponbehaviour", "2.0.0")]
	public class Templar : BaseUnityPlugin
	{
		// Token: 0x0600004A RID: 74 RVA: 0x00004D40 File Offset: 0x00002F40
		public static float GetFreeCastBuildup(Character character)
		{
			bool? flag;
			if (character == null)
			{
				flag = null;
			}
			else
			{
				StatusEffectManager statusEffectMngr = character.StatusEffectMngr;
				flag = ((statusEffectMngr != null) ? new bool?(statusEffectMngr.HasStatusEffect(Templar.Instance.surgeOfDivinityInstance.IdentifierName)) : null);
			}
			bool? flag2 = flag;
			return (float)((flag2.GetValueOrDefault() ? 2 : 1) * 100) * 0.3f / 7f;
		}

		// Token: 0x0600004B RID: 75 RVA: 0x00004DAC File Offset: 0x00002FAC
		internal void Awake()
		{
			Templar.Instance = this;
			SL.OnPacksLoaded += this.OnPackLoaded;
			SL.OnSceneLoaded += this.OnSceneLoaded;
			Harmony harmony = new Harmony("com.ehaugw.templarclass");
			harmony.PatchAll();
		}

		// Token: 0x0600004C RID: 76 RVA: 0x00004DF8 File Offset: 0x00002FF8
		private void OnPackLoaded()
		{
			try
			{
				this.radiatingInstance = EffectInitializer.MakeRadiatingPrefab();
				this.burstOfDivinityInstance = EffectInitializer.MakeBurstOfDivinityPrefab();
				this.surgeOfDivinityInstance = EffectInitializer.MakeSurgeOfDivinityPrefab();
				EffectInitializer.MakeRadiantLightInfusion();
				this.prayerStatusEffectInstance = EffectInitializer.MakePrayerPrefab();
				this.prayerCooldownStatusEffectInstance = EffectInitializer.MakePrayerCooldownPrefab();
			}
			catch (Exception ex)
			{
				Debug.Log(string.Format("Unhandled {0} on client callback: {1}", ex.GetType(), ex.Message));
			}
			this.cureWoundsInstance = CureWoundsSpell.Init();
			this.divineFavourInstance = DivineFavourSpell.Init();
			this.infuseBurstOfLightInstance = InfuseBurstOfLight.Init();
			this.blessedDeterminationInstance = BlessedDeterminationSpell.Init();
			this.restorationInstance = RestorationSpell.Init();
			this.retrubutiveSmiteInstance = RetributiveSmiteSpell.Init();
			this.wrathfulSmiteInstance = WrathfulSmiteSpell.Init();
			this.shieldofFaithInstance = ShieldOfFaithSpell.Init();
			this.channelDivinityInstance = ChannelDivinitySpell.Init();
			this.prayerInstance = PrayerSpell.Init();
			this.celestialSurgeInstance = CelestialSurgeSpell.Init();
			this.prayerOfHealingInstance = PrayerOfHealingSpell.Init();
			this.thunderousSmiteInstance = this.ThunderousSmite();
			TemplarSkillTree.SetupSkillTree(ref Templar.templarTreeInstance);
		}

		// Token: 0x0600004D RID: 77 RVA: 0x00004F14 File Offset: 0x00003114
		private void OnSceneLoaded()
		{
			SetupTrainers.SetupRufusInteraction();
			SetupTrainers.SetupAltarInteraction(ref this.altarTrainer, ref Templar.templarTreeInstance);
		}

		// Token: 0x0600004E RID: 78 RVA: 0x00004F30 File Offset: 0x00003130
		private Skill ThunderousSmite()
		{
			SL_AttackSkill sl_AttackSkill = new SL_AttackSkill
			{
				Name = "Thunderous Smite",
				EffectBehaviour = EffectBehaviours.OverrideEffects,
				Target_ItemID = 8100020,
				New_ItemID = 2502004,
				SLPackName = "Templar",
				SubfolderName = "Thunderous Smite",
				Description = "Required: Melee Weapon\n\nJumping attack that summons a lightning bolt upon landing.",
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
				EffectTransforms = new SL_EffectTransform[]
				{
					new SL_EffectTransform
					{
						TransformName = "Effects",
						Effects = new SL_Effect[0]
					},
					new SL_EffectTransform
					{
						TransformName = "ActivationEffects",
						Effects = new SL_Effect[0]
					}
				},
				Cooldown = 2f,
				StaminaCost = 1f,
				ManaCost = 1f
			};
			Skill skill = (Skill)CustomItems.CreateCustomItem(sl_AttackSkill.Target_ItemID, sl_AttackSkill.New_ItemID, sl_AttackSkill.Name, sl_AttackSkill);
			Item itemPrefab = ResourcesPrefabManager.Instance.GetItemPrefab(8100150);
			ShootBlastsProximity componentInChildren = itemPrefab.transform.Find("Lightning").Find("ExtraEffects").GetComponentInChildren<ShootBlastsProximity>();
			Transform transform = skill.transform.Find("Effects");
			ShootBlast shootBlast = transform.gameObject.AddComponent<ShootBlast>();
			shootBlast.transform.parent = transform.transform;
			shootBlast.BaseBlast = componentInChildren.BaseBlast;
			shootBlast.InstanstiatedAmount = 5;
			shootBlast.NoTargetForwardMultiplier = 5f;
			shootBlast.CastPosition = Shooter.CastPositionType.Character;
			shootBlast.TargetType = Shooter.TargetTypes.Enemies;
			shootBlast.TransformName = "ShooterTransform";
			shootBlast.UseTargetCharacterPositionType = false;
			shootBlast.SyncType = Effect.SyncTypes.OwnerSync;
			shootBlast.OverrideEffectCategory = EffectSynchronizer.EffectCategories.None;
			shootBlast.BasePotencyValue = 1f;
			shootBlast.Delay = 0f;
			shootBlast.LocalCastPositionAdd = new Vector3(0f, 1f, 1.5f);
			shootBlast.BaseBlast.Radius = 3.5f;
			GameObject gameObject = skill.gameObject.GetComponentInChildren<HasStatusEffectEffectCondition>().gameObject;
			UnityEngine.Object.Destroy(gameObject);
			return skill;
		}

		// Token: 0x04000010 RID: 16
		public static Templar Instance;

		// Token: 0x04000011 RID: 17
		public const float BLESSED_DETERMINATION_MANA_REGEN = 0.25f;

		// Token: 0x04000012 RID: 18
		public const float BLESSED_DETERMINATION_STAMINA_REGEN = 1f;

		// Token: 0x04000013 RID: 19
		public const float BLESSED_DETERMINATION_EFFICIENCY = 0.3f;

		// Token: 0x04000014 RID: 20
		public const float FREECAST_PROVIDED_MANA = 7f;

		// Token: 0x04000015 RID: 21
		public const float FREECAST_LIFESPAN = 30f;

		// Token: 0x04000016 RID: 22
		public const string GUID = "com.ehaugw.templarclass";

		// Token: 0x04000017 RID: 23
		public const string VERSION = "3.0.1";

		// Token: 0x04000018 RID: 24
		public const string NAME = "Templar Class";

		// Token: 0x04000019 RID: 25
		public Skill cureWoundsInstance;

		// Token: 0x0400001A RID: 26
		public Skill divineFavourInstance;

		// Token: 0x0400001B RID: 27
		public Skill infuseBurstOfLightInstance;

		// Token: 0x0400001C RID: 28
		public Skill blessedDeterminationInstance;

		// Token: 0x0400001D RID: 29
		public Skill restorationInstance;

		// Token: 0x0400001E RID: 30
		public Skill thunderousSmiteInstance;

		// Token: 0x0400001F RID: 31
		public Skill wrathfulSmiteInstance;

		// Token: 0x04000020 RID: 32
		public Skill retrubutiveSmiteInstance;

		// Token: 0x04000021 RID: 33
		public Skill blessingOfProtectionInstance;

		// Token: 0x04000022 RID: 34
		public Skill shieldofFaithInstance;

		// Token: 0x04000023 RID: 35
		public Skill channelDivinityInstance;

		// Token: 0x04000024 RID: 36
		public Skill prayerInstance;

		// Token: 0x04000025 RID: 37
		public Skill celestialSurgeInstance;

		// Token: 0x04000026 RID: 38
		public Skill prayerOfHealingInstance;

		// Token: 0x04000027 RID: 39
		public static SkillSchool templarTreeInstance;

		// Token: 0x04000028 RID: 40
		public StatusEffect radiatingInstance;

		// Token: 0x04000029 RID: 41
		public StatusEffect burstOfDivinityInstance;

		// Token: 0x0400002A RID: 42
		public StatusEffect surgeOfDivinityInstance;

		// Token: 0x0400002B RID: 43
		public StatusEffect prayerStatusEffectInstance;

		// Token: 0x0400002C RID: 44
		public StatusEffect prayerCooldownStatusEffectInstance;

		// Token: 0x0400002D RID: 45
		public Trainer altarTrainer;

		// Token: 0x02000027 RID: 39
		[HarmonyPatch(typeof(TOD_Sky), "UpdateShaderProperties")]
		public class TOD_Sky_UpdateShaderProperties
		{
			// Token: 0x06000067 RID: 103 RVA: 0x000053CC File Offset: 0x000035CC
			[HarmonyPostfix]
			public static void Postfix(TOD_Sky __instance)
			{
			}
		}
	}
}
