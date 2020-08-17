using System;
using System.Collections.Generic;
using CustomWeaponBehaviour;
using SideLoader;
using TinyHelper;
using UnityEngine;

namespace Juggernaut
{
	// Token: 0x02000020 RID: 32
	public class TackleSpell
	{
		// Token: 0x06000062 RID: 98 RVA: 0x000042A8 File Offset: 0x000024A8
		public static Skill Init()
		{
			SL_AttackSkill sl_AttackSkill = new SL_AttackSkill
			{
				Name = "Tackle",
				EffectBehaviour = EffectBehaviours.OverrideEffects,
				Target_ItemID = 8100072,
				New_ItemID = 2502017,
				SLPackName = "Juggernaut",
				SubfolderName = "Tackle",
				Description = "Ram into your opponent! Either of you will fall. The most stable at foot will triump!",
				CastType = Character.SpellCastType.ShieldCharge,
				CastModifier = Character.SpellCastModifier.Attack,
				CastLocomotionEnabled = false,
				MobileCastMovementMult = -1f,
				CastSheatheRequired = 0,
				RequiredOffHandTypes = new Weapon.WeaponType[0],
				Cooldown = 30f,
				StaminaCost = 8f,
				ManaCost = 0f
			};
			Skill skill = (Skill)CustomItems.CreateCustomItem(sl_AttackSkill.Target_ItemID, sl_AttackSkill.New_ItemID, sl_AttackSkill.Name, sl_AttackSkill);
			EmptyOffHandCondition.AddToSkill(skill, false, false);

			GameObject gameObject = skill.transform.Find("ActivationEffects").gameObject;
			for (int i = 0; i < 4; i++)
			{
				EnableHitDetection enableHitDetection = gameObject.AddComponent<EnableHitDetection>();
				enableHitDetection.Delay = 0.2f + (float)i * 0.08f;
			}
			foreach (PlaySoundEffect obj in gameObject.GetComponentsInChildren<PlaySoundEffect>())
			{
				UnityEngine.Object.Destroy(obj);
			}
			GameObject gameObject2 = ResourcesPrefabManager.Instance.GetItemPrefab(8100190).gameObject;
			foreach (PlaySoundEffect playSoundEffect in gameObject2.gameObject.GetComponentsInChildren<PlaySoundEffect>())
			{
				bool flag = (double)playSoundEffect.Delay <= 0.2;
				if (flag)
				{
					PlaySoundEffect playSoundEffect2 = gameObject.AddComponent<PlaySoundEffect>();
					playSoundEffect2.Sounds = playSoundEffect.Sounds;
					playSoundEffect2.Delay = playSoundEffect.Delay;
					playSoundEffect2.Follow = playSoundEffect.Follow;
					playSoundEffect2.MinPitch = playSoundEffect.MinPitch;
					playSoundEffect2.MaxPitch = playSoundEffect.MaxPitch;
					playSoundEffect2.SyncType = playSoundEffect.SyncType;
				}
			}
			GameObject gameObject3 = skill.gameObject.transform.FindInAllChildren("HitEffects").gameObject;
			TackleEffect tackleEffect = gameObject3.AddComponent<TackleEffect>();
			tackleEffect.Knockback = 10f;
			tackleEffect.Damages = new DamageType[]
			{
				new DamageType(DamageType.Types.Physical, 3f)
			};
			PunctualDamage component = gameObject3.GetComponent<PunctualDamage>();
			UnityEngine.Object.Destroy(component);
			return skill;
		}
	}
}
