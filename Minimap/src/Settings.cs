using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BepInEx.Configuration;

namespace Minimap
{
    public class Settings
    {
        internal const string CTG_GENERAL = "General Settings";
        public static ConfigEntry<bool> LOWPOLY_CAMERA;
        //public static ConfigEntry<float> CULLING_DEPTH;
        //public static ConfigEntry<float> CAMERA_HEIGHT;
        public static ConfigEntry<bool> PLAYER_MARKERS;
        public static ConfigEntry<bool> ENEMY_MARKERS;
        public static ConfigEntry<bool> LOOT_MARKERS;
        public static ConfigEntry<bool> NPC_MARKERS;

        internal const string CTG_PLAYER1 = "Player 1 Settings";
        public static ConfigEntry<float> P1_ZOOM;
        public static ConfigEntry<float> P1_OUTDOOREXTRA;

        internal const string CTG_PLAYER2 = "Player 2 Settings";
        public static ConfigEntry<float> P2_ZOOM;
        public static ConfigEntry<float> P2_OUTDOOREXTRA;

        internal static void Init(ConfigFile config)
        {
            LOWPOLY_CAMERA = config.Bind(CTG_GENERAL, "Use low poly camera?", false, 
                "If enabled, the minimap camera will use a low poly renderer, resulting in better performance.");
            LOWPOLY_CAMERA.SettingChanged += OnSettingChanged;

            PLAYER_MARKERS = config.Bind(CTG_GENERAL, "Enable Player minimap markers", true,
                "Enable markers to indicate Players on the minimap?");
            PLAYER_MARKERS.SettingChanged += OnSettingChanged;

            ENEMY_MARKERS = config.Bind(CTG_GENERAL, "Enable Enemy minimap markers", true,
                "Enable markers to indicate Enemies on the minimap?");
            ENEMY_MARKERS.SettingChanged += OnSettingChanged;

            LOOT_MARKERS = config.Bind(CTG_GENERAL, "Enable Loot minimap markers", true,
                "Enable markers to indicate Loot on the minimap? (Does not include Item Spawns)");
            LOOT_MARKERS.SettingChanged += OnSettingChanged;

            NPC_MARKERS = config.Bind(CTG_GENERAL, "Enable NPC minimap markers", true,
                "Enable markers to indicate NPCs on the minimap?");
            NPC_MARKERS.SettingChanged += OnSettingChanged;

            P1_ZOOM = config.Bind(CTG_PLAYER1, "Minimap zoom-out", 14.0f,
                new ConfigDescription("How much the minimap is zoomed out", new AcceptableValueRange<float>(1f, 250f)));
            P1_ZOOM.SettingChanged += OnSettingChanged;

            P1_OUTDOOREXTRA = config.Bind(CTG_PLAYER1, "Extra outdoor zoom-out", 100.0f,
                new ConfigDescription("How much extra the minimap zooms out when outdoors", new AcceptableValueRange<float>(1f, 500f)));
            P1_OUTDOOREXTRA.SettingChanged += OnSettingChanged;

            P2_ZOOM = config.Bind(CTG_PLAYER2, "Minimap zoom-out", 14.0f,
                new ConfigDescription("How much the minimap is zoomed out", new AcceptableValueRange<float>(1f, 250f)));
            P2_ZOOM.SettingChanged += OnSettingChanged;

            P2_OUTDOOREXTRA = config.Bind(CTG_PLAYER2, "Extra outdoor zoom-out", 100.0f,
                new ConfigDescription("How much extra the minimap zooms out when outdoors", new AcceptableValueRange<float>(1f, 500f)));
            P2_OUTDOOREXTRA.SettingChanged += OnSettingChanged;
        }

        private static void OnSettingChanged(object _, EventArgs __)
        {
            foreach (var script in MinimapScript.Instances)
                script?.ApplyFromConfig();

            MarkerScript.ApplyConfigToInstances();
        }
    }
}
