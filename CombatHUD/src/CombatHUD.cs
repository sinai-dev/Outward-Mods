using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SideLoader;
using SharedModConfig;
using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace CombatHUD
{
    [BepInPlugin(GUID, NAME, VERSION)]
    [BepInDependency(SLPlugin.GUID, BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency(SharedModConfig.SharedModConfig.GUID, BepInDependency.DependencyFlags.HardDependency)]
    public class CombatHUD : BaseUnityPlugin
    {
        public const string GUID = "com.sinai.combathud";
        public const string VERSION = "4.5";
        public const string NAME = "Combat HUD";

        public static CombatHUD Instance;

        public static ModConfig config;
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

                config = SetupConfig();
                config.Register();

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
            var packName = "CombatHUD";

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

        private ModConfig SetupConfig()
        {
            config = new ModConfig
            {
                ModName = "CombatHUD",
                SettingsVersion = 1.1,
                Settings = new List<BBSetting>()
                {
                    new BoolSetting
                    {
                        Name = Settings.PlayerVitals,
                        SectionTitle = "Player Settings",
                        Description = "Show player vitals as numerical values",
                        DefaultValue = true
                    },
                    new BoolSetting
                    {
                        Name = Settings.PlayerDamageLabels,
                        Description = "Show player's damage dealt",
                        DefaultValue = true
                    },
                    new BoolSetting
                    {
                        Name = Settings.PlayerStatusTimers,
                        Description = "Show remaining lifespan on player's status effects",
                        DefaultValue = true
                    },
                    new BoolSetting
                    {
                        Name = Settings.EnemyHealth,
                        SectionTitle = "Enemy Settings",
                        Description = "Show targeted enemy's health as numerical value",
                        DefaultValue = true
                    },
                    new BoolSetting
                    {
                        Name = Settings.EnemyStatus,
                        Description = "Show inflicted status effects on targeted enemy",
                        DefaultValue = true
                    },
                    new BoolSetting
                    {
                        Name = Settings.EnemyStatusTimers,
                        Description = "Show remaining lifespans on enemy status effects",
                        DefaultValue = true
                    },
                    new BoolSetting
                    {
                        Name = Settings.EnemyBuildup,
                        Description = "Show the 'build-up' value for status effects",
                        DefaultValue = true
                    },
                    new BoolSetting
                    {
                        Name = Settings.EnemyDamageLabels,
                        Description = "Show damage dealt by enemies",
                        DefaultValue = true
                    },
                    new BoolSetting
                    {
                        Name = Settings.EnemyInfobox,
                        Description = "Show a detailed info-box for the targeted enemy",
                        DefaultValue = true
                    },
                    new BoolSetting
                    {
                        Name = Settings.LabelsStayAtHitPos,
                        SectionTitle = "Damage Labels",
                        Description = "Damage labels stay at the position of the hit (otherwise track to the Character)",
                        DefaultValue = false
                    },
                    new BoolSetting
                    {
                        Name = Settings.DisableColors,
                        Description = "White damage label text (otherwise color of highest damage)",
                        DefaultValue = false
                    },
                    new FloatSetting
                    {
                        Name = Settings.MinFontSize,
                        Description = "Minimum damage label font size (smallest end of scale)",
                        DefaultValue = 15f,
                        MaxValue = 40,
                        MinValue = 8,
                        RoundTo = 0,
                        ShowPercent = false
                    },
                    new FloatSetting
                    {
                        Name = Settings.MaxFontSize,
                        Description = "Maximum damage label font size (highest end of scale)",
                        DefaultValue = 30f,
                        MaxValue = 40f,
                        MinValue = 8f,
                        RoundTo = 0,
                        ShowPercent = false
                    },
                    new FloatSetting
                    {
                        Name = Settings.MaxDistance,
                        Description = "Maximum distance from player to show damage labels",
                        DefaultValue = 40f,
                        MaxValue = 250f,
                        MinValue = 0f,
                        RoundTo = 0,
                        ShowPercent = false
                    },
                    new FloatSetting
                    {
                        Name = Settings.DamageCeiling,
                        Description = "Damage Ceiling: highest damage number you want to scale to (for label size and speed)",
                        DefaultValue = 50f,
                        MaxValue = 1000f,
                        MinValue = 10f,
                        RoundTo = 0,
                        ShowPercent = false
                    },
                    new FloatSetting
                    {
                        Name = Settings.MinimumDamage,
                        Description = "Minimum Damage: any damage below this number will not be shown",
                        DefaultValue = 0f,
                        MaxValue = 50f,
                        MinValue = 0f,
                        RoundTo = 1,
                        ShowPercent = false
                    },
                    new FloatSetting
                    {
                        Name = Settings.LabelLifespan,
                        Description = "Label Lifespan: how long damage labels are shown for",
                        DefaultValue = 2.5f,
                        MaxValue = 5f,
                        MinValue = 0.5f,
                        RoundTo = 1,
                        ShowPercent = false
                    },
                    new FloatSetting
                    {
                        Name = Settings.Infobox_P1_X,
                        SectionTitle = "Target Infobox Settings",
                        Description = "Player 1 Infobox: Horizontal offset",
                        DefaultValue = 0f,
                        MaxValue = 4000,
                        MinValue = 0f,
                        RoundTo = 0,
                        ShowPercent = false
                    },
                    new FloatSetting
                    {
                        Name = Settings.Infobox_P1_Y,
                        Description = "Player 1 Infobox: Vertical offset",
                        DefaultValue = 0f,
                        MaxValue = 2000,
                        MinValue = 0f,
                        RoundTo = 0,
                        ShowPercent = false
                    },
                    new FloatSetting
                    {
                        Name = Settings.Infobox_P2_X,
                        Description = "Player 2 Infobox: Horizontal offset",
                        DefaultValue = 0f,
                        MaxValue = 4000,
                        MinValue = 0f,
                        RoundTo = 0,
                        ShowPercent = false
                    },
                    new FloatSetting
                    {
                        Name = Settings.Infobox_P2_Y,
                        Description = "Player 2 Infobox: Vertical offset",
                        DefaultValue = 0f,
                        MaxValue = 2000,
                        MinValue = 0f,
                        RoundTo = 0,
                        ShowPercent = false
                    }
                }
            };

            return config;
        }

        public static float Rel(float offset, bool height = false) // false for width, true for height
        {
            return offset * (height ? Screen.height : Screen.width) * 100f / (height ? 1080f : 1920f) * 0.01f;
        }
    }
}
