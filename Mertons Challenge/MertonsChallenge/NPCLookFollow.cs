using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
//using SinAPI;

namespace MertonsChallenge
{
    public class NPCLookFollow : MonoBehaviour
    {
        internal void Update()
        {
            UpdateLookTarget();
        }

        private void UpdateLookTarget()
        {
            float closest = -1;
            Character c = null;

            foreach (PlayerSystem ps in Global.Lobby.PlayersInLobby)
            {
                Character c2 = ps.ControlledCharacter;
                float distance = Vector3.Distance(transform.position, c2.transform.position);
                if (closest == -1 || distance < closest)
                {
                    c = c2;
                    closest = distance;
                }
            }

            if (c && closest < 10)
            {
                // relative vector3 position
                Vector3 relativePos = c.transform.position - transform.position;
                // look rotation
                Quaternion r1 = Quaternion.LookRotation(relativePos, Vector3.up);
                // lerp
                transform.rotation = Quaternion.RotateTowards(transform.rotation, r1, 1);                
            }
        }
    }
}
