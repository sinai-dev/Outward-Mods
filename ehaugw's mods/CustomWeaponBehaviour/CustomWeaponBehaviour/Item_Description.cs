using System;
using HarmonyLib;

namespace CustomWeaponBehaviour
{
	// Token: 0x02000008 RID: 8
	[HarmonyPatch(typeof(Item), "Description", MethodType.Getter)]
	public class Item_Description
	{
		// Token: 0x0600001A RID: 26 RVA: 0x000026F4 File Offset: 0x000008F4
		[HarmonyPostfix]
		public static void Postfix(Item __instance, ref string __result)
		{
			bool flag = __instance.HasTag(CustomWeaponBehaviour.BastardTag);
			if (flag)
			{
				bool flag2 = __result.Length > 0;
				if (flag2)
				{
					__result += "\n\n";
				}
				__result += "This blade should probably be wielded in two hands.";
			}
			bool flag3 = __instance.HasTag(CustomWeaponBehaviour.FinesseTag);
			if (flag3)
			{
				bool flag4 = __result.Length > 0;
				if (flag4)
				{
					__result += "\n\n";
				}
				__result += "A lightweight blade that can be swung at an overwhelming speed.";
			}
		}
	}
}
