using System;
using UnityEngine;

namespace CustomWeaponBehaviour
{
	// Token: 0x02000005 RID: 5
	public class WeaponSkillAnimationSelector : Effect
	{
		// Token: 0x06000013 RID: 19 RVA: 0x000025D2 File Offset: 0x000007D2
		protected override void ActivateLocally(Character character, object[] _infos)
		{
			CustomWeaponBehaviour.ChangeGrip(character, this.WeaponType);
		}

		// Token: 0x06000014 RID: 20 RVA: 0x000025E4 File Offset: 0x000007E4
		public static bool SetCustomAttackAnimation(Skill skill, Weapon.WeaponType weaponType)
		{
			Transform transform = skill.transform.Find("ActivationEffects");
			bool flag = transform != null;
			bool result;
			if (flag)
			{
				transform.gameObject.AddComponent<WeaponSkillAnimationSelector>().WeaponType = weaponType;
				result = true;
			}
			else
			{
				result = false;
			}
			return result;
		}

		// Token: 0x04000001 RID: 1
		public Weapon.WeaponType WeaponType;
	}
}
