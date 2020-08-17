using System;
using CustomWeaponBehaviour;
using HarmonyLib;

namespace Juggernaut
{
	// Token: 0x0200000E RID: 14
	[HarmonyPatch(typeof(CustomBehaviourFormulas), "GetBastardDamageBonus")]
	public class CustomBehaviourFormulas_GetBastardDamageBonus
	{
		// Token: 0x0600003A RID: 58 RVA: 0x00003094 File Offset: 0x00001294
		[HarmonyPostfix]
		public static void Postfix(ref Weapon weapon, ref float __result)
		{
			Character ownerCharacter = null;
			bool flag;
			if (weapon != null && weapon.IsEquipped)
			{
				ownerCharacter = weapon.OwnerCharacter;
				flag = (ownerCharacter != null);
			}
			else
			{
				flag = false;
			}
			bool flag2 = flag;
			if (flag2)
			{
				bool flag3 = SkillRequirements.CanAddBonusBastardWeaponDamage(ownerCharacter);
				if (flag3)
				{
					__result += 0.1f;
				}
			}
		}
	}
}
