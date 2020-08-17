using System;
using HarmonyLib;

namespace CustomWeaponBehaviour
{
	// Token: 0x0200000B RID: 11
	[HarmonyPatch(typeof(Character), "HitEnded")]
	public class Character_HitEnded
	{
		// Token: 0x06000020 RID: 32 RVA: 0x000027D4 File Offset: 0x000009D4
		[HarmonyPrefix]
		public static void Prefix(Character __instance, ref int _attackID)
		{
			bool flag = _attackID != -2;
			if (flag)
			{
				CustomWeaponBehaviour.ResetGrip(__instance);
			}
		}
	}
}
