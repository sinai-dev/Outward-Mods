using System;
using System.Collections.Generic;
using System.Linq;
using SideLoader;
using TinyHelper;
using UnityEngine;

namespace Templar
{
	// Token: 0x02000014 RID: 20
	internal class EffectInitializer
	{
		// Token: 0x0600002D RID: 45 RVA: 0x00004028 File Offset: 0x00002228
		public static StatusEffect MakePrayerPrefab()
		{
			StatusEffect statusEffect = TinyEffectManager.MakeStatusEffectPrefab("Prayer", "Prayer", "You are praying...", -1f, 0.25f, StatusEffectFamily.StackBehaviors.Override, "Mana Ratio Recovery 3", true, null, null, "com.ehaugw.templarclass");
			EffectSignature statusEffectSignature = statusEffect.StatusEffectSignature;
			PrayerEffect prayerEffect = TinyEffectManager.MakeFreshObject("Effects", true, true, statusEffectSignature.transform).AddComponent<PrayerEffect>();
			prayerEffect.UseOnce = false;
			statusEffectSignature.Effects = new List<Effect>
			{
				prayerEffect
			};
			statusEffect.IsHidden = true;
			statusEffect.DisplayInHud = false;
			return statusEffect;
		}

		// Token: 0x0600002E RID: 46 RVA: 0x000040BC File Offset: 0x000022BC
		public static StatusEffect MakePrayerCooldownPrefab()
		{
			StatusEffect statusEffect = TinyEffectManager.MakeStatusEffectPrefab("PrayOnCooldownEffect", "PrayOnCooldownEffect", "Try to not bother Elatt too much.", 150f, -1f, StatusEffectFamily.StackBehaviors.Override, "Mana Ratio Recovery 3", true, null, null, "com.ehaugw.templarclass");
			statusEffect.IsHidden = true;
			statusEffect.DisplayInHud = false;
			return statusEffect;
		}

		// Token: 0x0600002F RID: 47 RVA: 0x00004114 File Offset: 0x00002314
		public static StatusEffect MakeSurgeOfDivinityPrefab()
		{
			StatusEffect statusEffect = TinyEffectManager.MakeStatusEffectPrefab("Surge of Divinity", "Surge of Divinity", "Greatly increases Burst of Divinity buildup.", 90f, -1f, StatusEffectFamily.StackBehaviors.Override, "Mana Ratio Recovery 3", false, null, null, "com.ehaugw.templarclass");
			statusEffect.OverrideIcon = CustomTextures.CreateSprite(SL.Packs["Templar"].Texture2D["surgeOfDivinityIcon"], 0);
			return statusEffect;
		}

		// Token: 0x06000030 RID: 48 RVA: 0x00004188 File Offset: 0x00002388
		public static StatusEffect MakeBurstOfDivinityPrefab()
		{
			StatusEffect statusEffect = TinyEffectManager.MakeStatusEffectPrefab("Burst of Divinity", "Burst of Divinity", "Reduces the mana cost of spells, but stacks are expended when a spell is casted.", 30f, -1f, StatusEffectFamily.StackBehaviors.StackAll, "Mana Ratio Recovery 3", false, null, null, "com.ehaugw.templarclass");
			statusEffect.OverrideIcon = CustomTextures.CreateSprite(SL.Packs["Templar"].Texture2D["burstOfDivinityIcon"], 0);
			return statusEffect;
		}

		// Token: 0x06000031 RID: 49 RVA: 0x000041FC File Offset: 0x000023FC
		public static StatusEffect MakeRadiatingPrefab()
		{
			StatusEffect statusEffect = TinyEffectManager.MakeStatusEffectPrefab("Radiating", "Radiating", "Damages you and your nearby allies.", 15f, 1f, StatusEffectFamily.StackBehaviors.Override, "Doom", true, 134.ToString(), null, "com.ehaugw.templarclass");
			EffectSignature statusEffectSignature = statusEffect.StatusEffectSignature;
			Radiating radiating = TinyEffectManager.MakeFreshObject("Effects", true, true, statusEffectSignature.transform).AddComponent<Radiating>();
			radiating.UseOnce = false;
			statusEffectSignature.Effects = new List<Effect>
			{
				radiating
			};
			statusEffect.OverrideIcon = CustomTextures.CreateSprite(SL.Packs["Templar"].Texture2D["radiatingIcon"], 0);
			return statusEffect;
		}

		// Token: 0x06000032 RID: 50 RVA: 0x000042B8 File Offset: 0x000024B8
		public static void MakeRadiantLightInfusion()
		{
			ImbueEffectPreset item = TinyEffectManager.MakeImbuePreset(269, "Radiant Light Imbue", "Weapon deals some Lightning Damage, applies Radiating and emits light.", null, 219, 6f, 0.25f, DamageType.Types.Electric, "Radiating", null, 0, 100);
			Item itemPrefab = ResourcesPrefabManager.Instance.GetItemPrefab(8200310);
			GameObject gameObject = (itemPrefab != null) ? itemPrefab.transform.Find("NormalBolt").gameObject : null;
			bool flag = gameObject != null;
			if (flag)
			{
				ImbueEffectORCondition component = gameObject.GetComponent<ImbueEffectORCondition>();
				bool flag2 = component != null;
				if (flag2)
				{
					List<ImbueEffectPreset> list = component.ImbueEffectPresets.ToList<ImbueEffectPreset>();
					list.Add(item);
					component.ImbueEffectPresets = list.ToArray();
				}
			}
			Item itemPrefab2 = ResourcesPrefabManager.Instance.GetItemPrefab(8100200);
			GameObject gameObject2 = (itemPrefab2 != null) ? itemPrefab2.transform.Find("ElementalEffect/NormalLightning").gameObject : null;
			bool flag3 = gameObject2 != null;
			if (flag3)
			{
				ImbueEffectORCondition component2 = gameObject2.GetComponent<ImbueEffectORCondition>();
				bool flag4 = component2 != null;
				if (flag4)
				{
					List<ImbueEffectPreset> list2 = component2.ImbueEffectPresets.ToList<ImbueEffectPreset>();
					list2.Add(item);
					component2.ImbueEffectPresets = list2.ToArray();
				}
			}
		}
	}
}
