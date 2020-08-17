using System;
using HarmonyLib;

namespace CustomWeaponBehaviour
{
	// Token: 0x0200000A RID: 10
	[HarmonyPatch(typeof(Character), "SendPerformAttackTrivial", new Type[]
	{
		typeof(int),
		typeof(int)
	})]
	public class Character_SendPerformAttackTrivial
	{
		// Token: 0x0600001E RID: 30 RVA: 0x000027BF File Offset: 0x000009BF
		[HarmonyPrefix]
		public static void Prefix(Character __instance, ref int _type, ref int _id)
		{
			BehaviourManager.AdaptGrip(__instance, ref _type, ref _id);
		}
	}
}
