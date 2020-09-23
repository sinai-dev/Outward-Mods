using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharedModConfig;

namespace Minimap
{
    public class Settings
    {
        public static ModConfig Instance;

        // setting names
        public const string P1_ZOOM = "p1zoom";
        public const string P1_OUTDOOREXTRA = "p1outdoorextra";
        public const string P2_ZOOM = "p2zoom";
        public const string P2_OUTDOOREXTRA = "p2outdoorextra";

        public static void OnSettingsApplied()
        {
            foreach (var script in MinimapScript.Instances)
            {
                script?.ApplyFromConfig();
            }
        }

        public static void SetupConfig()
        {
            Instance = NewConfig;

            Instance.OnSettingsLoaded += OnSettingsApplied;
            Instance.OnSettingsSaved += OnSettingsApplied;

            Instance.Register();
        }

        private static ModConfig NewConfig => new ModConfig
        {
            ModName = "Minimap",
            SettingsVersion = 1.0,
            Settings = new List<BBSetting>
            {
                new FloatSetting
                {
                    SectionTitle = "Player 1",
                    Name = P1_ZOOM,
                    Description = "Minimap Zoom (Default = 14)",
                    DefaultValue = 14.0f,
                    MinValue = 1f,
                    MaxValue = 250f,
                    RoundTo = 1,
                    Increment = 0.5f
                },
                new FloatSetting
                {
                    Name = P1_OUTDOOREXTRA,
                    Description = "Extra outdoor zoom-out (Default = 100)",
                    DefaultValue = 100.0f,
                    MinValue = 1f,
                    MaxValue = 500f,
                    RoundTo = 0,
                    Increment = 1
                },
                new FloatSetting
                {
                    SectionTitle = "Player 2",
                    Name = P2_ZOOM,
                    Description = "Minimap Zoom (Default = 14)",
                    DefaultValue = 14.0f,
                    MinValue = 1f,
                    MaxValue = 250f,
                    RoundTo = 1,
                    Increment = 0.5f
                },
                new FloatSetting
                {
                    Name = P2_OUTDOOREXTRA,
                    Description = "Extra outdoor zoom-out (Default = 100)",
                    DefaultValue = 100.0f,
                    MinValue = 1f,
                    MaxValue = 500f,
                    RoundTo = 0,
                    Increment = 1
                },
            }
        };
    }
}
