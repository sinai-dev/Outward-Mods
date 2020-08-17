using System;

namespace Juggernaut
{
	// Token: 0x02000009 RID: 9
	public class SkillRequirements
	{
		// Token: 0x0600001A RID: 26 RVA: 0x00002AA0 File Offset: 0x00000CA0
		private static bool SafeHasSkillKnowledge(Character character, int skillID)
		{
			bool? flag;
			if (character == null)
			{
				flag = null;
			}
			else
			{
				CharacterInventory inventory = character.Inventory;
				if (inventory == null)
				{
					flag = null;
				}
				else
				{
					CharacterSkillKnowledge skillKnowledge = inventory.SkillKnowledge;
					flag = ((skillKnowledge != null) ? new bool?(skillKnowledge.IsItemLearned(skillID)) : null);
				}
			}
			bool? flag2 = flag;
			return flag2.GetValueOrDefault();
		}

		// Token: 0x0600001B RID: 27 RVA: 0x00002AFC File Offset: 0x00000CFC
		public static bool IsRageEffect(StatusEffect effect)
		{
			return effect != null && effect.HasMatch(TagSourceManager.Instance.GetTag(225.ToString()));
		}

		// Token: 0x0600001C RID: 28 RVA: 0x00002B38 File Offset: 0x00000D38
		public static bool IsDisciplineEffect(StatusEffect effect)
		{
			return effect != null && effect.HasMatch(TagSourceManager.Instance.GetTag(226.ToString()));
		}

		// Token: 0x0600001D RID: 29 RVA: 0x00002B74 File Offset: 0x00000D74
		public static bool Enraged(Character character)
		{
			return character.StatusEffectMngr.HasStatusEffect("Rage") || character.StatusEffectMngr.HasStatusEffect("Rage Amplified");
		}

		// Token: 0x0600001E RID: 30 RVA: 0x00002BAC File Offset: 0x00000DAC
		public static bool Disciplined(Character character)
		{
			return character.StatusEffectMngr.HasStatusEffect("Discipline") || character.StatusEffectMngr.HasStatusEffect("Discipline Amplified");
		}

		// Token: 0x0600001F RID: 31 RVA: 0x00002BE4 File Offset: 0x00000DE4
		public static bool Careful(Character character)
		{
			return SkillRequirements.SafeHasSkillKnowledge(character, 2502019);
		}

		// Token: 0x06000020 RID: 32 RVA: 0x00002C04 File Offset: 0x00000E04
		public static bool Vengeful(Character character)
		{
			return SkillRequirements.SafeHasSkillKnowledge(character, 2502020);
		}

		// Token: 0x06000021 RID: 33 RVA: 0x00002C24 File Offset: 0x00000E24
		public static bool CanParryCancelnimations(Character character)
		{
			return SkillRequirements.SafeHasSkillKnowledge(character, 2502019);
		}

		// Token: 0x06000022 RID: 34 RVA: 0x00002C44 File Offset: 0x00000E44
		public static bool CanAddProtectionToImpactResistance(Character character)
		{
			return SkillRequirements.SafeHasSkillKnowledge(character, 2502021);
		}

		// Token: 0x06000023 RID: 35 RVA: 0x00002C64 File Offset: 0x00000E64
		public static bool CanAddProtectionToPhysicalResistance(Character character)
		{
			return SkillRequirements.SafeHasSkillKnowledge(character, 2502021);
		}

		// Token: 0x06000024 RID: 36 RVA: 0x00002C84 File Offset: 0x00000E84
		public static bool CanAddProtectionToAnyDamageResistance(Character character)
		{
			return SkillRequirements.SafeHasSkillKnowledge(character, 2502021);
		}

		// Token: 0x06000025 RID: 37 RVA: 0x00002CA4 File Offset: 0x00000EA4
		public static bool CanAddBonusBastardWeaponSpeed(Character character)
		{
			return SkillRequirements.SafeHasSkillKnowledge(character, 2502015);
		}

		// Token: 0x06000026 RID: 38 RVA: 0x00002CC4 File Offset: 0x00000EC4
		public static bool CanAddBonusBastardWeaponDamage(Character character)
		{
			return SkillRequirements.SafeHasSkillKnowledge(character, 2502015);
		}

		// Token: 0x06000027 RID: 39 RVA: 0x00002CE4 File Offset: 0x00000EE4
		public static bool CanEnrageFromDamage(Character character)
		{
			return SkillRequirements.SafeHasSkillKnowledge(character, 2502020);
		}

		// Token: 0x06000028 RID: 40 RVA: 0x00002D04 File Offset: 0x00000F04
		public static bool CanAddBonusRageDamage(Character character)
		{
			return SkillRequirements.SafeHasSkillKnowledge(character, 2502022) && SkillRequirements.Enraged(character) && SkillRequirements.Vengeful(character);
		}

		// Token: 0x06000029 RID: 41 RVA: 0x00002D34 File Offset: 0x00000F34
		public static bool CanReduceWeaponAttackStaminaCost(Character character)
		{
			return SkillRequirements.SafeHasSkillKnowledge(character, 2502022) && SkillRequirements.Enraged(character) && SkillRequirements.Vengeful(character);
		}

		// Token: 0x0600002A RID: 42 RVA: 0x00002D64 File Offset: 0x00000F64
		public static bool ShouldPurgeAllExceptRageGivenEnraged(Character character)
		{
			return SkillRequirements.SafeHasSkillKnowledge(character, 2502022) && SkillRequirements.Vengeful(character);
		}

		// Token: 0x0600002B RID: 43 RVA: 0x00002D8C File Offset: 0x00000F8C
		public static bool CanReduceMoveSpeedArmorPenalty(Character character)
		{
			return SkillRequirements.SafeHasSkillKnowledge(character, 2502022) && SkillRequirements.SafeHasSkillKnowledge(character, 2502019);
		}

		// Token: 0x0600002C RID: 44 RVA: 0x00002DBC File Offset: 0x00000FBC
		public static bool CanReduceStaminaCostArmorPenalty(Character character)
		{
			return SkillRequirements.SafeHasSkillKnowledge(character, 2502022) && SkillRequirements.SafeHasSkillKnowledge(character, 2502019);
		}

		// Token: 0x0600002D RID: 45 RVA: 0x00002DEC File Offset: 0x00000FEC
		public static bool CanTerrify(Character character)
		{
			return SkillRequirements.SafeHasSkillKnowledge(character, 2502022) && SkillRequirements.SafeHasSkillKnowledge(character, 2502019);
		}

		// Token: 0x0600002E RID: 46 RVA: 0x00002E1C File Offset: 0x0000101C
		public static bool ApplyRuthlessSize(Character character)
		{
			return SkillRequirements.SafeHasSkillKnowledge(character, 2502022);
		}

		// Token: 0x0600002F RID: 47 RVA: 0x00002E3C File Offset: 0x0000103C
		public static bool CanConvertToRawDamage(Character character)
		{
			return false;
		}

		// Token: 0x06000030 RID: 48 RVA: 0x00002E50 File Offset: 0x00001050
		public static bool ShouldPurgeOnlyRageGivenDisciplined(Character character)
		{
			return false;
		}
	}
}
