using System;

namespace Templar
{
	// Token: 0x02000013 RID: 19
	internal class ChannelDivinity : Effect
	{
		// Token: 0x0600002A RID: 42 RVA: 0x00003F24 File Offset: 0x00002124
		protected override void ActivateLocally(Character character, object[] _infos)
		{
			bool flag = this.TryRemove(character, "Dez Runes");
			if (flag)
			{
				character.StatusEffectMngr.AddStatusEffect(ResourcesPrefabManager.Instance.GetStatusEffectPrefab("Runic Protection Amplified"), character);
			}
			else
			{
				bool flag2 = this.TryRemove(character, "Shim Runes");
				if (flag2)
				{
					CelestialSurge.StaticActivate(character, _infos, this);
				}
				else
				{
					bool flag3 = this.TryRemove(character, "Egoth Runes");
					if (flag3)
					{
						character.Stats.AffectHealth(HealingAoE.AmpHealing(character, 40f, DamageType.Types.Electric));
					}
					else
					{
						bool flag4 = this.TryRemove(character, "Fal Runes");
						if (flag4)
						{
							character.StatusEffectMngr.AddStatusEffect(Templar.Instance.surgeOfDivinityInstance, character);
						}
						else
						{
							HealingAoE.StaticActivate(this, character, _infos, 30f, 30f, DamageType.Types.Electric, true);
						}
					}
				}
			}
		}

		// Token: 0x0600002B RID: 43 RVA: 0x00003FE8 File Offset: 0x000021E8
		private bool TryRemove(Character character, string spellIdentifierName)
		{
			bool flag = character.StatusEffectMngr.HasStatusEffect(spellIdentifierName);
			bool result;
			if (flag)
			{
				character.StatusEffectMngr.RemoveStatusWithIdentifierName(spellIdentifierName);
				result = true;
			}
			else
			{
				result = false;
			}
			return result;
		}
	}
}
