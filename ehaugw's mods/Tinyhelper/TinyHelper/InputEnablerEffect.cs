using System;

namespace TinyHelper
{
	// Token: 0x02000004 RID: 4
	internal class InputEnablerEffect : Effect
	{
		// Token: 0x06000009 RID: 9 RVA: 0x00002300 File Offset: 0x00000500
		protected override void ActivateLocally(Character _affectedCharacter, object[] _infos)
		{
			StatusEffect parentStatusEffect = this.m_parentStatusEffect;
			bool flag = parentStatusEffect != null;
			if (flag)
			{
				bool flag2 = (double)((_affectedCharacter != null) ? _affectedCharacter.AnimMoveSqMagnitude : 0f) > 0.1 && (double)parentStatusEffect.Age > 0.5;
				if (flag2)
				{
					StatusEffectManager statusEffectMngr = _affectedCharacter.StatusEffectMngr;
					if (statusEffectMngr != null)
					{
						statusEffectMngr.CleanseStatusEffect(parentStatusEffect.IdentifierName);
					}
					_affectedCharacter.ForceCancel(true, true);
				}
			}
		}
	}
}
