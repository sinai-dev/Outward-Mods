using System;
using System.Collections.Generic;
using Localizer;
using UnityEngine;

namespace TinyHelper
{
	// Token: 0x0200000A RID: 10
	public class TinyEffectManager
	{
		// Token: 0x06000018 RID: 24 RVA: 0x000025D0 File Offset: 0x000007D0
		public static StatusEffect MakeStatusEffectPrefab(string effectName, string familyName, string description, float lifespan, float refreshRate, StatusEffectFamily.StackBehaviors stackBehavior, string targetStatusName, bool isMalusEffect, string tagID = null, UID? uid = null, string modGUID = null)
		{
			Dictionary<string, StatusEffect> dictionary = At.GetValue(typeof(ResourcesPrefabManager), null, "STATUSEFFECT_PREFABS") as Dictionary<string, StatusEffect>;
			StatusEffectFamily statusEffectFamily = TinyEffectManager.MakeStatusEffectFamiliy(familyName, stackBehavior, -1, StatusEffectFamily.LengthTypes.Short, null, null);
			GameObject gameObject = TinyEffectManager.InstantiateClone(dictionary[targetStatusName].gameObject, effectName, false, true);
			StatusEffect statusEffect = dictionary[effectName] = (gameObject.GetComponent<StatusEffect>() ?? gameObject.AddComponent<StatusEffect>());
			At.SetValue<string>(effectName, typeof(StatusEffect), statusEffect, "m_identifierName");
			At.SetValue<StatusEffectFamily>(statusEffectFamily, typeof(StatusEffect), statusEffect, "m_bindFamily");
			At.SetValue<string>(effectName, typeof(StatusEffect), statusEffect, "m_nameLocKey");
			At.SetValue<string>(description, typeof(StatusEffect), statusEffect, "m_descriptionLocKey");
			statusEffect.RefreshRate = refreshRate;
			statusEffect.IsMalusEffect = isMalusEffect;
			At.SetValue<StatusEffect.EffectSignatureModes>(StatusEffect.EffectSignatureModes.Reference, typeof(StatusEffect), statusEffect, "m_effectSignatureMode");
			At.SetValue<StatusEffect.FamilyModes>(StatusEffect.FamilyModes.Bind, typeof(StatusEffect), statusEffect, "m_familyMode");
			TagSourceSelector value = (tagID != null) ? new TagSourceSelector(TagSourceManager.Instance.GetTag(tagID)) : new TagSourceSelector();
			At.SetValue<TagSourceSelector>(value, typeof(StatusEffect), statusEffect, "m_effectType");
			StatusData statusData = statusEffect.StatusData = new StatusData(statusEffect.StatusData);
			statusData.LifeSpan = lifespan;
			List<StatusData> list = At.GetValue(typeof(StatusEffect), statusEffect, "m_statusStack") as List<StatusData>;
			list[0] = statusData;
			UnityEngine.Object.Destroy(gameObject.GetComponentInChildren<EffectSignature>().gameObject);
			EffectSignature effectSignature = statusEffectFamily.EffectSignature = (statusData.EffectSignature = TinyEffectManager.MakeFreshObject("Signature", true, true, null).AddComponent<EffectSignature>());
			effectSignature.name = "Signature";
			effectSignature.SignatureUID = (uid ?? ((modGUID != null) ? TinyUIDManager.MakeUID(effectName, modGUID, "Status Effect") : UID.Generate()));
			StatusEffectFamilySelector statusEffectFamilySelector = new StatusEffectFamilySelector();
			statusEffectFamilySelector.Set(statusEffectFamily);
			At.SetValue<StatusEffectFamilySelector>(statusEffectFamilySelector, typeof(StatusEffect), statusEffect, "m_stackingFamily");
			return statusEffect;
		}

		// Token: 0x06000019 RID: 25 RVA: 0x00002810 File Offset: 0x00000A10
		public static StatusEffectFamily MakeStatusEffectFamiliy(string familyName, StatusEffectFamily.StackBehaviors stackBehavior, int maxStackCount, StatusEffectFamily.LengthTypes lengthType, UID? uid = null, string modGUID = null)
		{
			StatusEffectFamily statusEffectFamily = new StatusEffectFamily();
			uid = new UID?(uid ?? ((modGUID != null) ? TinyUIDManager.MakeUID(familyName, modGUID, "Status Effect Family") : UID.Generate()));
			At.SetValue<UID>(uid.Value, typeof(StatusEffectFamily), statusEffectFamily, "m_uid");
			statusEffectFamily.Name = familyName;
			statusEffectFamily.StackBehavior = stackBehavior;
			statusEffectFamily.MaxStackCount = maxStackCount;
			statusEffectFamily.LengthType = lengthType;
			return statusEffectFamily;
		}

		// Token: 0x0600001A RID: 26 RVA: 0x00002898 File Offset: 0x00000A98
		public static GameObject InstantiateClone(GameObject sourceGameObject, string newGameObjectName, bool setActive, bool dontDestroyOnLoad)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(sourceGameObject);
			gameObject.SetActive(setActive);
			gameObject.name = newGameObjectName;
			if (dontDestroyOnLoad)
			{
				UnityEngine.Object.DontDestroyOnLoad(gameObject);
			}
			return gameObject;
		}

		// Token: 0x0600001B RID: 27 RVA: 0x000028D0 File Offset: 0x00000AD0
		public static GameObject MakeFreshObject(string newGameObjectName, bool setActive, bool dontDestroyOnLoad, Transform parent = null)
		{
			GameObject gameObject = new GameObject();
			gameObject.SetActive(setActive);
			gameObject.name = newGameObjectName;
			if (dontDestroyOnLoad)
			{
				UnityEngine.Object.DontDestroyOnLoad(gameObject);
			}
			bool flag = parent != null;
			if (flag)
			{
				gameObject.transform.SetParent(parent);
			}
			return gameObject;
		}

		// Token: 0x0600001C RID: 28 RVA: 0x00002920 File Offset: 0x00000B20
		public static void SetNameAndDesc(ImbueEffectPreset imbueEffect, string name, string desc)
		{
			ItemLocalization value = new ItemLocalization(name, desc);
			Dictionary<int, ItemLocalization> dictionary = At.GetValue(typeof(LocalizationManager), LocalizationManager.Instance, "m_itemLocalization") as Dictionary<int, ItemLocalization>;
			bool flag = dictionary != null;
			if (flag)
			{
				bool flag2 = dictionary.ContainsKey(imbueEffect.PresetID);
				if (flag2)
				{
					dictionary[imbueEffect.PresetID] = value;
				}
				else
				{
					dictionary.Add(imbueEffect.PresetID, value);
				}
			}
		}

		// Token: 0x0600001D RID: 29 RVA: 0x00002990 File Offset: 0x00000B90
		public static AddStatusEffectBuildUp AddStatusEffectBuildUp(ImbueEffectPreset effect, float buildup, string statusEffectName)
		{
			AddStatusEffectBuildUp addStatusEffectBuildUp = effect.transform.Find("Effects").gameObject.AddComponent<AddStatusEffectBuildUp>();
			addStatusEffectBuildUp.BuildUpValue = buildup;
			addStatusEffectBuildUp.Status = ResourcesPrefabManager.Instance.GetStatusEffectPrefab(statusEffectName);
			addStatusEffectBuildUp.OverrideEffectCategory = EffectSynchronizer.EffectCategories.Hit;
			return addStatusEffectBuildUp;
		}

		// Token: 0x0600001E RID: 30 RVA: 0x000029E0 File Offset: 0x00000BE0
		public static AddStatusEffect AddStatusEffectChance(ImbueEffectPreset effect, int chance, string statusEffectName)
		{
			AddStatusEffect addStatusEffect = effect.transform.Find("Effects").gameObject.AddComponent<AddStatusEffect>();
			addStatusEffect.SetChanceToContract(chance);
			addStatusEffect.Status = ResourcesPrefabManager.Instance.GetStatusEffectPrefab(statusEffectName);
			addStatusEffect.OverrideEffectCategory = EffectSynchronizer.EffectCategories.Hit;
			return addStatusEffect;
		}

		// Token: 0x0600001F RID: 31 RVA: 0x00002A30 File Offset: 0x00000C30
		public static WeaponDamage AddWeaponDamage(ImbueEffectPreset effect, float baseDamage, float damageScaling, DamageType.Types damageType)
		{
			WeaponDamage weaponDamage = effect.transform.Find("Effects").gameObject.AddComponent<WeaponDamage>();
			weaponDamage.WeaponDamageMult = 1f + damageScaling;
			weaponDamage.OverrideDType = damageType;
			weaponDamage.Damages = new DamageType[]
			{
				new DamageType(damageType, baseDamage)
			};
			weaponDamage.OverrideEffectCategory = EffectSynchronizer.EffectCategories.Hit;
			return weaponDamage;
		}

		// Token: 0x06000020 RID: 32 RVA: 0x00002A90 File Offset: 0x00000C90
		public static ImbueEffectPreset MakeImbuePreset(int imbueID, string name, string description, string iconFileName, int visualEffectID, float flatDamage, float scalingDamage, DamageType.Types damageType, string statusEffect, Skill skill, int chanceToContract = 0, int buildUp = 0)
		{
			Dictionary<int, EffectPreset> dictionary = (Dictionary<int, EffectPreset>)At.GetValue(typeof(ResourcesPrefabManager), null, "EFFECTPRESET_PREFABS");
			bool flag = !dictionary.ContainsKey(imbueID);
			ImbueEffectPreset result;
			if (flag)
			{
				GameObject gameObject = new GameObject(imbueID.ToString() + "_" + name.Replace(" ", ""));
				gameObject.SetActive(true);
				UnityEngine.Object.DontDestroyOnLoad(gameObject);
				ImbueEffectPreset imbueEffectPreset = gameObject.AddComponent<ImbueEffectPreset>();
				imbueEffectPreset.name = imbueID.ToString() + "_" + name.Replace(" ", "");
				At.SetValue<int>(imbueID, typeof(EffectPreset), imbueEffectPreset, "m_StatusEffectID");
				At.SetValue<string>(name, typeof(ImbueEffectPreset), imbueEffectPreset, "m_imbueNameKey");
				At.SetValue<string>(description, typeof(ImbueEffectPreset), imbueEffectPreset, "m_imbueDescKey");
				imbueEffectPreset.ImbueStatusIcon = ((ImbueEffectPreset)dictionary[visualEffectID]).ImbueStatusIcon;
				imbueEffectPreset.ImbueFX = ((ImbueEffectPreset)dictionary[visualEffectID]).ImbueFX;
				TinyEffectManager.SetNameAndDesc(imbueEffectPreset, name, description);
				dictionary.Add(imbueID, imbueEffectPreset);
				GameObject gameObject2 = new GameObject("Effects");
				gameObject2.SetActive(true);
				UnityEngine.Object.DontDestroyOnLoad(gameObject2);
				gameObject2.transform.SetParent(imbueEffectPreset.transform);
				bool flag2 = statusEffect != null && chanceToContract > 0;
				if (flag2)
				{
					TinyEffectManager.AddStatusEffectChance(imbueEffectPreset, chanceToContract, statusEffect);
				}
				bool flag3 = statusEffect != null && buildUp > 0;
				if (flag3)
				{
					TinyEffectManager.AddStatusEffectBuildUp(imbueEffectPreset, (float)buildUp, statusEffect);
				}
				bool flag4 = scalingDamage > 0f || flatDamage > 0f;
				if (flag4)
				{
					TinyEffectManager.AddWeaponDamage(imbueEffectPreset, flatDamage, scalingDamage, damageType);
				}
				bool flag5 = skill != null;
				if (flag5)
				{
					skill.GetComponentInChildren<ImbueWeapon>().ImbuedEffect = imbueEffectPreset;
				}
				result = imbueEffectPreset;
			}
			else
			{
				result = null;
			}
			return result;
		}
	}
}
