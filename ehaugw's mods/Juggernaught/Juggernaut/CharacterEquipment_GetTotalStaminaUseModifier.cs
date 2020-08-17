using System;
using HarmonyLib;
using TinyHelper;
using UnityEngine;

namespace Juggernaut
{
	// Token: 0x02000010 RID: 16
	[HarmonyPatch(typeof(CharacterEquipment), "GetTotalStaminaUseModifier")]
	public class CharacterEquipment_GetTotalStaminaUseModifier
	{
		// Token: 0x0600003F RID: 63 RVA: 0x0000322C File Offset: 0x0000142C
		[HarmonyPrefix]
		public static void Prefix(CharacterEquipment __instance, out Tuple<float?, Stat, Character> __state)
		{
			__state = null;
			Character character = At.GetValue(typeof(CharacterEquipment), __instance, "m_character") as Character;
			bool flag = character != null && SkillRequirements.CanReduceStaminaCostArmorPenalty(character);
			if (flag)
			{
				CharacterStats stats = character.Stats;
				Stat stat = At.GetValue(typeof(CharacterStats), stats, "m_equipementPenalties") as Stat;
				bool flag2 = stat != null;
				if (flag2)
				{
					__state = new Tuple<float?, Stat, Character>(new float?(stat.CurrentValue), stat, character);
					At.SetValue<float>(__state.Item1.Value * JuggernautFormulas.GetUnyieldingStaminaCostForgivenes(character), typeof(Stat), stat, "m_currentValue");
				}
			}
		}

		// Token: 0x06000040 RID: 64 RVA: 0x000032DC File Offset: 0x000014DC
		[HarmonyPostfix]
		public static void Postfix(CharacterEquipment __instance, Tuple<float?, Stat, Character> __state)
		{
			bool flag = __state != null;
			if (flag)
			{
				float? item = __state.Item1;
				float num = __state.Item2.CurrentValue / JuggernautFormulas.GetUnyieldingStaminaCostForgivenes(__state.Item3);
				bool flag2 = !(item.GetValueOrDefault() == num & item != null);
				if (flag2)
				{
					Debug.Log("Logic error at CharacterEquipment_GetTotalStaminaUseModifier in Juggernaut class. m_equipementPenalties changed during call!");
				}
				At.SetValue<float>(__state.Item1.Value, typeof(Stat), __state.Item2, "m_currentValue");
			}
		}
	}
}
