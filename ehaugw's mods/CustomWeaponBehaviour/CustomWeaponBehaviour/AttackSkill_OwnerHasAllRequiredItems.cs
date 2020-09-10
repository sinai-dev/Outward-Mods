using System;
using System.Collections.Generic;
using HarmonyLib;

namespace CustomWeaponBehaviour
{
	// Token: 0x0200000E RID: 14
	[HarmonyPatch(typeof(AttackSkill), "OwnerHasAllRequiredItems")]
	public class AttackSkill_OwnerHasAllRequiredItems
	{
		// Token: 0x06000026 RID: 38 RVA: 0x0000284C File Offset: 0x00000A4C
		[HarmonyPrefix]
		public static void Prefix(AttackSkill __instance, out List<Weapon.WeaponType> __state)
		{
			__state = null;
			Weapon weapon;
			if (__instance == null)
			{
				weapon = null;
			}
			else
			{
				Character ownerCharacter = __instance.OwnerCharacter;
				weapon = ((ownerCharacter != null) ? ownerCharacter.CurrentWeapon : null);
			}
			Weapon weapon2 = weapon;
			bool flag = weapon2 != null;
			if (flag)
			{
				Weapon.WeaponType valueOrDefault = (Weapon.WeaponType)0;
				bool flag2;
				if (__instance.RequiredWeaponTypes != null)
				{
					Weapon.WeaponType? bastardType = BehaviourManager.GetBastardType(weapon2.Type);
					if (bastardType != null)
					{
						valueOrDefault = bastardType.GetValueOrDefault();
						flag2 = true;
					}
					else
					{
						flag2 = false;
					}
				}
				else
				{
					flag2 = false;
				}
				bool flag3 = flag2;
				if (flag3)
				{
					bool flag4 = __instance.RequiredWeaponTypes.Contains(valueOrDefault) && !__instance.RequiredWeaponTypes.Contains(weapon2.Type) && BehaviourManager.IsBastardMode(weapon2);
					if (flag4)
					{
						__state = __instance.RequiredWeaponTypes;
						__instance.RequiredWeaponTypes = new List<Weapon.WeaponType>(__state);
						__instance.RequiredWeaponTypes.Add(weapon2.Type);
					}
				}
			}
		}

		// Token: 0x06000027 RID: 39 RVA: 0x00002910 File Offset: 0x00000B10
		public static void Postfix(AttackSkill __instance, List<Weapon.WeaponType> __state)
		{
			bool flag = __state != null;
			if (flag)
			{
				__instance.RequiredWeaponTypes = __state;
			}
		}
	}
}
