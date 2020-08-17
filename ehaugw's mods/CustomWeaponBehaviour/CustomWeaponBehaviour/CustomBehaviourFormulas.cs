using System;
using TinyHelper;

namespace CustomWeaponBehaviour
{
	// Token: 0x02000003 RID: 3
	public class CustomBehaviourFormulas
	{
		// Token: 0x0600000A RID: 10 RVA: 0x000023F0 File Offset: 0x000005F0
		public static float GetBastardSpeedBonus(Weapon weapon)
		{
			return 0f;
		}

		// Token: 0x0600000B RID: 11 RVA: 0x00002408 File Offset: 0x00000608
		public static float GetBastardDamageBonus(Weapon weapon)
		{
			return 0.15f;
		}

		// Token: 0x0600000C RID: 12 RVA: 0x00002420 File Offset: 0x00000620
		public static float GetHolyWeaponConvertionRate(Weapon weapon)
		{
			return 0.3f;
		}

		// Token: 0x0600000D RID: 13 RVA: 0x00002438 File Offset: 0x00000638
		public static void PostAmplifySpeed(Weapon weapon, ref float result)
		{
			bool flag = weapon == null;
			if (!flag)
			{
				bool flag2 = BehaviourManager.IsBastardMode(weapon);
				if (flag2)
				{
					result += CustomBehaviourFormulas.GetBastardSpeedBonus(weapon);
				}
				bool flag3 = WeaponManager.Speeds.ContainsKey(weapon.Type) && WeaponManager.Speeds.ContainsKey(BehaviourManager.GetCurrentAnimationType(weapon));
				if (flag3)
				{
					result *= WeaponManager.Speeds[BehaviourManager.GetCurrentAnimationType(weapon)] / WeaponManager.Speeds[weapon.Type];
				}
			}
		}

		// Token: 0x0600000E RID: 14 RVA: 0x000024B8 File Offset: 0x000006B8
		public static DamageType.Types GetHolyDamageType()
		{
			return DamageType.Types.Electric;
		}

		// Token: 0x0600000F RID: 15 RVA: 0x000024CC File Offset: 0x000006CC
		public static void PreAmplifyDamage(Weapon weapon, ref DamageList _list)
		{
			bool flag = weapon == null;
			if (!flag)
			{
				bool flag2 = BehaviourManager.IsHolyWeaponMode(weapon);
				if (flag2)
				{
					float holyWeaponConvertionRate = CustomBehaviourFormulas.GetHolyWeaponConvertionRate(weapon);
					float totalDamage = _list.TotalDamage;
					_list *= 1f - holyWeaponConvertionRate;
					_list.Add(new DamageType(CustomBehaviourFormulas.GetHolyDamageType(), totalDamage * holyWeaponConvertionRate));
				}
				bool flag3 = BehaviourManager.IsBastardMode(weapon);
				if (flag3)
				{
					_list *= 1f + CustomBehaviourFormulas.GetBastardDamageBonus(weapon);
				}
			}
		}
	}
}
