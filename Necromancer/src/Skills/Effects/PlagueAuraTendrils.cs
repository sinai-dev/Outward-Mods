using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SideLoader;
using UnityEngine;

namespace Necromancer.Skills.Effects
{ 
    public class PlagueAuraTendrils : ShootBlast
    {
        // this is the passive Lich Tendrils effect from the Plague Aura t3 skill, not the t1 tendrils skill.
        //private static readonly float PassiveEffectInterval = 3f;

        private Coroutine tendrilsCoroutine = null;

        private float m_timeOfLastActivation = -1f;
        private float m_timeOfLastBlast = -1f;

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
            while (Time.time - m_timeOfLastActivation < NecromancerMod.settings.PlagueAura_SigilLifespan)
            {
                if (Time.time - m_timeOfLastBlast > NecromancerMod.settings.PlagueAura_TendrilInterval)
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
