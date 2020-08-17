using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Templar
{
	// Token: 0x0200001B RID: 27
	internal class CondemnConjureLightning : Effect
	{
		// Token: 0x06000046 RID: 70 RVA: 0x00004B28 File Offset: 0x00002D28
		protected override void ActivateLocally(Character character, object[] _infos)
		{
			Debug.Log("EHAUGE DETONATED LIGHTNING");
			List<Character> list = new List<Character>();
			CharacterManager.Instance.FindCharactersInRange(character.CenterPosition, 50f, ref list);
			list = (from c in list
			where c.StatusEffectMngr.HasStatusEffect("Doom") && c.Faction != character.Faction
			select c).ToList<Character>();
			foreach (Character character2 in list)
			{
				character2.Stats.ReceiveDamage(100f);
			}
		}
	}
}
