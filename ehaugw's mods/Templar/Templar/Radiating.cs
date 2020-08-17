using System;
using System.Collections.Generic;
using System.Linq;
using SideLoader;
using UnityEngine;

namespace Templar
{
	// Token: 0x0200001A RID: 26
	internal class Radiating : Effect
	{
		// Token: 0x06000040 RID: 64 RVA: 0x00004884 File Offset: 0x00002A84
		public static float AmpDamage(Character character, float damage, DamageType.Types type)
		{
			bool flag = type == DamageType.Types.Count;
			float result;
			if (flag)
			{
				result = damage;
			}
			else
			{
				DamageList damageList = new DamageList(type, damage);
				character.Stats.GetAmplifiedDamage(null, ref damageList);
				result = damageList.TotalDamage;
			}
			return result;
		}

		// Token: 0x06000041 RID: 65 RVA: 0x000048C0 File Offset: 0x00002AC0
		protected override void ActivateLocally(Character character, object[] _infos)
		{
			float num = 15f;
			object value = At.GetValue(typeof(Effect), this, "m_parentStatusEffect");
			StatusEffect parent = value as StatusEffect;
			bool flag = parent != null;
			if (flag)
			{
				float num2 = num - parent.RemainingLifespan;
				float damage = Radiating.AmpDamage(parent.SourceCharacter, 15f * Convert.ToSingle(Math.Exp((double)(-(double)num2 / 10f))) / Radiating.normalize(), DamageType.Types.Electric);
				List<Character> list = new List<Character>();
				CharacterManager.Instance.FindCharactersInRange(character.CenterPosition, 10f, ref list);
				IEnumerable<Character> source = list;

				foreach (Character character2 in source.Where(x => x.Faction != parent.SourceCharacter.Faction))
				{
					bool flag2 = !character2.Invincible;
					if (flag2)
					{
						DamageList damageList = new DamageList(DamageType.Types.Electric, damage);
						character2.Stats.GetMitigatedDamage(null, ref damageList);
						character2.Stats.ReceiveDamage(damageList.TotalDamage);
					}
				}
			}
			else
			{
				Debug.Log("Radiating Effect had not parent Status Effect");
			}
		}

		// Token: 0x06000042 RID: 66 RVA: 0x00004A28 File Offset: 0x00002C28
		public static float normalize()
		{
			double num = Math.Exp(-0.10000000149011612);
			double y = 15.0;
			return Convert.ToSingle((1.0 - Math.Pow(num, y)) / (1.0 - num));
		}

		// Token: 0x06000043 RID: 67 RVA: 0x00004A78 File Offset: 0x00002C78
		public static float sumTicks(float remain)
		{
			double num = Math.Exp(-0.10000000149011612);
			double num2 = 15.0;
			return Convert.ToSingle((Math.Pow(num, num2 - (double)remain) - Math.Pow(num, num2)) / (1.0 - num));
		}

		// Token: 0x06000044 RID: 68 RVA: 0x00004AC8 File Offset: 0x00002CC8
		public static float sumToRemain(float s)
		{
			double num = Math.Exp(-0.10000000149011612);
			double num2 = 15.0;
			return Convert.ToSingle(num2 - Math.Log((double)s * (1.0 - num) + Math.Pow(num, num2)) / Math.Log(num));
		}

		// Token: 0x0400000A RID: 10
		public const float RANGE = 10f;

		// Token: 0x0400000B RID: 11
		public const float RADIATING_LIFE_SPAN = 15f;

		// Token: 0x0400000C RID: 12
		public const float DECAY = 10f;

		// Token: 0x0400000D RID: 13
		public const float DAMAGE = 15f;
	}
}
