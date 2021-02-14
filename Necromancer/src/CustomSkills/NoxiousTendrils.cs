using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SideLoader;
using UnityEngine;

namespace NecromancerSkills
{
    // this custom effect class duplicates a ShootProjectile effect onto any summoned AIs

    public class NoxiousTendrils : ShootProjectile
    {
        // skill setup called from SkillManager
        #region Tendrils Skill Setup
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

            shootTendrils.SyncType = SyncTypes.Everyone;

            // disable clone target before cloning it
            var origProjectile = shootTendrils.BaseProjectile.gameObject;
            origProjectile.SetActive(false);
            var projectileObj = Instantiate(origProjectile);
            DontDestroyOnLoad(projectileObj);
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
                heal.AffectQuantity = NecromancerBase.settings.ShootTendrils_Heal_NoPlagueAura;
            }

            // change damage and hit effects
            var hit = projectile.transform.Find("HitEffects").gameObject;
            hit.GetComponent<PunctualDamage>().Damages = NecromancerBase.settings.ShootTendrils_Damage_NoPlagueAura;
            hit.GetComponent<PunctualDamage>().Knockback = NecromancerBase.settings.ShootTendrils_Knockback_NoPlagueAura;
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
            var strongProjObj = Instantiate(origProjectile);
            DontDestroyOnLoad(strongProjObj);
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
                strongHeal.AffectQuantity = NecromancerBase.settings.ShootTendrils_Heal_InsideAura;
            }

            // change damage and hit effects.
            var strongHit = strongProj.transform.Find("HitEffects").gameObject;
            strongHit.GetComponent<PunctualDamage>().Damages = NecromancerBase.settings.ShootTendrils_Damage_InsideAura;
            strongHit.GetComponent<PunctualDamage>().Knockback = NecromancerBase.settings.ShootTendrils_Knockback_InsideAura;
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

        public override void Setup(Character.Factions[] _targetFactions, Transform _parent)
        {
            base.Setup(_targetFactions, _parent);

            this.m_proximityCondition = base.GetComponent<ProximityCondition>();

            foreach (SubEffect effect in m_subEffects)
            {
                effect.gameObject.SetActive(true);
            }
        }

        protected override void ActivateLocally(Character _affectedCharacter, object[] _infos)
        {
            // invoke the normal ShootProjectile first.
            base.ActivateLocally(_affectedCharacter, _infos);

            // duplicate this skill onto summoned AIs
            if (SummonManager.Instance.SummonedCharacters.ContainsKey(_affectedCharacter.UID))
            {
                foreach (string summonUID in SummonManager.Instance.SummonedCharacters[_affectedCharacter.UID])
                {
                    var _summonChar = CharacterManager.Instance.GetCharacter(summonUID);

                    if (!_summonChar)
                    {
                        //OLogger.Warning("Tendrils: Could not find summon UID of :" + summonUID);
                    }
                    else
                    {
                        // copy of ShootProjectile.ActivateLocally(), but changing the projectile start locations and directions to the summon AI
                        Vector3 vector = _summonChar.transform.position;
                        Vector3 direction = _summonChar.transform.TransformDirection(Vector3.forward);
                        float projectileForce = (float)_infos[2];
                        if (_infos.Length > 3)
                        {
                            for (int i = 3; i < _infos.Length; i++)
                            {
                                Vector3 zero = Vector3.zero;
                                string empty = string.Empty;
                                if (ProjectileShot.ParseShotInstance((string)_infos[i], ref zero, ref empty))
                                {
                                    this.PerformShootProjectile(_affectedCharacter, vector, zero, projectileForce, empty);
                                    Debug.DrawRay(vector, zero * 2f, Color.cyan, 5f);
                                }
                            }
                        }
                        else
                        {
                            this.PerformShootProjectile(_affectedCharacter, vector, direction, projectileForce, string.Empty);
                        }
                    }
                }
            }
        }
    }
}
