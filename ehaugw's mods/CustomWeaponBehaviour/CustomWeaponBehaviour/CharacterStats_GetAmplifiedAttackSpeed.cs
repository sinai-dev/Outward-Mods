using System;
using HarmonyLib;

namespace CustomWeaponBehaviour
{
	// Token: 0x02000007 RID: 7
	[HarmonyPatch(typeof(CharacterStats), "GetAmplifiedAttackSpeed")]
	public class CharacterStats_GetAmplifiedAttackSpeed
	{
		// Token: 0x06000018 RID: 24 RVA: 0x000026B8 File Offset: 0x000008B8
		[HarmonyPostfix]
		public static void Postfix(CharacterStats __instance, ref float __result, ref Character ___m_character)
		{
			Character character = ___m_character;
			Weapon weapon = (character != null) ? character.CurrentWeapon : null;
			bool flag = weapon == null;
			if (!flag)
			{
				CustomBehaviourFormulas.PostAmplifySpeed(weapon, ref __result);
			}
		}
	}
}
