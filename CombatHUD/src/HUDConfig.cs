using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx.Configuration;

namespace CombatHUD
{
    public static class HUDConfig
    {
        private const string CTG_PLAYER = "Player";
        public static ConfigEntry<bool> Player_NumericalVitals;
        public static ConfigEntry<bool> Player_DamageLabels;
        public static ConfigEntry<bool> Player_StatusTimers;

        private const string CTG_ENEMY = "Enemy";
        public static ConfigEntry<bool> Enemy_ShowHealth;
        public static ConfigEntry<bool> Enemy_ShowStatuses;
        public static ConfigEntry<bool> Enemy_StatusTimers;
        public static ConfigEntry<bool> Enemy_StatusBuildup;
        public static ConfigEntry<bool> Enemy_DamageLabels;
        public static ConfigEntry<bool> Enemy_Infobox;

        private const string CTG_LABELS = "Damage Labels";
        public static ConfigEntry<bool> DamageLabel_StayAtHitPos;
        public static ConfigEntry<bool> DamageLabel_DisableColors;
        public static ConfigEntry<int> DamageLabel_MinFontSize;
        public static ConfigEntry<int> DamageLabel_MaxFontSize;
        public static ConfigEntry<float> DamageLabel_MaxDistance;
        public static ConfigEntry<float> DamageLabel_DamageCeiling;
        public static ConfigEntry<float> DamageLabel_MinimumDamage;
        public static ConfigEntry<float> DamageLabel_Lifespan;

        private const string CTG_P1 = "Player 1";
        public static ConfigEntry<float> Player1_InfoboxHorizontal;
        public static ConfigEntry<float> Player1_InfoboxVertical;

        private const string CTG_P2 = "Player 2";
        public static ConfigEntry<float> Player2_InfoboxHorizontal;
        public static ConfigEntry<float> Player2_InfoboxVertical;

        public static void Init(ConfigFile config)
        {
            Player_NumericalVitals = config.Bind(CTG_PLAYER, "Numerical Vitals", true, "Show player vitals as numerical values");
            Player_DamageLabels = config.Bind(CTG_PLAYER, "Show Damage Labels", true, "Show player's damage dealt");
            Player_StatusTimers = config.Bind(CTG_PLAYER, "Show Status Timers", true, "Show remaining lifespan on player's status effects");

            Enemy_ShowHealth = config.Bind(CTG_ENEMY, "Show Health", true, "Show targeted enemy's health as numerical value");
            Enemy_DamageLabels = config.Bind(CTG_ENEMY, "Show Damage Labels", true, "Show damage dealt by enemies");
            Enemy_ShowStatuses = config.Bind(CTG_ENEMY, "Show Statuses", true, "Show inflicted status effects on targeted enemy");
            Enemy_StatusTimers = config.Bind(CTG_ENEMY, "Status Timers", true, "Show remaining lifespans on enemy status effects");
            Enemy_StatusBuildup = config.Bind(CTG_ENEMY, "Show Build-Up", true, "Show the 'build-up' value for status effects");
            Enemy_Infobox = config.Bind(CTG_ENEMY, "Show Infobox", true, "Show a detailed info-box for the targeted enemy");

            DamageLabel_StayAtHitPos = config.Bind(CTG_LABELS, "Labels stay at hit position", false, "Damage labels stay at the position of the hit (otherwise track to the Character)");
            DamageLabel_DisableColors = config.Bind(CTG_LABELS, "Disable label colors", false, "Forces white damage label text (otherwise color of highest damage)");
            DamageLabel_MinFontSize = config.Bind(CTG_LABELS, "Minimum label font size", 15, 
                new ConfigDescription("Minimum damage label font size (smallest end of scale)", new AcceptableValueRange<int>(8, 40)));
            DamageLabel_MaxFontSize = config.Bind(CTG_LABELS, "Maximum label font size", 30, 
                new ConfigDescription("Maximum damage label font size (highest end of scale)", new AcceptableValueRange<int>(8, 40)));
            DamageLabel_MaxDistance = config.Bind(CTG_LABELS, "Maximum label distance from player", 40f, 
                new ConfigDescription("Maximum distance from player to show damage labels", new AcceptableValueRange<float>(0f, 250f)));
            DamageLabel_DamageCeiling = config.Bind(CTG_LABELS, "Damage Scale Ceiling", 50f, 
                new ConfigDescription("Highest damage number you want to scale to (for label size and speed)", new AcceptableValueRange<float>(10f, 500f)));
            DamageLabel_MinimumDamage = config.Bind(CTG_LABELS, "Minimum damage", 0f,
                new ConfigDescription("The minimum damage to display. Anything below this value will be hidden.", new AcceptableValueRange<float>(0f, 100f)));
            DamageLabel_Lifespan = config.Bind(CTG_LABELS, "Label Lifespan", 2.5f, 
                new ConfigDescription("How long damage labels are shown for", new AcceptableValueRange<float>(0.5f, 5f)));

            Player1_InfoboxHorizontal = config.Bind(CTG_P1, "Infobox Horizontal Offset", 0f, 
                new ConfigDescription("Horizontal screen offset of Infobox (Show Infobox setting)", new AcceptableValueRange<float>(0f, 10000f)));
            Player1_InfoboxVertical = config.Bind(CTG_P1, "Infobox Vertical Offset", 0f,
                new ConfigDescription("Vertical screen offset of Infobox (Show Infobox setting)", new AcceptableValueRange<float>(0f, 10000f)));

            Player2_InfoboxHorizontal = config.Bind(CTG_P2, "Infobox Horizontal Offset", 0f,
                new ConfigDescription("Horizontal screen offset of Infobox (Show Infobox setting)", new AcceptableValueRange<float>(0f, 10000f)));
            Player2_InfoboxVertical = config.Bind(CTG_P2, "Infobox Vertical Offset", 0f,
                new ConfigDescription("Vertical screen offset of Infobox (Show Infobox setting)", new AcceptableValueRange<float>(0f, 10000f)));
        }

    }
}
