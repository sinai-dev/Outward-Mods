using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NecromancerSkills
{
    public class SummonTeleport : MonoBehaviour
    {
        public Character m_character;
        public Transform TargetCharacter;
        public float TeleportDistance = 25f;

        private bool m_triedFailsafeInit = false;

        private float m_timeOfLastUpdate = -1f;

        private void AwakeInit()
        {
            if (this.GetComponentInChildren<AISWander>() is AISWander wander && wander.FollowTransform != null)
            {
                this.TargetCharacter = wander.FollowTransform;
            }
            if (this.GetComponent<Character>() is Character character)
            {
                this.m_character = character;
            }
        }

        internal void Update()
        {
            if (Time.time - m_timeOfLastUpdate > 1.0f)
            {
                if (m_character == null || TargetCharacter == null) 
                {
                    if (!m_triedFailsafeInit)
                    {
                        m_triedFailsafeInit = true;
                        this.AwakeInit();
                    }
                    
                    return; 
                }

                m_timeOfLastUpdate = Time.time;

                if (Vector3.Distance(this.transform.position, TargetCharacter.transform.position) > TeleportDistance)
                {
                    m_character.Teleport(TargetCharacter.transform.position + (Vector3.forward * 0.3f), Quaternion.identity);
                }
            }
        }

    }
}
