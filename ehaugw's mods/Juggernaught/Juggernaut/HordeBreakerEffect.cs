using System;
using TinyHelper;
using UnityEngine;

namespace Juggernaut
{
	// Token: 0x02000007 RID: 7
	internal class HordeBreakerEffect : Effect
	{
		// Token: 0x06000014 RID: 20 RVA: 0x00002988 File Offset: 0x00000B88
		public static void StaticActivate(HordeBreakerEffect effect, Character defender, Character offender, object[] _infos, Effect instance)
		{
			bool flag = SkillRequirements.Careful(offender) && defender.StatusEffectMngr.HasStatusEffect("Confusion");
			if (flag)
			{
				defender.AutoKnock(true, Vector3.zero);
			}
			else
			{
				CasualStagger.Stagger(defender);
			}
			bool flag2 = SkillRequirements.Vengeful(offender) && defender.StatusEffectMngr.HasStatusEffect("Pain");
			if (flag2)
			{
				Debug.Log("SLOW");
				defender.StatusEffectMngr.AddStatusEffect(ResourcesPrefabManager.Instance.GetStatusEffectPrefab("Slow Down"), offender);
			}
		}

		// Token: 0x06000015 RID: 21 RVA: 0x00002A17 File Offset: 0x00000C17
		protected override void ActivateLocally(Character _affectedCharacter, object[] _infos)
		{
			HordeBreakerEffect.StaticActivate(this, _affectedCharacter, base.OwnerCharacter, _infos, this);
		}
	}
}
