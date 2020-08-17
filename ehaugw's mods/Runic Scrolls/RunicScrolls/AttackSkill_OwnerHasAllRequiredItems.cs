using System;
using HarmonyLib;

namespace RunicScrolls
{
	// Token: 0x02000002 RID: 2
	[HarmonyPatch(typeof(AttackSkill), "OwnerHasAllRequiredItems")]
	public class AttackSkill_OwnerHasAllRequiredItems
	{
		// Token: 0x06000001 RID: 1 RVA: 0x00002050 File Offset: 0x00000250
		[HarmonyPrefix]
		public static bool Prefix(AttackSkill __instance, ref bool __result)
		{
			bool flag = RunicScrolls.IsRune(__instance);
			if (flag)
			{
				for (int i = 0; i < __instance.RequiredTags.Length; i++)
				{
					Item equippedItem = __instance.OwnerCharacter.Inventory.Equipment.GetEquippedItem(EquipmentSlot.EquipmentSlotIDs.Quiver);
					bool flag2 = equippedItem != null && equippedItem.HasTag(__instance.RequiredTags[i].Tag);
					if (flag2)
					{
						__result = true;
						return false;
					}
				}
			}
			return true;
		}
	}
}
