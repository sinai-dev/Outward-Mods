using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using SideLoader;
using HarmonyLib;
using Necromancer.Skills.Effects;
using Necromancer.Skills.EffectConditions;

namespace Necromancer
{
    public static class SkillManager
    {
        public static void Init()
        {
            Debug.Log("Setting up Necromancer skills");

            try
            {
                SetupSkills();
            }
            catch (Exception e)
            {
                Debug.LogWarning("Exception setting up Necromancer skills!");
                Debug.LogWarning(e.Message);
                Debug.LogWarning(e.StackTrace);
            }
        }

        private static void SetupSkills()
        {
            // I use the SideLoader to do the preliminary setup (clone existing skill, change ID / name / etc..)

            SetupTranscendence();

            SetupSummon();

            SetupTendrils();

            SetupLifeRitual();

            SetupDeathRitual();

            SetupPlagueAura();
        }

        #region Transcendance (breakthrough)

        private static void SetupTranscendence()
        {
            var transcendence = ResourcesPrefabManager.Instance.GetItemPrefab(8890104) as PassiveSkill;
            var passiveTransform = transcendence.transform.Find("Passive");
            GameObject.DestroyImmediate(passiveTransform.GetComponent<AffectStat>());

            // add elemental bonuses using custom ManaPointAffectStat class

            passiveTransform.gameObject.AddComponent<ManaPointAffectStat>().Stat = CharacterStats.StatType.EtherealDamage;
            passiveTransform.gameObject.AddComponent<ManaPointAffectStat>().Stat = CharacterStats.StatType.DecayDamage;
            passiveTransform.gameObject.AddComponent<ManaPointAffectStat>().Stat = CharacterStats.StatType.LightDamage;
            passiveTransform.gameObject.AddComponent<ManaPointAffectStat>().Stat = CharacterStats.StatType.FrostDamage;
            passiveTransform.gameObject.AddComponent<ManaPointAffectStat>().Stat = CharacterStats.StatType.FireDamage;
        }

        #endregion

        #region Summon
        public static void SetupSummon()
        {
            var summon = ResourcesPrefabManager.Instance.GetItemPrefab(8890103) as Skill;

            // destroy the existing skills, but keep the rest (VFX / Sound).
            GameObject.DestroyImmediate(summon.transform.Find("Lightning").gameObject);
            GameObject.DestroyImmediate(summon.transform.Find("SummonSoul").gameObject);

            var effects = new GameObject("Effects");
            effects.transform.parent = summon.transform;
            effects.AddComponent<SummonSkeleton>();

            // setup custom blade visuals
            try
            {
                var blade = ResourcesPrefabManager.Instance.GetItemPrefab(2598500) as Weapon;
                var bladeVisuals = CustomItemVisuals.GetOrAddVisualLink(blade).ItemVisuals;
                if (bladeVisuals.transform.Find("Weapon3DVisual").GetComponent<MeshRenderer>() is MeshRenderer mesh)
                {
                    mesh.material.color = new Color(-0.5f, 1.5f, -0.5f);
                }
            }
            catch { }

            //// make sure the config is applied from the save
            //SummonManager.Skeleton.Health = NecromancerMod.settings.Summon_MaxHealth;
            //SummonManager.Skeleton.HealthRegen = NecromancerMod.settings.Summon_HealthLoss;
            //SummonManager.Ghost.Health = NecromancerMod.settings.StrongSummon_MaxHealth;
            //SummonManager.Ghost.HealthRegen = NecromancerMod.settings.StrongSummon_HealthLoss;
        }
        #endregion

        #region Noxious Tendrils
        public static void SetupTendrils()
        {
            // ============== setup base skill ==============

            var tendrils = ResourcesPrefabManager.Instance.GetItemPrefab(8890100) as AttackSkill;

            // setup skill
            tendrils.CastModifier = Character.SpellCastModifier.Mobile; // can move while casting but movement speed is 0.3x
            tendrils.MobileCastMovementMult = 0.3f;

            // clear existing effects
            SideLoader.Helpers.UnityHelpers.DestroyChildren(tendrils.transform);

            // ============= normal effects =============== //

            // create new effects
            var effects = new GameObject("Effects");
            effects.transform.parent = tendrils.transform;

            // add our custom PlagueAura proximity condition component (INVERT = TRUE, we DONT want the aura on these effects).
            var auraCondition1 = effects.AddComponent<PlagueAuraProximityCondition>();
            auraCondition1.ProximityDist = 2.5f;
            auraCondition1.RequiredActivatedItemID = 8999050;
            auraCondition1.Invert = true;

            // create the Tendrils effect, a custom class derived from ShootProjectile
            NoxiousTendrils shootTendrils = effects.AddComponent<NoxiousTendrils>();
            var orig = ResourcesPrefabManager.Instance.GetItemPrefab(8300292).transform.Find("Effects").GetComponent<ShootProjectile>();
            At.CopyFields(shootTendrils, orig, null, true);

            shootTendrils.SyncType = Effect.SyncTypes.Everyone;

            // disable clone target before cloning it
            var origProjectile = shootTendrils.BaseProjectile.gameObject;
            origProjectile.SetActive(false);
            var projectileObj = GameObject.Instantiate(origProjectile);
            GameObject.DontDestroyOnLoad(projectileObj);
            //projectileObj.SetActive(true);

            projectileObj.name = "NoxiousTendrils";

            // get the actual Projectile component from our new Projectile Object, and set our "BaseProjectile" to this component
            var projectile = projectileObj.GetComponent<RaycastProjectile>();
            shootTendrils.BaseProjectile = projectile;
            shootTendrils.IntanstiatedAmount = 8; // 2 per character, potential 3 summoned skeletons, so 8 total subeffects needed.

            projectile.Lifespan = 0.75f;
            projectile.DisableOnHit = false;
            projectile.EndMode = Projectile.EndLifeMode.LifetimeOnly;
            projectile.HitEnemiesOnly = true;

            // sound play
            if (projectileObj.GetComponentInChildren<SoundPlayer>() is SoundPlayer lightPlayer)
            {
                lightPlayer.Sounds = new List<GlobalAudioManager.Sounds> { GlobalAudioManager.Sounds.SFX_FireThrowLight };
            }

            // heal on hit
            if (projectile.GetComponentInChildren<AffectHealthParentOwner>() is AffectHealthParentOwner heal)
            {
                heal.AffectQuantity = NecromancerMod.settings.ShootTendrils_Heal_NoPlagueAura;
            }

            // change damage and hit effects
            var hit = projectile.transform.Find("HitEffects").gameObject;
            hit.GetComponent<PunctualDamage>().Damages = NecromancerMod.settings.ShootTendrils_Damage_NoPlagueAura;
            hit.GetComponent<PunctualDamage>().Knockback = NecromancerMod.settings.ShootTendrils_Knockback_NoPlagueAura;
            var comp = hit.AddComponent<AddStatusEffectBuildUp>();
            comp.Status = ResourcesPrefabManager.Instance.GetStatusEffectPrefab("Curse");
            comp.BuildUpValue = 25;

            // adjust visuals
            foreach (ParticleSystem ps in projectileObj.GetComponentsInChildren<ParticleSystem>())
            {
                var m = ps.main;
                m.startColor = Color.green;
                m.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.09f);
            }

            // ================= plague aura interaction effects ===============

            var plagueEffectsObj = new GameObject("Effects");
            plagueEffectsObj.transform.parent = tendrils.transform;

            // add our custom PlagueAura proximity condition component
            var auraCondition2 = plagueEffectsObj.AddComponent<PlagueAuraProximityCondition>();
            auraCondition2.ProximityDist = 2.5f;
            auraCondition2.RequiredActivatedItemID = 8999050;
            auraCondition2.Invert = false;

            // add our custom ShootTendrils component
            NoxiousTendrils strongTendrils = plagueEffectsObj.AddComponent<NoxiousTendrils>();
            At.CopyFields(strongTendrils, orig, null, true);

            // clone the projectile
            origProjectile.SetActive(false);
            var strongProjObj = GameObject.Instantiate(origProjectile);
            GameObject.DontDestroyOnLoad(strongProjObj);
            origProjectile.SetActive(true);

            strongProjObj.name = "StrongNoxiousTendrils";
            var strongProj = strongProjObj.GetComponent<RaycastProjectile>();
            strongTendrils.BaseProjectile = strongProj;
            strongTendrils.IntanstiatedAmount = 8;

            strongProj.Lifespan = 0.75f;
            strongProj.DisableOnHit = false;
            strongProj.EndMode = Projectile.EndLifeMode.LifetimeOnly;
            strongProj.HitEnemiesOnly = true;

            // sound play
            if (strongProjObj.GetComponentsInChildren<SoundPlayer>() is SoundPlayer[] strongPlayers && strongPlayers.Count() > 0)
            {
                foreach (SoundPlayer player in strongPlayers)
                {
                    player.Sounds = new List<GlobalAudioManager.Sounds> { GlobalAudioManager.Sounds.SFX_SKILL_ElemantalProjectileWind_Shot };
                }
            }
            // heal on hit
            if (strongProj.GetComponentInChildren<AffectHealthParentOwner>() is AffectHealthParentOwner strongHeal)
            {
                //DestroyImmediate(heal);
                strongHeal.AffectQuantity = NecromancerMod.settings.ShootTendrils_Heal_InsideAura;
            }

            // change damage and hit effects.
            var strongHit = strongProj.transform.Find("HitEffects").gameObject;
            strongHit.GetComponent<PunctualDamage>().Damages = NecromancerMod.settings.ShootTendrils_Damage_InsideAura;
            strongHit.GetComponent<PunctualDamage>().Knockback = NecromancerMod.settings.ShootTendrils_Knockback_InsideAura;
            comp = strongHit.AddComponent<AddStatusEffectBuildUp>();
            comp.Status = ResourcesPrefabManager.Instance.GetStatusEffectPrefab("Curse");
            comp.BuildUpValue = 60;

            // adjust visuals
            foreach (ParticleSystem ps in strongProjObj.GetComponentsInChildren<ParticleSystem>())
            {
                var m = ps.main;
                m.startColor = Color.green;
                m.startSize = new ParticleSystem.MinMaxCurve(0.12f, 0.20f);
            }
        }
        #endregion

        #region Life Ritual
        public static void SetupLifeRitual()
        {
            var frenzySkill = ResourcesPrefabManager.Instance.GetItemPrefab(8890105) as Skill;

            // setup skill
            frenzySkill.CastSheathRequired = -1;

            // destroy the existing skills, but keep the rest (VFX / Sound).
            GameObject.DestroyImmediate(frenzySkill.transform.Find("Lightning").gameObject);
            GameObject.DestroyImmediate(frenzySkill.transform.Find("SummonSoul").gameObject);

            // set custom spellcast anim
            At.SetField(frenzySkill as Item, "m_activateEffectAnimType", Character.SpellCastType.Focus);

            var effects = new GameObject("Effects");
            effects.transform.parent = frenzySkill.transform;

            effects.AddComponent<LifeRitual>();
        }
        #endregion

        #region Death Ritual
        public static void SetupDeathRitual()
        {
            var detonateSkill = ResourcesPrefabManager.Instance.GetItemPrefab(8890106) as AttackSkill;

            // setup skill
            detonateSkill.CastSheathRequired = -1;
            detonateSkill.RequiredOffHandTypes.Clear();
            detonateSkill.RequiredTags = new TagSourceSelector[0];
            detonateSkill.StartVFX = null;

            // destroy these existing effects
            GameObject.Destroy(detonateSkill.transform.Find("RunicRay").gameObject);
            GameObject.DestroyImmediate(detonateSkill.transform.Find("Effects").gameObject);

            // hang onto the ShootBlast, we want to use this
            var shootBlast = detonateSkill.transform.Find("RunicBlast").GetChild(0).GetComponent<ShootBlast>();

            // ======== setup weak blast ============

            // create new Effects object
            var effects = new GameObject("Effects");
            effects.transform.parent = detonateSkill.transform;
            var detonateBlast = effects.AddComponent<DeathRitual>();
            At.CopyFields(detonateBlast, shootBlast, null, true);
            // destroy the old RunicBlast now that we stole the blast component
            GameObject.Destroy(detonateSkill.transform.Find("RunicBlast").gameObject);

            // add condition. Required item on summon is Mertons Bones.
            var condition = effects.AddComponent<DeathRitualCondition>();
            condition.RequiredSummonEquipment = 3200030;
            condition.Invert = false;

            // disable clone target before cloning it
            var origBlast = detonateBlast.BaseBlast.gameObject;
            origBlast.SetActive(false);
            var blastObj = GameObject.Instantiate(origBlast);
            GameObject.DontDestroyOnLoad(blastObj);
            blastObj.name = "DetonateBlast";

            var blast = blastObj.GetComponentInChildren<CircularBlast>();
            detonateBlast.BaseBlast = blast;

            if (blast.GetComponentInChildren<PunctualDamage>() is PunctualDamage pDamage)
            {
                pDamage.Damages = NecromancerMod.settings.DeathRitual_WeakExplosionDamage;
                pDamage.Knockback = NecromancerMod.settings.DeathRitual_WeakKnockback;
            }

            var explosionFX = blast.transform.Find("ExplosionFX").gameObject;
            foreach (var particles in explosionFX.GetComponentsInChildren<ParticleSystem>())
            {
                var m = particles.main;
                m.startColor = Color.green;
            }

            // =========== STRONG DETONATION (blue ghost) ================= //

            var effects2 = new GameObject("Effects");
            effects2.transform.parent = detonateSkill.transform;
            var detonateBlast2 = effects2.AddComponent<DeathRitual>();
            At.CopyFields(detonateBlast2, detonateBlast, null, true);

            // add condition. Required item on summon is Blue Ghost robes.
            var condition2 = effects2.AddComponent<DeathRitualCondition>();
            condition2.RequiredSummonEquipment = 3200040;
            condition2.Invert = false;

            origBlast.SetActive(false);
            var blastObj2 = GameObject.Instantiate(origBlast);
            origBlast.SetActive(false);
            GameObject.DontDestroyOnLoad(blastObj2);
            blastObj2.name = "StrongDetonateBlast";

            var blast2 = blastObj2.GetComponent<CircularBlast>();
            detonateBlast2.BaseBlast = blast2;

            if (blast2.GetComponentInChildren<PunctualDamage>() is PunctualDamage pDamage2)
            {
                pDamage2.Damages = NecromancerMod.settings.DeathRitual_StrongExplosionDamage;
                pDamage2.Knockback = NecromancerMod.settings.DeathRitual_StrongKnockback;

                var comp = pDamage2.gameObject.AddComponent<AddStatusEffect>();
                comp.Status = ResourcesPrefabManager.Instance.GetStatusEffectPrefab("Slow Down");
            }

            var explosionFX2 = blast2.transform.Find("ExplosionFX").gameObject;
            foreach (ParticleSystem particles in explosionFX2.GetComponentsInChildren<ParticleSystem>())
            {
                var m = particles.main;
                m.startColor = new Color() { r = 0.2f, g = 0.4f, b = 1, a = 1 }; // cyan-ish
            }
        }
        #endregion

        #region Plague Aura

        // this also sets up the PlagueTendrils effect.

        public static void SetupPlagueAura()
        {
            // 8890107
            var plagueSkill = ResourcesPrefabManager.Instance.GetItemPrefab(8890107) as Skill;

            // destroy wind altar condition
            GameObject.Destroy(plagueSkill.transform.Find("AdditionalActivationConditions").gameObject);

            // setup skill
            plagueSkill.CastSheathRequired = -1;
            plagueSkill.RequiredItems = new Skill.ItemRequired[0];

            // get the Summon component, change to custom activation fx
            var effectTrans = plagueSkill.transform.Find("Effects").gameObject;
            var origSummon = effectTrans.GetComponent<Summon>();
            var plagueAuraComp = effectTrans.AddComponent<PlagueAura>();
            At.CopyFields(plagueAuraComp, origSummon, null, true);
            GameObject.Destroy(origSummon);

            // ======== set summoned prefab to our custom activated item (loaded with sideloader) ========
            var plagueStone = ResourcesPrefabManager.Instance.GetItemPrefab(8999050);
            plagueAuraComp.SummonedPrefab = plagueStone.transform;

            var ephemeral = plagueStone.GetComponent<Ephemeral>();
            ephemeral.LifeSpan = NecromancerMod.settings.PlagueAura_SigilLifespan;

            var newVisuals = CustomItemVisuals.GetOrAddVisualLink(plagueStone).ItemVisuals;

            var magiccircle = newVisuals.transform.Find("mdl_fx_magicCircle");
            // destroy rotating bolt fx
            GameObject.Destroy(magiccircle.transform.Find("FX_Bolt").gameObject);

            // setup the clouds
            if (newVisuals.transform.Find("mdl_itm_firestone") is Transform t)
            {
                t.parent = magiccircle;
                GameObject.Destroy(t.Find("FX_Bolt").gameObject);

                var ps = t.Find("smoke_desu").GetComponent<ParticleSystem>();
                var m = ps.main;
                m.startColor = Color.green;

                t.Find("smoke_desu").position += Vector3.down * 3.2f;
            }

            // setup the Plague Tendrils effect (from inside that class)
            SetupPlagueTendrils(effectTrans);
        }

        public static void SetupPlagueTendrils(GameObject effects)
        {
            var plagueTendrils = effects.gameObject.AddComponent<PlagueAuraTendrils>();
            var orig = ResourcesPrefabManager.Instance.GetItemPrefab(8300180).GetComponentInChildren<ShootBlast>();
            At.CopyFields(plagueTendrils, orig, null, true);

            plagueTendrils.InstanstiatedAmount = 2;
            plagueTendrils.NoTargetForwardMultiplier = 0f;

            // disable clone target before cloning it
            var origBlast = plagueTendrils.BaseBlast.gameObject;
            origBlast.SetActive(false);
            var newBlast = GameObject.Instantiate(origBlast);
            GameObject.DontDestroyOnLoad(newBlast);
            //newBlast.SetActive(true);
            newBlast.name = "PlagueTendrils";
            var blast = newBlast.GetComponent<BlastGround>();
            plagueTendrils.BaseBlast = blast;

            newBlast.transform.Find("mdl_Fx@LichDarkTendril_c").localScale = new Vector3(0.5f, 0.5f, 0.5f);

            // change damage and hit effects
            var hit = blast.transform.Find("Effects").gameObject;
            hit.GetComponent<PunctualDamage>().Damages = NecromancerMod.settings.PlagueAura_TendrilDamage;
            hit.GetComponent<PunctualDamage>().Knockback = NecromancerMod.settings.PlagueAura_TendrilKnockback;
            var comp = hit.AddComponent<AddStatusEffectBuildUp>();
            comp.Status = ResourcesPrefabManager.Instance.GetStatusEffectPrefab("Cripple");
            comp.BuildUpValue = 100;

        }

        #endregion
    }
}
