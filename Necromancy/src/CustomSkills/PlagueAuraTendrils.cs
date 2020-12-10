using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SideLoader;
using UnityEngine;

namespace NecromancerSkills
{ 
    public class PlagueAuraTendrils : ShootBlast
    {
        // this is the passive Lich Tendrils effect from the Plague Aura t3 skill, not the t1 tendrils skill.
        //private static readonly float PassiveEffectInterval = 3f;

        private Coroutine tendrilsCoroutine = null;

        private float m_timeOfLastActivation = -1f;
        private float m_timeOfLastBlast = -1f;

        // setup called from PlagueAura Setup
        #region setup Plague Tendrils effect
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
            var newBlast = Instantiate(origBlast);
            DontDestroyOnLoad(newBlast);
            //newBlast.SetActive(true);
            newBlast.name = "PlagueTendrils";
            var blast = newBlast.GetComponent<BlastGround>();
            plagueTendrils.BaseBlast = blast;

            newBlast.transform.Find("mdl_Fx@LichDarkTendril_c").localScale = new Vector3(0.5f, 0.5f, 0.5f);

            // change damage and hit effects
            var hit = blast.transform.Find("Effects").gameObject;
            hit.GetComponent<PunctualDamage>().Damages = NecromancerBase.settings.PlagueAura_TendrilDamage;
            hit.GetComponent<PunctualDamage>().Knockback = NecromancerBase.settings.PlagueAura_TendrilKnockback;
            var comp = hit.AddComponent<AddStatusEffectBuildUp>();
            comp.Status = ResourcesPrefabManager.Instance.GetStatusEffectPrefab("Cripple"); 
            comp.BuildUpValue = 100;

        }
        #endregion

        public override void Setup(Character.Factions[] _targetFactions, Transform _parent)
        {
            base.Setup(_targetFactions, _parent);

            foreach (SubEffect effect in m_subEffects)
            {
                effect.gameObject.SetActive(true);
            }
        }

        protected override void ActivateLocally(Character _targetCharacter, object[] _infos)
        {
            if (tendrilsCoroutine != null)
            {
                StopCoroutine(tendrilsCoroutine);
            }

            m_timeOfLastActivation = Time.time;
            tendrilsCoroutine = StartCoroutine(TendrilsCoroutine(_infos));
        }

        // passive tendril shootblast

        private IEnumerator TendrilsCoroutine(object[] _infos)
        {
            while (Time.time - m_timeOfLastActivation < NecromancerBase.settings.PlagueAura_SigilLifespan)
            {
                if (Time.time - m_timeOfLastBlast > NecromancerBase.settings.PlagueAura_TendrilInterval)
                {
                    List<Character> nearCharacters = new List<Character>();

                    CharacterManager.Instance.FindCharactersInRange(this.transform.position, 2.5f, ref nearCharacters);

                    if (nearCharacters.Count() > 0)
                    {
                        float nearest = float.MaxValue;
                        Character nearestChar = null;
                        var targetfactions = this.m_targetingSystem.TargetableFactions.ToList();
                        
                        foreach (Character c in nearCharacters.Where(x =>!x.IsDead && x.isActiveAndEnabled && targetfactions.Contains(x.Faction)))
                        {
                            if (Vector3.Distance(c.transform.position, this.transform.position) is float f && f < nearest)
                            {
                                nearest = f;
                                nearestChar = c;
                            }
                        }

                        if (nearestChar)
                        {
                            m_timeOfLastBlast = Time.time;
                            base.ActivateLocally(nearestChar, new object[] { nearestChar.transform.position, Vector3.zero });
                        }
                        else 
                        {
                            // OLogger.Warning("None of the characters in range are targetable");
                        }
                    }
                }

                yield return new WaitForSeconds(0.25f);
            }
        }

        internal void OnDisable()
        {
            if (tendrilsCoroutine != null)
            {
                StopCoroutine(tendrilsCoroutine);
            }
        }
    }


}
