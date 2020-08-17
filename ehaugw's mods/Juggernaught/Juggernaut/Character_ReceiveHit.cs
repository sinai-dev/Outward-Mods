using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using TinyHelper;
using UnityEngine;

namespace Juggernaut
{
	// Token: 0x02000014 RID: 20
	[HarmonyPatch(typeof(Character), "ReceiveHit", new Type[]
	{
		typeof(Weapon),
		typeof(DamageList),
		typeof(Vector3),
		typeof(Vector3),
		typeof(float),
		typeof(float),
		typeof(Character),
		typeof(float),
		typeof(bool)
	})]
	public class Character_ReceiveHit
	{
		// Token: 0x06000048 RID: 72 RVA: 0x00003544 File Offset: 0x00001744
		[HarmonyPostfix]
		public static void Postfix(Character __instance, ref DamageList __result, Weapon _weapon, DamageList _damage, Vector3 _hitDir, Vector3 _hitPoint, float _angle, float _angleDir, Character _dealerChar, float _knockBack, bool _hitInventory)
		{
			bool flag = SkillRequirements.CanEnrageFromDamage(__instance) && __result.TotalDamage > 0f;
			if (flag)
			{
				__instance.StatusEffectMngr.AddStatusEffectBuildUp(ResourcesPrefabManager.Instance.GetStatusEffectPrefab("Rage"), _damage.TotalDamage, __instance);
			}
			bool flag2 = SkillRequirements.CanTerrify((_weapon != null) ? _weapon.OwnerCharacter : null) && __result.TotalDamage > 0f;
			if (flag2)
			{
				List<Character> list = new List<Character>();
				CharacterManager.Instance.FindCharactersInRange(__instance.CenterPosition, 7f, ref list);
				list = (from c in list
				where c.Faction == __instance.Faction
				select c).ToList<Character>();
				foreach (Character character in list)
				{
					StatusEffectManager statusEffectMngr = character.StatusEffectMngr;
					bool flag3 = statusEffectMngr != null;
					if (flag3)
					{
						bool flag4 = statusEffectMngr.HasStatusEffect("Confusion");
						statusEffectMngr.AddStatusEffectBuildUp(ResourcesPrefabManager.Instance.GetStatusEffectPrefab("Confusion"), Mathf.Clamp(__result.TotalDamage, 0f, 20f), character);
						bool flag5 = !flag4 && statusEffectMngr.HasStatusEffect("Confusion");
						if (flag5)
						{
							CasualStagger.Stagger(character);
						}
					}
				}
			}
		}
	}
}
