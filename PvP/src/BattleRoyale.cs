using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SideLoader;
using UnityEngine;

namespace PvP
{
    public class BattleRoyale : MonoBehaviour
    {
        public static BattleRoyale Instance;

        public bool IsGameplayStarting = false;
        public bool IsGameplayEnding = false;

        //public bool ForceNoSaves = false;
        //public bool WasCheatsEnabled = false;

        // Supply Drops
        public static int SupplyDropInterval = 120; // supply drop interval
        public float LastSupplyDropTime = 0f;
        public int SupplyDropCounter = 0;
        public List<GameObject> ActiveItemContainers = new List<GameObject>(); // list of all chest objects
        public List<GameObject> ActiveBeamObjects = new List<GameObject>(); // list of beams
        public GameObject beamObject; // the master beam

        // Enemies
        public static int EnemyDropInterval = 45; // enemy spawn interval (GOES LOWER DEPENDING ON # OF PLAYERS! -2 PER PLAYER!)
        public float LastEnemySpawnTime = 0;
        public List<Character> EnemyCharacters = new List<Character>(); // scourge enemies

        //// area reset
        //public float LastAreaResetTime = 0f;

        internal void Awake()
        {
            Instance = this;
        }

        // important logic for battle royale mode (runs outside of gameplay update)
        internal void Update()
        {
            if (MenuManager.Instance.IsInMainMenuScene)
            {
                if (IsGameplayStarting) { IsGameplayStarting = false; StopAllCoroutines(); }
                if (PvP.Instance.CurrentGame == PvP.GameModes.BattleRoyale)
                {
                    PvP.Instance.CurrentGame = PvP.GameModes.NONE;
                }
            }

            if (IsGameplayStarting) { return; }

            if (IsGameplayEnding)
            {
                if (PvPGUI.Instance.ShowGUI == false)
                {
                    PvPGUI.Instance.ShowGUI = true;
                }
                return;
            }

            //if (ForceNoSaves && PvP.Instance.CurrentGame == PvP.GameModes.NONE && !MenuManager.Instance.IsReturningToMainMenu)
            //{
            //    //Debug.Log("[BR] Update turning off ForceNoSaves");
            //    ForceNoSaves = false;
            //}
            //if (WasCheatsEnabled && !IsGameplayStarting && PvP.Instance.CurrentGame == PvP.GameModes.NONE)
            //{
            //    //OLogger.Warning("[BR] Enabling cheats!");
            //    WasCheatsEnabled = false;
            //    Global.CheatsEnabled = true;
            //}
        }


        // ================================ BATTLE ROYALE SETUP ================================

        public bool CheckCanStart()
        {
            List<Character.Factions> list = new List<Character.Factions>();
            foreach (PlayerSystem ps in Global.Lobby.PlayersInLobby)
            {
                if (!list.Contains(ps.ControlledCharacter.Faction))
                {
                    list.Add(ps.ControlledCharacter.Faction);
                }
            }
            if (list.Count() > 1)
            {
                return true;
            }

            return false;
        }

        // custom BR start function (call this one directly)
        public void StartBattleRoyale(bool skipLoad = false)
        {
            RPCManager.Instance.photonView.RPC("RPCStartBattleRoyale", PhotonTargets.All, new object[] { skipLoad });
        }

        // LOCAL - this coroutine starts immediately after RPCStartBattleRoyale() is called.
        public IEnumerator SetupAfterSceneLoad(bool skipLoad = false)
        {
            if (!skipLoad)
            {
                // wait for scene to start loading
                while (!NetworkLevelLoader.Instance.IsSceneLoading)
                {
                    yield return new WaitForSeconds(0.1f);
                }

                // wait for scene to finish loading
                while (!NetworkLevelLoader.Instance.IsOverallLoadingDone)
                {
                    yield return new WaitForSeconds(0.1f);
                }
            }

            // ============== Character Setup ==============
            SetupCharacters();

            while (NetworkLevelLoader.Instance.IsGameplayPaused)
            {
                yield return null;
            }

            // wait a second in case we just loaded the scene
            yield return new WaitForSeconds(1f);

            // ========= SCENE SETUP ===========

            Debug.Log("Setting up local scene...");

            LocalSceneSetup();

            if (!PhotonNetwork.isNonMasterClientInRoom)
            {
                Debug.Log("Setting up as host...");
                HostSceneSetup();
            }

            Debug.Log("Battle Royale is about to start, finalizing...");

            // ======== finalize =======

            // each player calls this locally after setting up their scene
            RPCManager.Instance.StartGameplayRPC((int)PvP.GameModes.BattleRoyale, "A Battle Royale has begun!");

            // get enemy drop interval now (currentplayers list created). faster enemy spawns depending on remaining players. Min 15, max 45.
            EnemyDropInterval = Mathf.Clamp(45 - (PlayerManager.Instance.GetRemainingPlayers().Count() * 2), 15, 45);

            Debug.Log("Finalized!");
        }

        // =============== SCENE SETUP ==============

        private void HostSceneSetup()
        {
            // cleanup world items
            while (ItemManager.Instance.WorldItems.Keys.Count() > 0)
            {
                ItemManager.Instance.DestroyItem(ItemManager.Instance.WorldItems.Keys[0]);
            }

            // setup characters
            if (Templates.BR_Templates.SpawnLocations.ContainsKey(SceneManagerHelper.ActiveSceneName))
            {
                var allSpawns = Templates.BR_Templates.SpawnLocations[SceneManagerHelper.ActiveSceneName].ToList();

                // HOST SETUP EACH PLAYER
                foreach (PlayerSystem ps in Global.Lobby.PlayersInLobby)
                {
                    Character _char = ps.ControlledCharacter;

                    if (allSpawns.Count == 0)
                    {
                        allSpawns = Templates.BR_Templates.SpawnLocations[SceneManagerHelper.ActiveSceneName].ToList();
                    }

                    int randomSpawn = UnityEngine.Random.Range(0, allSpawns.Count());
                    _char.Teleport(allSpawns[randomSpawn], _char.transform.rotation);
                    allSpawns.RemoveAt(randomSpawn);

                    // purge statuses
                    _char.StatusEffectMngr.Purge();
                }
            }
            else
            {
                PvP.Instance.StopGameplay("An error has occured! We seem to be in the wrong scene.");
            }
        }

        private void LocalSceneSetup()
        {
            // set TOD
            EnvironmentConditions.Instance.SetTimeOfDay(Templates.BR_Templates.TimeOfDayStarts[SceneManagerHelper.ActiveSceneName]);

            // DEACTIVATE OBJECTS
            if (Templates.BR_Templates.ObjectsToDeactivate.ContainsKey(SceneManagerHelper.ActiveSceneName))
            {
                foreach (string s in Templates.BR_Templates.ObjectsToDeactivate[SceneManagerHelper.ActiveSceneName])
                {
                    if (GameObject.Find(s) is GameObject obj)
                    {
                        if (s == "JesusBeam") { beamObject = obj; } // hold onto the jesusbeam

                        obj.SetActive(false);
                    }
                }
            }

            // ACTIVATE OBJECTS
            if (Templates.BR_Templates.ObjectsToActivate.ContainsKey(SceneManagerHelper.ActiveSceneName))
            {
                foreach (string s in Templates.BR_Templates.ObjectsToActivate[SceneManagerHelper.ActiveSceneName])
                {
                    if (GameObject.Find(s) is GameObject obj)
                    {
                        obj.SetActive(true);
                    }
                    else if (FindInactiveObjectByName(s) is GameObject obj2)
                    {
                        obj2.SetActive(true);
                    }
                }
            }

            // get enemy list for RPC setactive calls
            EnemyCharacters.Clear();
            foreach (Character c in CharacterManager.Instance.Characters.Values.Where(x => x.IsAI && x.Faction != Character.Factions.Player))
            {
                //Debug.Log("adding " + c.Name + " to enemycharacters dict!");
                EnemyCharacters.Add(c);
                c.gameObject.SetActive(false);
            }

            //// ====== setup local characters =======

            foreach (PlayerSystem ps in Global.Lobby.PlayersInLobby.Where(x => x.IsLocalPlayer))
            {
                Character c = ps.ControlledCharacter;

                var pouchUID = "Pouch_" + c.UID;
                if (ItemManager.Instance.GetItem(pouchUID))
                {
                    ItemManager.Instance.DestroyItem(pouchUID);
                }

                // fix inventory/pouch
                ItemContainer pouchContainer = null;
                if (c.Inventory.PouchPrefab)
                {
                    var transform = Instantiate(c.Inventory.PouchPrefab);
                    transform.SetParent(base.transform, false);
                    pouchContainer = transform.GetComponent<ItemContainer>();
                    pouchContainer.SetKeepAlive();
                    pouchContainer.UID = "CustomPouch_" + c.UID;
                    pouchContainer.ProcessInit();
                    At.SetField(c.Inventory, "m_inventoryPouch", pouchContainer);

                    if (PhotonNetwork.isNonMasterClientInRoom)
                    {
                        pouchContainer.ForceUpdateParentChange();
                    }
                }
                if (c.Inventory.MapKnowledge == null)
                {
                    var mk = new CharacterMapKnowledge();
                    mk.Init();
                    At.SetField(c.Inventory, "m_mapKnowledge", mk);
                }
                c.Inventory.Equipment.ProcessStart();

                // give starter items
                SetupStarterPack(c);

                // DOUBLE PURGE
                c.StatusEffectMngr.Purge();
            }

            // ==== disable compass ====

            foreach (UICompass compass in Resources.FindObjectsOfTypeAll<UICompass>())
            {
                compass.Hide();
            }
        }

        // ============= CHARACTER SETUP =================== //

        private void SetupCharacters()
        {
            foreach (PlayerSystem ps in Global.Lobby.PlayersInLobby)
            {
                CustomPlayerSetup(ps.ControlledCharacter);
            }
        }

        private void CustomPlayerSetup(Character _char)
        {
            // wipe character

            var save = SaveManager.Instance.CreateNewCharacterSave();
            save.PSave.NewSave = true;
            save.PSave.ManaPoint = 0;
            save.PSave.UID = _char.UID;
            save.PSave.Name = _char.Name;
            save.PSave.VisualData = _char.VisualData;
            save.PSave.Food = 1000f;
            save.PSave.Drink = 1000f;
            save.PSave.HardcoreMode = false;

            save.ApplyLoadedSaveToChar(_char);

            // ========= set custom stats ==========

            At.SetField(_char.Stats, "m_maxHealthStat", new Stat(500f));
            At.SetField(_char.Stats, "m_maxStamina", new Stat(200f));
            At.SetField(_char.Stats, "m_maxManaStat", new Stat(75f));
            _char.Stats.GiveManaPoint(1);

            // ========= finalize ==========

            // disable character cheats
            _char.Cheats.Invincible = false;
            _char.Cheats.IndestructibleEquipment = false;
            _char.Cheats.NotAffectedByWeightPenalties = false;

            _char.Cheats.NeedsEnabled = true;

            // refresh stats
            At.Invoke(_char.Stats, "UpdateVitalStats");
            _char.Stats.Reset();
            _char.Stats.RestoreAllVitals();

            if (At.GetField(_char.QuickSlotMngr, "m_quickSlots") is QuickSlot[] m_quickSlots)
            {
                for (int i = 0; i < m_quickSlots.Count(); i++)
                {
                    _char.QuickSlotMngr.GetQuickSlot(i).Clear();
                }
            }
        }

        private void SetupStarterPack(Character _char)
        {
            Debug.Log("SetupStarterPack for " + _char.gameObject.name);

            // give bag
            Item bag = ItemManager.Instance.GenerateItemNetwork(5300000); // adventurer bag
            if (bag != null)
            {
                _char.Inventory.TakeItem(bag.UID, false);
                At.Invoke(_char.Inventory.Equipment, "EquipWithoutAssociating", new object[] { bag, false });

                // 1 random weapon, 1 random offhand
                ItemContainer target = _char.Inventory.Pouch;
                if (bag.GetComponentInChildren<ItemContainer>() is ItemContainer container)
                {
                    target = container;
                }
                AddItemsToContainer(Templates.BR_Templates.Weapons_Low, 1, target.transform, true); // 1 random weapon
                AddItemsToContainer(Templates.BR_Templates.Offhands_Low, 1, target.transform, true); // 1 random offhand
                AddItemsToContainer(Templates.BR_Templates.Supplies_Low, 5, target.transform, true); // 5 random supplies

                // add starter skills
                AddItemsToContainer(Templates.StarterSkills, Templates.StarterSkills.Count(), target.transform);
            }
            else
            {
                //OLogger.Error("bag was null");
            }
        }

        // ================= BATTLE ROYALE GAMEPLAY ====================

        public void UpdateBR()
        {
            if (!PhotonNetwork.isNonMasterClientInRoom)
            {
                WorldHostUpdate();
            }
        }

        private void WorldHostUpdate()
        {
            List<Character.Factions> teamsLeft = PlayerManager.Instance.GetRemainingTeams();
            if (teamsLeft.Count() < 2)
            {
                string winners = teamsLeft[0].ToString();
                if (!winners.EndsWith("s")) { winners += "s"; }
                PvP.Instance.StopGameplay(winners + " have won!");
            }

            if (LastSupplyDropTime <= 0 || Time.time - LastSupplyDropTime > SupplyDropInterval)
            {
                LastSupplyDropTime = Time.time;
                SupplyDropCounter++;

                StartCoroutine(GenerateSupplyDrops());
            }

            if (Time.time - LastEnemySpawnTime > EnemyDropInterval && EnemyCharacters.Count() > 0)
            {
                LastEnemySpawnTime = Time.time;

                Character c = null;
                var list = EnemyCharacters.Where(x => !x.gameObject.activeSelf).ToList();

                if (list.Count() > 0)
                {
                    var butcher = list.Find(x => x.Name.ToLower().Contains("butcher"));

                    if (SupplyDropCounter < 3 || !butcher)
                    {
                        if (butcher) { list.Remove(butcher); }
                        int random = UnityEngine.Random.Range(0, list.Count());
                        c = list.ElementAt(random);
                    }
                    else
                    {
                        c = butcher; // time to spawn the Butcher of Men!
                    }

                    if (c)
                    {
                        if (c.IsDead)
                        {
                            c.Resurrect();
                        }

                        string scene = SceneManagerHelper.ActiveSceneName;
                        var spawnlist = Templates.BR_Templates.SpawnLocations[scene];
                        int random = UnityEngine.Random.Range(0, spawnlist.Count());
                        Vector3 loc = spawnlist[random];

                        if (PhotonNetwork.offlineMode)
                        {
                            RPCManager.Instance.SendSpawnEnemyRPC(c.UID.ToString(), loc.x, loc.y, loc.z);
                        }
                        else
                        {
                            RPCManager.Instance.photonView.RPC("SendSpawnEnemyRPC", PhotonTargets.All, new object[] { c.UID.ToString(), loc.x, loc.y, loc.z });
                        }

                        // send butcher message
                        if (c.Name.ToLower().Contains("butcher"))
                        {
                            RPCManager.SendMessageToAll("The Butcher of Men has spawned!");
                            EnemyCharacters.RemoveAll(x => x.UID == butcher.UID);
                        }

                        // change faction to NONE
                        PlayerManager.Instance.ChangeFactions(c, Character.Factions.NONE);
                    }
                }
            }
        }

        // ============== END GAMEPLAY ================ //

        public void EndBattleRoyale()
        {
            //// fix area reset
            //if (!PhotonNetwork.isNonMasterClientInRoom)
            //{
            //    Area area = AreaManager.Instance.GetAreaFromSceneName(SceneManagerHelper.ActiveSceneName);
            //    At.SetValue(LastAreaResetTime, typeof(Area), area, "m_resetTime");
            //}

            // restore compass
            foreach (UICompass compass in Resources.FindObjectsOfTypeAll<UICompass>())
            {
                compass.Show(true);
            }

            // cleanup objects
            try
            {
                if (PhotonNetwork.offlineMode)
                {
                    RPCManager.Instance.RPCSendCleanup();
                }
                else
                {
                    RPCManager.Instance.photonView.RPC("RPCSendCleanup", PhotonTargets.All, new object[0]);
                }
                ActiveItemContainers.Clear();
                ActiveBeamObjects.Clear();
            }
            catch { }

            if (!PhotonNetwork.isNonMasterClientInRoom && !MenuManager.Instance.IsReturningToMainMenu && !MenuManager.Instance.IsInMainMenuScene)
            {
                IsGameplayEnding = true;
            }
        }

        // ================ SUPPLY DROPS =================== //

        public void AddItemsToContainer(List<int> _itemList, int _amountToAdd, Transform _container, bool _removeWhenAdded = true)
        {
            List<int> list2 = _itemList.ToList();
            //Debug.Log("Generating " + _amountToAdd + " drops from a list size of " + list2.Count());
            for (int i = 0; i < _amountToAdd; i++)
            {
                if (list2.Count() == 0) { return; }

                int random = UnityEngine.Random.Range(0, list2.Count());
                //Debug.Log("Rolled a " + random);
                Item item = ItemManager.Instance.GenerateItemNetwork(list2[random]);
                item.ChangeParent(_container);

                if (_removeWhenAdded)
                {
                    list2.RemoveAt(random);
                }
            }
        }

        private IEnumerator GenerateSupplyDrops()
        {
            //OLogger.Warning("generating supply drops!");

            if (ActiveItemContainers.Count() > 0)
            {
                if (PhotonNetwork.offlineMode)
                {
                    RPCManager.Instance.RPCSendCleanup();
                }
                else
                {
                    RPCManager.Instance.photonView.RPC("RPCSendCleanup", PhotonTargets.All, new object[0]);
                }

                yield return new WaitForSeconds(2f); // wait 2 seconds after destroying the active objects, so we dont start destroying the new ones!
            }

            if (SupplyDropCounter == 1)
            {
                // first drop. generate initial supply caches for all players

                foreach (PlayerSystem ps in Global.Lobby.PlayersInLobby)
                {
                    Character _char = ps.ControlledCharacter;
                    TreasureChest stash = ItemManager.Instance.GenerateItemNetwork(1000110).GetComponent<TreasureChest>();
                    stash.transform.position = _char.transform.position + new Vector3(1.5f, 0, 1.5f);
                    ActiveItemContainers.Add(stash.gameObject);

                    if (!PhotonNetwork.offlineMode)
                    {
                        RPCManager.Instance.photonView.RPC("RPCGenerateStash", PhotonTargets.Others, new object[]
                        {
                            stash.ItemID,
                            stash.UID,
                            stash.transform.position.x,
                            stash.transform.position.y,
                            stash.transform.position.z
                        });
                    }

                    At.Invoke(stash, "InitDrops");
                    for (int i = 0; i < 5; i++)
                    {
                        At.SetField(stash, "m_hasGeneratedContent", false);
                        stash.GenerateContents();
                    }
                }
            }
            else
            {
                RPCManager.SendMessageToAll("Supply Chests are spawning!");

                // generate chest or ornate chest
                List<Vector3> locations = Templates.BR_Templates.SupplyDropLocations[SceneManagerHelper.ActiveSceneName].ToList();

                // 1 supply drop per 2 players. minimum of 2 drops.
                int numOfDrops = (int)Math.Ceiling(PlayerManager.Instance.GetRemainingPlayers().Count() * 0.5f);
                numOfDrops = (int)Mathf.Clamp(numOfDrops, 2, locations.Count());

                //Debug.Log("Generating " + numOfDrops + " supply drops from a locations list size of " + locations.Count());
                for (int i = 0; i < numOfDrops; i++)
                {
                    if (locations.Count == 0) { break; }

                    int random = UnityEngine.Random.Range(0, locations.Count());
                    //Debug.Log("Rolled location index " + random);
                    GenerateChestSingle(locations[random]);
                    locations.RemoveAt(random);
                }
            }
        }

        private void GenerateChestSingle(Vector3 position)
        {
            int chestPrefabID;
            if (SupplyDropCounter == 2)
            {
                // first drop regular chest (1 is stash)
                chestPrefabID = 1000000;
            }
            else
            {
                // second drop onwards is ornate chest
                chestPrefabID = 1000040;
            }

            TreasureChest chest = ItemManager.Instance.GenerateItemNetwork(chestPrefabID).GetComponent<TreasureChest>();
            ActiveItemContainers.Add(chest.gameObject);

            // set position (direct local, RPC to others)
            chest.transform.position = position;
            if (!PhotonNetwork.offlineMode)
            {
                RPCManager.Instance.photonView.RPC("RPCGenerateStash", PhotonTargets.Others, new object[]
                {
                    chest.ItemID,
                    chest.UID,
                    position.x,
                    position.y,
                    position.z
                });
            }

            // generate drop contents
            var container = chest.GetComponentInChildren<ItemContainer>().transform;
            if (SupplyDropCounter == 2)
            {
                // first major supply drop. not too powerful yet   
                AddItemsToContainer(Templates.BR_Templates.Weapons_Med, 1, container);
                AddItemsToContainer(Templates.BR_Templates.Offhands_High, 1, container);
                AddItemsToContainer(Templates.BR_Templates.Armor_Low, 2, container);
                AddItemsToContainer(Templates.BR_Templates.Supplies_Low, 6, container, false);
            }
            else
            {
                // second drop onwards is the high table
                AddItemsToContainer(Templates.BR_Templates.Weapons_High, 1, container);
                AddItemsToContainer(Templates.BR_Templates.Offhands_High, 1, container);
                AddItemsToContainer(Templates.BR_Templates.Armor_High, 2, container);
                AddItemsToContainer(Templates.BR_Templates.Supplies_High, 6, container);
            }

            // send RPC to sync the drop with other clients
            if (PhotonNetwork.offlineMode)
            {
                RPCManager.Instance.RPCSendSupplyDrop(chest.UID, position.x, position.y, position.z);
            }
            else
            {
                RPCManager.Instance.photonView.RPC("RPCSendSupplyDrop", PhotonTargets.All, new object[]
                {
                        chest.UID,
                        position.x,
                        position.y,
                        position.z
                });
            }
        }

        // the global RPC call for supply drops starts this coroutine.
        public IEnumerator SupplyDropLocalCoroutine(string itemUID, Vector3 location)
        {
            while (!ItemManager.Instance.GetItem(itemUID))
            {
                yield return new WaitForSeconds(0.1f);
            }
            //Item supplyDrop = ItemManager.Instance.GetItem(itemUID);
            //supplyDrop.transform.position = location;

            if (beamObject != null)
            {
                var newbeam = Instantiate(beamObject);
                newbeam.SetActive(true);
                newbeam.transform.GetChild(0).gameObject.SetActive(false); // disable "quad" thing
                DestroyImmediate(newbeam.GetComponent<MeshCollider>()); // destroy collider
                newbeam.transform.localScale = new Vector3(100f, 10f, 100f);
                newbeam.transform.position = location + (Vector3.up * 10);

                ActiveBeamObjects.Add(newbeam);
            }
        }

        public void CleanupSupplyObjects()
        {
            for (int i = 0; i < ActiveItemContainers.Count(); i++)
            {
                if (ActiveItemContainers.Count() < 1) { break; }

                var obj = ActiveItemContainers[i];
                if (obj && obj.GetComponent<TreasureChest>() is TreasureChest chest)
                {
                    //DestroyImmediate(obj);
                    try
                    {
                        ItemManager.Instance.DestroyItem(chest.UID);
                    }
                    catch { }
                }
                ActiveItemContainers.RemoveAt(i);
                i--;
            }

            for (int i = 0; i < ActiveBeamObjects.Count(); i++)
            {
                if (ActiveBeamObjects.Count() < 1) { break; }

                var obj = ActiveBeamObjects[i];
                if (obj)
                {
                    DestroyImmediate(obj);
                }
                ActiveBeamObjects.RemoveAt(i);
                i--;
            }
        }

        public GameObject FindInactiveObjectByName(string name)
        {
            Transform[] objs = Resources.FindObjectsOfTypeAll<Transform>() as Transform[];
            for (int i = 0; i < objs.Length; i++)
            {
                if (objs[i].hideFlags == HideFlags.None)
                {
                    if (objs[i].name == name)
                    {
                        return objs[i].gameObject;
                    }
                }
            }
            return null;
        }


    }
}
