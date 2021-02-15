using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Necromancer
{
    // Forces summons to stay within a certain distance from their summoner.
    // Teleports them when distance is exceeded.

    public class SummonTeleport : MonoBehaviour
    {
        public Character m_character;
        public Transform TargetCharacter;
        public float TeleportDistance = 25f;

        private float m_timeOfLastUpdate = -1f;

        internal void Update()
        {
            if (Time.time - m_timeOfLastUpdate > 1.0f)
            {
                m_timeOfLastUpdate = Time.time;

                if (!m_character || !TargetCharacter)
                {
                    this.InitReferences();
                    return; 
                }

                if (Vector3.Distance(this.transform.position, TargetCharacter.transform.position) > TeleportDistance)
                    m_character.Teleport(TargetCharacter.transform.position + (Vector3.forward * 0.3f), Quaternion.identity);

            }
        }

        private void InitReferences()
        {
            if (this.GetComponentInChildren<AISWander>() is AISWander wander && wander.FollowTransform != null)
                this.TargetCharacter = wander.FollowTransform;

            if (this.GetComponent<Character>() is Character character)
                this.m_character = character;
        }
    }
}
