using System;
using HarmonyLib;

namespace CustomWeaponBehaviour
{
	// Token: 0x02000009 RID: 9
	[HarmonyPatch(typeof(CharacterStats), "GetAmplifiedDamage")]
	public class CharacterStats_GetAmplifiedDamage
	{
		// Token: 0x0600001C RID: 28 RVA: 0x00002784 File Offset: 0x00000984
		[HarmonyPrefix]
		public static void Prefix(CharacterStats __instance, ref DamageList _damages, ref Character ___m_character)
		{
			Character character = ___m_character;
			Weapon weapon = (character != null) ? character.CurrentWeapon : null;
			bool flag = weapon == null;
			if (!flag)
			{
				CustomBehaviourFormulas.PreAmplifyDamage(weapon, ref _damages);
			}
		}
	}
}
