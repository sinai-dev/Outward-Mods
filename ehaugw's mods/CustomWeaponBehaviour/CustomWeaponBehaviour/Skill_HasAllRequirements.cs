using System;
using HarmonyLib;
using TinyHelper;

namespace CustomWeaponBehaviour
{
	// Token: 0x02000006 RID: 6
	[HarmonyPatch(typeof(Skill), "HasAllRequirements")]
	public class Skill_HasAllRequirements
	{
		// Token: 0x06000016 RID: 22 RVA: 0x00002630 File Offset: 0x00000830
		[HarmonyPrefix]
		public static void Prefix(Skill __instance, bool _tryingToActivate)
		{
			bool flag;
			if (_tryingToActivate)
			{
				MeleeSkill meleeSkill = __instance as MeleeSkill;
				if (meleeSkill != null && __instance.OwnerCharacter.NextAtkAllowed == 2)
				{
					flag = meleeSkill.HasAllRequirements(false);
					goto IL_26;
				}
			}
			flag = false;
			IL_26:
			bool flag2 = flag;
			if (flag2)
			{
				__instance.OwnerCharacter.ForceCancel(true, true);
				At.SetValue<bool>(true, typeof(Character), __instance.OwnerCharacter, "m_inLocomotion");
				At.SetValue<bool>(true, typeof(Character), __instance.OwnerCharacter, "m_nextIsLocomotion");
			}
		}
	}
}
