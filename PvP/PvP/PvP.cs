using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Reflection;
using UnityEngine.UI;
using System.IO;
using BepInEx;
using HarmonyLib;
using UnityEngine.SceneManagement;

namespace PvP
{
    [BepInPlugin(GUID, NAME, VERSION)]
    public class PvP : BaseUnityPlugin
    {
        public const string GUID = "com.sinai.pvp";
        public const string NAME = "PvP";
        public const string VERSION = "2.3";

        public static PvP Instance;

        public Settings settings = new Settings();
        private const string savePath = @"Mods\PvP.json";

        private const string MenuKey = "PvP Menu";

        public GameModes CurrentGame = GameModes.NONE;
        public float GameStartTime = 0f;
        public Dictionary<Character.Factions, List<PlayerSystem>> CurrentPlayers = new Dictionary<Character.Factions, List<PlayerSystem>>();

        public bool FriendlyFireEnabled;
        public bool FriendlyTargetingEnabled;
        public bool EnemiesDisabled;

        public enum GameModes
        {
            NONE,
            Deathmatch,
            BattleRoyale
        }

        internal void Awake()
        {
            Instance = this;

            CustomKeybindings.AddAction(MenuKey, CustomKeybindings.KeybindingsCategory.Menus, CustomKeybindings.ControlType.Both, 5);

            LoadSettings();

            SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;

            var harmony = new Harmony(GUID);
            harmony.PatchAll();

            var obj = new GameObject("PvP");
            DontDestroyOnLoad(obj);
            obj.AddComponent<RPCManager>();

            var view = obj.AddComponent<PhotonView>();
            view.viewID = 998;
            Debug.Log("Registered PvP with ViewID " + view.viewID);

            obj.AddComponent<PvPGUI>();
            obj.AddComponent<PlayerManager>();
            //obj.AddComponent<BattleRoyale>();
            obj.AddComponent<DeathMatch>();
        }

        // Disable enemies feature

        private void SceneManager_activeSceneChanged(Scene arg0, Scene arg1)
        {
            var scene = SceneManagerHelper.ActiveSceneName.ToLower();
            if (scene.Contains("lowmemory") || scene.Contains("mainmenu"))
            {
                return;
            }

            Instance.StartCoroutine(OnSceneChange());
        }

        private IEnumerator OnSceneChange()
        {
            while (NetworkLevelLoader.Instance.IsGameplayLoading || !NetworkLevelLoader.Instance.AllPlayerDoneLoading)
            {
                yield return new WaitForSeconds(0.3f);
            }

            if (EnemiesDisabled)
            {
                SetEnemiesActive(false);
            }
        }

        public static void SetEnemiesActive(bool active)
        {
            Instance.EnemiesDisabled = !active;
            AISquadManager.Instance.gameObject.SetActive(active);
        }

        // On Update

        internal void Update()
        {
            if ((MenuManager.Instance.IsReturningToMainMenu || Global.IsApplicationClosing) && CurrentGame != GameModes.NONE)
            {
                CurrentGame = GameModes.NONE;
                if (!PhotonNetwork.isNonMasterClientInRoom)
                {
                    StopGameplay("The host has left the game!");
                }
            }

            // make sure game is running
            if (Global.Lobby.PlayersInLobbyCount < 1 || NetworkLevelLoader.Instance.IsGameplayPaused)
            {
                return;
            }

            // handle player input 
            foreach (PlayerSystem ps in Global.Lobby.PlayersInLobby.Where(x => x.ControlledCharacter.IsLocalPlayer))
            {
                if (CustomKeybindings.m_playerInputManager[ps.PlayerID].GetButtonDown(MenuKey))
                {
                    PvPGUI.Instance.showGui = !PvPGUI.Instance.showGui;
                }
            }

            // update custom gameplay
            if (CurrentGame != GameModes.NONE)
            {
                UpdateGameplay();
            }
        }

        // ========================== GENERAL GAMEPLAY CONTROL =========================

        public void StartGameplay(int _mode, string messageToPlayers = "")
        {
            CurrentGame = (GameModes)_mode;

            RPCManager.Instance.photonView.RPC(nameof(RPCManager.StartGameplayRPC), PhotonTargets.All, new object[] { _mode, messageToPlayers });
        }

        // only the host can call this whenever they want, other players may trigger it when they are the winning player.
        public void StopGameplay(string messageToPlayers = "")
        {
            RPCManager.Instance.photonView.RPC(nameof(RPCManager.StopGameplayRPC), PhotonTargets.All, new object[] { messageToPlayers });
        }

        // the game-mode specific Update functions, called directly instead of MonoBehaviour.Update()
        private void UpdateGameplay()
        {
            if (CurrentGame == GameModes.BattleRoyale)
            {
                //if (!BattleRoyale.Instance.IsGameplayStarting)
                //{
                //    BattleRoyale.Instance.UpdateBR();
                //}
            }
            else if (CurrentGame == GameModes.Deathmatch)
            {
                DeathMatch.Instance.UpdateDM();
            }
        }

        // ================================== HOOKS ===================================

        [HarmonyPatch(typeof(InteractionRevive), "OnActivate")]
        public class InteractionRevive_OnActivate
        {
            [HarmonyPrefix]
            public static bool Prefix(InteractionRevive __instance)
            {
                var self = __instance;

                if (Instance.CurrentGame == GameModes.NONE)
                {
                    return true;
                }
                else
                {
                    if (At.GetValue(typeof(InteractionBase), self, "m_lastCharacter") is Character m_lastCharacter && m_lastCharacter.IsLocalPlayer)
                    {
                        RPCManager.Instance.SendUIMessageLocal(m_lastCharacter, "You cannot revive players during a game!");
                        Instance.StartCoroutine(Instance.FixReviveInteraction(self, self.OnActivationEvent));
                        self.OnActivationEvent = null;
                    }

                    return false;
                }
            }
        }

        private IEnumerator FixReviveInteraction(InteractionBase _base, UnityEngine.Events.UnityAction action)
        {
            yield return new WaitForSeconds(0.1f);

            _base.OnActivationEvent = action;
        }

        [HarmonyPatch(typeof(InteractionRevive), "ProcessText")]
        public class InteractionRevive_ProcessText
        {
            [HarmonyPostfix]
            public static void Postfix(ref string __result)
            {
                if (Instance.CurrentGame != GameModes.NONE)
                {
                    __result = "";
                }
            }
        }

        // ============= settings ==============

        private void LoadSettings()
        {
            bool flag = false;
            if (File.Exists(savePath))
            {
                if (JsonUtility.FromJson<Settings>(File.ReadAllText(savePath)) is Settings s2)
                {
                    settings = s2;
                    flag = true;
                }
            }
            if (!flag)
            {
                settings = new Settings();
                SaveSettings();
            }
        }

        internal void OnDisable()
        {
            SaveSettings();
        }

        private void SaveSettings()
        {
            if (!Directory.Exists("Mods")) { Directory.CreateDirectory("Mods"); }

            if (File.Exists(savePath)) { File.Delete(savePath); }

            File.WriteAllText(savePath, JsonUtility.ToJson(settings, true));
        }
    }
}