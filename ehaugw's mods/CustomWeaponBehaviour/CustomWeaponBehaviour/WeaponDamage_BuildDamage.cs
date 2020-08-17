using System;
using System.Collections.Generic;
using HarmonyLib;

namespace CustomWeaponBehaviour
{
	// Token: 0x0200000F RID: 15
	[HarmonyPatch(typeof(WeaponDamage), "BuildDamage", new Type[]
	{
		typeof(Character),
		typeof(DamageList),
		typeof(float)
	}, new ArgumentType[]
	{
		ArgumentType.Normal,
		ArgumentType.Ref,
		ArgumentType.Ref
	})]
	public class WeaponDamage_BuildDamage
	{
		// Token: 0x06000029 RID: 41 RVA: 0x00002938 File Offset: 0x00000B38
		[HarmonyPrefix]
		public static void Prefix(WeaponDamage __instance, ref AttackSkill ___m_attackSkill, out List<Weapon.WeaponType> __state)
		{
			__state = null;
			AttackSkill attackSkill = ___m_attackSkill;
			Weapon weapon;
			if (attackSkill == null)
			{
				weapon = null;
			}
			else
			{
				Character ownerCharacter = attackSkill.OwnerCharacter;
				weapon = ((ownerCharacter != null) ? ownerCharacter.CurrentWeapon : null);
			}
			Weapon weapon2 = weapon;
			bool flag = weapon2 != null;
			if (flag)
			{
				Weapon.WeaponType valueOrDefault;
				bool flag2;
				if (___m_attackSkill.RequiredWeaponTypes != null)
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
					bool flag4 = ___m_attackSkill.RequiredWeaponTypes.Contains(valueOrDefault) && !___m_attackSkill.RequiredWeaponTypes.Contains(weapon2.Type) && BehaviourManager.IsBastardMode(weapon2);
					if (flag4)
					{
						__state = ___m_attackSkill.RequiredWeaponTypes;
						___m_attackSkill.RequiredWeaponTypes = new List<Weapon.WeaponType>(__state);
						___m_attackSkill.RequiredWeaponTypes.Add(weapon2.Type);
					}
				}
			}
		}

		// Token: 0x0600002A RID: 42 RVA: 0x00002A04 File Offset: 0x00000C04
		public static void Postfix(AttackSkill ___m_attackSkill, List<Weapon.WeaponType> __state)
		{
			bool flag = __state != null && ___m_attackSkill != null;
			if (flag)
			{
				___m_attackSkill.RequiredWeaponTypes = __state;
			}
		}
	}
}
