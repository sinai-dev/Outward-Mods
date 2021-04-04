using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SideLoader;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using BepInEx.Configuration;

namespace CombatHUD
{
    [BepInPlugin(GUID, NAME, VERSION)]
    [BepInDependency(SL.GUID, BepInDependency.DependencyFlags.HardDependency)]
    public class CombatHUD : BaseUnityPlugin
    {
        public const string GUID = "com.sinai.combathud";
        public const string VERSION = "5.2";
        public const string NAME = "Combat HUD";

        public static CombatHUD Instance;

        public static GameObject HUDCanvas;

        public static bool IsHudHidden(int playerID) => m_hideUI || !m_playerShowHud[playerID];
        internal static bool[] m_playerShowHud = new bool[] { false, false };
        internal static bool m_hideUI;

        internal void Awake()
        {
            Instance = this;

            try
            {
                var harmony = new Harmony(GUID);
                harmony.PatchAll();

                HUDConfig.Init(Config);

                SL.OnPacksLoaded += Setup;

                Logger.LogMessage($"{NAME} started, version {VERSION}");
            }
            catch (Exception e)
            {
                Logger.LogMessage("Exception setting up CombatHUD: " + e);
            }
        }

        internal void Update()
        {
            m_hideUI = (bool)At.GetField(Global.Instance, "m_hideUI");
            m_playerShowHud = (bool[])At.GetField<OptionManager>("m_playerShowHUD");

            if (!HUDCanvas || Global.Lobby.PlayersInLobbyCount < 1)
            {
                return;
            }

            // main canvas disable
            if (!NetworkLevelLoader.Instance.IsOverallLoadingDone || !NetworkLevelLoader.Instance.AllPlayerReadyToContinue)
            {
                if (HUDCanvas.activeSelf)
                {
                    //HUDCanvas.SetActive(false);
                }
            }
            else if (!HUDCanvas.activeSelf)
            {
                HUDCanvas.SetActive(true);
            }
        }

        private void Setup()
        {
            var packName = "sinai-dev CombatHUD";

            var pack = SL.GetSLPack(packName);
            var bundle = pack.AssetBundles["combathud"];

            var canvasAsset = bundle.LoadAsset<GameObject>("HUDCanvas");

            HUDCanvas = UnityEngine.Object.Instantiate(canvasAsset);
            UnityEngine.Object.DontDestroyOnLoad(HUDCanvas);
            HUDCanvas.hideFlags |= HideFlags.HideAndDontSave;

            // setup draw order
            var canvas = HUDCanvas.GetComponent<Canvas>();
            canvas.sortingOrder = 999; // higher = shown above other layers.

            // setup the autonomous components

            // ====== target manager ======
            var targetMgrHolder = HUDCanvas.transform.Find("TargetManager_Holder");

            for (int i = 0; i < 2; i++)
            {
                var mgr = targetMgrHolder.transform.Find($"TargetManager_P{i + 1}").gameObject.AddComponent<TargetManager>();
                mgr.Split_ID = i;
            }

            // ====== player manager ======
            var statusTimerHolder = HUDCanvas.transform.Find("PlayerStatusTimers");
            statusTimerHolder.gameObject.AddComponent<PlayersManager>();

            // ====== damage labels ======
            var damageLabels = HUDCanvas.transform.Find("DamageLabels");
            damageLabels.gameObject.AddComponent<DamageLabels>();

            Logger.LogMessage("Combat HUD finished setting up");
        }

        public static float Rel(float offset, bool height = false) // false for width, true for height
        {
            return offset * (height ? Screen.height : Screen.width) * 100f / (height ? 1080f : 1920f) * 0.01f;
        }
    }
}
