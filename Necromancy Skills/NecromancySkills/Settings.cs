using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NecromancySkills
{
    public class Settings
    {
        // Summon
        public float Summon_HealthCost = 0.1f; 
        public bool Summon_BurnsHealth = false;
        public int Summon_MaxSummons_NoArmyOfDeath = 1;
        public int Summon_MaxSummons_WithArmyOfDeath = 3;
        public int Summon_Summoned_Per_Cast_withArmyOfDeath = 1;
        public float Summon_MaxHealth = 75f;
        public float Summon_HealthLoss = -0.75f;
        public float StrongSummon_MaxHealth = 250;
        public float StrongSummon_HealthLoss = 2.5f;

        // Shoot Tendrils
        public float ShootTendrils_Heal_NoPlagueAura = 2.5f;
        public DamageType[] ShootTendrils_Damage_NoPlagueAura = new DamageType[]
        {
            new DamageType(DamageType.Types.Decay, 5),
            new DamageType(DamageType.Types.Physical, 5)
        };
        public float ShootTendrils_Knockback_NoPlagueAura = 15f;
        public float ShootTendrils_Heal_InsideAura = 6f;
        public DamageType[] ShootTendrils_Damage_InsideAura = new DamageType[]
        {
            new DamageType(DamageType.Types.Decay, 10),
            new DamageType(DamageType.Types.Ethereal, 5)
        };
        public float ShootTendrils_Knockback_InsideAura = 30f;

        // vital attunement
        public float VitalAttunement_HealthBonus = 20f;
        public float VitalAttunement_StaminaBonus = 20f;

        // Life Ritual
        public float LifeRitual_Heal_NoPlagueAura = 0.33f;
        public float LifeRitual_Heal_InsideAura = 0.66f;

        // Transcendence
        public float Transcendence_DamageBonus = 2.5f;

        // Death Ritual
        public DamageType[] DeathRitual_WeakExplosionDamage = new DamageType[]
        {
            new DamageType(DamageType.Types.Decay, 40),
            new DamageType(DamageType.Types.Ethereal, 20)
        };
        public float DeathRitual_WeakKnockback = 75f;
        public DamageType[] DeathRitual_StrongExplosionDamage = new DamageType[]
        {
            new DamageType(DamageType.Types.Decay, 50),
            new DamageType(DamageType.Types.Ethereal, 20),
            new DamageType(DamageType.Types.Frost, 20)
        };
        public float DeathRitual_StrongKnockback = 150f;

        // Plague Aura
        public float PlagueAura_SigilLifespan = 40f;
        public float PlagueAura_TendrilInterval = 3f;
        public DamageType[] PlagueAura_TendrilDamage = new DamageType[]
        {
            new DamageType(DamageType.Types.Decay, 15)
        };
        public float PlagueAura_TendrilKnockback = 25f;
    }
}
