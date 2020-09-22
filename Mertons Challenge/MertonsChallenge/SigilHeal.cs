using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
//using SinAPI;

namespace MertonsChallenge
{
    public class SigilHeal : MonoBehaviour
    {
        public ChallengeGlobal global = null;
        public bool SetupVisuals = false;

        internal void Update()
        {
            if (global.IsGameplayStarted)
            {
                if (!SetupVisuals)
                {
                    VisualSetup();
                }

                HealPlayers();
            }
            else
            {
                DestroyImmediate(gameObject);
            }
        }

        private void HealPlayers()
        {
            // restore 0.5 hp, 1 mana and 3 stamina each second for players in the sigil
            foreach (PlayerSystem ps in Global.Lobby.PlayersInLobby)
            {
                Character c = ps.ControlledCharacter;

                if (Vector3.Distance(c.transform.position, transform.position) < 2.5f)
                {
                    c.Stats.SetHealth(Mathf.Clamp(c.Health + (0.5f * Time.deltaTime), 0, c.ActiveMaxHealth));
                    c.Stats.SetMana(Mathf.Clamp(c.Mana + (1.0f * Time.deltaTime), 0, c.Stats.MaxMana));
                    c.Stats.AffectStamina(3.0f * Time.deltaTime);
                }
            }
        }

        private void VisualSetup()
        {
            transform.position = global.CurrentTemplate.InteractorPos;

            StartCoroutine(global.AddItemToInteractor(8000010, Vector3.zero, Vector3.zero, false, transform));

            SetupVisuals = true;
        }
    }
}
