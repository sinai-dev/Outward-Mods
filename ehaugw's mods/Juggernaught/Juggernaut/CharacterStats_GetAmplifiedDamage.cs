using System;
using HarmonyLib;
using TinyHelper;

namespace Juggernaut
{
	// Token: 0x0200000B RID: 11
	[HarmonyPatch(typeof(CharacterStats), "GetAmplifiedDamage")]
	public class CharacterStats_GetAmplifiedDamage
	{
		// Token: 0x06000034 RID: 52 RVA: 0x00002F4C File Offset: 0x0000114C
		[HarmonyPostfix]
		public static void Postfix(CharacterStats __instance, ref DamageList _damages)
		{
			Character character = At.GetValue(typeof(CharacterStats), __instance, "m_character") as Character;
			bool flag = character != null;
			if (flag)
			{
				bool flag2 = SkillRequirements.CanAddBonusRageDamage(character);
				if (flag2)
				{
					_damages *= 1f + JuggernautFormulas.GetRuthlessDamageBonus(character);
				}
				bool flag3 = SkillRequirements.CanConvertToRawDamage(character);
				if (flag3)
				{
					float totalDamage = _damages.TotalDamage;
					float ruthlessRawDamageRatio = JuggernautFormulas.GetRuthlessRawDamageRatio(character);
					_damages *= 1f - ruthlessRawDamageRatio;
					_damages.Add(new DamageType(DamageType.Types.Raw, totalDamage * ruthlessRawDamageRatio));
				}
			}
		}
	}
}
