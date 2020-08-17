using System;

namespace Juggernaut
{
	// Token: 0x02000004 RID: 4
	public class JuggernautFormulas
	{
		// Token: 0x06000009 RID: 9 RVA: 0x00002628 File Offset: 0x00000828
		public static float GetUnyieldingMovementSpeedForgivenes(Character character)
		{
			return 0.5f;
		}

		// Token: 0x0600000A RID: 10 RVA: 0x00002640 File Offset: 0x00000840
		public static float GetUnyieldingStaminaCostForgivenes(Character character)
		{
			return 0.5f;
		}

		// Token: 0x0600000B RID: 11 RVA: 0x00002658 File Offset: 0x00000858
		public static float GetRuthlessAttackStaminaCostReduction(Character character)
		{
			return 0.5f;
		}

		// Token: 0x0600000C RID: 12 RVA: 0x00002670 File Offset: 0x00000870
		public static float GetRuthlessDamageBonus(Character character)
		{
			return 0.3f;
		}

		// Token: 0x0600000D RID: 13 RVA: 0x00002688 File Offset: 0x00000888
		public static float GetRuthlessRawDamageRatio(Character character)
		{
			return 0.3f;
		}

		// Token: 0x0600000E RID: 14 RVA: 0x000026A0 File Offset: 0x000008A0
		public static float GetRuthlessBodySize(Character character)
		{
			return SkillRequirements.ApplyRuthlessSize(character) ? 1.1f : 1f;
		}

		// Token: 0x04000006 RID: 6
		public const float UNYIELDING_MOVEMENT_SPEED_FORGIVENESS = 0.5f;

		// Token: 0x04000007 RID: 7
		public const float UNYIELDING_STAMINA_COST_FORGIVENESS = 0.5f;

		// Token: 0x04000008 RID: 8
		public const float RUTHLESS_ATTACK_STAMINA_COST_REDUCTION = 0.5f;

		// Token: 0x04000009 RID: 9
		public const float RUTHLESS_DAMAGE_BONUS = 0.3f;

		// Token: 0x0400000A RID: 10
		public const float RUTHLESS_RAW_DAMAGE_RATIO = 0.3f;
	}
}
