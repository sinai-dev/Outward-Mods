using System;
using HarmonyLib;
using SideLoader;

namespace Templar
{
	// Token: 0x02000017 RID: 23
	[HarmonyPatch(typeof(CharacterStats), "UseStamina", new Type[]
	{
		typeof(float),
		typeof(float)
	})]
	public class CharacterStats_UseStamina
	{
		// Token: 0x06000039 RID: 57 RVA: 0x00004548 File Offset: 0x00002748
		[HarmonyPostfix]
		public static void Postfix(CharacterStats __instance, float _staminaConsumed)
		{
			Character character = (Character)At.GetValue(typeof(CharacterStats), __instance, "m_character");
			bool flag = character != null && character.Inventory.SkillKnowledge.IsItemLearned(2502002) && (character.StatusEffectMngr.HasStatusEffect("Bless") || character.StatusEffectMngr.HasStatusEffect("Bless Amplified"));
			if (flag)
			{
				character.StatusEffectMngr.AddStatusEffectBuildUp(Templar.Instance.burstOfDivinityInstance, _staminaConsumed * Templar.GetFreeCastBuildup(character), character);
			}
		}
	}
}
