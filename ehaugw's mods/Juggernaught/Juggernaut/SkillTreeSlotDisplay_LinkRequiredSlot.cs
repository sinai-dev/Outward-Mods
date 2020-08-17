using System;
using HarmonyLib;
using TinyHelper;
using UnityEngine;

namespace Juggernaut
{
	// Token: 0x0200000A RID: 10
	[HarmonyPatch(typeof(SkillTreeSlotDisplay), "LinkRequiredSlot")]
	public class SkillTreeSlotDisplay_LinkRequiredSlot
	{
		// Token: 0x06000032 RID: 50 RVA: 0x00002E6C File Offset: 0x0000106C
		[HarmonyPrefix]
		public static bool Prefix(SkillTreeSlotDisplay __instance, SkillTreeSlotDisplay _slotToLink)
		{
			RectTransform rectTransform = At.GetValue(typeof(SkillTreeSlotDisplay), __instance, "m_link") as RectTransform;
			bool flag = rectTransform != null;
			if (flag)
			{
				rectTransform.gameObject.SetActive(true);
				Rect rect = rectTransform.rect;
				rect.height = Vector2.Distance(_slotToLink.RectTransform.anchoredPosition, __instance.RectTransform.anchoredPosition);
				rectTransform.sizeDelta = rect.size;
				Vector2 vector = _slotToLink.RectTransform.anchoredPosition - __instance.RectTransform.anchoredPosition;
				float num = Mathf.Atan2(vector.y, vector.x) * 57.29578f;
				rectTransform.rotation = Quaternion.Euler(0f, 0f, num - 90f);
			}
			return false;
		}
	}
}
