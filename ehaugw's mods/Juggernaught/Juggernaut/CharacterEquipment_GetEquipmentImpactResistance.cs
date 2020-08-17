using System;
using HarmonyLib;
using TinyHelper;

namespace Juggernaut
{
	// Token: 0x02000012 RID: 18
	[HarmonyPatch(typeof(CharacterEquipment), "GetEquipmentImpactResistance")]
	public class CharacterEquipment_GetEquipmentImpactResistance
	{
		// Token: 0x06000044 RID: 68 RVA: 0x000033D0 File Offset: 0x000015D0
		[HarmonyPostfix]
		public static void Postfix(CharacterEquipment __instance, ref float __result)
		{
			Character character = At.GetValue(typeof(CharacterEquipment), __instance, "m_character") as Character;
			bool flag = character != null && SkillRequirements.CanAddProtectionToImpactResistance(character);
			if (flag)
			{
				__result += __instance.GetEquipmentDamageProtection(DamageType.Types.Physical);
			}
		}
	}
}
