using System;
using System.Collections.Generic;
using System.Linq;

namespace Templar
{
	// Token: 0x02000011 RID: 17
	internal class HealingAoE : Effect
	{
		// Token: 0x06000020 RID: 32 RVA: 0x00003A94 File Offset: 0x00001C94
		public static float AmpHealing(Character character, float heal, DamageType.Types type)
		{
			bool flag = type == DamageType.Types.Count;
			float result;
			if (flag)
			{
				result = heal;
			}
			else
			{
				DamageList damageList = new DamageList(type, heal);
				character.Stats.GetAmplifiedDamage(null, ref damageList);
				result = damageList.TotalDamage;
			}
			return result;
		}

		// Token: 0x06000021 RID: 33 RVA: 0x00003AD0 File Offset: 0x00001CD0
		public static void StaticActivate(Effect instance, Character _affectedCharacter, object[] _infos, float _range, float _restoredHealth, DamageType.Types _amplificationType, bool _canRevive)
		{
			List<Character> list = new List<Character>();
			CharacterManager.Instance.FindCharactersInRange(_affectedCharacter.CenterPosition, _range, ref list);
			list = (from c in list
			where c.Faction == _affectedCharacter.Faction
			select c).ToList<Character>();
			float quantity = HealingAoE.AmpHealing(_affectedCharacter, _restoredHealth, _amplificationType);
			foreach (Character character in list)
			{
				bool flag = character.IsDead && _canRevive;
				if (flag)
				{
					HealingAoE.Revive(character);
				}
				bool flag2 = !character.IsDead || _canRevive;
				if (flag2)
				{
					character.Stats.AffectHealth(quantity);
				}
			}
		}

		// Token: 0x06000022 RID: 34 RVA: 0x00003BAC File Offset: 0x00001DAC
		public static void Revive(Character character)
		{
			bool flag = character;
			if (flag)
			{
				bool flag2 = !character.IsAI;
				if (flag2)
				{
					PlayerSaveData playerSaveData = new PlayerSaveData(character);
					playerSaveData.BurntHealth += character.ActiveMaxHealth * 0.5f;
					playerSaveData.Health = character.ActiveMaxHealth;
					playerSaveData.Stamina = character.ActiveMaxStamina;
				}
				character.Resurrect(null, true);
			}
		}

		// Token: 0x06000023 RID: 35 RVA: 0x00003C18 File Offset: 0x00001E18
		protected override void ActivateLocally(Character character, object[] _infos)
		{
			HealingAoE.StaticActivate(this, character, _infos, this.Range, this.RestoredHealth, this.AmplificationType, this.CanRevive);
		}

		// Token: 0x04000005 RID: 5
		public float RestoredHealth = 0f;

		// Token: 0x04000006 RID: 6
		public bool CanRevive = false;

		// Token: 0x04000007 RID: 7
		public float Range = 30f;

		// Token: 0x04000008 RID: 8
		public DamageType.Types AmplificationType = DamageType.Types.Count;
	}
}
