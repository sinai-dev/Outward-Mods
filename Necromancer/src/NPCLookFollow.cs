using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NecromancerSkills
{
    // simple script, just add this as a component to an NPC character to have them automatically look at the closest player. 
    // this is not as smooth as the SNPC class, but its ok for now.

    public class NPCLookFollow : MonoBehaviour
    {
        float m_timeOfLastUpdate = -1f;

        Vector3 m_currentLookPosition = Vector3.zero;
        float m_closestDistance = -1f;

        internal void Update()
        {
            // update every 500ms
            if (Time.time - m_timeOfLastUpdate > 0.5f)
            {
                m_timeOfLastUpdate = Time.time;
                UpdateLookTarget();
            }

            // this is a low-cost function, do it every update for smooth look lerping
            if (m_closestDistance < 10)
            {
                // relative vector3 position
                Vector3 relativePos = m_currentLookPosition - transform.position;
                // look rotation
                Quaternion r1 = Quaternion.LookRotation(relativePos, Vector3.up);
                // lerp
                transform.rotation = Quaternion.RotateTowards(transform.rotation, r1, 1);
            }
        }

        private void UpdateLookTarget()
        {
            m_closestDistance = float.MaxValue;

            foreach (PlayerSystem ps in Global.Lobby.PlayersInLobby)
            {
                Character player = ps.ControlledCharacter;
                float distance = Vector3.Distance(transform.position, player.transform.position);
                if (distance < 5 && distance < m_closestDistance)
                {
                    m_currentLookPosition = player.transform.position;
                    m_closestDistance = distance;
                }
            }
        }
    }
}
