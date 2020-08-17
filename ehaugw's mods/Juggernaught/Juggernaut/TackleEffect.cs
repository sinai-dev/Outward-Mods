using System;
using UnityEngine;

namespace Juggernaut
{
	// Token: 0x02000005 RID: 5
	internal class TackleEffect : PunctualDamage
	{
		// Token: 0x06000010 RID: 16 RVA: 0x000026D0 File Offset: 0x000008D0
		protected override void ActivateLocally(Character _affectedCharacter, object[] _infos)
		{
			Character ownerCharacter = base.OwnerCharacter;
			bool flag = _affectedCharacter.Stability <= 0f || _affectedCharacter.IsInKnockback || ownerCharacter.Stability <= 0f || ownerCharacter.IsInKnockback;
			if (!flag)
			{
				float num = Mathf.Min(_affectedCharacter.Stats.GetImpactResistance() * 0.01f, 1f);
				float num2 = Mathf.Min(ownerCharacter.Stats.GetImpactResistance() * 0.01f, 1f);
				Vector3 vector = (Vector3)_infos[1];
				bool flag2 = num2 == num;
				if (flag2)
				{
					_affectedCharacter.AutoKnock(true, vector);
					ownerCharacter.AutoKnock(true, -vector);
				}
				else
				{
					bool flag3 = num2 == 1f;
					if (flag3)
					{
						_affectedCharacter.AutoKnock(true, vector);
					}
					else
					{
						bool flag4 = num == 1f;
						if (flag4)
						{
							ownerCharacter.AutoKnock(true, -vector);
						}
						else
						{
							float num3 = _affectedCharacter.Stability / (1f - num);
							float num4 = ownerCharacter.Stability / (1f - num2);
							this.Knockback = Mathf.Min(num3, num4);
							this.DamageAmplifiedByOwner = false;
							bool flag5 = num3 < num4;
							if (flag5)
							{
								_affectedCharacter.AutoKnock(true, (Vector3)_infos[1]);
								_infos[1] = -(Vector3)_infos[1];
								base.ActivateLocally(ownerCharacter, _infos);
							}
							else
							{
								base.ActivateLocally(_affectedCharacter, _infos);
								_infos[1] = -(Vector3)_infos[1];
								ownerCharacter.AutoKnock(true, (Vector3)_infos[1]);
							}
						}
					}
				}
			}
		}
	}
}
