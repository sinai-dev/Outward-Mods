using System;
using HarmonyLib;

namespace Templar
{
	// Token: 0x02000018 RID: 24
	[HarmonyPatch(typeof(Skill), "HasEnoughMana")]
	public class Skill_HasEnoughMana
	{
		// Token: 0x0600003B RID: 59 RVA: 0x000045EC File Offset: 0x000027EC
		[HarmonyPrefix]
		public static bool Prefix(Skill __instance, ref bool _tryingToActivate, ref bool __result)
		{
			Character ownerCharacter = __instance.OwnerCharacter;
			float? num;
			if (ownerCharacter == null)
			{
				num = null;
			}
			else
			{
				CharacterStats stats = ownerCharacter.Stats;
				num = ((stats != null) ? new float?(stats.GetFinalManaConsumption(null, __instance.ManaCost)) : null);
			}
			float num2 = num ?? __instance.ManaCost;
			Character ownerCharacter2 = __instance.OwnerCharacter;
			int? num3;
			if (ownerCharacter2 == null)
			{
				num3 = null;
			}
			else
			{
				StatusEffectManager statusEffectMngr = ownerCharacter2.StatusEffectMngr;
				if (statusEffectMngr == null)
				{
					num3 = null;
				}
				else
				{
					StatusEffect statusEffectOfName = statusEffectMngr.GetStatusEffectOfName(Templar.Instance.burstOfDivinityInstance.IdentifierName);
					num3 = ((statusEffectOfName != null) ? new int?(statusEffectOfName.StackCount) : null);
				}
			}
			int? num4 = num3;
			int valueOrDefault = num4.GetValueOrDefault();
			bool flag = __instance.OwnerCharacter.Mana + (float)valueOrDefault * 7f >= num2;
			bool result;
			if (flag)
			{
				__result = true;
				result = false;
			}
			else
			{
				result = true;
			}
			return result;
		}
	}
}
