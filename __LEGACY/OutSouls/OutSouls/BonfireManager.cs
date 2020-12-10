using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;
using System.Reflection;
using System.IO;
using UnityEngine.Events;
using HarmonyLib;
using SideLoader;

namespace OutSoulsMod
{
    public class BonfireInfo
    {
        public Vector3 position;
        public string uid;
    }

    public class BonfireManager : MonoBehaviour
    {
        public static BonfireManager Instance;

        public List<BonfireInfo> CurrentBonfires = new List<BonfireInfo>();
        private float m_timeOfLastSetup = -1;

        public string currentScene = "";
        public UnityAction BonfireEvent = null;
        public bool SceneChangeFlag = false;

        // currently sitting at a bonfire
        public bool IsBonfireInteracting = false;
        public float lastInteractionTime;

        internal void Awake()
        {
            Instance = this;

            

            if (BonfireEvent == null)
            {
                BonfireEvent = new UnityAction(StartBonfireInteraction);
            }
        }

        internal void Update()
        {
            // check for scene change
            if (currentScene != SceneManagerHelper.ActiveSceneName)
            {
                currentScene = SceneManagerHelper.ActiveSceneName;
                SceneChangeFlag = true;
            }

            // return if gameplay paused
            if (Global.Lobby.PlayersInLobbyCount < 1 || NetworkLevelLoader.Instance.IsGameplayPaused)
            {
                return;
            }

            // if scene has changed since last unpause, run the LoadScene()
            if (SceneChangeFlag)
            {
                if ((bool)OutSouls.config.GetValue(Settings.Enable_Bonfire_System) && bonfirePositions.ContainsKey(currentScene))
                {
                    LoadScene();
                }

                SceneChangeFlag = false;
            }

            // handle some bonfire menu show/hide logic
            if (IsBonfireInteracting && (bool)OutSouls.config.GetValue(Settings.Enable_Bonfire_System) && (bool)OutSouls.config.GetValue(Settings.Cant_Use_Bonfires_In_Combat))
            {
                foreach (PlayerSystem ps in Global.Lobby.PlayersInLobby)
                {
                    if (ps.ControlledCharacter.InCombat)
                    {
                        IsBonfireInteracting = false;
                        break;
                    }
                }
            }
        }

        public void LoadScene()
        {
            if (PhotonNetwork.isNonMasterClientInRoom) 
            {
                RequestSyncInfo(CharacterManager.Instance.GetFirstLocalCharacter().UID.ToString());
                return; 
            }

            CurrentBonfires.Clear();
            m_timeOfLastSetup = Time.time;

            List<Vector3> positions = bonfirePositions[currentScene];

            if (positions == null) { return; }

            for (int i = 0; i < positions.Count(); i++)
            {
                string _uid = UID.Generate().ToString();
                Vector3 pos = positions[i];

                RPCManager.Instance.SendBonfire(pos, _uid);

                CurrentBonfires.Add(new BonfireInfo
                {
                    position = pos,
                    uid = _uid
                });
            }
        }
        
        // this sync is just for when someone joins the host. 
        // they need a list of current bonfire UIDs and locations so the interactions work.
        // for scene changes after the first one, we handle RPC ourselves.
        public void RequestSyncInfo(string _asker)
        {
            RPCManager.Instance.RequestBonfireSyncInfo(_asker);
        }

        public void SendSyncInfo(string _asker)
        {
            // We only want to do this when someone joins the first time. 
            // If we just started setting up less than 10 seconds ago, ignore the request.
            if (Time.time - m_timeOfLastSetup > 10 && CurrentBonfires.Count() > 0)
            {
                RPCManager.Instance.SendSyncInfo(_asker, CurrentBonfires);
            }
        }

        public void ReceiveSyncInfo(List<BonfireInfo> positions)
        {
            foreach (BonfireInfo info in positions)
            {
                StartCoroutine(SetupBonfireLocal(info.position, info.uid));
            }
        }

        // usually called from LoadScene. Also by the Sync functions when you join a host.
        public IEnumerator SetupBonfireLocal(Vector3 position, string uid)
        {
            Debug.Log("Setting up bonfire at: " + position.ToString() + ", uid: " + uid);

            var bonfire = new GameObject("Bonfire_" + uid);
            bonfire.transform.position = position;

            var activatedCampfire = CustomItemVisuals.GetOrigItemVisuals(ResourcesPrefabManager.Instance.GetItemPrefab(5000101).GetComponent<Deployable>().DeployedStateItemPrefab, VisualPrefabType.VisualPrefab);
            var fireVisuals = Instantiate(activatedCampfire.gameObject);
            fireVisuals.transform.parent = bonfire.transform;
            fireVisuals.transform.position = bonfire.transform.position;

            var swordVisuals = Instantiate(CustomItemVisuals.GetOrigItemVisuals(ResourcesPrefabManager.Instance.GetItemPrefab(2000151), VisualPrefabType.VisualPrefab).gameObject);
            swordVisuals.transform.parent = bonfire.transform;
            swordVisuals.transform.position = bonfire.transform.position + Vector3.up;
            swordVisuals.transform.rotation = Quaternion.Euler(0, 0, 90);

            var sigilVisuals = Instantiate(CustomItemVisuals.GetOrigItemVisuals(ResourcesPrefabManager.Instance.GetItemPrefab(8000010), VisualPrefabType.VisualPrefab).gameObject);
            sigilVisuals.transform.parent = bonfire.transform;
            sigilVisuals.transform.position = bonfire.transform.position;

            // interaction gameobject base
            GameObject actionHolder = new GameObject("Interaction_Holder");
            actionHolder.transform.parent = bonfire.transform;
            actionHolder.transform.position = bonfire.transform.position;

            // setup components
            InteractionTriggerBase triggerBase = actionHolder.AddComponent<InteractionTriggerBase>();
            InteractionActivator activator = actionHolder.AddComponent<InteractionActivator>();
            InteractionBase interactBase = actionHolder.AddComponent<InteractionBase>();

            // setup the trigger base
            triggerBase.SetHolderUID(uid);
            triggerBase.GenerateColliderIfNone = true;
            triggerBase.IsLargeTrigger = true;
            triggerBase.DetectionPriority = -9999;
            triggerBase.SetActivator(activator);

            // setup the interaction activator
            activator.SetUID(uid);
            At.SetValue("Rest at <color=#fc4e03>Bonfire</color>", typeof(InteractionActivator), activator, "m_overrideBasicText");
            At.SetValue(interactBase, typeof(InteractionActivator), activator, "m_sceneBasicInteraction");

            // setup the event interaction base
            interactBase.OnActivationEvent = BonfireEvent;

            // =========== finalize =========== //
            bonfire.gameObject.SetActive(true);

            yield return new WaitForSeconds(0.1f);

            //activator.Reregister();
            foreach (ParticleSystem ps in fireVisuals.GetComponentsInChildren<ParticleSystem>())
            {
                ps.Play();
                var m = ps.main;
                m.prewarm = true;
                m.playOnAwake = true;
            }
            foreach (Light light in fireVisuals.GetComponentsInChildren<Light>())
            {
                light.gameObject.SetActive(true);
            }
            foreach (SoundPlayer sound in fireVisuals.GetComponentsInChildren<SoundPlayer>())
            {
                sound.Play(false);
            }
        }

        // ======================= ACTUAL BONFIRE INTERACTION ========================

        // custom UnityAction event, triggered on Bonfire Interaction
        public void StartBonfireInteraction()
        {
            if (IsBonfireInteracting)
            {
                return;
            }

            // cannot use bonfires in combat
            if ((bool)OutSouls.config.GetValue(Settings.Cant_Use_Bonfires_In_Combat))
            {
                foreach (PlayerSystem ps in Global.Lobby.PlayersInLobby)
                {
                    if (ps.ControlledCharacter.InCombat)
                    {
                        if (ps.IsLocalPlayer)
                        {
                            ps.ControlledCharacter.CharacterUI.ShowInfoNotification("You cannot use Bonfires while players in combat!");
                        }
                        return;
                    }
                }
            }

            StartCoroutine(BonfireCoroutine());
            IsBonfireInteracting = true;
            lastInteractionTime = Time.time;

            BonfireGUI.Instance.cur_Main_Page = 0;
            BonfireGUI.Instance.cur_Tele_Page = 0;
            BonfireGUI.Instance.scroll = Vector2.zero;
        }

        public IEnumerator BonfireCoroutine()
        {
            foreach (PlayerSystem ps in Global.Lobby.PlayersInLobby.Where(x => x.IsLocalPlayer))
            {
                Character c = ps.ControlledCharacter;
                c.Stats.RestoreAllVitals();
                At.SetValue(true, typeof(Character), c, "m_invincible");
                c.CharacterControl.InputLocked = true;
                c.CastSpell(Character.SpellCastType.Sit, c.gameObject, Character.SpellCastModifier.Immobilized, 1, -1f);
            }

            yield return new WaitForSeconds(1f);

            while (IsBonfireInteracting || NetworkLevelLoader.Instance.IsGameplayPaused)
            {
                yield return null;
            }

            foreach (Character c in CharacterManager.Instance.Characters.Values)
            {
                if (c.IsAI)
                {
                    if ((bool)OutSouls.config.GetValue(Settings.Bonfires_Heal_Enemies))
                    {
                        if (c.Health <= 0)
                        {
                            if (!PhotonNetwork.isNonMasterClientInRoom)
                            {
                                var wasEnabled = c.gameObject.activeSelf;
                                c.RessurectCampingSquad();
                                c.gameObject.SetActive(false);
                                if (wasEnabled) { c.gameObject.SetActive(true); }
                            }
                        }
                        else
                        {
                            c.ResetStats();
                        }
                            
                    }
                }
                else
                {
                    At.SetValue(false, typeof(Character), c, "m_invincible");
                    c.CharacterControl.InputLocked = false;
                    c.ForceCancel(true, true);
                }
            }

            if ((bool)OutSouls.config.GetValue(Settings.Bonfires_Heal_Enemies))
            {
                foreach(SplitPlayer player in SplitScreenManager.Instance.LocalPlayers)
                {
                    player.CharUI.ShowInfoNotification("Nearby enemies have been resurrected...");
                }
                //StartCoroutine(OutSoulsGUI.Instance.SetMessage("Nearby foes have been resurrected...", 3));
            }
        }

        public IEnumerator ResetArea()
        {
            Dictionary<string, Vector3> positions = new Dictionary<string, Vector3>();
            if (!PhotonNetwork.isNonMasterClientInRoom)
            {
                foreach (PlayerSystem ps in Global.Lobby.PlayersInLobby)
                {
                    Character c = ps.ControlledCharacter;
                    positions.Add(c.UID, c.transform.position);
                }
            }

            BlackFade.Instance.StartFade(true);
            yield return new WaitForSeconds(1f);

            IsBonfireInteracting = false;

            Area area = AreaManager.Instance.GetAreaFromSceneName(SceneManagerHelper.ActiveSceneName);
            float origReset = (float)At.GetValue(typeof(Area), area, "m_resetTime");
            At.SetValue(-999, typeof(Area), area, "m_resetTime");
            NetworkLevelLoader.Instance.ReloadLevel(0, true, -1, 0, true);

            while (NetworkLevelLoader.Instance.IsGameplayPaused)
            {
                yield return null;
            }

            foreach (KeyValuePair<string, Vector3> entry in positions)
            {
                if (CharacterManager.Instance.GetCharacter(entry.Key) is Character c)
                {
                    c.Teleport(entry.Value, Quaternion.identity);

                    c.CharacterControl.InputLocked = false;
                    At.SetValue(false, typeof(Character), c, "m_invincible");

                    c.Stats.RestoreAllVitals();
                    c.ResetCastType();
                    c.ForceCancel(true, true);
                }
            }

            Debug.Log("Restoring original reset time to " + origReset);
            At.SetValue(origReset, typeof(Area), area, "m_resetTime");

            yield return null;
        }

        public void TeleportButton(string label, string scene, int spawnID)
        {
            if (GUILayout.Button(label))
            {
                if (!(bool)OutSouls.config.GetValue(Settings.Disable_Bonfire_Costs))
                {
                    Character c = CharacterManager.Instance.GetWorldHostCharacter();
                    if (c.Inventory.OwnsItem(6500010))
                    {
                        c.Inventory.RemoveItem(6500010, 1);
                        RPCManager.Instance.SendTeleport(scene, spawnID);
                    }
                    else
                    {
                        CharacterManager.Instance.GetFirstLocalCharacter().CharacterUI.ShowInfoNotification("Host does not own a Fire Stone!");
                    }
                }
                else
                {
                    RPCManager.Instance.SendTeleport(scene, spawnID);
                }
            }
        }

        public IEnumerator Teleport(string scene, Vector3 position)
        {
            BlackFade.Instance.StartFade(true);

            yield return new WaitForSeconds(1);

            IsBonfireInteracting = false;

            if (SceneManagerHelper.ActiveSceneName != scene)
            {
                NetworkLevelLoader.Instance.LoadLevel(scene, -1, 0, true);
            }

            while (NetworkLevelLoader.Instance.IsGameplayPaused)
            {
                yield return null;
            }

            foreach (string uid in CharacterManager.Instance.PlayerCharacters.Values)
            {
                if (CharacterManager.Instance.GetCharacter(uid) is Character c)
                {
                    c.Teleport(position, c.transform.rotation);
                }
            }

            yield return new WaitForSeconds(1);

            BlackFade.Instance.StartFade(false);
        }

        // hook for enemies owning required weapon after resurrection

        [HarmonyPatch(typeof(AISCombat), "EquipRequiredWeapons")]
        public class AISCombat_EquipRequiredWeapons
        {
            public static bool Prefix(AISCombat __instance)
            {
                var self = __instance;

                Character c = At.GetValue(typeof(AIState), self as AIState, "m_character") as Character;

                foreach (Weapon w in self.RequiredWeapon)
                {
                    if (!c.Inventory.OwnsItem(w.ItemID))
                    {
                        Item i = ItemManager.Instance.GenerateItemNetwork(w.ItemID);
                        c.Inventory.TakeItem(i, false);
                    }
                }

                return true;
            }
        }

        // ============= BONFIRE POSITIONS =============

        public Dictionary<string, List<Vector3>> bonfirePositions = new Dictionary<string, List<Vector3>>
        {
            {
                "ChersoneseNewTerrain",
                new List<Vector3>
                {
                    new Vector3(248.1f, 34.7f, 1436.2f), // outside Cierzo gate
                    new Vector3(678.8f, 29.7f, 488.1f), // near Vendavel
                    new Vector3(1264.2f, 64.2f, 859.0f), // on the hill in south-eastern cherso
                    new Vector3(1513.9f, 21.1f, 1593.2f), // through ghost pass, just over the bridge
                    new Vector3(1007.9f, 44.7f, 1066), // Conflux Mountain north
                    new Vector3(33.5f, 5.5f, 1180.4f), // Cierzo Storage Beach
                }
            },
            {
                "Chersonese_Dungeon2", // blister burrow
                new List<Vector3>
                {
                    new Vector3(-9.8f, 0, -32.5f),
                }
            },
            {
                "Chersonese_Dungeon5", // voltaic hatchery
                new List<Vector3>
                {
                    new Vector3(89, 16.5f, -26.8f),
                }
            },
            {
                "Chersonese_Dungeon6", // corrupted tombs
                new List<Vector3>
                {
                    new Vector3(-79, 4.5f, -7.9f),
                }
            },
            {
                "Emercar",
                new List<Vector3>
                {
                    new Vector3(1147.518f, -27.93924f, 1171.312f), // outside berg gate (northern forest)
                    new Vector3(483.6f, -1.2f, 1251.3f), // north of "Ruined Settlement" (western forest)
                    new Vector3(651.6f, 41.4f, 673.9f), // cabal of wind temple, east side (southern forest)
                    new Vector3(1228.5f, -4.2f, 567.9f), // by old hunter's cabin (eastern forest)
                    new Vector3(926.2f, -32.5f, 952.3f), // central lake (tree Husk)
                }
            },
            {
                "Emercar_Dungeon1", // Royal Manticore's Lair
                new List<Vector3>
                {
                    new Vector3(-29.4f, 14.5f, 23.2f),
                }
            },
            {
                "Emercar_Dungeon2", // Forest Hives
                new List<Vector3>
                {
                    new Vector3(-50.2f, 19.7f, -118.5f),
                }
            },
            {
                "Emercar_Dungeon4", // Face of the Ancients
                new List<Vector3>
                {
                    new Vector3(-1.6f, 0f, 13.4f),
                }
            },
            {
                "Abrassar",
                new List<Vector3>
                {
                    new Vector3(-166.9f, 133.9f, -463.4f), // outside Levant
                    new Vector3(-37.6f, 127.1f, 299.7f), // south of Stone Titan Caves
                    new Vector3(308.3f, 140.4f, -267.5f), // next to the Walled Garden slightly north-east
                    new Vector3(509, 142.4f, 289), // by the Abandoned Docks
                    new Vector3(-455.7f, 124.9f, 149.1f), // Ancient Hive
                }
            },
            {
                "Abrassar_Dungeon1", // Undercity Passage
                new List<Vector3>
                {
                    new Vector3(-35.1f, 5.8f, -8f),
                }
            },
            {
                "Abrassar_Dungeon2", // Electric Lab
                new List<Vector3>
                {
                    new Vector3(-0.2f, -19.9f, -0.7f),
                }
            },
            {
                "Abrassar_Dungeon3", // The Slide
                new List<Vector3>
                {
                    new Vector3(51f, 7f, -24.9f),
                }
            },
            {
                "Abrassar_Dungeon6", // Sand Rose Cave
                new List<Vector3>
                {
                    new Vector3(27.2f, 0.2f, -7f),
                }
            },
            {
                "HallowedMarshNewTerrain",
                new List<Vector3>
                {
                    new Vector3(276.1f, -58.5f, 923.5f), // outside Monsoon
                    new Vector3(847.7f, -48.5f, 1363), // near Under Island
                    new Vector3(973.1f, -58.8f, 656.7f), // on the flat little island between Dead Roots, Abandoned Ziggurat and Reptilian Lair
                    new Vector3(1424.2f, -49f, 472.6f), // by Cabal of Wind Totem (south-central swamp)
                    new Vector3(1408.2f, -51.6f, 1316.2f), // north-west of Spire of Light by the Caravan Trader spot
                }
            },
            {
                "Hallowed_Dungeon1", // Jade Quarry
                new List<Vector3>
                {
                    new Vector3(-5f, 30f, 300.7f),
                }
            },
            {
                "Hallowed_Dungeon3", // Reptilian Lair
                new List<Vector3>
                {
                    new Vector3(32.9f, -4.5f, -75f),
                }
            },
            {
                "Hallowed_Dungeon4_Interior", // Dark Ziggurat
                new List<Vector3>
                {
                    new Vector3(-134.5f, 59f, -101.6f),
                }
            },
            {
                "Hallowed_Dungeon7", // Dead Roots
                new List<Vector3>
                {
                    new Vector3(-9.1f, 0f, -65.2f),
                }
            },
        };

    }
}
