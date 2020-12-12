using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PvP
{
    //SETTINGS
    public class Settings
    {
        public bool Enable_Menu_Scaling = false;
        public bool Show_Menu_On_Startup = true;
    }

    // static simple lists used for custom gameplay modes. Droptables, spawn locations, etc...

    public static class Templates
    {
        public static readonly List<int> StarterSkills = new List<int>
        {
            8100010, // throw lantern
            8200040, // spark
            8100072, // dagger slash
            8200600, // fire reload
            8100120, // push kick
            8100190, // shield charge
        };

        // weapon skill dictionary

        // KEY: WeaponType, VALUE: Skill Item ID
        public static readonly Dictionary<int, int> Weapon_Skills = new Dictionary<int, int>
        {
            {
                0, 8100290 // 1h sword : Puncture
            },
            {
                1, 8100380 // 1h axe: talus cleaver
            },
            {
                2, 8100270 // 1h mace: mace infusion
            },
            {
                50, 8100320 // polearm: moon swipe
            },
            {
                51, 8100362 // 2h sword: pommel counter
            },
            {
                52, 8100300 // 2h axe : execution
            },
            {
                53, 8100310 // 2h mace : juggernuaght
            },
            {
                54, 8100340 // spear : simeons gambit
            },
        };

        // ================= BATTLE ROYALE TEMPLATES ======================
        public static class BR_Templates
        {
            // not used yet
            //public static readonly List<string> SupportedLevels = new List<string> 
            //{
            //    "Monsoon",
            //};

            // =========== SCENE TEMPLATES ==============

            public static readonly Dictionary<string, float> TimeOfDayStarts = new Dictionary<string, float>
            {
                {
                    "Monsoon",
                    23.0f // 11pm
                }
            };

            public static readonly Dictionary<string, List<Vector3>> SpawnLocations = new Dictionary<string, List<Vector3>>
            {
                {
                    "Monsoon",
                    new List<Vector3>
                    {
                        new Vector3(50.4f, -2.7f, 102.6f), // monsoon gate
                        new Vector3(93.9f, -6.97f, 109.1f), // south-east docks
                        new Vector3(137.5f, 22.13f, 172.5f), // roof of Fang & Claws Inn
                        new Vector3(91.7f, -4.37f, 195.2f), // auditorium thing
                        new Vector3(81.82f, -3.38f, 258.07f), // bottom of temple steps
                        new Vector3(146.4f, 9.03f, 255.2f), // roof near galira
                        new Vector3(-11.6f, 4.78f, 276.93f), // outside damian lockwell
                        new Vector3(61.30f, 4.46f, 192.4f), // merchants promenade
                        new Vector3(3.04f, -0.744f, 200.03f), // north-east of mofat
                        new Vector3(-15.39f, -0.37f, 146.63f), // south of mofat
                        new Vector3(40.20f, -4.43f, 208.093f), // central small dock (rocks)
                        new Vector3(77.34f, 4.465f, 136.96f), // ountz
                    }
                }
            };

            public static readonly Dictionary<string, List<Vector3>> SupplyDropLocations = new Dictionary<string, List<Vector3>>
            {
                {
                    "Monsoon",
                    new List<Vector3>
                    {
                        new Vector3(92.48f, -5.45f, 138.99f),   // south-east
                        new Vector3(-18.96f, -0.29f, 208.71f),  // west spawn
                        new Vector3(90.05f, 4.465f, 162.2f),    // promenade
                        new Vector3(97.65f, 10.52f, 290.009f),  // temple
                        new Vector3(121.197f, 0.182f, 232.48f), // east bell
                        new Vector3(-6.46f, 11.67f, 287.92f), // damian lockwell roof
                    }
                }
            };

            public static readonly Dictionary<string, List<string>> ObjectsToDeactivate = new Dictionary<string, List<string>>()
            {
                {
                    "Monsoon",
                    new List<string>
                    {
                        "JesusBeam",
                        "Tree Behavior UNPC",
                        "Tree_Behavior_CityCheck",
                        "Tree Behavior Teleport",
                        "Tree_Behavior_Council",
                        "CharactersToDesactivate",
                        "Allies",
                        "DialogueAutomaticGuards",
                        "AutomaticDialogueInn",
                        "AreaSwitchToMarsh"
                    }
                }
            };

            public static readonly Dictionary<string, List<string>> ObjectsToActivate = new Dictionary<string, List<string>>
            {
                {
                    "Monsoon",
                    new List<string>
                    {
                        "WarzoneMonsoon",
                        "Ennemies",
                    }
                }
            };


            // ========== DROP TABLES =============


            public static readonly List<int> Skills_Low = new List<int>
            {
                8100090, // flamethrower
                8100140, // mana ward
                8100130, // mana push
                8100071, // opportunist stab
                8100101, // sniper shot
                8100121, // sweep kick
                8100260, // counterstrike
                8100360, // brace
                8205350, // alchemical experiment
                8205000, // marathoner
                8205120, // swift foot
                8205050, // master of motion
                8205080, // fire affinity
                8205150, // shamanic resonance
                8205210, // pressure plate
            };

            public static readonly List<int> Skills_High = new List<int>
            {
                8100020, // predator leap
                8100370, // feral strikes
                8100350, // perfect strike
                8100280, // flash onslaught
                8100200, // gong strike
                8200310, // elemental discharge
                8200030, // sigil of wind
                8200031, // sigil of fire
                8200032, // sigil of ice
                8200100, // infuse light
                8200101, // infuse wind
                8200102, // infuse frost
                8200103, // infuse fire
                8205320, // peacemaker
                8205330, // blood of giants
            };

            public static readonly List<int> Weapons_Low = new List<int>
            {
                2000061, // gold machete
                2000110, // desert khopesh
                2000151, // stranged rusted sword
                2000120, // jade scimitar
                2010020, // living wood axe
                2010040, // fang axe
                2010041, // savage axe
                2010080, // beast golem axe
                2020030, // jade lich mace
            };

            public static readonly List<int> Weapons_Med = new List<int>
            {
                2000021, // cerulean sabre
                2000031, // radiant wolf sword
                2020060, // gold lich mace
                2020110, // obsidian mace
                2020020, // brutal club
                2100000, // prayer claymore
                2100110, // assassin claymore
                2130000, // brutal spear
                2130022, // savage trident
                2120000, // brutal greatmace
                2120030, // marble greathammer
                2140070, // crescent sythe
                2200010, // recurve bow
                2200040, // coralhorn bow
            };

            public static readonly List<int> Weapons_High = new List<int>
            {
                2000150, // brand
                2000170, // maelstrom blade
                2010070, // sunfall
                2010130, // pain in the axe
                2010140, // butcher cleaver
                2020140, // skycrown mace
                2020160, // mace of seasons
                2100100, // starchild
                2100160, // thermal claymore
                2110100, // worldedge
                2110110, // kelvins
                2120070, // pillar greathammer
                2130021, // werlig
                2130160, // griigmerk
                2140120, // dreamer halberd
                2140130, // ghost reaper
                2200030, // horror bow
                2200040, // coralhorn bow
                2200020, // war bow
            };

            public static readonly List<int> Offhands_Low = new List<int>
            {
                2300120, // dragon shield
                2300070, // old legion shield
                2300210, // savage shield
                2300050 // wolf shield
            };

            public static readonly List<int> Offhands_High = new List<int>
            {
                5110100, // flintlock
                5110110, // cannon pistol
                5110002, // red lady dagger
                5110004, // manticore dagger
                2300171, // fabulous shield
                2300160, // horror shield
                2300091, // inner marble shield
                2300220, // spore shield
                5100090, // coil lantern
            };

            public static readonly List<int> Armor_Low = new List<int>
            {
                3000010, // padded
                3000011,
                3000012,
                3000020, // adventurer
                3000021,
                3000022,
                3000030, // scavenger
                3000031,
                3000034,
                3000180, // dancer clothes
                3000181,
                3000182,
                3000183,
                3000184,
                3000103, // hound mask
                3000250, // pearlbird mask
            };

            public static readonly List<int> Armor_High = new List<int>
            {
                3000061, // white priest mitre
                3100120, // runic armor
                3100201, // white kintsugi
                3100202, 
                3100200, 
                3000103, // hound mask
                3000252, // jewel mask
                3000302, // red hat
                3000303, // white hat
                3100010, // wolf set
                3100012,
                3100013, 
                3100040, // candle plate
                3100041,
                3100042,
                3100080, // blue sand set
                3100081,
                3100082,
                3100090, // crimson set
                3100091,
                3100092,
                3100150, // squire set
                3100151,
                3100152,
                3100170, // shock set
                3100171,
                3100172,
            };

            public static readonly List<int> Supplies_Low = new List<int>
            {
                4300010, // health pot x7
                4300010, // 
                4300010, // 
                4300010, // 
                4300010, // 
                4300010, // 
                4300010, // 
                4300060, // discipline pot x3
                4300060, // 
                4300060, // 
                4300020, // mana potion x2
                4300020, // 
                4300030, // stamina potion x2
                4300030, // 
                4300050, // rage pot
                4300070, // warm potion
                4300080, // cool potion
                4300100, // blessed potion
                4300120, // possesed potion
            };

            public static readonly List<int> Supplies_High = new List<int>
            {
                4300240, // mega health
                4300240, // 
                4300240, // 
                4300260, // mega stamina
                4300260, // 
                4300260, // 
                4300250, // mega mana
                4300250, // 
                4300250, // 
                4400070, // dark varnish
                4400060, // spiritual varnish
                4400051, // ice varnish
                4400041, // fire varnish
                4400031, // glow varnish
            };
        }

    }
}
