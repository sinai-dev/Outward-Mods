using System;
using HarmonyLib;
using TinyHelper;
using UnityEngine;

namespace Juggernaut
{
	// Token: 0x0200000F RID: 15
	[HarmonyPatch(typeof(CharacterEquipment), "GetTotalMovementModifier")]
	public class CharacterEquipment_GetTotalMovementModifier
	{
		// Token: 0x0600003C RID: 60 RVA: 0x000030EC File Offset: 0x000012EC
		[HarmonyPrefix]
		public static void Prefix(CharacterEquipment __instance, out Tuple<float?, Stat, Character> __state)
		{
			__state = null;
			Character character = At.GetValue(typeof(CharacterEquipment), __instance, "m_character") as Character;
			bool flag = character != null && SkillRequirements.CanReduceMoveSpeedArmorPenalty(character);
			if (flag)
			{
				CharacterStats stats = character.Stats;
				Stat stat = At.GetValue(typeof(CharacterStats), stats, "m_equipementPenalties") as Stat;
				bool flag2 = stat != null;
				if (flag2)
				{
					__state = new Tuple<float?, Stat, Character>(new float?(stat.CurrentValue), stat, character);
					At.SetValue<float>(__state.Item1.Value * JuggernautFormulas.GetUnyieldingMovementSpeedForgivenes(character), typeof(Stat), stat, "m_currentValue");
				}
			}
		}

		// Token: 0x0600003D RID: 61 RVA: 0x0000319C File Offset: 0x0000139C
		[HarmonyPostfix]
		public static void Postfix(CharacterEquipment __instance, Tuple<float?, Stat, Character> __state)
		{
			bool flag = __state != null;
			if (flag)
			{
				float? item = __state.Item1;
				float num = __state.Item2.CurrentValue / JuggernautFormulas.GetUnyieldingMovementSpeedForgivenes(__state.Item3);
				bool flag2 = !(item.GetValueOrDefault() == num & item != null);
				if (flag2)
				{
					Debug.Log("Logic error at CharacterEquipment_GetTotalMovementModifier in Juggernaut class. m_equipementPenalties changed during call!");
				}
				At.SetValue<float>(__state.Item1.Value, typeof(Stat), __state.Item2, "m_currentValue");
			}
		}
	}
}
