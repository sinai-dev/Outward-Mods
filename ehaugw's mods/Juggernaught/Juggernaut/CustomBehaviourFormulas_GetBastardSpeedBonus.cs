using System;
using CustomWeaponBehaviour;
using HarmonyLib;

namespace Juggernaut
{
	// Token: 0x0200000D RID: 13
	[HarmonyPatch(typeof(CustomBehaviourFormulas), "GetBastardSpeedBonus")]
	public class CustomBehaviourFormulas_GetBastardSpeedBonus
	{
		// Token: 0x06000038 RID: 56 RVA: 0x0000303C File Offset: 0x0000123C
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
				bool flag3 = SkillRequirements.CanAddBonusBastardWeaponSpeed(ownerCharacter);
				if (flag3)
				{
					__result += 0.1f;
				}
			}
		}
	}
}
