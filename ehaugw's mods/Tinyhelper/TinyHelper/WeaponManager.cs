using System;
using System.Collections.Generic;

namespace TinyHelper
{
	// Token: 0x02000005 RID: 5
	public class WeaponManager
	{
		// Token: 0x04000004 RID: 4
		public static Dictionary<Weapon.WeaponType, float> Speeds = new Dictionary<Weapon.WeaponType, float>
		{
			{
				Weapon.WeaponType.Sword_1H,
				1.251f
			},
			{
				Weapon.WeaponType.Axe_1H,
				1.399f
			},
			{
				Weapon.WeaponType.Mace_1H,
				1.629f
			},
			{
				Weapon.WeaponType.Sword_2H,
				1.71f
			},
			{
				Weapon.WeaponType.Axe_2H,
				1.667f
			},
			{
				Weapon.WeaponType.Mace_2H,
				2.036f
			},
			{
				Weapon.WeaponType.Spear_2H,
				1.499f
			},
			{
				Weapon.WeaponType.Halberd_2H,
				1.612f
			}
		};
	}
}
