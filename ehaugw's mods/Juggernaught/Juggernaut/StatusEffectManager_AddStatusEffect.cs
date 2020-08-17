using System;
using System.Collections.Generic;
using HarmonyLib;
using TinyHelper;

namespace Juggernaut
{
	// Token: 0x02000015 RID: 21
	[HarmonyPatch(typeof(StatusEffectManager), "AddStatusEffect", new Type[]
	{
		typeof(StatusEffect),
		typeof(Character),
		typeof(string[])
	})]
	public class StatusEffectManager_AddStatusEffect
	{
		// Token: 0x0600004A RID: 74 RVA: 0x000036E0 File Offset: 0x000018E0
		[HarmonyPrefix]
		public static bool Prefix(StatusEffectManager __instance, ref StatusEffect _statusEffect)
		{
			StatusEffect statusEffect = _statusEffect;
			IList<Tag> inheritedTags = statusEffect.InheritedTags;
			Character character = null;
			bool flag;
			if (statusEffect != null && statusEffect.HasMatch(TagSourceManager.Boon))
			{
				character = (At.GetValue(typeof(StatusEffectManager), __instance, "m_character") as Character);
				flag = (character != null);
			}
			else
			{
				flag = false;
			}
			bool flag2 = flag;
			if (flag2)
			{
				bool? flag3 = null;
				bool flag4 = SkillRequirements.ShouldPurgeAllExceptRageGivenEnraged(character) && (SkillRequirements.Enraged(character) || SkillRequirements.IsRageEffect(statusEffect));
				if (flag4)
				{
					flag3 = new bool?(false);
				}
				bool flag5 = SkillRequirements.ShouldPurgeOnlyRageGivenDisciplined(character) && (SkillRequirements.Disciplined(character) || SkillRequirements.IsDisciplineEffect(statusEffect));
				if (flag5)
				{
					flag3 = new bool?(true);
				}
				bool flag6 = flag3 != null;
				if (flag6)
				{
					bool? flag8;
					foreach (StatusEffect statusEffect2 in __instance.Statuses)
					{
						bool flag9;
						if (statusEffect2.HasMatch(TagSourceManager.Boon))
						{
							bool flag7 = SkillRequirements.IsRageEffect(statusEffect2);
							flag8 = flag3;
							flag9 = (flag7 == flag8.GetValueOrDefault() & flag8 != null);
						}
						else
						{
							flag9 = false;
						}
						bool flag10 = flag9;
						if (flag10)
						{
							statusEffect2.IncreaseAge(Convert.ToInt32(statusEffect2.RemainingLifespan));
						}
					}
					bool flag11 = SkillRequirements.IsRageEffect(statusEffect);
					flag8 = flag3;
					bool flag12 = flag11 == flag8.GetValueOrDefault() & flag8 != null;
					if (flag12)
					{
						return false;
					}
				}
			}
			return true;
		}
	}
}
