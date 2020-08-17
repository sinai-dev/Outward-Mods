using System;
using System.Collections.Generic;
using System.Linq;
using TinyHelper;

namespace Juggernaut
{
	// Token: 0x02000006 RID: 6
	public class WarCryEffect : Effect
	{
		// Token: 0x06000012 RID: 18 RVA: 0x0000287C File Offset: 0x00000A7C
		protected override void ActivateLocally(Character _affectedCharacter, object[] _infos)
		{
			List<Character> list = new List<Character>();
			CharacterManager.Instance.FindCharactersInRange(_affectedCharacter.CenterPosition, 20f, ref list);
			list = (from c in list
			where c.Faction != _affectedCharacter.Faction
			select c).ToList<Character>();
			foreach (Character character in list)
			{
				bool flag = SkillRequirements.Careful(_affectedCharacter);
				if (flag)
				{
					character.StatusEffectMngr.AddStatusEffect(ResourcesPrefabManager.Instance.GetStatusEffectPrefab("Confusion"), _affectedCharacter);
				}
				bool flag2 = SkillRequirements.Vengeful(_affectedCharacter);
				if (flag2)
				{
					character.StatusEffectMngr.AddStatusEffect(ResourcesPrefabManager.Instance.GetStatusEffectPrefab("Pain"), _affectedCharacter);
				}
				CasualStagger.Stagger(character);
			}
		}

		// Token: 0x0400000B RID: 11
		public const float Range = 20f;
	}
}
