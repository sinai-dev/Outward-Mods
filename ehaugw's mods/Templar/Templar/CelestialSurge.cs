using System;
using System.Collections.Generic;
using System.Linq;
using TinyHelper;
using UnityEngine;

namespace Templar
{
	// Token: 0x02000012 RID: 18
	internal class CelestialSurge : Effect
	{
		// Token: 0x06000025 RID: 37 RVA: 0x00003C6C File Offset: 0x00001E6C
		public static float AmpDamage(Character character, float damage, DamageType.Types type)
		{
			DamageList damageList = new DamageList(type, damage);
			character.Stats.GetAmplifiedDamage(null, ref damageList);
			return damageList.TotalDamage;
		}

		// Token: 0x06000026 RID: 38 RVA: 0x00003C9C File Offset: 0x00001E9C
		public static void StaticActivate(Character character, object[] _infos, Effect instance)
		{
			float localRangeSquared = 100f;
			Dictionary<Character, float> dictionary = new Dictionary<Character, float>();
			List<Character> list = new List<Character>();
			CharacterManager.Instance.FindCharactersInRange(character.CenterPosition, CelestialSurge.range + 10f, ref list);
			list = (from c in list
			where c.Faction != character.Faction
			select c).ToList<Character>();
			using (IEnumerator<Character> enumerator = (from c in list
			where c.StatusEffectMngr.HasStatusEffect("Radiating")
			select c).GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					Character chr = enumerator.Current;
					float num = CelestialSurge.AmpDamage(character, Radiating.sumTicks(chr.StatusEffectMngr.GetStatusEffectOfName("Radiating").RemainingLifespan) + 40f, DamageType.Types.Electric);
					IEnumerable<Character> source = list;

					foreach (Character character2 in source.Where(c => (c.transform.position - chr.transform.position).sqrMagnitude <= localRangeSquared))
					{
						bool flag = !character2.Invincible;
						if (flag)
						{
							float num2;
							dictionary[character2] = (dictionary.TryGetValue(character2, out num2) ? num2 : 0f) + num;
						}
					}
				}
			}
			foreach (KeyValuePair<Character, float> keyValuePair in dictionary)
			{
				DamageList damageList = new DamageList(DamageType.Types.Electric, keyValuePair.Value);
				keyValuePair.Key.Stats.GetMitigatedDamage(null, ref damageList);
				keyValuePair.Key.Stats.ReceiveDamage(damageList.TotalDamage);
				bool flag2 = damageList.TotalDamage > 55f;
				if (flag2)
				{
					keyValuePair.Key.AutoKnock(true, Vector3.zero);
				}
				else
				{
					CasualStagger.Stagger(keyValuePair.Key);
				}
			}
		}

		// Token: 0x06000027 RID: 39 RVA: 0x00003F00 File Offset: 0x00002100
		protected override void ActivateLocally(Character character, object[] _infos)
		{
			CelestialSurge.StaticActivate(character, _infos, this);
		}

		// Token: 0x04000009 RID: 9
		public static float range = 50f;
	}
}
