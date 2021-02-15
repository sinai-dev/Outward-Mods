using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SideLoader;
using UnityEngine;

namespace Necromancer.Skills.Effects
{
    // this custom effect class duplicates a ShootProjectile effect onto any summoned AIs

    public class NoxiousTendrils : ShootProjectile
    {
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
            if (SummonManager.SummonedCharacters.ContainsKey(_affectedCharacter.UID))
            {
                foreach (string summonUID in SummonManager.SummonedCharacters[_affectedCharacter.UID])
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
