using System;
using HarmonyLib;

namespace CustomWeaponBehaviour
{
	// Token: 0x0200000C RID: 12
	[HarmonyPatch(typeof(Character), "StopLocomotionAction")]
	public class Character_StopLocomotionAction
	{
		// Token: 0x06000022 RID: 34 RVA: 0x000027FF File Offset: 0x000009FF
		[HarmonyPrefix]
		public static void Prefix(Character __instance)
		{
			CustomWeaponBehaviour.ResetGrip(__instance);
		}
	}
}
