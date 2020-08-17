using System;
using UnityEngine;

namespace SkilledAtSitting
{
	// Token: 0x02000002 RID: 2
	internal class Sitting : Effect
	{
		// Token: 0x06000001 RID: 1 RVA: 0x00002050 File Offset: 0x00000250
		protected override void ActivateLocally(Character _affectedCharacter, object[] _infos)
		{
			if (_affectedCharacter != null)
			{
				Animator animator = _affectedCharacter.Animator;
				Vector3? vector = (animator != null) ? new Vector3?(animator.velocity) : null;
				if (vector != null && (double)_affectedCharacter.Animator.velocity.sqrMagnitude > 0.1)
				{
					goto IL_80;
				}
			}
			bool flag;
			if ((double)((_affectedCharacter != null) ? _affectedCharacter.AnimMoveSqMagnitude : 0f) <= 0.1 || (double)this.m_parentStatusEffect.Age <= 0.5)
			{
				flag = false;
				goto IL_95;
			}
			IL_80:
			flag = (this.m_parentStatusEffect.Age > 1f);
			IL_95:
			bool flag2 = flag;
			if (flag2)
			{
				_affectedCharacter.StatusEffectMngr.CleanseStatusEffect(SkilledAtSitting.SITTING_EFFECT_NAME);
			}
			float num = 150f;
			float num2 = 0.75f;
			_affectedCharacter.Stats.AffectHealth(35f / num * num2);
			_affectedCharacter.Stats.RestoreBurntHealth(0.05f / num * num2, true);
			_affectedCharacter.Stats.RestoreBurntStamina(20f / num * num2, false);
		}

		// Token: 0x04000001 RID: 1
		public const float TICK_RATE = 1f;
	}
}
