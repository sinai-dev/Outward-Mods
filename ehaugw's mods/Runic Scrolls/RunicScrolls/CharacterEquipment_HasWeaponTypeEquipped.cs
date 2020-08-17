using System;
using HarmonyLib;

namespace RunicScrolls
{
	// Token: 0x02000003 RID: 3
	[HarmonyPatch(typeof(CharacterEquipment), "HasWeaponTypeEquipped")]
	public class CharacterEquipment_HasWeaponTypeEquipped
	{
		// Token: 0x06000003 RID: 3 RVA: 0x000020D8 File Offset: 0x000002D8
		[HarmonyPostfix]
		public static void Postfix(CharacterEquipment __instance, ref bool __result, ref EquipmentSlot.EquipmentSlotIDs _slotID, ref Weapon.WeaponType _weaponType)
		{
			bool flag = __result && _weaponType == Weapon.WeaponType.Arrow;
			if (flag)
			{
				Weapon weapon = __instance.GetEquippedItem(_slotID) as Weapon;
				__result = !weapon.HasTag(TagSourceManager.Instance.GetTag(161.ToString()));
			}
		}
	}
}
