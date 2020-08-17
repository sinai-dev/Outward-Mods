using System;
using HarmonyLib;
using TinyHelper;

namespace Juggernaut
{
	// Token: 0x02000013 RID: 19
	[HarmonyPatch(typeof(Character), "BlockInput")]
	public class Character_BlockInput
	{
		// Token: 0x06000046 RID: 70 RVA: 0x00003420 File Offset: 0x00001620
		[HarmonyPrefix]
		public static bool Prefix(Character __instance, ref bool _active)
		{
			bool isAI = __instance.IsAI;
			bool result;
			if (isAI)
			{
				result = true;
			}
			else
			{
				bool flag = !__instance.IsPhotonPlayerLocal || __instance.Sheathing || __instance.PreparingToSleep || __instance.Dodging || __instance.CurrentSpellCast == Character.SpellCastType.Flamethrower || __instance.CurrentSpellCast == Character.SpellCastType.PushKick;
				if (flag)
				{
					result = true;
				}
				else
				{
					bool flag2 = !_active || (__instance.InLocomotion && __instance.NextIsLocomotion);
					if (flag2)
					{
						result = true;
					}
					else
					{
						bool flag3 = __instance.ShieldEquipped || __instance.CurrentWeapon == null || !SkillRequirements.CanParryCancelnimations(__instance);
						if (flag3)
						{
							result = true;
						}
						else
						{
							int num = (int)At.GetValue(typeof(Character), __instance, "m_dodgeAllowedInAction");
							bool flag4 = num > 0 && __instance.NextAtkAllowed > 0;
							if (flag4)
							{
								__instance.photonView.RPC("SendBlockStateTrivial", 0, new object[]
								{
									true
								});
								__instance.StealthInput(false);
								result = false;
							}
							else
							{
								result = true;
							}
						}
					}
				}
			}
			return result;
		}
	}
}
