using System;
using HarmonyLib;
using TinyHelper;

namespace Juggernaut
{
	// Token: 0x02000011 RID: 17
	[HarmonyPatch(typeof(CharacterEquipment), "GetEquipmentDamageResistance")]
	public class CharacterEquipment_GetEquipmentDamageResistance
	{
		// Token: 0x06000042 RID: 66 RVA: 0x0000336C File Offset: 0x0000156C
		[HarmonyPostfix]
		public static void Postfix(CharacterEquipment __instance, ref float __result, ref DamageType.Types _type)
		{
			Character character = At.GetValue(typeof(CharacterEquipment), __instance, "m_character") as Character;
			bool flag = character != null;
			if (flag)
			{
				bool flag2 = (SkillRequirements.CanAddProtectionToPhysicalResistance(character) && _type == DamageType.Types.Physical) || SkillRequirements.CanAddProtectionToAnyDamageResistance(character);
				if (flag2)
				{
					__result += __instance.GetEquipmentDamageProtection(DamageType.Types.Physical);
				}
			}
		}
	}
}
