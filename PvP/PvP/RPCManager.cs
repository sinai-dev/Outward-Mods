using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Reflection;

namespace PvP
{
    public class RPCManager : Photon.MonoBehaviour
    {
        public static RPCManager Instance;

        internal void Awake()
        {
            Instance = this;
        }

        // ==== Set Enemies Disabled state ====

        public static void SendSetEnemiesActive(bool active)
        {
            if (PhotonNetwork.isNonMasterClientInRoom) return;

            Instance.photonView.RPC(nameof(RPCSetEnemiesActive), PhotonTargets.All, new object[] { active });
        }

        [PunRPC]
        private void RPCSetEnemiesActive(bool active)
        {
            PvP.SetEnemiesActive(active);
        }

        // ==== Set Player gameobject active state ====

        public static void SendSetPlayerActive(string uid, bool active)
        {
            Instance.photonView.RPC(nameof(RPCSetPlayerActive), PhotonTargets.All, new object[] { uid, active });
        }

        [PunRPC]
        private void RPCSetPlayerActive(string uid, bool active)
        {
            CharacterManager.Instance.GetCharacter(uid)?.gameObject.SetActive(active);
        }

        // ==== Send Friendly Fire state ====

        public static void SendFriendyFire(bool enabled)
        {
            SendMessageToAll($"{(enabled ? "Enabling" : "Disabling")} Friendly Fire!");
            Instance.photonView.RPC(nameof(SendFriendlyFireRPC), PhotonTargets.All, new object[] { enabled });
        }

        [PunRPC]
        private void SendFriendlyFireRPC(bool enabled)
        {
            PvP.Instance.FriendlyFireEnabled = enabled;
        }

        // ==== Send Friendly Targeting state ====

        public static void SendFriendyTargeting(bool enabled)
        {
            SendMessageToAll($"{(enabled ? "Enabling" : "Disabling")} Friendly Targeting!");
            Instance.photonView.RPC(nameof(SendFriendlyTargetingRPC), PhotonTargets.All, new object[] { enabled });
        }

        [PunRPC]
        private void SendFriendlyTargetingRPC(bool enabled)
        {
            PvP.Instance.FriendlyTargetingEnabled = enabled;
        }

        // ==== Direct RPCs for start/stop gameplay (only call with .RPC) ====

        [PunRPC]
        public void StartGameplayRPC(int _mode, string messageToPlayers = "")
        {
            if (PvPGUI.Instance.showGui) { PvPGUI.Instance.showGui = false; }

            if (_mode == (int)PvP.GameModes.BattleRoyale)
            {
                //// actual moment that gameplay starts for the players
                //BattleRoyale.Instance.IsGameplayStarting = false;
                //BattleRoyale.Instance.LastSupplyDropTime = -1;
                //BattleRoyale.Instance.LastEnemySpawnTime = Time.time;
                //BattleRoyale.Instance.SupplyDropCounter = 0;
                //BattleRoyale.Instance.ActiveItemContainers.Clear();
                //BattleRoyale.Instance.ActiveBeamObjects.Clear();
            }

            // get the current teams to a list. send message to local players.
            PvP.Instance.CurrentPlayers.Clear();
            foreach (PlayerSystem ps in Global.Lobby.PlayersInLobby)
            {
                if (ps.ControlledCharacter.Faction == Character.Factions.NONE)
                {
                    if (ps.IsLocalPlayer)
                    {
                        ps.gameObject.AddComponent<Spectate>();
                    }
                    continue;
                }

                if (PvP.Instance.CurrentPlayers.ContainsKey(ps.ControlledCharacter.Faction))
                {
                    PvP.Instance.CurrentPlayers[ps.ControlledCharacter.Faction].Add(ps);
                }
                else
                {
                    PvP.Instance.CurrentPlayers.Add(ps.ControlledCharacter.Faction, new List<PlayerSystem> { ps });
                }

                if (ps.ControlledCharacter.IsLocalPlayer)
                {
                    Instance.SendUIMessageLocal(ps.ControlledCharacter, messageToPlayers);
                }
            }

            PvP.Instance.CurrentGame = (PvP.GameModes)_mode;
            PvP.Instance.GameStartTime = Time.time;
        }

        [PunRPC]
        public void StopGameplayRPC(string messageToPlayers = "")
        {
            // custom fix for Battle Royale when game ends
            if (PvP.Instance.CurrentGame == PvP.GameModes.BattleRoyale)
            {
                //BattleRoyale.Instance.EndBattleRoyale();
            }

            PvP.Instance.CurrentGame = PvP.GameModes.NONE;

            var list = Global.Lobby.PlayersInLobby;
            if (PhotonNetwork.isNonMasterClientInRoom)
            {
                list = list.Where(x => x.ControlledCharacter != null && x.ControlledCharacter.IsLocalPlayer).ToList();
            }

            foreach (PlayerSystem ps in list)
            {
                if (ps.ControlledCharacter.Faction == Character.Factions.NONE && ps.IsLocalPlayer)
                {
                    ps.gameObject.GetComponent<Spectate>()?.EndSpectate();
                }

                if (messageToPlayers != "" && ps.ControlledCharacter != null && ps.ControlledCharacter.IsLocalPlayer)
                {
                    Instance.SendUIMessageLocal(ps.ControlledCharacter, messageToPlayers);
                }

                if (!PhotonNetwork.isNonMasterClientInRoom && ps.ControlledCharacter != null && ps.ControlledCharacter.IsDead)
                {
                    Instance.SendResurrect(ps.ControlledCharacter);
                }
            }
        }

        // ======================= SMALL GAMEPLAY FUNCTIONS ============================== //

        public static void SendMessageToAll(string message)
        {
            Instance.photonView.RPC(nameof(SendMessageToAllRPC), PhotonTargets.All, message);
        }

        [PunRPC]
        private void SendMessageToAllRPC(string message)
        {
            foreach (PlayerSystem ps in Global.Lobby.PlayersInLobby.Where(x => x.ControlledCharacter.IsLocalPlayer))
            {
                SendUIMessageLocal(ps.ControlledCharacter, message);
            }
        }

        // Send UI message - should only be sent to local players.

        public void SendUIMessageLocal(Character c, string message)
        {
            c.CharacterUI.NotificationPanel.ShowNotification(message, 5);
        }

        // SendChangeFactions, and will also fix targeting system (to NOT target own faction)

        [PunRPC]
        public void SendChangeFactionsRPC(int factionInt, string UID, bool alliedToSame = true)
        {
            if (CharacterManager.Instance.GetCharacter(UID) is Character c)
            {
                var faction = (Character.Factions)factionInt;
                c.Faction = faction;
                c.DetectabilityEmitter.Faction = faction;
                //c.BroadcastMessage("ReprocessEffects", SendMessageOptions.DontRequireReceiver);

                var list = PlayerManager.Instance.AllFactions.Where(x => (int)x != (int)faction).ToList();
                c.TargetingSystem.TargetableFactions = list.ToArray();

                if (!alliedToSame)
                {
                    c.TargetingSystem.AlliedToSameFaction = false;
                }
                else
                {
                    c.TargetingSystem.AlliedToSameFaction = true;
                }
            }
        }

        // resurrect 

        public void SendResurrect(Character _character)
        {
            if (!PhotonNetwork.offlineMode)
            {
                _character.photonView.RPC("SendResurrect", PhotonTargets.All, new object[]
                {
                    true,
                    string.Empty,
                    true
                });
                //photonView.RPC("SendResurrectRPC", PhotonTargets.All, new object[] { _character.UID.ToString() });
            }
            else
            {
                _character.Resurrect();
            }
        }

        #region Deprecated_Battle_Royale


        // ================= BATTLE ROYALE RPC =======================

        // RPC calls for the Battle Royale mode. Putting it in here so there is only 1 photonView class for the mod.

        //[PunRPC]
        //public void RPCStartBattleRoyale(bool skipLoad = false)
        //{
        //    if (SceneManagerHelper.ActiveSceneName == "Monsoon") { skipLoad = true; }

        //    BattleRoyale.Instance.IsGameplayStarting = true;
        //    BattleRoyale.Instance.ForceNoSaves = true;
        //    PvP.Instance.CurrentGame = PvP.GameModes.BattleRoyale;

        //    if (!skipLoad)
        //    {
        //        foreach (PlayerSystem ps in Global.Lobby.PlayersInLobby.Where(x => x.ControlledCharacter.IsLocalPlayer))
        //        {
        //            Character c = ps.ControlledCharacter;
        //            CharacterManager.Instance.RequestAreaSwitch(c, AreaManager.Instance.GetAreaFromSceneName("Monsoon"), 0, 0, 1.5f, "Battle Royale!");
        //        }
        //    }

        //    if (Global.CheatsEnabled)
        //    {
        //        //OLogger.Warning("Disabling cheats!");
        //        BattleRoyale.Instance.WasCheatsEnabled = true;
        //        Global.CheatsEnabled = false;

        //        foreach (PlayerSystem ps in Global.Lobby.PlayersInLobby.Where(x => x.ControlledCharacter.IsLocalPlayer))
        //        {
        //            Character c = ps.ControlledCharacter;
        //            (c.CharacterControl as LocalCharacterControl).MovementMultiplier = 1f;
        //        }
        //    }

        //    StartCoroutine(BattleRoyale.Instance.SetupAfterSceneLoad(skipLoad));
        //}

        //[PunRPC]
        //public void SendSpawnEnemyRPC(string uid, float x, float y, float z)
        //{
        //    if (BattleRoyale.Instance.EnemyCharacters.Find(w => w.UID == uid) is Character c)
        //    {
        //        c.gameObject.SetActive(true);

        //        c.Teleport(new Vector3(x, y, z), c.transform.rotation);

        //        // HIGHLY SPECIFIC TO MONSOON
        //        int value = 50;
        //        if (c.Name.ToLower().Contains("butcher"))
        //        {
        //            value = 200;
        //        }
        //        else if (c.name.ToLower().Contains("illuminator"))
        //        {
        //            value = 20;
        //        }
        //        At.SetValue(new Stat(value), typeof(CharacterStats), c.Stats, "m_maxHealthStat");

        //        //BattleRoyale.Instance..FixEnemyStats(c.Stats);

        //        //foreach (PlayerSystem ps in Global.Lobby.PlayersInLobby.Where(w => w.IsLocalPlayer))
        //        //{
        //        //    SendUIMessageLocal(ps.ControlledCharacter, c.Name + " has spawned!");
        //        //}

        //        //BattleRoyale.Instance..EnemyCharacters.Remove(c);
        //    }
        //}

        //[PunRPC]
        //public void EndBattleRoyaleRPC()
        //{
        //    MenuManager.Instance.BackToMainMenu();
        //    //foreach (PlayerSystem ps in Global.Lobby.PlayersInLobby.Where(x => x.ControlledCharacter.IsLocalPlayer))
        //    //{
        //    //    Character c = ps.ControlledCharacter;
        //    //    CharacterManager.Instance.RequestAreaSwitch(c, AreaManager.Instance.GetAreaFromSceneName(previousScene), 0, 0, 1.5f, "");
        //    //}
        //}

        //[PunRPC]
        //public void RPCSendSupplyDrop(string itemUID, float x, float y, float z)
        //{
        //    StartCoroutine(BattleRoyale.Instance.SupplyDropLocalCoroutine(itemUID, new Vector3(x, y, z)));
        //}

        //[PunRPC]
        //public void RPCSendCleanup()
        //{
        //    BattleRoyale.Instance.CleanupSupplyObjects();
        //}

        //[PunRPC]
        //public void RPCGenerateStash(int itemID, string UID, float x, float y, float z)
        //{
        //    TreasureChest chest = ItemManager.Instance.GenerateItemNetwork(itemID).GetComponent<TreasureChest>();
        //    chest.UID = UID;
        //    chest.SaveType = Item.SaveTypes.Savable;
        //    chest.transform.position = new Vector3(x, y, z);
        //    BattleRoyale.Instance.ActiveItemContainers.Add(chest.gameObject);
        //}
        #endregion
    }
}
