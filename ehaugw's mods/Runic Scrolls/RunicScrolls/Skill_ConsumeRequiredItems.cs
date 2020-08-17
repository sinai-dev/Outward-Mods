using System;
using HarmonyLib;

namespace RunicScrolls
{
	// Token: 0x02000004 RID: 4
	[HarmonyPatch(typeof(Skill), "ConsumeRequiredItems")]
	public class Skill_ConsumeRequiredItems
	{
		// Token: 0x06000005 RID: 5 RVA: 0x0000213C File Offset: 0x0000033C
		[HarmonyPrefix]
		public static void Prefix(Skill __instance)
		{
			AttackSkill attackSkill = __instance as AttackSkill;
			bool flag = attackSkill != null && RunicScrolls.IsRune(__instance);
			if (flag)
			{
				bool flag2 = attackSkill.RequiredTags != null && attackSkill.RequiredTags.Length != 0;
				if (flag2)
				{
					bool flag3 = false;
					bool flag4 = attackSkill.OwnerCharacter.Inventory.SkillKnowledge.IsItemLearned(8205170);
					if (flag4)
					{
						flag3 = true;
					}
					for (int i = 0; i < attackSkill.RequiredTags.Length; i++)
					{
						bool flag5 = (attackSkill.OwnerCharacter.CurrentWeapon != null && attackSkill.OwnerCharacter.CurrentWeapon.HasTag(attackSkill.RequiredTags[i].Tag)) || (attackSkill.OwnerCharacter.LeftHandWeapon != null && attackSkill.OwnerCharacter.LeftHandWeapon.HasTag(attackSkill.RequiredTags[i].Tag)) || (attackSkill.OwnerCharacter.LeftHandEquipment != null && attackSkill.OwnerCharacter.LeftHandEquipment.HasTag(attackSkill.RequiredTags[i].Tag));
						if (flag5)
						{
							flag3 = true;
						}
					}
					bool flag6 = !flag3;
					if (flag6)
					{
						Item equippedItem = attackSkill.OwnerCharacter.Inventory.Equipment.GetEquippedItem(EquipmentSlot.EquipmentSlotIDs.Quiver);
						bool flag7 = equippedItem != null && equippedItem.HasTag(TagSourceManager.Instance.GetTag(161.ToString()));
						if (flag7)
						{
							equippedItem.RemoveQuantity(1);
						}
					}
				}
			}
		}
	}
}
