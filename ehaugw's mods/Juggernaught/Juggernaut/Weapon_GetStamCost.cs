using System;
using HarmonyLib;

namespace Juggernaut
{
	// Token: 0x0200000C RID: 12
	[HarmonyPatch(typeof(Weapon), "GetStamCost")]
	public class Weapon_GetStamCost
	{
		// Token: 0x06000036 RID: 54 RVA: 0x00002FEC File Offset: 0x000011EC
		[HarmonyPostfix]
		public static void Postfix(Weapon __instance, ref float __result)
		{
			Character ownerCharacter = null;
			bool flag;
			if (__instance.IsEquipped)
			{
				ownerCharacter = __instance.OwnerCharacter;
				flag = (ownerCharacter != null);
			}
			else
			{
				flag = false;
			}
			bool flag2 = flag;
			if (flag2)
			{
				bool flag3 = SkillRequirements.CanReduceWeaponAttackStaminaCost(ownerCharacter);
				if (flag3)
				{
					__result *= 1f - JuggernautFormulas.GetRuthlessAttackStaminaCostReduction(ownerCharacter);
				}
			}
		}
	}
}
