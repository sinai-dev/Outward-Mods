using System;
using HarmonyLib;

namespace CustomWeaponBehaviour
{
	// Token: 0x0200000D RID: 13
	[HarmonyPatch(typeof(Character), "NextAttack")]
	public class Character_NextAttack
	{
		// Token: 0x06000024 RID: 36 RVA: 0x00002814 File Offset: 0x00000A14
		[HarmonyPrefix]
		public static bool Prefix(Character __instance, ref int _type, ref int _id)
		{
			bool flag = __instance.NextAttack1Only && _type != 0;
			return !flag;
		}
	}
}
