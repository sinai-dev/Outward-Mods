using System;
using HarmonyLib;

namespace Templar
{
	// Token: 0x02000016 RID: 22
	[HarmonyPatch(typeof(Weapon), "AddImbueEffect")]
	public class Weapon_AddImbueEffect
	{
		// Token: 0x06000037 RID: 55 RVA: 0x000044C8 File Offset: 0x000026C8
		[HarmonyPrefix]
		public static void Prefix(Weapon __instance, ref ImbueEffectPreset _effect)
		{
			CharacterSkillKnowledge characterSkillKnowledge;
			if (__instance == null)
			{
				characterSkillKnowledge = null;
			}
			else
			{
				Character ownerCharacter = __instance.OwnerCharacter;
				if (ownerCharacter == null)
				{
					characterSkillKnowledge = null;
				}
				else
				{
					CharacterInventory inventory = ownerCharacter.Inventory;
					characterSkillKnowledge = ((inventory != null) ? inventory.SkillKnowledge : null);
				}
			}
			CharacterSkillKnowledge characterSkillKnowledge2 = characterSkillKnowledge;
			bool flag = _effect.PresetID == 219 && characterSkillKnowledge2 != null && characterSkillKnowledge2.IsItemLearned(2502026);
			if (flag)
			{
				_effect = (ImbueEffectPreset)ResourcesPrefabManager.Instance.GetEffectPreset(269);
			}
		}
	}
}
