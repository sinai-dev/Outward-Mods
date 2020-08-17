using System;
using HarmonyLib;
using SideLoader;
using UnityEngine;

namespace Templar
{
	// Token: 0x02000019 RID: 25
	[HarmonyPatch(typeof(CharacterStats), "UseMana")]
	public class CharacterStats_UseMana
	{
		// Token: 0x0600003D RID: 61 RVA: 0x000046EC File Offset: 0x000028EC
		[HarmonyPrefix]
		public static void Prefix(CharacterStats __instance, ref float _amount, out Tuple<Character, float, int, StatusEffect> __state)
		{
			__state = null;
			Character character = At.GetValue(typeof(CharacterStats), __instance, "m_character") as Character;
			bool flag = character != null && character.StatusEffectMngr != null;
			if (flag)
			{
				StatusEffect statusEffectOfName = character.StatusEffectMngr.GetStatusEffectOfName(Templar.Instance.burstOfDivinityInstance.IdentifierName);
				bool flag2 = statusEffectOfName != null;
				if (flag2)
				{
					int num = statusEffectOfName.StackCount;
					bool flag3 = num > 0;
					if (flag3)
					{
						num = Math.Min(Convert.ToInt32(Math.Ceiling((double)(_amount / 7f))), num);
						float item = _amount;
						__state = new Tuple<Character, float, int, StatusEffect>(character, item, num, statusEffectOfName);
						_amount = Mathf.Max(0f, _amount - (float)num * 7f);
					}
				}
			}
		}

		// Token: 0x0600003E RID: 62 RVA: 0x000047B4 File Offset: 0x000029B4
		[HarmonyPostfix]
		public static void Postfix(CharacterStats __instance, ref float _amount, ref Tuple<Character, float, int, StatusEffect> __state)
		{
			bool flag = __state != null;
			if (flag)
			{
				_amount = __state.Item2;
				for (int i = 0; i < __state.Item3; i++)
				{
					__state.Item4.RemoveOldestStack();
				}
			}
			Character character = (Character)At.GetValue(typeof(CharacterStats), __instance, "m_character");
			bool flag2 = character != null && character.Inventory.SkillKnowledge.IsItemLearned(2502002) && (character.StatusEffectMngr.HasStatusEffect("Bless") || character.StatusEffectMngr.HasStatusEffect("Bless Amplified"));
			if (flag2)
			{
				character.Stats.AffectStamina(_amount * 1f);
			}
		}
	}
}
