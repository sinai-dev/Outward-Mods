using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using HarmonyLib;
using SideLoader;

namespace PvP
{
    public class PlayerManager : MonoBehaviour
    {
        public static PlayerManager Instance;

        //public List<Character> PlayerCharacters = new List<Character>();

        public List<Character.Factions> AllFactions;

        internal void Awake()
        {
            Instance = this;

            AllFactions = new List<Character.Factions>();
            for (int i = 0; i < (int)Character.Factions.COUNT; i++)
            {
                AllFactions.Add((Character.Factions)i);
            }
        }

        [HarmonyPatch(typeof(DeployableTrap), "ProcessEffect")]
        public class DeployableTrap_ProcessEffect
        {
            [HarmonyPrefix]
            public static bool Prefix(DeployableTrap __instance, Effect _effect)
            {
                var self = __instance;

                if (_effect is Shooter shooter && PvP.Instance.CurrentGame == PvP.GameModes.BattleRoyale)
                {
                    shooter.Setup(Instance.AllFactions.ToArray(), (!(self.FX_HolderTrans != null)) ? self.transform : self.FX_HolderTrans);
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        [HarmonyPatch(typeof(TrapTrigger), "OnTriggerEnter")]
        public class TrapTrigger_OnTriggerEnter
        {
            [HarmonyPrefix]
            public static bool Prefix(TrapTrigger __instance, Collider _other)
            {
                var self = __instance;

                if (_other == null)
                {
                    return true;
                }

                if (PvP.Instance.CurrentGame != PvP.GameModes.NONE)
                {
                    if (self.GetComponentInParent<DeployableTrap>() is DeployableTrap trap)
                    {
                        //Debug.Log("setting trap factions");
                        At.SetField(trap, "m_targetFactions", Instance.AllFactions.ToArray());
                    }

                    var m_charactersInTrigger = At.GetField(self, "m_charactersInTrigger") as List<Character>;

                    Character component = _other.GetComponent<Character>();

                    if (component != null && !m_charactersInTrigger.Contains(component))
                    {
                        m_charactersInTrigger.Add(component);
                        if (!(bool)At.GetField(self, "m_alreadyTriggered"))
                        {
                            self.Trigger.ActivateBasicAction(component, self.OnEnterState - TrapTrigger.ToggleState.Off);
                        }
                        At.SetField(self, "m_alreadyTriggered", true);
                    }

                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        public void ChangeFactions(Character c, Character.Factions faction)
        {
            RPCManager.Instance.photonView.RPC("SendChangeFactionsRPC", PhotonTargets.All, new object[] { (int)faction, c.UID.ToString(), true });
        }

        public List<Character.Factions> GetRemainingTeams()
        {
            List<Character.Factions> remainingTeams = new List<Character.Factions>();
            foreach (KeyValuePair<Character.Factions, List<PlayerSystem>> entry in PvP.Instance.CurrentPlayers)
            {
                if (entry.Key == Character.Factions.NONE) continue;

                bool anyAlive = false;
                foreach (PlayerSystem ps in entry.Value)
                {
                    if (ps.ControlledCharacter != null && !ps.ControlledCharacter.IsDead)
                    {
                        anyAlive = true;
                        break;
                    }
                }
                if (anyAlive)
                    remainingTeams.Add(entry.Key);
            }

            return remainingTeams;
        }

        public List<Character> GetRemainingPlayers()
        {
            List<Character> remainingPlayers = new List<Character>();

            foreach (KeyValuePair<Character.Factions, List<PlayerSystem>> entry in PvP.Instance.CurrentPlayers)
            {
                foreach (PlayerSystem ps in PvP.Instance.CurrentPlayers[entry.Key])
                {
                    if (ps.ControlledCharacter != null && !ps.ControlledCharacter.IsDead)
                        remainingPlayers.Add(ps.ControlledCharacter);
                }
            }

            return remainingPlayers;
        }
    }
}
