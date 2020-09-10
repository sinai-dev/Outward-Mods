using System;

namespace CustomWeaponBehaviour
{
	// Token: 0x02000004 RID: 4
	public class EnableHitDetection : Effect
	{
		// Token: 0x06000011 RID: 17 RVA: 0x00002554 File Offset: 0x00000754
		protected override void ActivateLocally(Character _affectedCharacter, object[] _infos)
		{
			MeleeSkill meleeSkill = base.ParentItem as MeleeSkill;
			MeleeHitDetector meleeHitDetector = null;
			bool flag;
			if (meleeSkill != null && meleeSkill.MeleeHitDetector != null)
			{
				meleeHitDetector = ((_affectedCharacter != null) ? _affectedCharacter.SkillMeleeDetector : null);
				flag = (meleeHitDetector != null);
			}
			else
			{
				flag = false;
			}
			bool flag2 = flag;
			if (flag2)
			{
				meleeHitDetector.HitStarted(-1);
			}
			else
			{
				MeleeWeapon meleeWeapon = ((_affectedCharacter != null) ? _affectedCharacter.CurrentWeapon : null) as MeleeWeapon;
				bool flag3 = meleeWeapon != null;
				if (flag3)
				{
					meleeWeapon.HitStarted(-1);
				}
			}
		}
	}
}
