using System;
using HarmonyLib;

namespace Templar
{
	// Token: 0x02000015 RID: 21
	[HarmonyPatch(typeof(StatusEffectManager), "AddStatusEffect", new Type[]
	{
		typeof(StatusEffect),
		typeof(Character),
		typeof(string[])
	})]
	public class StatusEffectManager_AddStatusEffect
	{
		// Token: 0x06000034 RID: 52 RVA: 0x000043E0 File Offset: 0x000025E0
		[HarmonyPrefix]
		public static void Prefix(StatusEffectManager __instance, StatusEffect _statusEffect, out StatusData __state)
		{
			__state = null;
			bool flag = _statusEffect.IdentifierName == Templar.Instance.radiatingInstance.IdentifierName && __instance.HasStatusEffect(_statusEffect.IdentifierName);
			if (flag)
			{
				__state = _statusEffect.StatusData;
				_statusEffect.StatusData = new StatusData(__state);
				float remainingLifespan = __instance.GetStatusEffectOfName(_statusEffect.IdentifierName).RemainingLifespan;
				float num = (_statusEffect.RemainingLifespan != 0f) ? _statusEffect.RemainingLifespan : _statusEffect.StartLifespan;
				float num2 = Radiating.sumTicks(remainingLifespan);
				float num3 = Radiating.sumTicks(num);
				num = Radiating.sumToRemain(num2 + num3);
				_statusEffect.StatusData.LifeSpan = num;
			}
		}

		// Token: 0x06000035 RID: 53 RVA: 0x0000448C File Offset: 0x0000268C
		[HarmonyPostfix]
		public static void Postfix(StatusEffectManager __instance, StatusEffect _statusEffect, StatusData __state)
		{
			bool flag = ((_statusEffect != null) ? _statusEffect.StatusData : null) != null && __state != null;
			if (flag)
			{
				_statusEffect.StatusData = __state;
			}
		}
	}
}
