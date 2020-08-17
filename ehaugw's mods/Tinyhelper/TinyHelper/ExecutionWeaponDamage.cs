using System;

namespace TinyHelper
{
	// Token: 0x0200000C RID: 12
	public class ExecutionWeaponDamage : WeaponDamage
	{
		// Token: 0x06000025 RID: 37 RVA: 0x00002CF8 File Offset: 0x00000EF8
		protected override void ActivateLocally(Character _affectedCharacter, object[] _infos)
		{
			bool isDead = _affectedCharacter.IsDead;
			base.ActivateLocally(_affectedCharacter, _infos);
			Skill skill = null;
			bool flag;
			if (!isDead && _affectedCharacter.IsDead)
			{
				skill = (base.ParentItem as Skill);
				flag = (skill != null);
			}
			else
			{
				flag = false;
			}
			bool flag2 = flag;
			if (flag2)
			{
				bool flag3 = this.SetCooldown != -1f;
				if (flag3)
				{
					At.SetValue<float>(this.SetCooldown, typeof(Skill), skill, "m_remainingCooldownTime");
				}
			}
		}

		// Token: 0x0400000A RID: 10
		public float SetCooldown = -1f;
	}
}
